using System.IO;
using System.Linq;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using UnityEditor;
using UnityEngine;

namespace ComputerExplorer.EditorTools
{
    /// <summary>
    /// Writes the seed content (DefaultContent) as editable ScriptableObject assets in Assets/ScriptableObjects
    /// and maintains the Resources/ContentDatabase index the app loads at runtime.
    /// </summary>
    public static class ContentAssetBuilder
    {
        private const string Root = "Assets/ScriptableObjects";
        private const string DatabasePath = "Assets/Resources/ContentDatabase.asset";

        [MenuItem("Computer Explorer/Create Content Assets", priority = 20)]
        public static void CreateContentAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<ContentDatabase>(DatabasePath) is ContentDatabase existing && existing.modules.Count > 0 &&
                !EditorUtility.DisplayDialog("Content assets already exist",
                    "Overwrite the content assets with the default seed content? Your edits to existing assets will be lost.",
                    "Overwrite", "Cancel"))
                return;
            CreateContentAssetsSilently();
            EditorUtility.DisplayDialog("Computer Explorer", "Content assets created in Assets/ScriptableObjects.", "OK");
        }

        public static void CreateContentAssetsSilently()
        {
            var b = DefaultContent.Build();
            foreach (var h in b.Hardware)
            {
                h.arTargetImage = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/ARTargets/{h.arTargetName}.png");
                h.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/Icons/{h.iconName}.png");
                Save(h, "Hardware");
            }
            foreach (var q in b.Questions) Save(q, "Questions");
            foreach (var s in b.Scenarios) Save(s, "Scenarios");
            foreach (var m in b.Modules) Save(m, "Modules");
            AssetDatabase.SaveAssets();
            RebuildDatabase();
        }

        [MenuItem("Computer Explorer/Rebuild Content Database", priority = 21)]
        public static void RebuildDatabase()
        {
            EnsureFolder("Assets/Resources");
            var db = AssetDatabase.LoadAssetAtPath<ContentDatabase>(DatabasePath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ContentDatabase>();
                AssetDatabase.CreateAsset(db, DatabasePath);
            }
            db.hardware = FindAll<HardwareData>();
            db.modules = FindAll<ModuleData>().OrderBy(m => m.order).ToList();
            db.scenarios = FindAll<ScenarioData>();
            db.questions = FindAll<QuestionData>();
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Computer Explorer] Content database: {db.hardware.Count} hardware, {db.modules.Count} modules, " +
                      $"{db.scenarios.Count} scenarios, {db.questions.Count} questions.");
        }

        private static System.Collections.Generic.List<T> FindAll<T>() where T : ScriptableObject =>
            AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { Root })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

        /// <summary>Creates a folder (and parents) through the AssetDatabase so assets can be created in it immediately.</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void Save(ScriptableObject asset, string folder)
        {
            string dir = $"{Root}/{folder}";
            EnsureFolder(dir);
            string path = $"{dir}/{asset.name}.asset";
            var old = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (old != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
