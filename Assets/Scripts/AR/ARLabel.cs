using ComputerExplorer.Accessibility;
using ComputerExplorer.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.AR
{
    /// <summary>World-space text label that always faces the camera (hotspot names in AR).</summary>
    public class ARLabel : MonoBehaviour
    {
        private Text text;
        private Image background;
        private Canvas canvas;

        public static ARLabel Create(Transform parent, Vector3 localPosition, string content, float worldWidth = 0.38f)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var label = go.AddComponent<ARLabel>();
            label.canvas = go.AddComponent<Canvas>();
            label.canvas.renderMode = RenderMode.WorldSpace;
            label.canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 4f;
            var rt = (RectTransform)go.transform;
            const float w = 560f, h = 120f;
            rt.sizeDelta = new Vector2(w, h);
            rt.localScale = Vector3.one * (worldWidth / w);

            label.background = new GameObject("Bg", typeof(RectTransform)).AddComponent<Image>();
            label.background.transform.SetParent(go.transform, false);
            UIKit.Stretch(label.background.rectTransform);
            UIKit.SetRounded(label.background, 60f);

            label.text = new GameObject("Text", typeof(RectTransform)).AddComponent<Text>();
            label.text.transform.SetParent(go.transform, false);
            UIKit.Stretch(label.text.rectTransform, 24, 24, 8, 8);
            label.text.font = UIAssets.Bold;
            label.text.alignment = TextAnchor.MiddleCenter;
            label.text.resizeTextForBestFit = true;
            label.text.resizeTextMinSize = 28;
            label.text.resizeTextMaxSize = 58;
            label.text.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.text.verticalOverflow = VerticalWrapMode.Truncate;
            label.SetContent(content, false);
            return label;
        }

        public void SetContent(string content, bool emphasised)
        {
            var p = ContrastController.Current;
            text.text = content;
            background.color = emphasised ? p.Primary : p.ARChrome;
            text.color = emphasised ? p.OnPrimary : p.OnARChrome;
        }

        public void SetVisible(bool visible) => canvas.enabled = visible;

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam != null) transform.rotation = cam.transform.rotation;
        }
    }
}
