using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using ComputerExplorer.Data;
using ComputerExplorer.UI;
using UnityEngine;

namespace ComputerExplorer.Quiz
{
    /// <summary>Correct / incorrect feedback with explanation. Always icon + words + color, never color alone.</summary>
    public static class QuizFeedback
    {
        public static string SpokenText(QuestionData q, bool correct, string extra = null) =>
            (correct ? "Benar! " : "Belum tepat. ") +
            (correct ? "" : $"Jawaban yang benar: {q.CorrectAnswerDisplay}. ") + q.explanation + (extra != null ? " " + extra : "");

        public static RectTransform Build(Transform parent, QuestionData q, bool correct, string extra = null)
        {
            var card = UIKit.Callout(parent,
                correct ? Icons.CheckCircle : Icons.XCircle,
                correct ? "Benar! Kerja bagus." : "Belum tepat — ayo pelajari lagi.",
                null,
                correct ? ColorRole.Success : ColorRole.Error,
                correct ? ColorRole.SuccessSoft : ColorRole.ErrorSoft);
            var col = card;
            if (!correct)
            {
                UIKit.Label(col, "Jawaban yang benar:", TextStyle.Label, ColorRole.TextSecondary);
                UIKit.Label(col, q.CorrectAnswerDisplay, TextStyle.BodyStrong);
            }
            UIKit.Label(col, "Penjelasan", TextStyle.Label, ColorRole.TextSecondary);
            UIKit.Label(col, q.explanation, TextStyle.Body);
            if (!string.IsNullOrEmpty(extra)) UIKit.Label(col, extra, TextStyle.Body);

            if (AccessibilityManager.Instance.Settings.autoNarrate)
                AudioManager.Instance.Narration.Play(SpokenText(q, correct, extra));
            return card;
        }
    }
}
