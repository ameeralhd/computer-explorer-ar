using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ComputerExplorer.Data
{
    /// <summary>
    /// Read-only access to all educational content. Loads the ContentDatabase asset (Resources/ContentDatabase),
    /// which indexes the ScriptableObjects in Assets/ScriptableObjects. On a fresh checkout — before
    /// "Computer Explorer > Setup Project" has generated those assets — it falls back to DefaultContent so the
    /// app is always runnable.
    /// </summary>
    public class ContentLibrary
    {
        public IReadOnlyList<HardwareData> Hardware { get; private set; }
        public IReadOnlyList<ModuleData> Modules { get; private set; }
        public IReadOnlyList<ScenarioData> Scenarios { get; private set; }
        public IReadOnlyList<QuestionData> Questions { get; private set; }
        public bool UsingSeedContent { get; private set; }

        public static ContentLibrary Load()
        {
            var lib = new ContentLibrary();
            var db = Resources.Load<ContentDatabase>(ContentDatabase.ResourcePath);
            if (db != null && db.hardware.Count > 0 && db.modules.Count > 0)
            {
                lib.Hardware = db.hardware.Where(x => x != null).ToList();
                lib.Modules = db.modules.Where(x => x != null).OrderBy(m => m.order).ToList();
                lib.Scenarios = db.scenarios.Where(x => x != null).ToList();
                lib.Questions = db.questions.Where(x => x != null).ToList();
                return lib;
            }

            var seed = DefaultContent.Build();
            lib.Hardware = seed.Hardware;
            lib.Modules = seed.Modules;
            lib.Scenarios = seed.Scenarios;
            lib.Questions = seed.Questions;
            lib.UsingSeedContent = true;
            return lib;
        }

        public HardwareData GetHardware(string id) => Hardware.FirstOrDefault(h => h.hardwareId == id);
        public ModuleData GetModule(string id) => Modules.FirstOrDefault(m => m.moduleId == id);
        public ScenarioData GetScenario(string id) => Scenarios.FirstOrDefault(s => s.scenarioId == id);
        public QuestionData GetQuestion(string id) => Questions.FirstOrDefault(q => q.questionId == id);

        public HardwareData GetHardwareByTarget(string targetName) =>
            Hardware.FirstOrDefault(h => h.arTargetName == targetName);

        /// <summary>Hardware that has an AR target.</summary>
        public IEnumerable<HardwareData> ARHardware => Hardware.Where(h => !string.IsNullOrEmpty(h.arTargetName));

        public ModuleData ModuleForQuestion(QuestionData q) =>
            Modules.FirstOrDefault(m => m.questions.Contains(q)) ?? GetModule(q.relatedModuleId);

        public ModuleData ModuleForScenario(ScenarioData s) => Modules.FirstOrDefault(m => m.scenarios.Contains(s));
    }
}
