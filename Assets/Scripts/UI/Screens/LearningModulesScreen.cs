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
        protected override string Title => Loc.T("Modul Pembelajaran", "Learning Modules");

        protected override string ScreenNarration =>
            Loc.T($"Daftar modul pembelajaran. Ada {Content.Modules.Count} modul. Modul berikutnya terbuka setelah kamu memulai modul sebelumnya.",
                  $"Learning modules. There are {Content.Modules.Count} modules. The next module unlocks after you start the previous one.");

        protected override string GuidanceText => Loc.T("Pilih modul yang bertanda \"Tersedia\" atau \"Sedang berjalan\". Modul terkunci akan terbuka berurutan.",
            "Choose a module marked \"Available\" or \"In progress\". Locked modules open in order.");

        protected override void BuildContent(RectTransform content)
        {
            UIKit.SectionHeader(content, Loc.T($"{Learning.Modules.CompletedCount} dari {Content.Modules.Count} modul selesai", $"{Learning.Modules.CompletedCount} of {Content.Modules.Count} modules completed"),
                Loc.T("Ikuti modul secara berurutan untuk hasil terbaik.", "Follow the modules in order for the best results."));
            foreach (var m in Content.Modules) ModuleCard(content, m);
        }

        public static (string text, ColorRole fg, Color bg, string icon) StateStyle(ModuleState s)
        {
            var p = ContrastController.Current;
            return s switch
            {
                ModuleState.Locked => (Loc.T("Terkunci", "Locked"), ColorRole.TextSecondary, p.SurfaceAlt, Icons.Lock),
                ModuleState.InProgress => (Loc.T("Sedang berjalan", "In progress"), ColorRole.Warning, p.WarningSoft, Icons.Clock),
                ModuleState.Completed => (Loc.T("Selesai", "Completed"), ColorRole.Success, p.SuccessSoft, Icons.CheckCircle),
                _ => (Loc.T("Tersedia", "Available"), ColorRole.Primary, p.PrimarySoft, Icons.Play)
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
            UIKit.Label(col, Loc.T("Modul", "Module") + $" {m.order}", TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(col, m.Title, TextStyle.Heading, locked ? ColorRole.TextSecondary : ColorRole.TextPrimary);
            UIKit.Label(col, m.Description, TextStyle.Body, ColorRole.TextSecondary);

            var meta = UIKit.HStack(card, DesignTokens.Space1);
            var st = StateStyle(state);
            UIKit.Badge(meta, st.text, st.bg, P.Get(st.fg), st.icon);
            UIKit.Badge(meta, $"{m.estimatedMinutes} " + Loc.T("menit", "min"), P.SurfaceAlt, P.TextSecondary, Icons.Clock);
            UIKit.FlexSpacer(meta);

            if (!locked)
            {
                int done = Saved.CompletedStepCount(m);
                UIKit.ProgressRow(card, Loc.T("Progres", "Progress"), $"{done}/{m.lessonSteps.Count} " + Loc.T("langkah", "steps"), Saved.ModuleProgress01(m),
                    state == ModuleState.Completed ? P.Success : P.Primary);
                string label = state switch
                {
                    ModuleState.Completed => Loc.T("Lihat & ulangi", "Review & repeat"),
                    ModuleState.InProgress => Loc.T("Lanjutkan", "Continue"),
                    _ => Loc.T("Mulai", "Start")
                };
                UIKit.Button(card, label, Open, state == ModuleState.Completed ? ButtonVariant.Secondary : ButtonVariant.Primary,
                    Icons.Forward, iconRight: true);
            }
            else
            {
                UIKit.Label(card, Loc.T($"Mulai Modul {m.order - 1} terlebih dahulu untuk membuka modul ini.", $"Start Module {m.order - 1} first to unlock this module."), TextStyle.Caption,
                    ColorRole.TextSecondary);
            }
        }
    }
}
