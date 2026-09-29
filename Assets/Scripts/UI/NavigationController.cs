using System.Collections.Generic;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI
{
    /// <summary>
    /// Back/forward navigation with a history stack. The Android back button (Escape) closes the top
    /// modal first, then goes back; on the Main Menu it asks before exiting.
    /// </summary>
    public class NavigationController : MonoBehaviour
    {
        public static NavigationController Instance { get; private set; }

        private readonly Stack<string> history = new Stack<string>();

        private void Awake() => Instance = this;

        private static string Current => SceneLoader.Instance.CurrentScene;

        /// <summary>Open a screen and remember the current one for Back.</summary>
        public void GoTo(string scene)
        {
            var current = Current;
            if (current != scene && current != AppConstants.Scenes.Boot && current != AppConstants.Scenes.Welcome)
                history.Push(current);
            SceneLoader.Instance.Load(scene);
        }

        /// <summary>Open a screen without adding a history entry (e.g. moving between lesson steps).</summary>
        public void Replace(string scene) => SceneLoader.Instance.Load(scene);

        public void Home()
        {
            history.Clear();
            SceneLoader.Instance.Load(AppConstants.Scenes.MainMenu);
        }

        public void Back()
        {
            if (ModalController.Instance != null && ModalController.Instance.CloseTop()) return;
            if (SceneLoader.Instance.IsLoading) return;

            while (history.Count > 0)
            {
                var previous = history.Pop();
                if (previous != Current)
                {
                    SceneLoader.Instance.Load(previous);
                    return;
                }
            }

            if (Current != AppConstants.Scenes.MainMenu)
            {
                SceneLoader.Instance.Load(AppConstants.Scenes.MainMenu);
                return;
            }

            ModalController.Instance.Confirm(Loc.T("Keluar dari aplikasi?", "Exit the app?"),
                Loc.T("Progres belajarmu sudah tersimpan otomatis.", "Your learning progress has been saved automatically."),
                Loc.T("Keluar", "Exit"), Application.Quit, Icons.Info);
        }

        private void Update()
        {
            if (BackPressed()) Back();
        }

        private static bool BackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }
    }
}
