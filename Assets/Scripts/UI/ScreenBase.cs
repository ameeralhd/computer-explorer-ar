using System.Collections;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Learning;
using ComputerExplorer.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI
{
    /// <summary>
    /// Base for every screen. Builds the same structure everywhere — safe area, header (back, title,
    /// narration, settings), guided-mode hint, lesson step bar, scrolling content and a bottom action bar —
    /// so screens only describe their content.
    ///
    /// Screens are rendered from state (like a small immediate-mode UI): call <see cref="Render"/> after
    /// changing state. Any accessibility change re-renders the screen, so text size, contrast, touch size
    /// and motion settings apply everywhere instantly without per-screen code.
    /// </summary>
    public abstract class ScreenBase : MonoBehaviour
    {
        protected static Palette P => ContrastController.Current;
        protected static ContentLibrary Content => AppManager.Instance.Content;
        protected static GameStateManager State => GameStateManager.Instance;
        protected static NavigationController Nav => NavigationController.Instance;
        protected static LearningManager Learning => LearningManager.Instance;
        protected static ProgressManager Saved => ProgressManager.Instance;
        protected static NarrationController Narration => AudioManager.Instance.Narration;
        protected static bool Guided => AccessibilityManager.Instance.IsGuided;

        protected virtual string Title => AppConstants.AppName;
        protected virtual bool ShowHeader => true;
        protected virtual bool ShowBack => true;
        /// <summary>Spoken summary of the screen (header speaker button). Every screen should provide one.</summary>
        protected virtual string ScreenNarration => null;
        /// <summary>Guided-mode instruction shown at the top of the content.</summary>
        protected virtual string GuidanceText => null;
        protected virtual bool Scrollable => true;
        protected virtual bool OpaqueBackground => true;
        /// <summary>Lesson step this screen represents (shows the "Langkah x dari y" bar when active).</summary>
        protected virtual LessonStepType? LessonStep => null;

        protected Canvas Canvas { get; private set; }
        protected RectTransform SafeRoot { get; private set; }
        protected RectTransform ContentRoot { get; private set; }

        private Image background;
        private ScrollRect scroll;
        private CanvasGroup group;
        private bool hasRendered;
        private bool resetScrollOnNextRender;

        protected virtual void Awake()
        {
            Canvas = CanvasFactory.Create(transform, "ScreenCanvas", 0);
            group = Canvas.gameObject.AddComponent<CanvasGroup>();
            background = CanvasFactory.FullScreenImage(Canvas.transform, P.Background);
            background.raycastTarget = false;
            SafeRoot = CanvasFactory.SafeArea(Canvas.transform);
            var v = UIKit.AddVertical(SafeRoot, 0);
            v.childForceExpandHeight = false;
        }

        protected virtual void Start()
        {
            AccessibilityManager.Instance.Changed += OnAccessibilityChanged;
            OnOpened();
            Render();
            if (AccessibilityManager.Instance.Settings.autoNarrate && !string.IsNullOrEmpty(ScreenNarration))
                Narration.Play(ScreenNarration);
        }

        protected virtual void OnDestroy()
        {
            if (AccessibilityManager.Instance != null) AccessibilityManager.Instance.Changed -= OnAccessibilityChanged;
        }

        private void OnAccessibilityChanged(AccessibilitySettings s) => Render();

        /// <summary>Called once before the first render: read state, start sessions.</summary>
        protected virtual void OnOpened() { }

        /// <summary>Describe the screen content. Called on every render.</summary>
        protected abstract void BuildContent(RectTransform content);

        /// <summary>Optional bottom action bar (primary action stays in the same place on every screen).</summary>
        protected virtual void BuildFooter(RectTransform footer) { }

        /// <summary>Optional custom header area below the standard header (e.g. filters).</summary>
        protected virtual void BuildSubHeader(RectTransform area) { }

        /// <summary>Re-render. Pass resetScroll when moving to a new step so the learner starts at the top.</summary>
        public void Render(bool resetScroll = false)
        {
            // Keep the absolute scroll offset across re-renders (ScrollRect clamps it if content got shorter).
            float keepY = scroll != null && scroll.content != null ? scroll.content.anchoredPosition.y : 0f;
            resetScroll |= resetScrollOnNextRender;
            resetScrollOnNextRender = false;

            background.color = OpaqueBackground ? P.Background : Color.clear;
            for (int i = SafeRoot.childCount - 1; i >= 0; i--)
            {
                var child = SafeRoot.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            if (ShowHeader) BuildHeader();

            var sub = UIKit.VStack(SafeRoot, 0, name: "SubHeader");
            BuildSubHeader(sub);
            if (LessonStep.HasValue && Learning.IsLessonStep(LessonStep.Value)) BuildLessonBar(sub);
            if (sub.childCount == 0) Destroy(sub.gameObject);

            if (Scrollable)
            {
                var content = CanvasFactory.ScrollView(SafeRoot, out scroll, DesignTokens.Space2);
                UIKit.Layout(scroll, flexibleHeight: 1);
                ContentRoot = content;
            }
            else
            {
                scroll = null;
                ContentRoot = UIKit.Rect(SafeRoot, "Content");
                UIKit.Layout(ContentRoot, flexibleHeight: 1);
            }

            if (Guided && !string.IsNullOrEmpty(GuidanceText)) BuildGuidance(ContentRoot);
            BuildContent(ContentRoot);

            var footer = UIKit.Rect(SafeRoot, "Footer");
            BuildFooter(footer);
            if (footer.childCount == 0) Destroy(footer.gameObject);
            else StyleFooter(footer);

            if (scroll != null)
            {
                scroll.movementType = MotionController.Reduced ? ScrollRect.MovementType.Clamped : ScrollRect.MovementType.Elastic;
                if (!resetScroll && hasRendered) scroll.content.anchoredPosition = new Vector2(0f, keepY);
            }

            if (!hasRendered)
            {
                hasRendered = true;
                StartCoroutine(MotionController.Fade(group, 0f, 1f, 0.2f));
            }
        }

        protected void RenderFromTop() => Render(true);

        // ------------------------------------------------------------------ chrome
        private void BuildHeader()
        {
            var bar = UIKit.Box(SafeRoot, P.Surface, 0, null, "Header");
            UIKit.Layout(bar, minHeight: DesignTokens.HeaderHeight);
            var row = UIKit.AddHorizontal(bar, DesignTokens.Dp(4), DesignTokens.Space1, DesignTokens.Dp(4));
            row.childAlignment = TextAnchor.MiddleLeft;

            if (ShowBack) UIKit.IconButton(bar, Icons.Back, "Kembali", OnBackPressed);
            else UIKit.IconButton(bar, Icons.Menu, "Menu", OpenMenuSheet);

            var title = UIKit.Label(bar, Title, TextStyle.Heading, ColorRole.TextPrimary, TextAnchor.MiddleLeft);
            UIKit.Flex(title);
            UIKit.Layout(title, minHeight: DesignTokens.TouchTarget);

            if (!string.IsNullOrEmpty(ScreenNarration))
            {
                bool speaking = Narration.IsSpeaking && Narration.CurrentText == ScreenNarration;
                UIKit.IconButton(bar, speaking ? Icons.Pause : Icons.Volume, speaking ? "Jeda narasi" : "Bacakan layar ini",
                    () =>
                    {
                        Narration.Toggle(ScreenNarration);
                        Render();
                    });
            }
            UIKit.IconButton(bar, Icons.Settings, "Pengaturan aksesibilitas", () => Nav.GoTo(AppConstants.Scenes.Accessibility));

            // bottom hairline
            var line = UIKit.Divider(SafeRoot);
            line.name = "HeaderDivider";
        }

        protected virtual void OnBackPressed() => Nav.Back();

        private void BuildGuidance(RectTransform content)
        {
            var card = UIKit.Callout(content, Icons.Lightbulb, "Petunjuk", GuidanceText);
            var row = UIKit.HStack(card, DesignTokens.Space1);
            UIKit.Button(row, "Dengarkan petunjuk", () => Narration.Play(GuidanceText, null, true), ButtonVariant.Ghost,
                Icons.Volume, compact: true);
        }

        private void BuildLessonBar(RectTransform area)
        {
            var lesson = Learning.Lesson;
            var m = lesson.ActiveModule;
            var box = UIKit.Box(area, P.PrimarySoft, 0, null, "LessonBar");
            UIKit.AddVertical(box, DesignTokens.Dp(6), DesignTokens.ScreenMargin, DesignTokens.Dp(10));
            var row = UIKit.HStack(box, DesignTokens.Space1);
            UIKit.Icon(row, lesson.CurrentStep.type.IconName(), DesignTokens.IconSizeSmall, ColorRole.OnPrimarySoft);
            UIKit.Flex(UIKit.Label(row, $"{lesson.StepLabel} · {lesson.CurrentStep.type.DisplayName()}", TextStyle.Label,
                ColorRole.OnPrimarySoft));
            UIKit.ProgressBar(box, (float)lesson.StepIndex / Mathf.Max(1, m.lessonSteps.Count), P.Primary, 6);
            UIKit.Label(box, m.title, TextStyle.Caption, ColorRole.OnPrimarySoft);
        }

        private void StyleFooter(RectTransform footer)
        {
            var img = footer.gameObject.AddComponent<Image>();
            img.color = P.Surface;
            var v = UIKit.AddVertical(footer, DesignTokens.Space1, DesignTokens.ScreenMargin, DesignTokens.Space2);
            v.childForceExpandHeight = false;
            footer.SetAsLastSibling();
            var line = UIKit.Divider(SafeRoot);
            line.transform.SetSiblingIndex(footer.GetSiblingIndex());
        }

        /// <summary>Standard "continue lesson" action for screens that are a lesson step.</summary>
        protected bool AddLessonContinue(RectTransform footer, bool enabled, string hint = null)
        {
            if (!LessonStep.HasValue || !Learning.IsLessonStep(LessonStep.Value)) return false;
            var lesson = Learning.Lesson;
            bool last = lesson.StepIndex >= lesson.ActiveModule.lessonSteps.Count - 1;
            if (!enabled && !string.IsNullOrEmpty(hint))
                UIKit.Label(footer, hint, TextStyle.Caption, ColorRole.TextSecondary, TextAnchor.MiddleCenter);
            UIKit.Button(footer, last ? "Selesaikan modul" : "Lanjut ke langkah berikutnya", Learning.CompleteCurrentStep,
                ButtonVariant.Primary, last ? Icons.Trophy : Icons.Forward, enabled ? ButtonState.Normal : ButtonState.Disabled,
                iconRight: !last);
            return true;
        }

        private void OpenMenuSheet()
        {
            ModalController.Instance.Show(new ModalController.Options
            {
                Title = "Menu",
                Icon = Icons.Menu,
                SecondaryLabel = null,
                BuildContent = c =>
                {
                    void Item(string label, string icon, string scene)
                    {
                        UIKit.Button(c, label, () =>
                        {
                            ModalController.Instance.CloseAll();
                            Nav.GoTo(scene);
                        }, ButtonVariant.Tonal, icon);
                    }
                    Item("Arsitektur Von Neumann", Icons.Workflow, AppConstants.Scenes.VonNeumann);
                    Item("Aksesibilitas", Icons.Accessibility, AppConstants.Scenes.Accessibility);
                    Item("Bantuan & Tutorial", Icons.Help, AppConstants.Scenes.Help);
                    Item("Panduan Guru", Icons.Teacher, AppConstants.Scenes.TeacherGuide);
                    Item("Tentang Aplikasi", Icons.Info, AppConstants.Scenes.Welcome);
                }
            });
        }

        // ------------------------------------------------------------------ helpers for subclasses
        /// <summary>Narration play/pause/replay control bound to a text (Audio Control: off, playing, paused).</summary>
        protected void NarrationControl(Transform parent, string text, AudioClip clip = null, string label = "Dengarkan")
        {
            var row = UIKit.HStack(parent, DesignTokens.Space1, name: "NarrationControl");
            var state = Narration.State;
            bool isThis = Narration.CurrentText == text;
            if (state == NarrationState.Off)
            {
                UIKit.Button(row, "Narasi nonaktif", () => Nav.GoTo(AppConstants.Scenes.Accessibility), ButtonVariant.Tonal,
                    Icons.VolumeOff, compact: true);
                return;
            }
            bool playing = isThis && state == NarrationState.Playing;
            bool paused = isThis && state == NarrationState.Paused;
            UIKit.Button(row, playing ? "Jeda" : paused ? "Lanjutkan" : label, () =>
            {
                Narration.Toggle(text, clip);
                StartCoroutine(RenderWhenNarrationChanges());
            }, ButtonVariant.Tonal, playing ? Icons.Pause : Icons.Volume, compact: true);
            if (playing || paused)
                UIKit.IconButton(row, Icons.Reset, "Putar ulang", () =>
                {
                    Narration.Replay();
                    Render();
                }, ButtonVariant.Tonal);
        }

        private IEnumerator RenderWhenNarrationChanges()
        {
            Render();
            var s = Narration.State;
            while (Narration.State == s) yield return null;
            if (this != null) Render();
        }

        protected void Go(string scene) => Nav.GoTo(scene);
    }
}
