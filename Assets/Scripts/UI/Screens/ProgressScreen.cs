using System.Linq;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 11_Progress — module completion, AR activities, scenarios, practice performance, reflections and the
    /// last accessed module, using simple progress indicators (blueprint §4.13, §13).
    /// </summary>
    public class ProgressScreen : ScreenBase
    {
        protected override string Title => Loc.T("Progres Belajar", "Learning Progress");

        protected override string ScreenNarration
        {
            get
            {
                var d = Saved.Data;
                var (c, a, _) = Saved.OverallQuizScore();
                return Loc.T(
                    $"Progres belajar. Modul selesai {d.completedModules.Count} dari {Content.Modules.Count}. " +
                    $"Aktivitas AR {d.completedARActivities.Count} dari {Content.ARHardware.Count()}. " +
                    $"Skenario {d.completedScenarios.Count} dari {Content.Scenarios.Count}. " +
                    (a > 0 ? $"Latihan: {c} dari {a} jawaban benar. " : "Belum ada latihan. ") +
                    $"Refleksi: {d.reflections.Count}.",
                    $"Learning progress. Modules completed {d.completedModules.Count} of {Content.Modules.Count}. " +
                    $"AR activities {d.completedARActivities.Count} of {Content.ARHardware.Count()}. " +
                    $"Scenarios {d.completedScenarios.Count} of {Content.Scenarios.Count}. " +
                    (a > 0 ? $"Practice: {c} of {a} answers correct. " : "No practice yet. ") +
                    $"Reflections: {d.reflections.Count}.");
            }
        }

        private string celebrate;

        protected override void OnOpened()
        {
            celebrate = State.JustCompletedModuleId;
            State.JustCompletedModuleId = null;
        }

        protected override void BuildContent(RectTransform content)
        {
            var d = Saved.Data;

            if (!string.IsNullOrEmpty(celebrate))
            {
                var m = Content.GetModule(celebrate);
                UIKit.Callout(content, Icons.Trophy, Loc.T("Modul selesai!", "Module complete!"),
                    m != null ? Loc.T($"Selamat, kamu menyelesaikan Modul {m.order}: {m.Title}.", $"Congratulations, you finished Module {m.order}: {m.Title}.") : Loc.T("Selamat!", "Congratulations!"), ColorRole.Success, ColorRole.SuccessSoft);
            }

            // Summary tiles
            var (qc, qa, qt) = Saved.OverallQuizScore();
            int arTotal = Content.ARHardware.Count();
            var tiles = new (string label, string value, float v, string icon)[]
            {
                (Loc.T("Modul selesai", "Modules done"), $"{d.completedModules.Count}/{Content.Modules.Count}", Ratio(d.completedModules.Count, Content.Modules.Count), Icons.Book),
                (Loc.T("Aktivitas AR", "AR activities"), $"{d.completedARActivities.Count}/{arTotal}", Ratio(d.completedARActivities.Count, arTotal), Icons.Scan),
                (Loc.T("Skenario", "Scenarios"), $"{d.completedScenarios.Count}/{Content.Scenarios.Count}", Ratio(d.completedScenarios.Count, Content.Scenarios.Count), Icons.Globe),
                (Loc.T("Latihan benar", "Practice correct"), qa == 0 ? "–" : $"{Mathf.RoundToInt(100f * qc / qa)}%", Ratio(qc, Mathf.Max(1, qt)), Icons.Pencil),
            };
            int perRow = TextSizeController.IsLarge ? 1 : 2;
            RectTransform row = null;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (i % perRow == 0) row = UIKit.EqualRow(content, DesignTokens.Space2);
                var t = tiles[i];
                var card = UIKit.Card(row, spacing: DesignTokens.Dp(8));
                var head = UIKit.HStack(card, DesignTokens.Space1);
                UIKit.Icon(head, t.icon, DesignTokens.IconSizeSmall, ColorRole.Primary);
                UIKit.Flex(UIKit.Label(head, t.label, TextStyle.Label, ColorRole.TextSecondary));
                UIKit.Label(card, t.value, TextStyle.Display);
                UIKit.ProgressBar(card, t.v);
            }

            // Reflection & last accessed
            var info = UIKit.Card(content, spacing: DesignTokens.Dp(10));
            var r1 = UIKit.HStack(info, DesignTokens.Dp(12));
            UIKit.Icon(r1, d.ReflectionCompleted ? Icons.CheckCircle : Icons.Message, DesignTokens.IconSize,
                d.ReflectionCompleted ? ColorRole.Success : ColorRole.TextSecondary);
            UIKit.Flex(UIKit.Label(r1, d.ReflectionCompleted ? Loc.T("Refleksi ditulis", "Reflections written") + $": {d.reflections.Count}" : Loc.T("Belum ada refleksi", "No reflections yet"), TextStyle.BodyStrong));
            UIKit.Divider(info);
            var last = Content.GetModule(d.lastAccessedModule);
            var r2 = UIKit.HStack(info, DesignTokens.Dp(12));
            UIKit.Icon(r2, Icons.Clock, DesignTokens.IconSize, ColorRole.TextSecondary);
            var lc = UIKit.Flex(UIKit.VStack(r2, DesignTokens.Dp(2)));
            UIKit.Label(lc, Loc.T("Terakhir diakses", "Last accessed"), TextStyle.Label, ColorRole.TextSecondary);
            UIKit.Label(lc, last != null ? $"{Loc.T("Modul", "Module")} {last.order}: {last.Title}" : Loc.T("Belum ada", "None yet"), TextStyle.BodyStrong);
            if (!string.IsNullOrEmpty(d.lastAccessedTime)) UIKit.Label(lc, d.lastAccessedTime, TextStyle.Caption, ColorRole.TextSecondary);
            if (last != null && Saved.StateOf(last) != ModuleState.Completed)
                UIKit.Button(info, Loc.T("Lanjutkan modul ini", "Continue this module"), () =>
                {
                    Learning.Modules.Select(last);
                    Go(AppConstants.Scenes.ModuleDetail);
                }, ButtonVariant.Primary, Icons.Play);

            // Per module
            UIKit.SectionHeader(content, Loc.T("Per modul", "By module"));
            foreach (var m in Content.Modules)
            {
                var card = UIKit.Card(content, spacing: DesignTokens.Dp(8));
                var head = UIKit.HStack(card, DesignTokens.Space1);
                UIKit.Flex(UIKit.Label(head, $"{Loc.T("Modul", "Module")} {m.order}: {m.Title}", TextStyle.BodyStrong));
                var st = LearningModulesScreen.StateStyle(Saved.StateOf(m));
                UIKit.Badge(card, st.text, st.bg, P.Get(st.fg), st.icon);
                UIKit.ProgressRow(card, Loc.T("Langkah", "Steps"), $"{Saved.CompletedStepCount(m)}/{m.lessonSteps.Count}", Saved.ModuleProgress01(m));
                var (c, a, total) = Saved.QuizScore(m);
                UIKit.ProgressRow(card, Loc.T("Latihan", "Practice"), a == 0 ? Loc.T("belum dikerjakan", "not started") : $"{c}/{total} " + Loc.T("benar", "correct"), Ratio(c, total), P.Success);
            }

            UIKit.Spacer(content, DesignTokens.Space2);
            UIKit.Button(content, Loc.T("Hapus semua progres", "Delete all progress"), () => ModalController.Instance.Confirm(Loc.T("Hapus semua progres?", "Delete all progress?"),
                Loc.T("Semua modul, skor latihan, dan refleksi akan dihapus dari perangkat ini. Tindakan ini tidak dapat dibatalkan.", "All modules, practice scores and reflections will be deleted from this device. This cannot be undone."),
                Loc.T("Hapus", "Delete"), () =>
                {
                    Saved.ResetAll();
                    Learning.Lesson.Exit();
                    Quiz.QuizManager.End();
                    PopupController.Instance.Toast(Loc.T("Progres dihapus", "Progress deleted"), Icons.Info);
                    RenderFromTop();
                }, Icons.Alert, destructive: true), ButtonVariant.Ghost, Icons.Close, compact: true);
        }

        private static float Ratio(int a, int b) => b <= 0 ? 0f : Mathf.Clamp01((float)a / b);

        protected override void BuildFooter(RectTransform footer)
        {
            UIKit.Button(footer, Loc.T("Kembali ke menu utama", "Back to main menu"), Nav.Home, ButtonVariant.Primary, Icons.Home);
        }
    }
}
