using ComputerExplorer.Data;
using ComputerExplorer.Progress;

namespace ComputerExplorer.Learning
{
    /// <summary>Stores a reflection and completes the lesson step when reflecting on a module.</summary>
    public static class ReflectionManager
    {
        public static readonly string[] ConfidenceLabels = { "Belum paham", "Cukup paham", "Sudah paham" };

        public static void Submit(string contextId, int confidence, string response, bool completesLessonStep)
        {
            ProgressManager.Instance.AddReflection(contextId, confidence, response?.Trim());
            if (completesLessonStep && LearningManager.Instance.IsLessonStep(LessonStepType.Reflection))
                LearningManager.Instance.CompleteCurrentStep();
        }
    }
}
