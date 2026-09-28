using ComputerExplorer.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI
{
    /// <summary>Creates canvases with the one shared scaling strategy (blueprint §14: consistent reference resolution).</summary>
    public static class CanvasFactory
    {
        public static Canvas Create(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvas.pixelPerfect = false;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(DesignTokens.ReferenceWidth, DesignTokens.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // Portrait: balance width/height. Landscape (tablets, desktop preview): follow height so
            // controls don't balloon on wide screens.
            scaler.matchWidthOrHeight = Screen.width > Screen.height ? 1f : DesignTokens.MatchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>Full-screen child that follows the device safe area.</summary>
        public static RectTransform SafeArea(Transform canvas)
        {
            var rt = UIKit.Rect(canvas, "SafeArea");
            UIKit.Stretch(rt);
            rt.gameObject.AddComponent<SafeAreaFitter>();
            return rt;
        }

        public static Image FullScreenImage(Transform parent, Color color, string name = "Background")
        {
            var rt = UIKit.Rect(parent, name);
            UIKit.Stretch(rt);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        /// <summary>Vertical scroll view. Returns the content stack (VerticalLayoutGroup + ContentSizeFitter).</summary>
        public static RectTransform ScrollView(Transform parent, out ScrollRect scroll, float spacing, bool limitWidth = true)
        {
            var root = UIKit.Rect(parent, "Scroll");
            scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = DesignTokens.Dp(16);
            scroll.decelerationRate = 0.12f;

            var viewport = UIKit.Rect(root, "Viewport");
            UIKit.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.color = new Color(0, 0, 0, 0); // receives drag events on empty areas

            var content = UIKit.Rect(viewport, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = content.offsetMax = Vector2.zero;
            UIKit.AddVertical(content, spacing, DesignTokens.ScreenMargin, DesignTokens.Space3);
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            if (limitWidth) content.gameObject.AddComponent<ContentWidthLimiter>();

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }
    }
}
