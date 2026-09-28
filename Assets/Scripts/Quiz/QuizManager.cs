using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Audio;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;

namespace ComputerExplorer.Quiz
{
    /// <summary>
    /// Question session logic and scoring for one module. The active session is static so it survives a
    /// trip to the AR scanner (AR identification questions) and back.
    /// </summary>
    public class QuizManager
    {
        public static QuizManager Active { get; private set; }

        public ModuleData Module { get; }
        public List<QuestionData> Questions { get; }
        public int Index { get; private set; }
        public AnswerController Answer { get; private set; }
        public bool Submitted { get; private set; }
        public bool LastCorrect { get; private set; }
        public bool Finished { get; private set; }
        public List<(QuestionData question, bool correct)> Results { get; } = new List<(QuestionData, bool)>();

        private QuizManager(ModuleData module)
        {
            Module = module;
            Questions = module.questions.Where(q => q != null).ToList();
            Index = 0;
            Finished = Questions.Count == 0;
            if (!Finished) Answer = new AnswerController(Questions[0]);
        }

        public static QuizManager Start(ModuleData module) => Active = new QuizManager(module);

        public static void End() => Active = null;

        public QuestionData Current => Finished ? null : Questions[Index];
        public int Score => Results.Count(r => r.correct);
        public int Total => Questions.Count;
        public float Percent => Total == 0 ? 0f : (float)Score / Total;

        public void Submit()
        {
            if (Submitted || Finished || !Answer.IsComplete) return;
            LastCorrect = Answer.Evaluate();
            Submitted = true;
            Results.Add((Current, LastCorrect));
            ProgressManager.Instance.RecordAnswer(Module.moduleId, Current.questionId, LastCorrect);
            AudioManager.Instance.PlayFeedback(LastCorrect ? FeedbackSound.Correct : FeedbackSound.Incorrect);
        }

        public void Next()
        {
            if (!Submitted) return;
            Submitted = false;
            Index++;
            if (Index >= Questions.Count)
            {
                Finished = true;
                Answer = null;
                AudioManager.Instance.PlayFeedback(FeedbackSound.Complete);
                return;
            }
            Answer = new AnswerController(Questions[Index]);
        }
    }
}
