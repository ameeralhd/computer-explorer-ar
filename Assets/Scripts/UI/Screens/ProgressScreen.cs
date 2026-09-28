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
        protected override string Title => "Progres Belajar";

        protected override string ScreenNarration
        {
            get
            {
                var d = Saved.Data;
                var (c, a, _) = Saved.OverallQuizScore();
                return $"Progres belajar. Modul selesai {d.completedModules.Count} dari {Content.Modules.Count}. " +
                       $"Aktivitas AR {d.completedARActivities.Count} dari {Content.ARHardware.Count()}. " +
                       $"Skenario {d.completedScenarios.Count} dari {Content.Scenarios.Count}. " +
                       (a > 0 ? $"Latihan: {c} dari {a} jawaban benar. " : "Belum ada latihan. ") +
                       $"Refleksi: {d.reflections.Count}.";
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
                UIKit.Callout(content, Icons.Trophy, "Modul selesai!",
                    m != null ? $"Selamat, kamu menyelesaikan Modul {m.order}: {m.title}." : "Selamat!", ColorRole.Success, ColorRole.SuccessSoft);
            }

            // Summary tiles
            var (qc, qa, qt) = Saved.OverallQuizScore();
            int arTotal = Content.ARHardware.Count();
            var tiles = new (string label, string value, float v, string icon)[]
            {
                ("Modul selesai", $"{d.completedModules.Count}/{Content.Modules.Count}", Ratio(d.completedModules.Count, Content.Modules.Count), Icons.Book),
                ("Aktivitas AR", $"{d.completedARActivities.Count}/{arTotal}", Ratio(d.completedARActivities.Count, arTotal), Icons.Scan),
                ("Skenario", $"{d.completedScenarios.Count}/{Content.Scenarios.Count}", Ratio(d.completedScenarios.Count, Content.Scenarios.Count), Icons.Globe),
                ("Latihan benar", qa == 0 ? "–" : $"{Mathf.RoundToInt(100f * qc / qa)}%", Ratio(qc, Mathf.Max(1, qt)), Icons.Pencil),
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
            UIKit.Flex(UIKit.Label(r1, d.ReflectionCompleted ? $"Refleksi ditulis: {d.reflections.Count}" : "Belum ada refleksi", TextStyle.BodyStrong));
            UIKit.Divider(info);
            var last = Content.GetModule(d.lastAccessedModule);
            var r2 = UIKit.HStack(info, DesignTokens.Dp(12));
            UIKit.Icon(r2, Icons.Clock, DesignTokens.IconSize, ColorRole.TextSecondary);
            var lc = UIKit.Flex(UIKit.VStack(r2, DesignTokens.Dp(2)));
            UIKit.Label(lc, "Terakhir diakses", TextStyle.Label, ColorRole.TextSecondary);
            UIKit.Label(lc, last != null ? $"Modul {last.order}: {last.title}" : "Belum ada", TextStyle.BodyStrong);
            if (!string.IsNullOrEmpty(d.lastAccessedTime)) UIKit.Label(lc, d.lastAccessedTime, TextStyle.Caption, ColorRole.TextSecondary);
            if (last != null && Saved.StateOf(last) != ModuleState.Completed)
                UIKit.Button(info, "Lanjutkan modul ini", () =>
                {
                    Learning.Modules.Select(last);
                    Go(AppConstants.Scenes.ModuleDetail);
                }, ButtonVariant.Primary, Icons.Play);

            // Per module
            UIKit.SectionHeader(content, "Per modul");
            foreach (var m in Content.Modules)
            {
                var card = UIKit.Card(content, spacing: DesignTokens.Dp(8));
                var head = UIKit.HStack(card, DesignTokens.Space1);
                UIKit.Flex(UIKit.Label(head, $"Modul {m.order}: {m.title}", TextStyle.BodyStrong));
                var st = LearningModulesScreen.StateStyle(Saved.StateOf(m));
                UIKit.Badge(card, st.text, st.bg, P.Get(st.fg), st.icon);
                UIKit.ProgressRow(card, "Langkah", $"{Saved.CompletedStepCount(m)}/{m.lessonSteps.Count}", Saved.ModuleProgress01(m));
                var (c, a, total) = Saved.QuizScore(m);
                UIKit.ProgressRow(card, "Latihan", a == 0 ? "belum dikerjakan" : $"{c}/{total} benar", Ratio(c, total), P.Success);
            }

            UIKit.Spacer(content, DesignTokens.Space2);
            UIKit.Button(content, "Hapus semua progres", () => ModalController.Instance.Confirm("Hapus semua progres?",
                "Semua modul, skor latihan, dan refleksi akan dihapus dari perangkat ini. Tindakan ini tidak dapat dibatalkan.",
                "Hapus", () =>
                {
                    Saved.ResetAll();
                    Learning.Lesson.Exit();
                    Quiz.QuizManager.End();
                    PopupController.Instance.Toast("Progres dihapus", Icons.Info);
                    RenderFromTop();
                }, Icons.Alert, destructive: true), ButtonVariant.Ghost, Icons.Close, compact: true);
        }

        private static float Ratio(int a, int b) => b <= 0 ? 0f : Mathf.Clamp01((float)a / b);

        protected override void BuildFooter(RectTransform footer)
        {
            UIKit.Button(footer, "Kembali ke menu utama", Nav.Home, ButtonVariant.Primary, Icons.Home);
        }
    }
}
