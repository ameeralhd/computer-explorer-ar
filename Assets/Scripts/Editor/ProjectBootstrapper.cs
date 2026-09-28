using System;
using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.AR;
using ComputerExplorer.Core;
using ComputerExplorer.UI;
using ComputerExplorer.UI.Screens;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if VUFORIA_ENGINE
using Vuforia;
#endif

namespace ComputerExplorer.EditorTools
{
    /// <summary>
    /// One-click project setup (menu: Computer Explorer > Setup Project):
    /// creates the 15 scenes from the blueprint with their screen components, registers them in Build Settings,
    /// writes the content ScriptableObjects, and applies Android player settings. Safe to run again — e.g. after
    /// installing Vuforia, re-run it so the AR scene gets a Vuforia AR camera.
    /// </summary>
    public static class ProjectBootstrapper
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string BundleId = "com.computerexplorer.arlearning";
        private const string ProductName = "Computer Explorer AR";

        private static readonly Dictionary<string, Type> ScreenTypes = new Dictionary<string, Type>
        {
            { AppConstants.Scenes.Boot, typeof(SplashScreen) },
            { AppConstants.Scenes.Welcome, typeof(WelcomeScreen) },
            { AppConstants.Scenes.MainMenu, typeof(MainMenuController) },
            { AppConstants.Scenes.LearningModules, typeof(LearningModulesScreen) },
            { AppConstants.Scenes.ModuleDetail, typeof(ModuleDetailScreen) },
            { AppConstants.Scenes.ARScanner, typeof(ARScannerScreen) },
            { AppConstants.Scenes.HardwareExplorer, typeof(HardwareExplorerScreen) },
            { AppConstants.Scenes.VonNeumann, typeof(VonNeumannScreen) },
            { AppConstants.Scenes.ContextualLearning, typeof(ContextualLearningScreen) },
            { AppConstants.Scenes.Practice, typeof(PracticeScreen) },
            { AppConstants.Scenes.Reflection, typeof(ReflectionScreen) },
            { AppConstants.Scenes.Progress, typeof(ProgressScreen) },
            { AppConstants.Scenes.Accessibility, typeof(AccessibilityScreen) },
            { AppConstants.Scenes.Help, typeof(HelpScreen) },
            { AppConstants.Scenes.TeacherGuide, typeof(TeacherGuideScreen) },
        };

        [MenuItem("Computer Explorer/Setup Project", priority = 0)]
        public static void SetupProject()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                EditorUtility.DisplayProgressBar("Computer Explorer", "Importing art…", 0.1f);
                ReimportArt();
                EditorUtility.DisplayProgressBar("Computer Explorer", "Creating content assets…", 0.3f);
                if (AssetDatabase.LoadAssetAtPath<Data.ContentDatabase>("Assets/Resources/ContentDatabase.asset") == null)
                    ContentAssetBuilder.CreateContentAssetsSilently();
                EditorUtility.DisplayProgressBar("Computer Explorer", "Creating scenes…", 0.5f);
                CreateScenes();
                EditorUtility.DisplayProgressBar("Computer Explorer", "Applying player settings…", 0.9f);
                ApplyPlayerSettings();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            EditorSceneManager.OpenScene($"{SceneFolder}/{AppConstants.Scenes.Boot}.unity");
            EditorUtility.DisplayDialog("Computer Explorer",
                "Project ready.\n\n• 15 scenes created and added to Build Settings\n• Content assets in Assets/ScriptableObjects\n" +
                "• Android settings applied\n\nPress Play in 00_Boot. " +
                (ARSessionController.VuforiaAvailable
                    ? "Vuforia detected: remember to paste your License Key in Vuforia Configuration."
                    : "Vuforia not installed: the AR scanner runs in Simulation Mode until you add it (see README)."),
                "OK");
        }

        [MenuItem("Computer Explorer/Create Scenes Only", priority = 1)]
        public static void CreateScenes()
        {
            ContentAssetBuilder.EnsureFolder(SceneFolder);
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (var sceneName in AppConstants.Scenes.All)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (sceneName == AppConstants.Scenes.ARScanner) BuildARScene();
                else BuildUIScene(sceneName);
                string path = $"{SceneFolder}/{sceneName}.unity";
                EditorSceneManager.SaveScene(scene, path);
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        private static void BuildUIScene(string sceneName)
        {
            var cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.957f, 0.965f, 0.984f);
            cam.cullingMask = 0; // UI is screen-space overlay; camera only clears the background
            cam.orthographic = true;

            var screen = new GameObject(ScreenTypes[sceneName].Name);
            screen.AddComponent(ScreenTypes[sceneName]);
        }

        private static void BuildARScene()
        {
            var camGo = new GameObject("ARCamera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 20f;
#if VUFORIA_ENGINE
            camGo.AddComponent<VuforiaBehaviour>();
#endif
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var sessionGo = new GameObject("ARSessionController");
            var session = sessionGo.AddComponent<ARSessionController>();
            var so = new SerializedObject(session);
            so.FindProperty("arCamera").objectReferenceValue = cam;
            so.ApplyModifiedPropertiesWithoutUndo();

            var managerGo = new GameObject("ARManager");
            var manager = managerGo.AddComponent<ARManager>();
            var mo = new SerializedObject(manager);
            mo.FindProperty("session").objectReferenceValue = session;
            mo.ApplyModifiedPropertiesWithoutUndo();

            new GameObject("ARCanvas").AddComponent<ARScannerScreen>();
        }

        private static void ReimportArt()
        {
            foreach (var folder in new[] { "Assets/Resources/Icons", "Assets/Resources/Images", "Assets/Resources/ARTargets" })
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
            }
        }

        [MenuItem("Computer Explorer/Apply Android Player Settings", priority = 2)]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Computer Explorer";
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.bundleVersion = "1.0.0";

            // Portrait-first design; the AR scanner is also designed for portrait.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // Vuforia requires 64-bit ARM (IL2CPP) and a modern Android version.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
#if UNITY_2023_1_OR_NEWER
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.Activity;
#endif
            PlayerSettings.iOS.cameraUsageDescription = "Kamera digunakan untuk menampilkan model 3D di atas kartu target (AR).";

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Images/UI/app_icon.png");
            if (icon != null) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

            AssetDatabase.SaveAssets();
        }
    }
}
