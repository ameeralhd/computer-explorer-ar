using System.Linq;
using System.Text;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>04_ModuleDetail — objectives, activity sequence, related hardware, Start Learning.</summary>
    public class ModuleDetailScreen : ScreenBase
    {
        private ModuleData module;

        protected override string Title => module != null ? $"Modul {module.order}" : "Detail Modul";

        protected override string ScreenNarration
        {
            get
            {
                if (module == null) return null;
                var sb = new StringBuilder($"Modul {module.order}. {module.title}. {module.description} Tujuan pembelajaran: ");
                foreach (var o in module.learningObjective) sb.Append(o).Append(' ');
                sb.Append($"Modul ini terdiri atas {module.lessonSteps.Count} langkah.");
                return sb.ToString();
            }
        }

        protected override string GuidanceText =>
            "Baca tujuan pembelajaran, lalu tekan \"Mulai Belajar\". Aplikasi akan membawamu ke setiap langkah secara berurutan.";

        protected override void OnOpened()
        {
            module = Learning.Modules.Current;
        }

        protected override void BuildContent(RectTransform content)
        {
            if (module == null)
            {
                UIKit.Label(content, "Modul tidak ditemukan.", TextStyle.Body);
                return;
            }

            // Hero
            var hero = UIKit.Card(content, spacing: DesignTokens.Dp(12));
            var row = UIKit.HStack(hero, DesignTokens.Dp(12));
            UIKit.IconTile(row, module.iconName, P.Category(HardwareCategory.Processing), P.OnCategory, DesignTokens.Dp(64));
            var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(4)));
            UIKit.Label(col, $"Modul {module.order}", TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(col, module.title, TextStyle.Title);
            UIKit.Label(hero, module.description, TextStyle.Body, ColorRole.TextSecondary);
            var meta = UIKit.HStack(hero, DesignTokens.Space1);
            var st = LearningModulesScreen.StateStyle(Saved.StateOf(module));
            UIKit.Badge(meta, st.text, st.bg, P.Get(st.fg), st.icon);
            UIKit.Badge(meta, $"{module.estimatedMinutes} menit", P.SurfaceAlt, P.TextSecondary, Icons.Clock);
            UIKit.Badge(meta, $"{module.lessonSteps.Count} langkah", P.SurfaceAlt, P.TextSecondary, Icons.ListChecks);
            UIKit.FlexSpacer(meta);
            NarrationControl(hero, ScreenNarration, null, "Dengarkan ringkasan");

            // Objectives
            UIKit.SectionHeader(content, "Tujuan pembelajaran", "Setelah modul ini, kamu dapat:");
            var obj = UIKit.Card(content, spacing: DesignTokens.Dp(12));
            for (int i = 0; i < module.learningObjective.Count; i++) UIKit.NumberedItem(obj, i + 1, module.learningObjective[i]);

            // Sequence
            UIKit.SectionHeader(content, "Urutan aktivitas");
            var seq = UIKit.Card(content, spacing: DesignTokens.Dp(4));
            for (int i = 0; i < module.lessonSteps.Count; i++)
            {
                var step = module.lessonSteps[i];
                bool done = Saved.IsStepCompleted(module, i);
                var r = UIKit.HStack(seq, DesignTokens.Dp(12), 0, DesignTokens.Dp(8));
                UIKit.IconTile(r, step.type.IconName(), done ? P.SuccessSoft : P.SurfaceAlt, done ? P.Success : P.TextSecondary,
                    DesignTokens.Dp(40));
                var t = UIKit.Flex(UIKit.VStack(r, DesignTokens.Dp(2)));
                UIKit.Label(t, $"Langkah {i + 1}", TextStyle.Overline, ColorRole.TextSecondary);
                UIKit.Label(t, StepTitle(step), TextStyle.BodyStrong);
                var s = UIKit.VStack(r, 0, align: TextAnchor.MiddleCenter);
                s.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childForceExpandWidth = false;
                UIKit.Icon(s, done ? Icons.CheckCircle : Icons.Chevron, DesignTokens.IconSizeSmall, done ? ColorRole.Success : ColorRole.TextDisabled);
                UIKit.Label(s, done ? "Selesai" : "", TextStyle.Overline, ColorRole.Success, TextAnchor.MiddleCenter);
                if (i < module.lessonSteps.Count - 1) UIKit.Divider(seq);
            }

            // Related hardware
            if (module.hardware.Count > 0)
            {
                UIKit.SectionHeader(content, "Perangkat terkait", "Ketuk untuk melihat penjelasan singkat.");
                var perRow = TextSizeController.IsLarge ? 1 : 2;
                RectTransform hr = null;
                for (int i = 0; i < module.hardware.Count; i++)
                {
                    if (i % perRow == 0) hr = UIKit.EqualRow(content, DesignTokens.Space1);
                    var h = module.hardware[i];
                    var chip = UIKit.Chip(hr, h.hardwareName, false, () => HardwareInfoSheet.Show(h, AppConstants.Scenes.ModuleDetail), h.iconName);
                    UIKit.Layout(chip, flexibleWidth: 1);
                }
                if (module.hardware.Count % perRow != 0) UIKit.Layout(UIKit.Rect(hr, "Pad"), flexibleWidth: 1);
            }

            UIKit.Button(content, "Atur tampilan & audio", () => Go(AppConstants.Scenes.Accessibility), ButtonVariant.Ghost,
                Icons.Accessibility, compact: true);
        }

        private static string StepTitle(LessonStep step) => step.type switch
        {
            LessonStepType.AR when step.hardware != null => $"Eksplorasi AR: {step.hardware.hardwareName}",
            LessonStepType.Scenario when step.scenario != null => $"Skenario: {step.scenario.title}",
            _ => step.type.DisplayName()
        };

        protected override void BuildFooter(RectTransform footer)
        {
            if (module == null) return;
            var state = Saved.StateOf(module);
            if (state == ModuleState.Locked)
            {
                UIKit.Button(footer, "Modul masih terkunci", null, ButtonVariant.Primary, Icons.Lock, ButtonState.Disabled);
                return;
            }
            int next = Saved.FirstIncompleteStep(module);
            string label = state switch
            {
                ModuleState.Completed => "Ulangi Modul",
                ModuleState.InProgress => $"Lanjutkan Belajar (Langkah {next + 1})",
                _ => "Mulai Belajar"
            };
            UIKit.Button(footer, label, () => Learning.StartModule(module, state == ModuleState.Completed ? 0 : (int?)null),
                ButtonVariant.Primary, state == ModuleState.Completed ? Icons.Refresh : Icons.Play);
        }
    }
}
