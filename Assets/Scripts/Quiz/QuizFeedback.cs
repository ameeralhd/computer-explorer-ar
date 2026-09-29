using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.UI;
using UnityEngine;

namespace ComputerExplorer.Quiz
{
    /// <summary>Correct / incorrect feedback with explanation. Always icon + words + color, never color alone.</summary>
    public static class QuizFeedback
    {
        public static string SpokenText(QuestionData q, bool correct, string extra = null) =>
            (correct ? Loc.T("Benar! ", "Correct! ") : Loc.T("Belum tepat. ", "Not quite. ")) +
            (correct ? "" : Loc.T($"Jawaban yang benar: {q.CorrectAnswerDisplay}. ", $"The correct answer is: {q.CorrectAnswerDisplay}. ")) +
            q.Explanation + (extra != null ? " " + extra : "");

        public static RectTransform Build(Transform parent, QuestionData q, bool correct, string extra = null)
        {
            var card = UIKit.Callout(parent,
                correct ? Icons.CheckCircle : Icons.XCircle,
                correct ? Loc.T("Benar! Kerja bagus.", "Correct! Well done.") : Loc.T("Belum tepat — ayo pelajari lagi.", "Not quite — let's review it."),
                null,
                correct ? ColorRole.Success : ColorRole.Error,
                correct ? ColorRole.SuccessSoft : ColorRole.ErrorSoft);
            if (!correct)
            {
                UIKit.Label(card, Loc.T("Jawaban yang benar:", "Correct answer:"), TextStyle.Label, ColorRole.TextSecondary);
                UIKit.Label(card, q.CorrectAnswerDisplay, TextStyle.BodyStrong);
            }
            UIKit.Label(card, Loc.T("Penjelasan", "Explanation"), TextStyle.Label, ColorRole.TextSecondary);
            UIKit.Label(card, q.Explanation, TextStyle.Body);
            if (!string.IsNullOrEmpty(extra)) UIKit.Label(card, extra, TextStyle.Body);

            if (AccessibilityManager.Instance.Settings.autoNarrate)
                AudioManager.Instance.Narration.Play(SpokenText(q, correct, extra));
            return card;
        }
    }
}
