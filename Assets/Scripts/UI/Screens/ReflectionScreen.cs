using System;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Learning;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 10_Reflection — a prompt, a simple confidence choice (no typing needed) and an optional written answer
    /// with sentence starters. Completion state then Continue.
    /// </summary>
    public class ReflectionScreen : ScreenBase
    {
        protected override string Title => Loc.T("Refleksi", "Reflection");
        protected override LessonStepType? LessonStep => LessonStepType.Reflection;
        protected override string ScreenNarration => Loc.T($"Refleksi. {Prompt} Pilih seberapa paham kamu, lalu tulis jawabanmu jika mau.", $"Reflection. {Prompt} Choose how well you understand, then write your answer if you like.");
        protected override string GuidanceText => Loc.T("Tidak ada jawaban benar atau salah. Cukup pilih seberapa paham kamu; menulis bersifat opsional.", "There are no right or wrong answers. Just choose how well you understand; writing is optional.");

        private string contextId;
        private string customPrompt;
        private Data.ModuleData module;
        private int confidence = -1;
        private string response = "";
        private bool submitted;

        private static string[] Starters => new[]
        {
            Loc.T("Hari ini saya belajar bahwa ", "Today I learned that "),
            Loc.T("Bagian yang paling mudah adalah ", "The easiest part was "),
            Loc.T("Saya masih bingung tentang ", "I am still unsure about "),
            Loc.T("Contoh di sekitar saya adalah ", "An example around me is ")
        };

        // Computed on every render so they follow the current language.
        private string Prompt => !string.IsNullOrEmpty(customPrompt) ? customPrompt
            : module != null && !string.IsNullOrEmpty(module.ReflectionPrompt) ? module.ReflectionPrompt
            : Loc.T("Apa hal terpenting yang kamu pelajari hari ini?", "What is the most important thing you learned today?");

        private string ContextTitle => module != null ? $"{Loc.T("Modul", "Module")} {module.order}: {module.Title}"
            : Loc.T("Pembelajaran hari ini", "Today's learning");

        protected override void OnOpened()
        {
            contextId = State.ReflectionContextId;
            customPrompt = State.ReflectionPrompt;
            module = Content.GetModule(contextId) ?? Learning.Modules.LastAccessed ?? Learning.Modules.Current;
            if (string.IsNullOrEmpty(contextId) && module != null) contextId = module.moduleId;
            var existing = Saved.Data.reflections.Find(r => r.contextId == contextId);
            if (existing != null)
            {
                confidence = existing.confidence - 1;
                response = existing.response ?? "";
            }
        }

        protected override void BuildContent(RectTransform content)
        {
            if (submitted)
            {
                UIKit.Callout(content, Icons.CheckCircle, Loc.T("Refleksi tersimpan", "Reflection saved"), Loc.T("Terima kasih! Refleksimu membantu guru memahami kebutuhan belajarmu.", "Thank you! Your reflection helps your teacher understand your learning needs."),
                    ColorRole.Success, ColorRole.SuccessSoft);
                return;
            }
            UIKit.Label(content, ContextTitle, TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Callout(content, Icons.Message, Loc.T("Pertanyaan refleksi", "Reflection question"), Prompt, ColorRole.Primary, ColorRole.PrimarySoft);
            NarrationControl(content, Prompt);
            BuildConfidence(content, confidence, v =>
            {
                confidence = v;
                Render();
            });
            BuildResponse(content, response, v => response = v, () => Render());
        }

        /// <summary>Three large, labelled choices (icon + text), shared with the scenario reflection step.</summary>
        public static void BuildConfidence(Transform parent, int selected, Action<int> onSelect)
        {
            UIKit.Label(parent, Loc.T("Seberapa paham kamu sekarang?", "How well do you understand now?"), TextStyle.Heading);
            string[] icons = { Icons.Alert, Icons.Help, Icons.CheckCircle };
            for (int i = 0; i < ReflectionManager.ConfidenceLabels.Length; i++)
            {
                int index = i;
                bool isSel = selected == i;
                var p = ContrastController.Current;
                var card = UIKit.Card(parent, () => onSelect(index), isSel ? ColorRole.PrimarySoft : ColorRole.Surface, DesignTokens.Dp(14),
                    borderOverride: isSel ? p.Primary : p.BorderStrong);
                var row = UIKit.HStack(card, DesignTokens.Dp(12));
                UIKit.Icon(row, icons[i], DesignTokens.IconSize, isSel ? ColorRole.Primary : ColorRole.TextSecondary);
                UIKit.Flex(UIKit.Label(row, ReflectionManager.ConfidenceLabels[i], TextStyle.BodyStrong));
                if (isSel) UIKit.Label(row, Loc.T("Dipilih", "Selected"), TextStyle.Overline, ColorRole.Primary, TextAnchor.MiddleRight);
            }
        }

        public static void BuildResponse(Transform parent, string value, Action<string> onChanged, Action rerender)
        {
            UIKit.SectionHeader(parent, Loc.T("Tulis jawabanmu (opsional)", "Write your answer (optional)"), Loc.T("Ketuk salah satu awal kalimat untuk membantu menulis.", "Tap a sentence starter to help you write."));
            var starters = UIKit.VStack(parent, DesignTokens.Space1);
            foreach (var s in Starters)
                UIKit.Chip(starters, s.Trim() + "…", false, () =>
                {
                    onChanged((string.IsNullOrWhiteSpace(value) ? "" : value.TrimEnd() + " ") + s);
                    rerender();
                }, Icons.Pencil);
            var input = UIKit.TextInput(parent, Loc.T("Tulis refleksimu di sini…", "Write your reflection here…"), value, true, onChanged);
            input.onEndEdit.AddListener(_ => rerender());
        }

        protected override void BuildFooter(RectTransform footer)
        {
            if (submitted)
            {
                if (!AddLessonContinue(footer, true))
                    UIKit.Button(footer, Loc.T("Lihat progres", "View progress"), () => Go(AppConstants.Scenes.Progress), ButtonVariant.Primary, Icons.Chart);
                return;
            }
            if (confidence < 0)
                UIKit.Label(footer, Loc.T("Pilih tingkat pemahamanmu untuk mengirim.", "Choose your level of understanding to submit."), TextStyle.Caption, ColorRole.TextSecondary, TextAnchor.MiddleCenter);
            UIKit.Button(footer, Loc.T("Kirim refleksi", "Submit reflection"), () =>
            {
                bool lesson = Learning.IsLessonStep(LessonStepType.Reflection);
                ReflectionManager.Submit(contextId, confidence + 1, response, completesLessonStep: lesson);
                if (lesson) return; // the lesson controller navigates onward
                submitted = true;
                PopupController.Instance.Toast(Loc.T("Refleksi tersimpan", "Reflection saved"), Icons.CheckCircle);
                RenderFromTop();
            }, ButtonVariant.Primary, Icons.Check, confidence >= 0 ? ButtonState.Normal : ButtonState.Disabled);
        }
    }
}
