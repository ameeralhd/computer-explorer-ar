using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Quiz;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 09_Practice — question, optional image/audio, answer controls for every question type, submit,
    /// immediate feedback with explanation, next, and a results summary (blueprint §4.11, §12).
    /// </summary>
    public class PracticeScreen : ScreenBase
    {
        protected override string Title => "Latihan";
        protected override LessonStepType? LessonStep => LessonStepType.Practice;

        protected override string ScreenNarration
        {
            get
            {
                var q = Quiz?.Current;
                if (q == null) return "Latihan selesai.";
                string options = q.questionType.IsSingleChoice() ? " Pilihan: " + string.Join(", ", q.options) + "." : "";
                return q.questionText + options;
            }
        }

        protected override string GuidanceText => "Pilih jawaban, lalu tekan \"Periksa jawaban\". Penjelasan akan muncul setelahnya.";

        private static QuizManager Quiz => QuizManager.Active;
        private ModuleData module;

        protected override void OnOpened()
        {
            module = Learning.Lesson.IsActive && Learning.Lesson.CurrentStep?.type == LessonStepType.Practice
                ? Learning.Lesson.ActiveModule
                : Learning.Modules.Current;
            StartOrResume(module);
        }

        private void StartOrResume(ModuleData m)
        {
            if (m == null) return;
            module = m;
            State.SetModule(m.moduleId);
            if (Quiz == null || Quiz.Module != m) QuizManager.Start(m);
        }

        protected override void BuildSubHeader(RectTransform area)
        {
            if (Learning.IsLessonStep(LessonStepType.Practice)) return; // lesson bar already shows the module
            var box = UIKit.Box(area, P.Surface, 0, null, "ModulePicker");
            UIKit.AddVertical(box, DesignTokens.Space1, DesignTokens.ScreenMargin, DesignTokens.Dp(12));
            UIKit.Label(box, "Pilih modul", TextStyle.Overline, ColorRole.TextSecondary);
            int perRow = TextSizeController.IsLarge ? 2 : 4;
            RectTransform row = null;
            var mods = Content.Modules;
            for (int i = 0; i < mods.Count; i++)
            {
                if (i % perRow == 0) row = UIKit.EqualRow(box, DesignTokens.Space1);
                var m = mods[i];
                UIKit.Layout(UIKit.Chip(row, $"Modul {m.order}", m == module, () =>
                {
                    QuizManager.Start(m);
                    StartOrResume(m);
                    RenderFromTop();
                }), flexibleWidth: 1);
            }
            UIKit.Divider(area);
        }

        protected override void BuildContent(RectTransform content)
        {
            if (Quiz == null)
            {
                UIKit.Label(content, "Belum ada soal.", TextStyle.Body);
                return;
            }
            if (Quiz.Finished)
            {
                BuildResults(content);
                return;
            }

            UIKit.ProgressRow(content, module.title, $"Soal {Quiz.Index + 1}/{Quiz.Total}", (float)Quiz.Index / Mathf.Max(1, Quiz.Total));

            var q = Quiz.Current;
            QuestionController.BuildQuestion(content, q, Quiz.Index + 1, Quiz.Total);
            NarrationControl(content, ScreenNarration, q.audio, "Dengarkan soal");

            if (q.questionType == QuestionType.ARIdentification && q.relatedHardware != null && !Quiz.Submitted)
            {
                UIKit.Button(content, $"Amati {q.relatedHardware.hardwareName} dalam AR", () =>
                {
                    State.SelectHardware(q.relatedHardware.hardwareId);
                    State.ARReturnScene = AppConstants.Scenes.Practice;
                    Go(AppConstants.Scenes.ARScanner);
                }, ButtonVariant.Tonal, Icons.Scan);
            }

            QuestionController.BuildAnswers(content, Quiz.Answer, Quiz.Submitted, () => Render());

            if (Quiz.Submitted)
            {
                QuizFeedback.Build(content, q, Quiz.LastCorrect);
                NarrationControl(content, QuizFeedback.SpokenText(q, Quiz.LastCorrect), null, "Dengarkan penjelasan");
            }
        }

        private void BuildResults(RectTransform content)
        {
            float pct = Quiz.Percent;
            var hero = UIKit.Card(content, fill: pct >= 0.7f ? ColorRole.SuccessSoft : ColorRole.WarningSoft,
                borderOverride: pct >= 0.7f ? P.Success : P.Warning);
            var row = UIKit.HStack(hero, DesignTokens.Dp(12));
            UIKit.Icon(row, pct >= 0.7f ? Icons.Trophy : Icons.Lightbulb, DesignTokens.Dp(40), pct >= 0.7f ? ColorRole.Success : ColorRole.Warning);
            var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(4)));
            UIKit.Label(col, "Hasil latihan", TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(col, $"{Quiz.Score} dari {Quiz.Total} benar ({Mathf.RoundToInt(pct * 100)}%)", TextStyle.Title);
            UIKit.Label(hero, pct >= 0.7f ? "Hebat! Pemahamanmu sudah baik." : "Tidak apa-apa. Pelajari lagi penjelasannya, lalu coba ulangi.",
                TextStyle.Body);

            UIKit.SectionHeader(content, "Rincian");
            var list = UIKit.Card(content, spacing: DesignTokens.Dp(10));
            for (int i = 0; i < Quiz.Results.Count; i++)
            {
                var (question, correct) = Quiz.Results[i];
                var r = UIKit.HStack(list, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
                UIKit.Icon(r, correct ? Icons.CheckCircle : Icons.XCircle, DesignTokens.IconSize, correct ? ColorRole.Success : ColorRole.Error);
                var t = UIKit.Flex(UIKit.VStack(r, DesignTokens.Dp(2)));
                UIKit.Label(t, $"Soal {i + 1} · {(correct ? "Benar" : "Belum tepat")}", TextStyle.Label, correct ? ColorRole.Success : ColorRole.Error);
                UIKit.Label(t, question.questionText, TextStyle.Caption, ColorRole.TextSecondary);
                if (i < Quiz.Results.Count - 1) UIKit.Divider(list);
            }
        }

        protected override void BuildFooter(RectTransform footer)
        {
            if (Quiz == null) return;
            if (Quiz.Finished)
            {
                bool lesson = AddLessonContinue(footer, true);
                UIKit.Button(footer, "Ulangi latihan", () =>
                {
                    QuizManager.Start(module);
                    RenderFromTop();
                }, lesson ? ButtonVariant.Secondary : ButtonVariant.Primary, Icons.Refresh);
                if (!lesson) UIKit.Button(footer, "Kembali ke menu", Nav.Home, ButtonVariant.Secondary, Icons.Home);
                return;
            }
            if (!Quiz.Submitted)
            {
                UIKit.Button(footer, "Periksa jawaban", () =>
                {
                    Quiz.Submit();
                    Render();
                }, ButtonVariant.Primary, Icons.Check, Quiz.Answer.IsComplete ? ButtonState.Normal : ButtonState.Disabled);
            }
            else
            {
                bool last = Quiz.Index >= Quiz.Total - 1;
                UIKit.Button(footer, last ? "Lihat hasil" : "Soal berikutnya", () =>
                {
                    Quiz.Next();
                    RenderFromTop();
                }, ButtonVariant.Primary, last ? Icons.Chart : Icons.Forward, iconRight: !last);
            }
        }
    }
}
