using System;
using System.Linq;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using UnityEngine;

namespace ComputerExplorer.Progress
{
    public enum ModuleState { Locked, Available, InProgress, Completed }

    /// <summary>Aggregates and persists learner progress. All writes save immediately.</summary>
    public class ProgressManager : MonoBehaviour
    {
        public static ProgressManager Instance { get; private set; }

        public ProgressData Data { get; private set; }

        public event Action Changed;

        private ContentLibrary Content => AppManager.Instance.Content;

        private void Awake()
        {
            Instance = this;
            Data = SaveSystem.Load<ProgressData>(AppConstants.Files.Progress) ?? new ProgressData();
        }

        public void Save() => SaveSystem.Save(AppConstants.Files.Progress, Data);

        private void Commit()
        {
            Save();
            Changed?.Invoke();
        }

        private static string Now => DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        private static bool AddUnique(System.Collections.Generic.List<string> list, string value)
        {
            if (string.IsNullOrEmpty(value) || list.Contains(value)) return false;
            list.Add(value);
            return true;
        }

        // ------------------------------------------------------------------ writes
        public void MarkWelcomeSeen()
        {
            Data.hasSeenWelcome = true;
            Commit();
        }

        public void MarkModuleStarted(string moduleId)
        {
            AddUnique(Data.startedModules, moduleId);
            Data.lastAccessedModule = moduleId;
            Data.lastAccessedTime = Now;
            Commit();
        }

        public void MarkStepCompleted(ModuleData module, int stepIndex)
        {
            AddUnique(Data.completedSteps, $"{module.moduleId}:{stepIndex}");
            if (module.lessonSteps.Count > 0 &&
                Enumerable.Range(0, module.lessonSteps.Count).All(i => IsStepCompleted(module, i)))
                AddUnique(Data.completedModules, module.moduleId);
            Data.lastAccessedModule = module.moduleId;
            Data.lastAccessedTime = Now;
            Commit();
        }

        public void MarkHardwareViewed(string hardwareId)
        {
            if (AddUnique(Data.viewedHardware, hardwareId)) Commit();
        }

        public void MarkHotspotExplored(string hardwareId, string hotspotId)
        {
            if (AddUnique(Data.exploredHotspots, $"{hardwareId}/{hotspotId}")) Commit();
        }

        public void MarkARActivityCompleted(string hardwareId)
        {
            if (AddUnique(Data.completedARActivities, hardwareId)) Commit();
        }

        public void MarkScenarioCompleted(string scenarioId)
        {
            if (AddUnique(Data.completedScenarios, scenarioId)) Commit();
        }

        public void RecordAnswer(string moduleId, string questionId, bool correct)
        {
            var r = Data.quizResults.FirstOrDefault(x => x.questionId == questionId);
            if (r == null)
            {
                r = new QuizResult { moduleId = moduleId, questionId = questionId };
                Data.quizResults.Add(r);
            }
            r.attempts++;
            r.correct = correct;
            r.timestamp = Now;
            Commit();
        }

        public void AddReflection(string contextId, int confidence, string response)
        {
            Data.reflections.RemoveAll(x => x.contextId == contextId);
            Data.reflections.Add(new ReflectionEntry
            {
                contextId = contextId, confidence = confidence, response = response, timestamp = Now
            });
            Commit();
        }

        public void SetUnlockAll(bool unlock)
        {
            Data.unlockAllModules = unlock;
            Commit();
        }

        public void ResetAll()
        {
            var keepWelcome = Data.hasSeenWelcome;
            Data = new ProgressData { hasSeenWelcome = keepWelcome };
            Commit();
        }

        // ------------------------------------------------------------------ queries
        public bool IsStepCompleted(ModuleData m, int stepIndex) => Data.completedSteps.Contains($"{m.moduleId}:{stepIndex}");

        public int CompletedStepCount(ModuleData m) =>
            Enumerable.Range(0, m.lessonSteps.Count).Count(i => IsStepCompleted(m, i));

        public float ModuleProgress01(ModuleData m) =>
            m.lessonSteps.Count == 0 ? 0f : (float)CompletedStepCount(m) / m.lessonSteps.Count;

        public int FirstIncompleteStep(ModuleData m)
        {
            for (int i = 0; i < m.lessonSteps.Count; i++)
                if (!IsStepCompleted(m, i)) return i;
            return 0;
        }

        public ModuleState StateOf(ModuleData m)
        {
            if (Data.completedModules.Contains(m.moduleId)) return ModuleState.Completed;
            if (Data.startedModules.Contains(m.moduleId) || CompletedStepCount(m) > 0) return ModuleState.InProgress;
            if (Data.unlockAllModules || m.order <= 1) return ModuleState.Available;
            var previous = Content.Modules.Where(x => x.order < m.order).OrderByDescending(x => x.order).FirstOrDefault();
            return previous == null || Data.completedModules.Contains(previous.moduleId) || Data.startedModules.Contains(previous.moduleId)
                ? ModuleState.Available
                : ModuleState.Locked;
        }

        public QuizResult ResultFor(string questionId) => Data.quizResults.FirstOrDefault(r => r.questionId == questionId);

        public (int correct, int answered, int total) QuizScore(ModuleData m)
        {
            int correct = 0, answered = 0;
            foreach (var q in m.questions)
            {
                var r = ResultFor(q.questionId);
                if (r == null) continue;
                answered++;
                if (r.correct) correct++;
            }
            return (correct, answered, m.questions.Count);
        }

        public (int correct, int answered, int total) OverallQuizScore()
        {
            int c = 0, a = 0, t = 0;
            foreach (var m in Content.Modules)
            {
                var s = QuizScore(m);
                c += s.correct; a += s.answered; t += s.total;
            }
            return (c, a, t);
        }

        public bool HasReflection(string contextId) => Data.reflections.Any(r => r.contextId == contextId);
    }
}
