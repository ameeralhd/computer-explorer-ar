using System;
using System.Collections.Generic;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI
{
    /// <summary>
    /// Reusable modal dialogs, presented as bottom sheets (easy thumb reach, content scrolls when long).
    /// States: open, close, confirmation/error. Android back closes the top sheet.
    /// </summary>
    public class ModalController : MonoBehaviour
    {
        public static ModalController Instance { get; private set; }

        private readonly List<GameObject> open = new List<GameObject>();
        public bool IsOpen => open.Count > 0;

        private void Awake() => Instance = this;

        private void Start()
        {
            SceneLoader.Instance.SceneWillChange += _ => CloseAll();
        }

        public class Options
        {
            public string Title;
            public string Body;
            public string Icon;
            public ColorRole IconTone = ColorRole.Primary;
            public Action<RectTransform> BuildContent;
            public string PrimaryLabel;
            public string PrimaryIcon;
            public Action OnPrimary;
            public ButtonVariant PrimaryVariant = ButtonVariant.Primary;
            public string SecondaryLabel = "Tutup";
            public Action OnSecondary;
            public bool Dismissible = true;
            public Action OnClosed;
        }

        public GameObject Show(Options o)
        {
            var p = ContrastController.Current;
            var canvas = CanvasFactory.Create(transform, "Modal: " + o.Title, 500 + open.Count * 10);
            var go = canvas.gameObject;
            open.Add(go);

            var scrim = CanvasFactory.FullScreenImage(canvas.transform, p.Scrim, "Scrim");
            if (o.Dismissible)
            {
                var b = scrim.gameObject.AddComponent<Button>();
                b.transition = Selectable.Transition.None;
                b.onClick.AddListener(() => Close(go));
            }

            var safe = CanvasFactory.SafeArea(canvas.transform);
            var sheetBox = UIKit.Box(safe, p.Surface, DesignTokens.RadiusCard * 1.5f, p.IsHighContrast ? p.Border : (Color?)null,
                "Sheet", raycast: true);
            var sheet = UIKit.Outer(sheetBox);
            sheet.anchorMin = new Vector2(0, 0);
            sheet.anchorMax = new Vector2(1, 0);
            sheet.pivot = new Vector2(0.5f, 0);
            float side = DesignTokens.Space1;
            sheet.offsetMin = new Vector2(side, side);
            sheet.offsetMax = new Vector2(-side, side);
            var fitter = sheet.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            // When outlined, the outer rect already sizes to the inner fill; the fill hosts the column.
            var col = sheetBox;
            if (!col.TryGetComponent<VerticalLayoutGroup>(out var colGroup)) colGroup = UIKit.AddVertical(col, 0);
            colGroup.spacing = DesignTokens.Space2;
            colGroup.padding = new RectOffset(0, 0, Mathf.RoundToInt(DesignTokens.Dp(12)), Mathf.RoundToInt(DesignTokens.Space3));

            // Grab handle (visual affordance only)
            var handleRow = UIKit.HStack(col, 0, align: TextAnchor.MiddleCenter);
            var handle = UIKit.Box(handleRow, p.BorderStrong, DesignTokens.RadiusPill, null, "Handle");
            UIKit.Layout(handle, minWidth: DesignTokens.Dp(40), preferredWidth: DesignTokens.Dp(40), minHeight: DesignTokens.Dp(4),
                preferredHeight: DesignTokens.Dp(4));

            // Header
            var header = UIKit.HStack(col, DesignTokens.Dp(12), DesignTokens.Space3, 0);
            if (!string.IsNullOrEmpty(o.Icon)) UIKit.Icon(header, o.Icon, DesignTokens.IconSize, o.IconTone);
            UIKit.Flex(UIKit.Label(header, o.Title, TextStyle.Title));
            if (o.Dismissible) UIKit.IconButton(header, Icons.Close, "Tutup", () => Close(go));

            // Body (scrolls when long)
            if (!string.IsNullOrEmpty(o.Body) || o.BuildContent != null)
            {
                var content = CanvasFactory.ScrollView(col, out var scroll, DesignTokens.Space2, limitWidth: false);
                var vg = content.GetComponent<VerticalLayoutGroup>();
                vg.padding = new RectOffset(Mathf.RoundToInt(DesignTokens.Space3), Mathf.RoundToInt(DesignTokens.Space3), 0, 0);
                var le = scroll.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = DesignTokens.Dp(80);
                var clamp = scroll.gameObject.AddComponent<ClampHeightToContent>();
                clamp.content = content;
                clamp.reference = (RectTransform)canvas.transform;
                clamp.maxFraction = 0.62f;
                if (!string.IsNullOrEmpty(o.Body)) UIKit.Label(content, o.Body, TextStyle.Body);
                o.BuildContent?.Invoke(content);
            }

            // Actions
            if (!string.IsNullOrEmpty(o.PrimaryLabel) || !string.IsNullOrEmpty(o.SecondaryLabel))
            {
                var actions = UIKit.VStack(col, DesignTokens.Space1, DesignTokens.Space3, 0, "Actions");
                if (!string.IsNullOrEmpty(o.PrimaryLabel))
                    UIKit.Button(actions, o.PrimaryLabel, () =>
                    {
                        Close(go);
                        o.OnPrimary?.Invoke();
                    }, o.PrimaryVariant, o.PrimaryIcon);
                if (!string.IsNullOrEmpty(o.SecondaryLabel))
                    UIKit.Button(actions, o.SecondaryLabel, () =>
                    {
                        Close(go);
                        o.OnSecondary?.Invoke();
                    }, ButtonVariant.Secondary);
            }

            go.AddComponent<ModalCloseHook>().onClosed = o.OnClosed;
            var group = go.AddComponent<CanvasGroup>();
            StartCoroutine(MotionController.Fade(group, 0f, 1f, 0.15f));
            return go;
        }

        public GameObject Info(string title, string body, string icon = Icons.Info) =>
            Show(new Options { Title = title, Body = body, Icon = icon });

        public GameObject Confirm(string title, string body, string confirmLabel, Action onConfirm, string icon = Icons.Alert,
            bool destructive = false) =>
            Show(new Options
            {
                Title = title, Body = body, Icon = icon, IconTone = destructive ? ColorRole.Error : ColorRole.Primary,
                PrimaryLabel = confirmLabel, OnPrimary = onConfirm,
                PrimaryVariant = destructive ? ButtonVariant.Danger : ButtonVariant.Primary, SecondaryLabel = "Batal"
            });

        public GameObject Error(string title, string body) =>
            Show(new Options { Title = title, Body = body, Icon = Icons.Alert, IconTone = ColorRole.Error, SecondaryLabel = "Mengerti" });

        public bool CloseTop()
        {
            if (open.Count == 0) return false;
            Close(open[open.Count - 1]);
            return true;
        }

        public void Close(GameObject modal)
        {
            if (modal == null || !open.Remove(modal)) return;
            modal.SetActive(false);
            Destroy(modal);
        }

        public void CloseAll()
        {
            foreach (var m in open.ToArray()) Close(m);
        }
    }

    /// <summary>Invokes a callback when a modal is destroyed (however it was closed).</summary>
    public class ModalCloseHook : MonoBehaviour
    {
        public Action onClosed;
        private void OnDestroy() => onClosed?.Invoke();
    }
}
