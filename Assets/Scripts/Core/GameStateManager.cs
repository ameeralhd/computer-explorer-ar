using System;
using UnityEngine;

namespace ComputerExplorer.Core
{
    /// <summary>Phases of a contextual activity: Scenario → Question → AR/Exploration → Task → Feedback → Reflection.</summary>
    public enum ScenarioPhase { Scenario, Question, Explore, Task, Feedback, Reflection, Done }

    [Serializable]
    public class LessonSession
    {
        public string moduleId;
        public int stepIndex;
        /// <summary>True while the learner browses freely (e.g. returned to the Main Menu mid-lesson).</summary>
        public bool paused;
    }

    [Serializable]
    public class ScenarioSession
    {
        public string scenarioId;
        public ScenarioPhase phase;
        public bool explored;
        public bool taskAnswered;
        public bool taskCorrect;
        public string taskAnswer;
    }

    /// <summary>
    /// Current learning/session state that must survive scene changes (not app restarts —
    /// persistent learner data lives in ProgressManager).
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public event Action Changed;

        /// <summary>Module whose detail screen / practice set is shown.</summary>
        public string CurrentModuleId { get; private set; }

        /// <summary>Hardware the AR scanner should focus on. Null = all targets active.</summary>
        public string SelectedHardwareId { get; private set; }

        /// <summary>Active guided lesson (Module Detail → Start Learning). Null when browsing freely.</summary>
        public LessonSession ActiveLesson { get; set; }

        /// <summary>Contextual scenario in progress; survives the round trip to the AR scanner.</summary>
        public ScenarioSession ActiveScenario { get; set; }

        /// <summary>What the Reflection screen is reflecting on (module id or scenario id).</summary>
        public string ReflectionContextId { get; private set; }
        public string ReflectionPrompt { get; private set; }

        /// <summary>Set when the AR scanner was opened from a scenario so it can hand control back.</summary>
        public string ARReturnScene { get; set; }

        /// <summary>Module just finished — the Progress screen shows a completion celebration once.</summary>
        public string JustCompletedModuleId { get; set; }

        /// <summary>Set by the Hardware Explorer so it can re-open the same detail after returning from AR.</summary>
        public string HardwareExplorerFilter { get; set; }

        private void Awake() => Instance = this;

        public void SetModule(string moduleId)
        {
            CurrentModuleId = moduleId;
            Changed?.Invoke();
        }

        public void SelectHardware(string hardwareId)
        {
            SelectedHardwareId = hardwareId;
            Changed?.Invoke();
        }

        public void SetReflection(string contextId, string prompt)
        {
            ReflectionContextId = contextId;
            ReflectionPrompt = prompt;
            Changed?.Invoke();
        }
    }
}
