using System.Collections;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI
{
    /// <summary>
    /// Short, non-blocking popups (toasts) for confirmations like "Progres tersimpan".
    /// Shown above the bottom safe area, never covering primary actions for long.
    /// </summary>
    public class PopupController : MonoBehaviour
    {
        public static PopupController Instance { get; private set; }

        private GameObject current;

        private void Awake() => Instance = this;

        public void Toast(string message, string icon = Icons.Info, float seconds = 2.8f)
        {
            if (current != null) Destroy(current);
            var p = ContrastController.Current;
            var canvas = CanvasFactory.Create(transform, "Toast", 900);
            current = canvas.gameObject;
            var safe = CanvasFactory.SafeArea(canvas.transform);

            var box = UIKit.Box(safe, p.IsHighContrast ? p.Surface : p.TextPrimary, DesignTokens.RadiusButton,
                p.IsHighContrast ? p.Border : (Color?)null, "Toast");
            var outer = UIKit.Outer(box);
            outer.anchorMin = new Vector2(0, 0);
            outer.anchorMax = new Vector2(1, 0);
            outer.pivot = new Vector2(0.5f, 0);
            outer.offsetMin = new Vector2(DesignTokens.ScreenMargin, DesignTokens.Dp(96));
            outer.offsetMax = new Vector2(-DesignTokens.ScreenMargin, DesignTokens.Dp(96));
            outer.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            if (outer != box && outer.TryGetComponent<VerticalLayoutGroup>(out var og)) og.childForceExpandHeight = true;

            UIKit.AddHorizontal(box, DesignTokens.Dp(12), DesignTokens.Space2, DesignTokens.Dp(14));
            var fg = p.IsHighContrast ? p.TextPrimary : p.Surface;
            UIKit.Icon(box, icon, DesignTokens.IconSizeSmall, fg);
            UIKit.Flex(UIKit.LabelColored(box, message, TextStyle.BodyStrong, fg));

            var group = current.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            StartCoroutine(Life(current, group, seconds));
        }

        private IEnumerator Life(GameObject toast, CanvasGroup group, float seconds)
        {
            yield return MotionController.Fade(group, 0f, 1f, 0.15f);
            yield return new WaitForSecondsRealtime(seconds);
            yield return MotionController.Fade(group, 1f, 0f, 0.25f);
            if (toast != null) Destroy(toast);
        }
    }
}
