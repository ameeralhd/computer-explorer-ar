using System;
using System.Collections;
using ComputerExplorer.Accessibility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ComputerExplorer.Core
{
    /// <summary>
    /// Controlled scene transitions with a short fade. The fade is skipped when reduced motion is on.
    /// Use NavigationController for user navigation (it keeps the back history).
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public event Action<string> SceneWillChange;
        public event Action<string> SceneLoaded;

        public bool IsLoading { get; private set; }
        public string CurrentScene => SceneManager.GetActiveScene().name;

        private CanvasGroup fade;
        private const float FadeDuration = 0.18f;

        private void Awake()
        {
            Instance = this;
            BuildFadeOverlay();
        }

        public void Load(string sceneName)
        {
            if (IsLoading) return;
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not in Build Settings. " +
                               "Run menu: Computer Explorer > Setup Project.");
                UI.PopupController.Instance?.Toast(Loc.T($"Layar '{sceneName}' belum tersedia.", $"Screen '{sceneName}' is not available yet."));
                return;
            }
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;
            SceneWillChange?.Invoke(sceneName);
            fade.blocksRaycasts = true;
            yield return MotionController.Fade(fade, fade.alpha, 1f, FadeDuration);

            var op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;

            SceneLoaded?.Invoke(sceneName);
            yield return MotionController.Fade(fade, 1f, 0f, FadeDuration);
            fade.blocksRaycasts = false;
            IsLoading = false;
        }

        private void BuildFadeOverlay()
        {
            var go = new GameObject("FadeCanvas", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            fade = go.AddComponent<CanvasGroup>();
            fade.alpha = 0f;
            fade.blocksRaycasts = false;
            go.AddComponent<GraphicRaycaster>();

            var img = new GameObject("Fade", typeof(RectTransform)).AddComponent<Image>();
            img.transform.SetParent(go.transform, false);
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            img.color = ContrastController.Current.Background;
            AccessibilityManager.Instance.Changed += _ => img.color = ContrastController.Current.Background;
        }
    }
}
