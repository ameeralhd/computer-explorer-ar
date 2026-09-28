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
        protected override string Title => "Refleksi";
        protected override LessonStepType? LessonStep => LessonStepType.Reflection;
        protected override string ScreenNarration => $"Refleksi. {prompt} Pilih seberapa paham kamu, lalu tulis jawabanmu jika mau.";
        protected override string GuidanceText => "Tidak ada jawaban benar atau salah. Cukup pilih seberapa paham kamu; menulis bersifat opsional.";

        private string contextId;
        private string prompt;
        private string contextTitle;
        private int confidence = -1;
        private string response = "";
        private bool submitted;

        private static readonly string[] Starters =
        {
            "Hari ini saya belajar bahwa ",
            "Bagian yang paling mudah adalah ",
            "Saya masih bingung tentang ",
            "Contoh di sekitar saya adalah "
        };

        protected override void OnOpened()
        {
            contextId = State.ReflectionContextId;
            prompt = State.ReflectionPrompt;
            var module = Content.GetModule(contextId) ?? Learning.Modules.LastAccessed ?? Learning.Modules.Current;
            if (string.IsNullOrEmpty(contextId) && module != null) contextId = module.moduleId;
            if (string.IsNullOrEmpty(prompt)) prompt = module != null && !string.IsNullOrEmpty(module.reflectionPrompt)
                ? module.reflectionPrompt
                : "Apa hal terpenting yang kamu pelajari hari ini?";
            contextTitle = module != null ? $"Modul {module.order}: {module.title}" : "Pembelajaran hari ini";
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
                UIKit.Callout(content, Icons.CheckCircle, "Refleksi tersimpan", "Terima kasih! Refleksimu membantu guru memahami kebutuhan belajarmu.",
                    ColorRole.Success, ColorRole.SuccessSoft);
                return;
            }
            UIKit.Label(content, contextTitle, TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Callout(content, Icons.Message, "Pertanyaan refleksi", prompt, ColorRole.Primary, ColorRole.PrimarySoft);
            NarrationControl(content, prompt);
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
            UIKit.Label(parent, "Seberapa paham kamu sekarang?", TextStyle.Heading);
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
                if (isSel) UIKit.Label(row, "Dipilih", TextStyle.Overline, ColorRole.Primary, TextAnchor.MiddleRight);
            }
        }

        public static void BuildResponse(Transform parent, string value, Action<string> onChanged, Action rerender)
        {
            UIKit.SectionHeader(parent, "Tulis jawabanmu (opsional)", "Ketuk salah satu awal kalimat untuk membantu menulis.");
            var starters = UIKit.VStack(parent, DesignTokens.Space1);
            foreach (var s in Starters)
                UIKit.Chip(starters, s.Trim() + "…", false, () =>
                {
                    onChanged((string.IsNullOrWhiteSpace(value) ? "" : value.TrimEnd() + " ") + s);
                    rerender();
                }, Icons.Pencil);
            var input = UIKit.TextInput(parent, "Tulis refleksimu di sini…", value, true, onChanged);
            input.onEndEdit.AddListener(_ => rerender());
        }

        protected override void BuildFooter(RectTransform footer)
        {
            if (submitted)
            {
                if (!AddLessonContinue(footer, true))
                    UIKit.Button(footer, "Lihat progres", () => Go(AppConstants.Scenes.Progress), ButtonVariant.Primary, Icons.Chart);
                return;
            }
            if (confidence < 0)
                UIKit.Label(footer, "Pilih tingkat pemahamanmu untuk mengirim.", TextStyle.Caption, ColorRole.TextSecondary, TextAnchor.MiddleCenter);
            UIKit.Button(footer, "Kirim refleksi", () =>
            {
                bool lesson = Learning.IsLessonStep(LessonStepType.Reflection);
                ReflectionManager.Submit(contextId, confidence + 1, response, completesLessonStep: lesson);
                if (lesson) return; // the lesson controller navigates onward
                submitted = true;
                PopupController.Instance.Toast("Refleksi tersimpan", Icons.CheckCircle);
                RenderFromTop();
            }, ButtonVariant.Primary, Icons.Check, confidence >= 0 ? ButtonState.Normal : ButtonState.Disabled);
        }
    }
}
