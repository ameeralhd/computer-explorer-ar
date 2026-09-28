using System.Linq;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Learning;
using ComputerExplorer.Quiz;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 08_ContextualLearning — Scenario → Question → AR/Exploration → Task → Feedback → Reflection,
    /// one major task per step (blueprint §4.10, §11). Without an active scenario it lists all scenarios.
    /// </summary>
    public class ContextualLearningScreen : ScreenBase
    {
        protected override string Title => flow != null ? "Skenario Kontekstual" : "Belajar Kontekstual";
        protected override LessonStepType? LessonStep => LessonStepType.Scenario;

        protected override string ScreenNarration
        {
            get
            {
                if (flow == null) return "Belajar kontekstual. Pilih satu situasi nyata untuk dipelajari.";
                var s = flow.Scenario;
                return flow.Session.phase switch
                {
                    ScenarioPhase.Scenario => $"{s.title}. {s.description}",
                    ScenarioPhase.Question => s.question,
                    ScenarioPhase.Explore => $"Jelajahi {s.hardware?.hardwareName}. {s.hardware?.function}",
                    ScenarioPhase.Task => s.activity != null ? s.activity.questionText : "",
                    ScenarioPhase.Feedback => s.feedback,
                    ScenarioPhase.Reflection => s.reflection,
                    _ => "Skenario selesai."
                };
            }
        }

        protected override string GuidanceText => flow == null
            ? "Setiap skenario punya 6 tahap singkat. Kamu bisa membuka AR atau membaca info perangkat pada tahap Eksplorasi."
            : null;

        private ContextualScenarioManager flow;
        private AnswerController task;
        private int confidence = -1;
        private string reflectionText = "";

        protected override void OnOpened()
        {
            flow = ContextualScenarioManager.FromState();
            if (flow != null && flow.Session.phase == ScenarioPhase.Done) flow = null;
            if (flow != null && flow.Scenario.activity != null) task = new AnswerController(flow.Scenario.activity);
            // Returning from the AR scanner: the exploration step is satisfied.
            if (flow != null && flow.Session.phase == ScenarioPhase.Explore && flow.Session.explored) flow.MarkExplored();
        }

        protected override void BuildSubHeader(RectTransform area)
        {
            if (flow == null) return;
            var box = UIKit.Box(area, P.Surface, 0, null, "Stepper");
            UIKit.AddVertical(box, DesignTokens.Dp(8), DesignTokens.ScreenMargin, DesignTokens.Dp(12));
            int i = flow.PhaseIndex;
            UIKit.Label(box, $"Tahap {i + 1} dari {ContextualScenarioManager.Phases.Length} · {ContextualScenarioManager.PhaseName(flow.Session.phase)}",
                TextStyle.Label, ColorRole.Primary);
            var seg = UIKit.EqualRow(box, DesignTokens.Dp(4));
            for (int k = 0; k < ContextualScenarioManager.Phases.Length; k++)
            {
                var bar = UIKit.Box(seg, k <= i ? P.Primary : P.SurfaceAlt, DesignTokens.RadiusPill, P.IsHighContrast ? P.Border : (Color?)null, "Seg");
                UIKit.Layout(UIKit.Outer(bar), minHeight: DesignTokens.Dp(6), preferredHeight: DesignTokens.Dp(6));
            }
            UIKit.Divider(area);
        }

        protected override void BuildContent(RectTransform content)
        {
            if (flow == null)
            {
                BuildList(content);
                return;
            }
            var s = flow.Scenario;
            switch (flow.Session.phase)
            {
                case ScenarioPhase.Scenario: BuildScenario(content, s); break;
                case ScenarioPhase.Question: BuildQuestion(content, s); break;
                case ScenarioPhase.Explore: BuildExplore(content, s); break;
                case ScenarioPhase.Task: BuildTask(content, s); break;
                case ScenarioPhase.Feedback: BuildFeedback(content, s); break;
                case ScenarioPhase.Reflection: BuildReflection(content, s); break;
                default: BuildDone(content, s); break;
            }
        }

        // ------------------------------------------------------------------ list
        private void BuildList(RectTransform content)
        {
            UIKit.SectionHeader(content, "Pilih situasi", "Hubungkan konsep perangkat komputer dengan kejadian sehari-hari.");
            foreach (var s in Content.Scenarios)
            {
                bool done = Saved.Data.completedScenarios.Contains(s.scenarioId);
                var card = UIKit.Card(content, () =>
                {
                    flow = ContextualScenarioManager.Begin(s);
                    task = s.activity != null ? new AnswerController(s.activity) : null;
                    confidence = -1;
                    reflectionText = "";
                    RenderFromTop();
                }, spacing: DesignTokens.Dp(10), name: "Scenario " + s.scenarioId);
                var row = UIKit.HStack(card, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
                var cat = s.hardware != null ? s.hardware.category : HardwareCategory.Architecture;
                UIKit.IconTile(row, s.iconName, P.Category(cat), P.OnCategory, DesignTokens.Dp(52));
                var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(4)));
                UIKit.Label(col, s.environment, TextStyle.Overline, ColorRole.TextSecondary);
                UIKit.Label(col, s.title, TextStyle.Heading);
                if (s.hardware != null) UIKit.Label(col, $"Perangkat: {s.hardware.hardwareName}", TextStyle.Caption, ColorRole.TextSecondary);
                var meta = UIKit.HStack(card, DesignTokens.Space1);
                if (done) UIKit.Badge(meta, "Selesai", P.SuccessSoft, P.Success, Icons.CheckCircle);
                else UIKit.Badge(meta, "6 tahap", P.SurfaceAlt, P.TextSecondary, Icons.ListChecks);
                UIKit.FlexSpacer(meta);
            }
        }

        // ------------------------------------------------------------------ phases
        private void BuildScenario(RectTransform content, ScenarioData s)
        {
            var hero = UIKit.Card(content, fill: ColorRole.PrimarySoft, borderOverride: P.IsHighContrast ? P.Primary : (Color?)null);
            var row = UIKit.HStack(hero, DesignTokens.Dp(12));
            var cat = s.hardware != null ? s.hardware.category : HardwareCategory.Architecture;
            UIKit.IconTile(row, s.iconName, P.Category(cat), P.OnCategory, DesignTokens.Dp(64));
            var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(4)));
            UIKit.Badge(col, s.environment, P.Surface, P.OnPrimarySoft, Icons.MapPin);
            UIKit.Label(col, s.title, TextStyle.Title, ColorRole.OnPrimarySoft);
            if (s.image != null) UIKit.Picture(content, s.image, DesignTokens.Dp(180));
            UIKit.Label(content, s.description, TextStyle.Body);
            NarrationControl(content, $"{s.title}. {s.description}", s.audio, "Dengarkan cerita");
        }

        private void BuildQuestion(RectTransform content, ScenarioData s)
        {
            UIKit.Callout(content, Icons.Help, "Coba pikirkan", s.question, ColorRole.Primary, ColorRole.PrimarySoft);
            UIKit.Label(content, "Kamu belum perlu menjawab sekarang. Pada tahap berikutnya kamu akan menjelajahi perangkatnya untuk menemukan jawabannya.",
                TextStyle.Body, ColorRole.TextSecondary);
            NarrationControl(content, s.question);
        }

        private void BuildExplore(RectTransform content, ScenarioData s)
        {
            var h = s.hardware;
            if (h == null)
            {
                UIKit.Label(content, "Tidak ada perangkat untuk dijelajahi di skenario ini.", TextStyle.Body);
                return;
            }
            var card = UIKit.Card(content, spacing: DesignTokens.Dp(12));
            var row = UIKit.HStack(card, DesignTokens.Dp(12));
            UIKit.IconTile(row, h.iconName, P.Category(h.category), P.OnCategory, DesignTokens.Dp(56));
            var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(2)));
            UIKit.Label(col, h.hardwareName, TextStyle.Heading);
            UIKit.Label(col, h.fullName, TextStyle.Caption, ColorRole.TextSecondary);
            UIKit.Label(card, "Jelajahi perangkat ini dalam AR: pindai kartu target, lalu ketuk bagian-bagiannya.", TextStyle.Body);
            UIKit.Button(card, "Buka dalam AR", () =>
            {
                State.SelectHardware(h.hardwareId);
                State.ARReturnScene = AppConstants.Scenes.ContextualLearning;
                Go(AppConstants.Scenes.ARScanner);
            }, ButtonVariant.Primary, Icons.Scan);
            UIKit.Button(card, "Baca info tanpa kamera", () => HardwareInfoSheet.Show(h, AppConstants.Scenes.ContextualLearning, () =>
            {
                if (this == null || flow == null) return;
                flow.MarkExplored();
                Render();
            }), ButtonVariant.Secondary, Icons.Book);
            if (flow.Session.explored)
                UIKit.Callout(content, Icons.CheckCircle, "Eksplorasi selesai", $"Kamu sudah menjelajahi {h.hardwareName}. Lanjutkan ke tugas.",
                    ColorRole.Success, ColorRole.SuccessSoft);
        }

        private void BuildTask(RectTransform content, ScenarioData s)
        {
            if (task == null)
            {
                UIKit.Label(content, "Skenario ini tidak memiliki tugas.", TextStyle.Body);
                return;
            }
            QuestionController.BuildQuestion(content, task.Question, 0, 0);
            NarrationControl(content, task.Question.questionText, task.Question.audio, "Dengarkan soal");
            QuestionController.BuildAnswers(content, task, false, () => Render());
        }

        private void BuildFeedback(RectTransform content, ScenarioData s)
        {
            if (task != null && flow.Session.taskAnswered)
                QuizFeedback.Build(content, task.Question, flow.Session.taskCorrect);
            UIKit.Callout(content, Icons.Lightbulb, "Hubungannya dengan situasi", s.feedback, ColorRole.Primary, ColorRole.PrimarySoft);
            NarrationControl(content, (task != null ? QuizFeedback.SpokenText(task.Question, flow.Session.taskCorrect) + " " : "") + s.feedback);
        }

        private void BuildReflection(RectTransform content, ScenarioData s)
        {
            UIKit.Callout(content, Icons.Message, "Refleksi", s.reflection, ColorRole.Primary, ColorRole.PrimarySoft);
            ReflectionScreen.BuildConfidence(content, confidence, v =>
            {
                confidence = v;
                Render();
            });
            ReflectionScreen.BuildResponse(content, reflectionText, v => reflectionText = v, () => Render());
        }

        private void BuildDone(RectTransform content, ScenarioData s)
        {
            UIKit.Callout(content, Icons.Trophy, "Skenario selesai!", $"Kerja bagus menyelesaikan \"{s.title}\".", ColorRole.Success, ColorRole.SuccessSoft);
        }

        // ------------------------------------------------------------------ footer
        protected override void BuildFooter(RectTransform footer)
        {
            if (flow == null) return;
            var phase = flow.Session.phase;

            if (phase == ScenarioPhase.Done)
            {
                if (!AddLessonContinue(footer, true))
                {
                    UIKit.Button(footer, "Pilih skenario lain", () =>
                    {
                        flow = null;
                        State.ActiveScenario = null;
                        RenderFromTop();
                    }, ButtonVariant.Primary, Icons.Globe);
                    UIKit.Button(footer, "Kembali ke menu", Nav.Home, ButtonVariant.Secondary, Icons.Home);
                }
                return;
            }

            var row = UIKit.EqualRow(footer, DesignTokens.Space1);
            if (flow.PhaseIndex > 0 && phase != ScenarioPhase.Feedback && phase != ScenarioPhase.Reflection)
                UIKit.Button(row, "Kembali", () => { flow.Previous(); RenderFromTop(); }, ButtonVariant.Secondary, Icons.Back);

            switch (phase)
            {
                case ScenarioPhase.Explore:
                    UIKit.Button(row, "Lanjut ke tugas", Advance, ButtonVariant.Primary, Icons.Forward,
                        flow.Session.explored ? ButtonState.Normal : ButtonState.Disabled, iconRight: true);
                    if (!flow.Session.explored)
                        UIKit.Label(footer, "Buka AR atau baca info perangkat terlebih dahulu.", TextStyle.Caption, ColorRole.TextSecondary,
                            TextAnchor.MiddleCenter).transform.SetAsFirstSibling();
                    break;
                case ScenarioPhase.Task:
                    UIKit.Button(row, "Periksa jawaban", () =>
                    {
                        if (task == null || !task.IsComplete) return;
                        bool correct = task.Evaluate();
                        flow.SubmitTask(task.Summary, correct);
                        Audio.AudioManager.Instance.PlayFeedback(correct ? Audio.FeedbackSound.Correct : Audio.FeedbackSound.Incorrect);
                        Advance();
                    }, ButtonVariant.Primary, Icons.Check, task != null && task.IsComplete ? ButtonState.Normal : ButtonState.Disabled);
                    break;
                case ScenarioPhase.Reflection:
                    UIKit.Button(row, "Kirim refleksi", () =>
                    {
                        flow.Complete(confidence + 1, reflectionText);
                        if (Learning.IsLessonStep(LessonStepType.Scenario))
                        {
                            Learning.CompleteCurrentStep();
                            return;
                        }
                        RenderFromTop();
                    }, ButtonVariant.Primary, Icons.Check, confidence >= 0 ? ButtonState.Normal : ButtonState.Disabled);
                    break;
                default:
                    UIKit.Button(row, "Lanjut", Advance, ButtonVariant.Primary, Icons.Forward, iconRight: true);
                    break;
            }
        }

        private void Advance()
        {
            flow.Next();
            RenderFromTop();
        }
    }
}
