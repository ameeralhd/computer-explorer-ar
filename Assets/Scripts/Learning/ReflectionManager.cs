using ComputerExplorer.Data;
using ComputerExplorer.Progress;

namespace ComputerExplorer.Learning
{
    /// <summary>Stores a reflection and completes the lesson step when reflecting on a module.</summary>
    public static class ReflectionManager
    {
        public static string[] ConfidenceLabels => new[]
        {
            Core.Loc.T("Belum paham", "I don't understand yet"),
            Core.Loc.T("Cukup paham", "I partly understand"),
            Core.Loc.T("Sudah paham", "I understand")
        };

        public static void Submit(string contextId, int confidence, string response, bool completesLessonStep)
        {
            ProgressManager.Instance.AddReflection(contextId, confidence, response?.Trim());
            if (completesLessonStep && LearningManager.Instance.IsLessonStep(LessonStepType.Reflection))
                LearningManager.Instance.CompleteCurrentStep();
        }
    }
}
