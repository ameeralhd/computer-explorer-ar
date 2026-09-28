using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using ComputerExplorer.Data;
using ComputerExplorer.Learning;
using ComputerExplorer.Progress;
using ComputerExplorer.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ComputerExplorer.Core
{
    /// <summary>
    /// Application lifecycle and shared state. Creates the persistent core services once
    /// (AccessibilityManager, AudioManager, ProgressManager, SceneLoader, ...) so every scene can be
    /// played directly in the Editor without first loading 00_Boot.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        public ContentLibrary Content { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (Instance != null) return;
            new GameObject("[AppManager]").AddComponent<AppManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Content = ContentLibrary.Load();

            // One persistent AudioListener so audio never depends on which camera a scene uses.
            foreach (var l in FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) l.enabled = false;
            gameObject.AddComponent<AudioListener>();

            // Order matters: accessibility and saved data are read before any UI is built.
            CreateService<AccessibilityManager>("AccessibilityManager");
            CreateService<ProgressManager>("ProgressManager");
            CreateService<AudioManager>("AudioManager");
            CreateService<GameStateManager>("GameStateManager");
            CreateService<SceneLoader>("SceneLoader");
            CreateService<NavigationController>("NavigationController");
            CreateService<LearningManager>("LearningManager");
            CreateService<ModalController>("ModalController");
            CreateService<PopupController>("PopupController");
            EnsureEventSystem();
        }

        private T CreateService<T>(string serviceName) where T : Component
        {
            var go = new GameObject(serviceName);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            go.transform.SetParent(transform, false);
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) ProgressManager.Instance?.Save();
        }

        private void OnApplicationQuit()
        {
            ProgressManager.Instance?.Save();
        }
    }
}
