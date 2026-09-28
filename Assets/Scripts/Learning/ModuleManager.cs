using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;

namespace ComputerExplorer.Learning
{
    /// <summary>Module selection, state (locked / available / in progress / completed) and recommendations.</summary>
    public class ModuleManager
    {
        private static ProgressManager Progress => ProgressManager.Instance;
        private static ContentLibrary Content => AppManager.Instance.Content;

        public IReadOnlyList<ModuleData> All => Content.Modules;

        public ModuleData Current => Content.GetModule(GameStateManager.Instance.CurrentModuleId) ?? All.FirstOrDefault();

        public ModuleState StateOf(ModuleData m) => Progress.StateOf(m);

        public ModuleData LastAccessed => Content.GetModule(Progress.Data.lastAccessedModule);

        /// <summary>The next module a learner should work on: in progress first, then the first available one.</summary>
        public ModuleData Recommended =>
            All.FirstOrDefault(m => StateOf(m) == ModuleState.InProgress) ??
            All.FirstOrDefault(m => StateOf(m) == ModuleState.Available);

        public void Select(ModuleData m) => GameStateManager.Instance.SetModule(m.moduleId);

        public int CompletedCount => All.Count(m => StateOf(m) == ModuleState.Completed);
    }
}
