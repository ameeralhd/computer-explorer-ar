using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.Data
{
    /// <summary>
    /// A contextual learning activity. Sequence: Scenario → Question → AR/Exploration → Task → Feedback → Reflection.
    /// Bahasa Indonesia fields with English in the "…En" fields.
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

        [Header("English")]
        public string titleEn;
        public string environmentEn;
        [TextArea(3, 8)] public string descriptionEn;
        [TextArea(2, 4)] public string questionEn;
        [TextArea(2, 6)] public string feedbackEn;
        [TextArea(2, 4)] public string reflectionEn;
        public AudioClip audioEn;

        public string Title => Loc.T(title, titleEn);
        public string Environment => Loc.T(environment, environmentEn);
        public string Description => Loc.T(description, descriptionEn);
        public string Question => Loc.T(question, questionEn);
        public string Feedback => Loc.T(feedback, feedbackEn);
        public string Reflection => Loc.T(reflection, reflectionEn);
        public AudioClip Clip => Loc.Clip(audio, audioEn);
    }
}
