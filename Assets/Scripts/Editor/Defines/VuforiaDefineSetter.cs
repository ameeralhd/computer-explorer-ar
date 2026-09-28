using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;

namespace ComputerExplorer.EditorTools
{
    /// <summary>
    /// Adds the VUFORIA_ENGINE scripting define when the Vuforia Engine package is installed and removes it when
    /// the package is removed. Lives in its own assembly with no dependency on game code, so it still runs even
    /// while the game assembly has compile errors.
    /// </summary>
    [InitializeOnLoad]
    public static class VuforiaDefineSetter
    {
        public const string Define = "VUFORIA_ENGINE";
        private const string PackageName = "com.ptc.vuforia.engine";

        private static readonly NamedBuildTarget[] Targets =
        {
            NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone
        };

        static VuforiaDefineSetter()
        {
            Events.registeredPackages += _ => Sync();
            EditorApplication.delayCall += Sync;
        }

        [MenuItem("Computer Explorer/Refresh Vuforia Define", priority = 40)]
        public static void Sync()
        {
            bool installed = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(p => p.name == PackageName);
            foreach (var target in Targets)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(target)
                    .Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                bool has = defines.Contains(Define);
                if (installed == has) continue;
                if (installed) defines.Add(Define);
                else defines.Remove(Define);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
            }
        }
    }
}
