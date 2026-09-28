using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>03_LearningModules — structured lessons with number, description, thumbnail, state and action.</summary>
    public class LearningModulesScreen : ScreenBase
    {
        protected override string Title => "Modul Pembelajaran";

        protected override string ScreenNarration =>
            $"Daftar modul pembelajaran. Ada {Content.Modules.Count} modul. Modul berikutnya terbuka setelah kamu memulai modul sebelumnya.";

        protected override string GuidanceText => "Pilih modul yang bertanda \"Tersedia\" atau \"Sedang berjalan\". Modul terkunci akan terbuka berurutan.";

        protected override void BuildContent(RectTransform content)
        {
            UIKit.SectionHeader(content, $"{Learning.Modules.CompletedCount} dari {Content.Modules.Count} modul selesai",
                "Ikuti modul secara berurutan untuk hasil terbaik.");
            foreach (var m in Content.Modules) ModuleCard(content, m);
        }

        public static (string text, ColorRole fg, Color bg, string icon) StateStyle(ModuleState s)
        {
            var p = ContrastController.Current;
            return s switch
            {
                ModuleState.Locked => ("Terkunci", ColorRole.TextSecondary, p.SurfaceAlt, Icons.Lock),
                ModuleState.InProgress => ("Sedang berjalan", ColorRole.Warning, p.WarningSoft, Icons.Clock),
                ModuleState.Completed => ("Selesai", ColorRole.Success, p.SuccessSoft, Icons.CheckCircle),
                _ => ("Tersedia", ColorRole.Primary, p.PrimarySoft, Icons.Play)
            };
        }

        private void ModuleCard(Transform parent, ModuleData m)
        {
            var state = Saved.StateOf(m);
            bool locked = state == ModuleState.Locked;
            void Open()
            {
                Learning.Modules.Select(m);
                Go(AppConstants.Scenes.ModuleDetail);
            }

            var card = UIKit.Card(parent, locked ? (System.Action)null : Open, spacing: DesignTokens.Dp(12), name: "Module " + m.moduleId);
            var top = UIKit.HStack(card, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
            UIKit.IconTile(top, locked ? Icons.Lock : m.iconName, locked ? P.SurfaceAlt : P.Category(HardwareCategory.Processing),
                locked ? P.TextSecondary : P.OnCategory, DesignTokens.Dp(56));
            var col = UIKit.Flex(UIKit.VStack(top, DesignTokens.Dp(4)));
            UIKit.Label(col, $"Modul {m.order}", TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(col, m.title, TextStyle.Heading, locked ? ColorRole.TextSecondary : ColorRole.TextPrimary);
            UIKit.Label(col, m.description, TextStyle.Body, ColorRole.TextSecondary);

            var meta = UIKit.HStack(card, DesignTokens.Space1);
            var st = StateStyle(state);
            UIKit.Badge(meta, st.text, st.bg, P.Get(st.fg), st.icon);
            UIKit.Badge(meta, $"{m.estimatedMinutes} menit", P.SurfaceAlt, P.TextSecondary, Icons.Clock);
            UIKit.FlexSpacer(meta);

            if (!locked)
            {
                int done = Saved.CompletedStepCount(m);
                UIKit.ProgressRow(card, "Progres", $"{done}/{m.lessonSteps.Count} langkah", Saved.ModuleProgress01(m),
                    state == ModuleState.Completed ? P.Success : P.Primary);
                string label = state switch
                {
                    ModuleState.Completed => "Lihat & ulangi",
                    ModuleState.InProgress => "Lanjutkan",
                    _ => "Mulai"
                };
                UIKit.Button(card, label, Open, state == ModuleState.Completed ? ButtonVariant.Secondary : ButtonVariant.Primary,
                    Icons.Forward, iconRight: true);
            }
            else
            {
                UIKit.Label(card, $"Mulai Modul {m.order - 1} terlebih dahulu untuk membuka modul ini.", TextStyle.Caption,
                    ColorRole.TextSecondary);
            }
        }
    }
}
