using System;
using System.Collections.Generic;
using UnityEngine;

namespace ComputerExplorer.Data
{
    public enum LessonStepType { VonNeumann, AR, HardwareExplorer, Scenario, Practice, Reflection }

    public static class LessonStepTypeExtensions
    {
        public static string DisplayName(this LessonStepType t) => t switch
        {
            LessonStepType.VonNeumann => "Arsitektur Von Neumann",
            LessonStepType.AR => "Eksplorasi AR",
            LessonStepType.HardwareExplorer => "Jelajah Hardware",
            LessonStepType.Scenario => "Skenario Kontekstual",
            LessonStepType.Practice => "Latihan",
            LessonStepType.Reflection => "Refleksi",
            _ => t.ToString()
        };

        public static string IconName(this LessonStepType t) => t switch
        {
            LessonStepType.VonNeumann => UI.Icons.Workflow,
            LessonStepType.AR => UI.Icons.Scan,
            LessonStepType.HardwareExplorer => UI.Icons.Monitor,
            LessonStepType.Scenario => UI.Icons.Globe,
            LessonStepType.Practice => UI.Icons.Pencil,
            LessonStepType.Reflection => UI.Icons.Message,
            _ => UI.Icons.Info
        };
    }

    [Serializable]
    public class LessonStep
    {
        public LessonStepType type;
        [Tooltip("Hardware for AR steps.")]
        public HardwareData hardware;
        [Tooltip("Scenario for Scenario steps.")]
        public ScenarioData scenario;
    }

    [CreateAssetMenu(menuName = "Computer Explorer/Module Data", fileName = "Module_")]
    public class ModuleData : ScriptableObject
    {
        public string moduleId;
        public int order = 1;
        public string title;
        [TextArea(2, 4)] public string description;
        [Tooltip("Learning objectives (one per entry).")]
        public List<string> learningObjective = new List<string>();
        public Sprite thumbnail;
        public string iconName = "book-open";
        public int estimatedMinutes = 20;
        public List<HardwareData> hardware = new List<HardwareData>();
        public List<ScenarioData> scenarios = new List<ScenarioData>();
        public List<QuestionData> questions = new List<QuestionData>();
        [Tooltip("The guided learning sequence started from Module Detail.")]
        public List<LessonStep> lessonSteps = new List<LessonStep>();
        [TextArea(2, 4)] public string reflectionPrompt;
    }
}
