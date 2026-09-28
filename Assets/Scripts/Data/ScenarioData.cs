using UnityEngine;

namespace ComputerExplorer.Data
{
    /// <summary>
    /// A contextual learning activity. Sequence: Scenario → Question → AR/Exploration → Task → Feedback → Reflection.
    /// </summary>
    [CreateAssetMenu(menuName = "Computer Explorer/Scenario Data", fileName = "Scenario_")]
    public class ScenarioData : ScriptableObject
    {
        public string scenarioId;
        public string title;
        [Tooltip("Where the situation happens, e.g. 'Lab komputer sekolah'.")]
        public string environment;
        [TextArea(3, 8)] public string description;
        public Sprite image;
        public string iconName = "globe";
        public AudioClip audio;
        [Tooltip("Guiding question shown before exploration.")]
        [TextArea(2, 4)] public string question;
        public HardwareData hardware;
        [Tooltip("Vuforia target used for the exploration step (defaults to hardware.arTargetName).")]
        public string ARTarget;
        [Tooltip("The learning task; answered inside the scenario.")]
        public QuestionData activity;
        [TextArea(2, 6)] public string feedback;
        [TextArea(2, 4)] public string reflection;
    }
}
