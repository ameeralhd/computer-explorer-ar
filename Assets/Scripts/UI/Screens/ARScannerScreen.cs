using System.Linq;
using ComputerExplorer.Accessibility;
using ComputerExplorer.AR;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 05_ARScanner — full-screen camera view with overlay (blueprint §4.6, §4.7, §4.18): back, lesson title,
    /// target status, scan guide, information panel (collapsed / expanded), narration and Reset / Zoom / Help.
    /// Controls sit at the top and bottom edges so the recognised object in the middle stays visible.
    /// </summary>
    public class ARScannerScreen : ScreenBase
    {
        protected override bool ShowHeader => false;
        protected override bool Scrollable => false;
        protected override bool OpaqueBackground => false;
        protected override LessonStepType? LessonStep => LessonStepType.AR;

        protected override string ScreenNarration
        {
            get
            {
                if (ar == null) return null;
                return ar.Status switch
                {
                    ARTrackingState.Detected when ar.SelectedHotspot != null => $"{ar.SelectedHotspot.Label}. {ar.SelectedHotspot.Description}",
                    ARTrackingState.Detected => $"{ar.Active.hardwareName}. {ar.Active.Function} " + Loc.T("Ketuk titik oranye pada model untuk melihat bagian-bagiannya.", "Tap the orange dots on the model to see its parts."),
                    ARTrackingState.Error => ar.ErrorMessage,
                    _ => Loc.T($"Arahkan kamera ke kartu target {TargetName}. Pegang perangkat dengan stabil sekitar 20 sampai 40 sentimeter di atas kartu.", $"Point the camera at the {TargetName} target card. Hold the device steady about 20 to 40 centimetres above the card.")
                };
            }
        }

        private ARManager ar;
        private bool expanded;
        private Text flowCaption;
        private int shownFlowStep = -1;

        private string TargetName => ar != null && ar.Focus != null ? ar.Focus.hardwareName : Loc.T("komputer", "computer");

        protected override void OnOpened()
        {
            ar = ARManager.Instance != null ? ARManager.Instance : FindAnyObjectByType<ARManager>();
            if (ar == null) ar = new GameObject("ARManager").AddComponent<ARManager>();
            ar.Changed += OnARChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ar != null) ar.Changed -= OnARChanged;
        }

        private void OnARChanged() => Render();

        private void Update()
        {
            var flow = ar != null ? ar.ActiveObject?.DataFlow : null;
            if (flowCaption != null && flow != null && flow.Step != shownFlowStep)
            {
                shownFlowStep = flow.Step;
                flowCaption.text = flow.Caption;
            }
        }

        protected override void BuildContent(RectTransform content)
        {
            flowCaption = null;
            shownFlowStep = -1;

            // Gesture layer (behind all controls)
            var surface = UIKit.Rect(content, "InteractionSurface");
            UIKit.Stretch(surface);
            surface.gameObject.AddComponent<Image>().color = Color.clear;
            surface.gameObject.AddComponent<ARInteractionController>();

            BuildTop(content);
            if (ar.Status != ARTrackingState.Detected && ar.Status != ARTrackingState.Error) BuildScanGuide(content);
            BuildBottom(content);
        }

        // ------------------------------------------------------------------ top
        private void BuildTop(RectTransform content)
        {
            var top = UIKit.Rect(content, "Top");
            top.anchorMin = new Vector2(0, 1);
            top.anchorMax = new Vector2(1, 1);
            top.pivot = new Vector2(0.5f, 1);
            top.offsetMin = top.offsetMax = Vector2.zero;
            var v = UIKit.AddVertical(top, DesignTokens.Dp(12), 0, 0, TextAnchor.UpperCenter);
            top.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bar = UIKit.Box(top, P.ARChrome, 0, null, "ARHeader");
            UIKit.Layout(bar, minHeight: DesignTokens.HeaderHeight);
            UIKit.AddHorizontal(bar, DesignTokens.Dp(4), DesignTokens.Space1, DesignTokens.Dp(4));
            var fg = P.OnARChrome;
            UIKit.IconButton(bar, Icons.Back, Loc.T("Kembali", "Back"), Nav.Back, ButtonVariant.Ghost, fg);
            var titles = UIKit.Flex(UIKit.VStack(bar, 0));
            UIKit.LabelColored(titles, "AR Explorer", TextStyle.Heading, fg);
            string sub = ar.Focus != null ? $"Target: {ar.Focus.hardwareName}" : Loc.T("Semua target aktif", "All targets active");
            if (Learning.IsLessonStep(LessonStepType.AR)) sub = Learning.Lesson.StepLabel + " · " + sub;
            UIKit.LabelColored(titles, sub, TextStyle.Caption, fg);
            bool speaking = Narration.IsSpeaking && Narration.CurrentText == ScreenNarration;
            UIKit.IconButton(bar, speaking ? Icons.Pause : Icons.Volume, Loc.T("Bacakan petunjuk", "Read instructions aloud"), () =>
            {
                Narration.Toggle(ScreenNarration);
                Render();
            }, ButtonVariant.Ghost, fg);
            UIKit.LanguageToggleButton(bar);
            UIKit.IconButton(bar, Icons.Settings, Loc.T("Pengaturan", "Settings"), () => Go(AppConstants.Scenes.Accessibility),
                ButtonVariant.Ghost, fg);

            var chips = UIKit.HStack(top, DesignTokens.Space1, align: TextAnchor.MiddleCenter);
            var (text, icon, bg, chipFg) = StatusStyle();
            var status = UIKit.Badge(chips, text, bg, chipFg, icon);
            if (ar.Status == ARTrackingState.Searching || ar.Status == ARTrackingState.Initializing)
                status.gameObject.AddComponent<PulseAnimator>();
            if (ar.IsSimulated) UIKit.Badge(chips, Loc.T("Mode Simulasi", "Simulation Mode"), P.ARChrome, P.OnARChrome, Icons.Phone);
        }

        private (string text, string icon, Color bg, Color fg) StatusStyle()
        {
            bool hc = P.IsHighContrast;
            Color onColor = hc ? Color.black : Color.white;
            return ar.Status switch
            {
                ARTrackingState.Initializing => (Loc.T("Menyiapkan kamera AR…", "Starting AR camera…"), Icons.Refresh, P.ARChrome, P.OnARChrome),
                ARTrackingState.Searching => (Loc.T("Mencari target…", "Searching for target…"), Icons.Scan, P.ARChrome, P.OnARChrome),
                ARTrackingState.Detected => (Loc.T("Target terdeteksi", "Target detected") + $": {ar.Active.hardwareName}", Icons.CheckCircle, P.Success, onColor),
                ARTrackingState.Lost => (Loc.T("Target hilang", "Target lost"), Icons.Alert, P.Warning, onColor),
                _ => (Loc.T("AR bermasalah", "AR problem"), Icons.XCircle, P.Error, onColor)
            };
        }

        // ------------------------------------------------------------------ centre guide
        private void BuildScanGuide(RectTransform content)
        {
            var guide = UIKit.Rect(content, "ScanGuide");
            guide.anchorMin = guide.anchorMax = new Vector2(0.5f, 0.56f);
            float size = DesignTokens.Dp(220);
            guide.sizeDelta = new Vector2(size, size);
            float len = DesignTokens.Dp(40), th = DesignTokens.Dp(5);
            var col = P.IsHighContrast ? P.Primary : Color.white;
            foreach (var (ax, ay) in new[] { (0f, 0f), (1f, 0f), (0f, 1f), (1f, 1f) })
            {
                Bracket(guide, new Vector2(ax, ay), new Vector2(len, th), col);
                Bracket(guide, new Vector2(ax, ay), new Vector2(th, len), col);
            }
            guide.gameObject.AddComponent<PulseAnimator>().minAlpha = 0.55f;

            var pill = UIKit.Box(guide, P.ARChrome, DesignTokens.RadiusPill, null, "Instruction");
            pill.anchorMin = new Vector2(0.5f, 0);
            pill.anchorMax = new Vector2(0.5f, 0);
            pill.pivot = new Vector2(0.5f, 1);
            pill.anchoredPosition = new Vector2(0, -DesignTokens.Space2);
            var fit = pill.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIKit.AddHorizontal(pill, DesignTokens.Space1, DesignTokens.Space2, DesignTokens.Dp(8), TextAnchor.MiddleCenter);
            UIKit.Icon(pill, Icons.Target, DesignTokens.IconSizeSmall, P.OnARChrome);
            UIKit.LabelColored(pill, ar.Status == ARTrackingState.Lost ? Loc.T("Arahkan kembali ke kartu target", "Point back at the target card") : Loc.T($"Letakkan target {TargetName} di sini", $"Place the {TargetName} target here"),
                TextStyle.Label, P.OnARChrome);
        }

        private static void Bracket(RectTransform parent, Vector2 corner, Vector2 size, Color color)
        {
            var rt = UIKit.Rect(parent, "Bracket");
            rt.anchorMin = rt.anchorMax = corner;
            rt.pivot = corner;
            rt.sizeDelta = size;
            var img = UIKit.SetRounded(rt.gameObject.AddComponent<Image>(), DesignTokens.Dp(3));
            img.color = color;
            img.raycastTarget = false;
        }

        // ------------------------------------------------------------------ bottom
        private void BuildBottom(RectTransform content)
        {
            var bottom = UIKit.Rect(content, "Bottom");
            bottom.anchorMin = new Vector2(0, 0);
            bottom.anchorMax = new Vector2(1, 0);
            bottom.pivot = new Vector2(0.5f, 0);
            bottom.offsetMin = bottom.offsetMax = Vector2.zero;
            UIKit.AddVertical(bottom, DesignTokens.Space1, DesignTokens.Space2, DesignTokens.Space2);
            bottom.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bottom.gameObject.AddComponent<ContentWidthLimiter>().margin = DesignTokens.Space2;

            // Information panel (scrolls when long so the object stays visible)
            var card = UIKit.Card(bottom, padding: 0, spacing: 0, name: "InfoPanel");
            var panel = CanvasFactory.ScrollView(card, out var scroll, DesignTokens.Dp(12), limitWidth: false);
            var vg = panel.GetComponent<VerticalLayoutGroup>();
            int pad = Mathf.RoundToInt(DesignTokens.CardPadding);
            vg.padding = new RectOffset(pad, pad, pad, pad);
            var le = scroll.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = DesignTokens.Dp(120);
            var clamp = scroll.gameObject.AddComponent<ClampHeightToContent>();
            clamp.content = panel;
            clamp.reference = (RectTransform)Canvas.transform;
            clamp.maxFraction = expanded ? 0.55f : 0.4f;
            BuildPanel(panel);

            // Hand-back actions sit outside the scrolling panel so they are always reachable.
            var actions = UIKit.VStack(bottom, DesignTokens.Space1, name: "Actions");
            BuildActivityActions(actions);
            if (actions.childCount == 0) Destroy(actions.gameObject);

            // Controls: Reset / Zoom / Help
            var controls = UIKit.EqualRow(bottom, DesignTokens.Space1);
            bool detected = ar.Status == ARTrackingState.Detected;
            var zoom = ar.ActiveObject != null ? ar.ActiveObject.Zoom : 1f;
            UIKit.Button(controls, "Reset", ar.ResetView, ButtonVariant.Secondary, Icons.Reset,
                detected ? ButtonState.Normal : ButtonState.Disabled, compact: true);
            UIKit.Button(controls, $"Zoom {zoom:0.#}×", () => ar.CycleZoom(), ButtonVariant.Secondary, Icons.ZoomIn,
                detected ? ButtonState.Normal : ButtonState.Disabled, compact: true);
            UIKit.Button(controls, Loc.T("Bantuan", "Help"), () => Go(AppConstants.Scenes.Help), ButtonVariant.Secondary, Icons.Help, compact: true);
        }

        private void BuildPanel(RectTransform panel)
        {
            switch (ar.Status)
            {
                case ARTrackingState.Error:
                    UIKit.Callout(panel, Icons.Alert, Loc.T("AR tidak dapat dimulai", "AR could not start"), ar.ErrorMessage, ColorRole.Error, ColorRole.ErrorSoft);
                    UIKit.Label(panel, Loc.T("Kamu tetap bisa belajar dengan Mode Simulasi: model 3D ditampilkan tanpa kamera.", "You can still learn with Simulation Mode: the 3D models are shown without the camera."), TextStyle.Body);
                    UIKit.Button(panel, Loc.T("Gunakan Mode Simulasi", "Use Simulation Mode"), ar.SwitchToSimulation, ButtonVariant.Primary, Icons.Phone);
                    break;

                case ARTrackingState.Initializing:
                    UIKit.Label(panel, Loc.T("Menyiapkan kamera…", "Starting camera…"), TextStyle.Heading);
                    UIKit.Label(panel, Loc.T("Jika diminta, izinkan aplikasi menggunakan kamera.", "If asked, allow the app to use the camera."), TextStyle.Body, ColorRole.TextSecondary);
                    break;

                case ARTrackingState.Detected:
                    if (ar.SelectedHotspot != null) BuildHotspotInfo(panel);
                    else BuildObjectInfo(panel);
                    break;

                default:
                    BuildSearching(panel);
                    break;
            }

            var target = ar.Focus ?? ar.Active;
            if (target == null) return;
            if (ar.IsActivityComplete(target) && ar.Status == ARTrackingState.Detected)
                UIKit.Callout(panel, Icons.Trophy, Loc.T("Aktivitas AR selesai!", "AR activity complete!"), Loc.T("Kamu sudah menjelajahi bagian-bagian penting.", "You have explored the important parts."),
                    ColorRole.Success, ColorRole.SuccessSoft);
            else if (Learning.IsLessonStep(LessonStepType.AR))
                UIKit.Label(panel, Loc.T($"Jelajahi minimal {ar.RequiredCount(target)} bagian untuk melanjutkan pelajaran.", $"Explore at least {ar.RequiredCount(target)} parts to continue the lesson."),
                    TextStyle.Caption, ColorRole.TextSecondary);
        }

        private void BuildSearching(RectTransform panel)
        {
            UIKit.Label(panel, Loc.T("Pindai kartu target", "Scan a target card"), TextStyle.Heading);
            UIKit.Label(panel, ar.Focus != null
                    ? Loc.T($"Arahkan kamera ke kartu {ar.Focus.hardwareName}. Pastikan seluruh kartu terlihat dan cukup terang.", $"Point the camera at the {ar.Focus.hardwareName} card. Make sure the whole card is visible and well lit.")
                    : Loc.T("Arahkan kamera ke salah satu kartu target (CPU, RAM, Storage, Keyboard, dll.).", "Point the camera at any target card (CPU, RAM, Storage, Keyboard, etc.)."),
                TextStyle.Body, ColorRole.TextSecondary);

            if (ar.Status == ARTrackingState.Lost && ar.ShowRecoveryHelp)
            {
                UIKit.Callout(panel, Icons.Lightbulb, Loc.T("Target sulit ditemukan?", "Having trouble finding the target?"),
                    Loc.T("• Nyalakan lampu atau pindah ke tempat lebih terang.\n• Jaga jarak 20–40 cm dari kartu.\n" +
                          "• Pastikan seluruh kartu terlihat dan tidak terlipat.\n• Gerakkan perangkat perlahan, tahan stabil 2 detik.",
                          "• Turn on a light or move somewhere brighter.\n• Keep 20–40 cm from the card.\n" +
                          "• Make sure the whole card is visible and flat.\n• Move the device slowly and hold it steady for 2 seconds."),
                    ColorRole.Warning, ColorRole.WarningSoft);
            }

            if (ar.IsSimulated)
            {
                UIKit.Label(panel, Loc.T("Mode Simulasi — pilih kartu yang ingin \"dipindai\":", "Simulation Mode — choose the card to \"scan\":"), TextStyle.Label, ColorRole.TextSecondary);
                var list = ar.AvailableHardware.ToList();
                int perRow = TextSizeController.IsLarge ? 2 : 3;
                RectTransform row = null;
                for (int i = 0; i < list.Count; i++)
                {
                    if (i % perRow == 0) row = UIKit.EqualRow(panel, DesignTokens.Space1);
                    var h = list[i];
                    UIKit.Layout(UIKit.Chip(row, h.hardwareName, false, () => ar.SimulateDetection(h), h.iconName), flexibleWidth: 1);
                }
                if (list.Count % perRow != 0 && list.Count > perRow)
                    for (int i = list.Count % perRow; i < perRow; i++) UIKit.Layout(UIKit.Rect(row, "Pad"), flexibleWidth: 1);
            }
        }

        private void BuildObjectInfo(RectTransform panel)
        {
            var h = ar.Active;
            var head = UIKit.HStack(panel, DesignTokens.Dp(12));
            UIKit.IconTile(head, h.iconName, P.Category(h.category), P.OnCategory, DesignTokens.Dp(44));
            var t = UIKit.Flex(UIKit.VStack(head, DesignTokens.Dp(2)));
            UIKit.Label(t, $"{h.hardwareName} — {h.FullName}", TextStyle.Heading);
            UIKit.Label(t, h.category.DisplayName(), TextStyle.Caption, ColorRole.TextSecondary);
            UIKit.Label(panel, $"<b>{Loc.T("Fungsi", "Function")}:</b> {h.Function}", TextStyle.Body);

            var flow = ar.ActiveObject?.DataFlow;
            if (flow != null)
            {
                var fc = UIKit.Callout(panel, Icons.Workflow, Loc.T("Alur data (ikuti titik kuning)", "Data flow (follow the yellow dot)"), flow.Caption, ColorRole.Primary, ColorRole.PrimarySoft);
                flowCaption = fc.GetComponentsInChildren<Text>().LastOrDefault();
                shownFlowStep = flow.Step;
            }

            int explored = ar.ExploredCount(h);
            UIKit.ProgressRow(panel, Loc.T("Bagian dijelajahi", "Parts explored"), $"{explored}/{h.hotspots.Count}",
                h.hotspots.Count == 0 ? 1f : (float)explored / h.hotspots.Count, P.Success);
            if (explored < h.hotspots.Count)
                UIKit.Label(panel, Loc.T("Ketuk titik oranye pada model, atau pilih bagian di bawah.", "Tap the orange dots on the model, or choose a part below."), TextStyle.Caption, ColorRole.TextSecondary);

            var actions = UIKit.EqualRow(panel, DesignTokens.Space1);
            NarrationControl(actions, h.NarrationText, h.NarrationClip);
            UIKit.Button(actions, expanded ? Loc.T("Ringkas", "Less") : Loc.T("Info Lengkap", "Full Info"), () =>
            {
                expanded = !expanded;
                Render();
            }, ButtonVariant.Secondary, expanded ? Icons.Close : Icons.Info, compact: true);

            // Hotspot list: an accessible alternative to tapping small 3D targets.
            UIKit.Label(panel, Loc.T("Bagian-bagian", "Parts"), TextStyle.Overline, ColorRole.TextSecondary);
            foreach (var hs in h.hotspots)
            {
                bool done = ar.IsExplored(h, hs.hotspotId);
                UIKit.Button(panel, ARHotspot.ShortLabel(hs.Label) + (done ? Loc.T(" · dilihat", " · seen") : ""), () => ar.SelectHotspot(h, hs),
                    done ? ButtonVariant.Tonal : ButtonVariant.Secondary, done ? Icons.CheckCircle : Icons.Target, compact: true);
            }

            if (expanded)
            {
                UIKit.Divider(panel);
                UIKit.Label(panel, h.DetailedDescription, TextStyle.Body);
                UIKit.Callout(panel, Icons.Globe, Loc.T("Contoh nyata", "Real-life example"), h.ContextualExample, ColorRole.Primary, ColorRole.PrimarySoft);
            }
        }

        private void BuildHotspotInfo(RectTransform panel)
        {
            var h = ar.Active;
            var hs = ar.SelectedHotspot;
            UIKit.Label(panel, h.hardwareName, TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(panel, hs.Label, TextStyle.Heading);
            UIKit.Label(panel, hs.Description, TextStyle.Body);
            NarrationControl(panel, $"{hs.Label}. {hs.Description}", hs.Clip);
            var row = UIKit.EqualRow(panel, DesignTokens.Space1);
            UIKit.Button(row, Loc.T("Tutup", "Close"), ar.ClearSelection, ButtonVariant.Secondary, Icons.Close, compact: true);
            UIKit.Button(row, Loc.T("Berikutnya", "Next"), ar.NextHotspot, ButtonVariant.Primary, Icons.Forward, compact: true, iconRight: true);
            int explored = ar.ExploredCount(h);
            UIKit.ProgressRow(panel, Loc.T("Bagian dijelajahi", "Parts explored"), $"{explored}/{h.hotspots.Count}",
                h.hotspots.Count == 0 ? 1f : (float)explored / h.hotspots.Count, P.Success);
        }

        /// <summary>Completion + hand-back actions (lesson step, scenario or explorer).</summary>
        private void BuildActivityActions(RectTransform panel)
        {
            var target = ar.Focus ?? ar.Active;
            bool complete = target != null && ar.IsActivityComplete(target);
            bool lessonShown = AddLessonContinue(panel, complete);

            if (!lessonShown && !string.IsNullOrEmpty(State.ARReturnScene))
            {
                string label = State.ARReturnScene == AppConstants.Scenes.ContextualLearning ? Loc.T("Kembali ke skenario", "Back to scenario")
                    : State.ARReturnScene == AppConstants.Scenes.Practice ? Loc.T("Kembali ke latihan", "Back to practice")
                    : Loc.T("Kembali", "Back");
                UIKit.Button(panel, label, Nav.Back, complete ? ButtonVariant.Primary : ButtonVariant.Secondary, Icons.Back);
            }
        }
    }
}
