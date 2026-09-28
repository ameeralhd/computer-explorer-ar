using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;
using ComputerExplorer.UI;

namespace ComputerExplorer.Learning
{
    /// <summary>
    /// Lesson step sequencing: opens the screen for the current step and advances when a step is done.
    /// Steps are data (ModuleData.lessonSteps), so new lesson flows need no code changes.
    /// </summary>
    public class LessonController
    {
        private static GameStateManager State => GameStateManager.Instance;
        private static ProgressManager Progress => ProgressManager.Instance;
        private static ContentLibrary Content => AppManager.Instance.Content;

        public ModuleData ActiveModule =>
            State.ActiveLesson != null ? Content.GetModule(State.ActiveLesson.moduleId) : null;

        public bool IsActive => ActiveModule != null && !State.ActiveLesson.paused;

        public int StepIndex => State.ActiveLesson?.stepIndex ?? 0;

        public LessonStep CurrentStep
        {
            get
            {
                var m = ActiveModule;
                if (m == null || StepIndex < 0 || StepIndex >= m.lessonSteps.Count) return null;
                return m.lessonSteps[StepIndex];
            }
        }

        public string StepLabel
        {
            get
            {
                var m = ActiveModule;
                return m == null ? "" : $"Langkah {StepIndex + 1} dari {m.lessonSteps.Count}";
            }
        }

        public void Start(ModuleData module, int? fromStep = null)
        {
            Progress.MarkModuleStarted(module.moduleId);
            State.SetModule(module.moduleId);
            State.ActiveLesson = new LessonSession
            {
                moduleId = module.moduleId,
                stepIndex = fromStep ?? Progress.FirstIncompleteStep(module)
            };
            OpenCurrentStep();
        }

        /// <summary>True when the screen showing <paramref name="type"/> is the active lesson step.</summary>
        public bool IsCurrent(LessonStepType type, string hardwareId = null, string scenarioId = null)
        {
            if (!IsActive) return false;
            var step = CurrentStep;
            if (step == null || step.type != type) return false;
            if (type == LessonStepType.AR && hardwareId != null && step.hardware != null && step.hardware.hardwareId != hardwareId)
                return false;
            if (type == LessonStepType.Scenario && scenarioId != null && step.scenario != null && step.scenario.scenarioId != scenarioId)
                return false;
            return true;
        }

        public void CompleteCurrentStep()
        {
            var m = ActiveModule;
            if (m == null) return;
            Progress.MarkStepCompleted(m, StepIndex);
            State.ActiveLesson.stepIndex++;
            if (State.ActiveLesson.stepIndex >= m.lessonSteps.Count)
            {
                State.ActiveLesson = null;
                State.JustCompletedModuleId = m.moduleId;
                Audio.AudioManager.Instance.PlayFeedback(Audio.FeedbackSound.Complete);
                NavigationController.Instance.GoTo(AppConstants.Scenes.Progress);
                return;
            }
            OpenCurrentStep();
        }

        public void OpenCurrentStep()
        {
            var m = ActiveModule;
            var step = CurrentStep;
            if (m == null || step == null) return;
            State.ActiveLesson.paused = false;
            string scene;
            switch (step.type)
            {
                case LessonStepType.VonNeumann:
                    scene = AppConstants.Scenes.VonNeumann;
                    break;
                case LessonStepType.AR:
                    State.SelectHardware(step.hardware != null ? step.hardware.hardwareId : null);
                    State.ARReturnScene = null;
                    scene = AppConstants.Scenes.ARScanner;
                    break;
                case LessonStepType.HardwareExplorer:
                    State.HardwareExplorerFilter = null;
                    scene = AppConstants.Scenes.HardwareExplorer;
                    break;
                case LessonStepType.Scenario:
                    State.ActiveScenario = new ScenarioSession { scenarioId = step.scenario != null ? step.scenario.scenarioId : null };
                    scene = AppConstants.Scenes.ContextualLearning;
                    break;
                case LessonStepType.Practice:
                    State.SetModule(m.moduleId);
                    scene = AppConstants.Scenes.Practice;
                    break;
                default:
                    State.SetReflection(m.moduleId, m.reflectionPrompt);
                    scene = AppConstants.Scenes.Reflection;
                    break;
            }
            NavigationController.Instance.GoTo(scene);
        }

        public void Pause()
        {
            if (State.ActiveLesson != null) State.ActiveLesson.paused = true;
        }

        public void Exit() => State.ActiveLesson = null;
    }
}
