using ComputerExplorer.Data;
using UnityEngine;

namespace ComputerExplorer.Learning
{
    /// <summary>Learning session orchestration: the single entry point screens use for modules and lessons.</summary>
    public class LearningManager : MonoBehaviour
    {
        public static LearningManager Instance { get; private set; }

        public ModuleManager Modules { get; } = new ModuleManager();
        public LessonController Lesson { get; } = new LessonController();

        private void Awake() => Instance = this;

        public void StartModule(ModuleData module, int? fromStep = null) => Lesson.Start(module, fromStep);

        public void ContinueLesson() => Lesson.OpenCurrentStep();

        public bool IsLessonStep(LessonStepType type, string hardwareId = null, string scenarioId = null) =>
            Lesson.IsCurrent(type, hardwareId, scenarioId);

        public void CompleteCurrentStep() => Lesson.CompleteCurrentStep();
    }
}
