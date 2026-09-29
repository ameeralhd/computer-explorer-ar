using System;
using System.Linq;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.UI
{
    /// <summary>02_MainMenu — central navigation hub (blueprint §4.3 / §4.17).</summary>
    public class MainMenuController : ScreenBase
    {
        protected override string Title => AppConstants.AppName;
        protected override bool ShowBack => false;

        protected override string ScreenNarration =>
            Loc.T("Menu utama. Pilih Mulai Belajar untuk mengikuti modul langkah demi langkah, AR Explorer untuk memindai " +
                  "kartu target, Hardware untuk melihat daftar perangkat, Konteks untuk situasi nyata, Latihan untuk menjawab soal, " +
                  "dan Progres untuk melihat hasil belajarmu.",
                  "Main menu. Choose Start Learning to follow a module step by step, AR Explorer to scan target cards, " +
                  "Hardware to browse the devices, Context for real-life situations, Practice to answer questions, " +
                  "and Progress to see your results.");

        protected override string GuidanceText =>
            Loc.T("Mulailah dari \"Mulai Belajar\". Modul 1 akan membimbingmu langkah demi langkah, dari diagram Von Neumann " +
                  "sampai eksplorasi CPU dalam AR.",
                  "Begin with \"Start Learning\". Module 1 guides you step by step, from the Von Neumann diagram " +
                  "to exploring the CPU in AR.");

        protected override void OnOpened()
        {
            // Returning to the hub pauses any guided lesson; it can be resumed from the card below.
            Learning.Lesson.Pause();
        }

        protected override void BuildContent(RectTransform content)
        {
            // Greeting
            var hello = UIKit.HStack(content, DesignTokens.Space2);
            var texts = UIKit.Flex(UIKit.VStack(hello, DesignTokens.Dp(4)));
            UIKit.Label(texts, Loc.T("Halo!", "Hello!"), TextStyle.Display);
            UIKit.Label(texts, Loc.T("Mari belajar tentang perangkat komputer.", "Let's learn about computer hardware."), TextStyle.Body, ColorRole.TextSecondary);
            var mascot = UIKit.Picture(hello, UIAssets.Image("mascot"), DesignTokens.Dp(88));
            UIKit.Layout(mascot, minWidth: DesignTokens.Dp(88), preferredWidth: DesignTokens.Dp(88), flexibleWidth: 0);

            // Language: two flag buttons, always one tap away.
            UIKit.LanguageSwitch(content);

            BuildContinueCard(content);

            // Primary navigation grid (2 columns; 1 column at large text sizes so labels never clip)
            UIKit.SectionHeader(content, Loc.T("Pilih aktivitas", "Choose an activity"));
            var items = new (string title, string caption, string icon, Color color, Action action)[]
            {
                (Loc.T("Mulai Belajar", "Start Learning"), ModulesCaption(), Icons.Book, P.Category(HardwareCategory.Processing), () => Go(AppConstants.Scenes.LearningModules)),
                ("AR Explorer", Loc.T("Pindai kartu target", "Scan target cards"), Icons.Scan, P.Category(HardwareCategory.Input), () =>
                {
                    State.SelectHardware(null);
                    State.ARReturnScene = null;
                    Go(AppConstants.Scenes.ARScanner);
                }),
                ("Hardware", Loc.T($"{Content.Hardware.Count} perangkat", $"{Content.Hardware.Count} devices"), Icons.Monitor, P.Category(HardwareCategory.Output), () =>
                {
                    State.HardwareExplorerFilter = null;
                    Go(AppConstants.Scenes.HardwareExplorer);
                }),
                (Loc.T("Konteks", "Context"), Loc.T("Situasi nyata", "Real-life situations"), Icons.Globe, P.Category(HardwareCategory.Storage), () =>
                {
                    State.ActiveScenario = null;
                    Go(AppConstants.Scenes.ContextualLearning);
                }),
                (Loc.T("Latihan", "Practice"), Loc.T("Uji pemahaman", "Test your understanding"), Icons.Pencil, P.Category(HardwareCategory.Memory), () => Go(AppConstants.Scenes.Practice)),
                (Loc.T("Progres", "Progress"), ProgressCaption(), Icons.Chart, P.Category(HardwareCategory.Architecture), () => Go(AppConstants.Scenes.Progress)),
            };
            int perRow = TextSizeController.IsLarge ? 1 : 2;
            RectTransform row = null;
            for (int i = 0; i < items.Length; i++)
            {
                if (i % perRow == 0) row = UIKit.EqualRow(content, DesignTokens.Space2);
                var it = items[i];
                MenuCard(row, it.title, it.caption, it.icon, it.color, it.action);
            }

            // Architecture shortcut (full width)
            var vn = UIKit.Card(content, () => Go(AppConstants.Scenes.VonNeumann));
            var vnRow = UIKit.HStack(vn, DesignTokens.Dp(12));
            UIKit.IconTile(vnRow, Icons.Workflow, P.PrimarySoft, P.OnPrimarySoft);
            var vnText = UIKit.Flex(UIKit.VStack(vnRow, DesignTokens.Dp(2)));
            UIKit.Label(vnText, Loc.T("Arsitektur Von Neumann", "Von Neumann Architecture"), TextStyle.BodyStrong);
            UIKit.Label(vnText, Loc.T("Lihat diagram dan alur data antar komponen", "See the diagram and how data flows between parts"), TextStyle.Caption, ColorRole.TextSecondary);
            UIKit.Icon(vnRow, Icons.Chevron, DesignTokens.IconSize, ColorRole.TextSecondary);

            // Secondary area
            UIKit.Spacer(content, DesignTokens.Space1);
            var sec = TextSizeController.IsLarge ? UIKit.VStack(content, DesignTokens.Space1) : UIKit.EqualRow(content, DesignTokens.Space1);
            UIKit.Button(sec, Loc.T("Bantuan", "Help"), () => Go(AppConstants.Scenes.Help), ButtonVariant.Secondary, Icons.Help, compact: true);
            UIKit.Button(sec, Loc.T("Panduan Guru", "Teacher Guide"), () => Go(AppConstants.Scenes.TeacherGuide), ButtonVariant.Secondary, Icons.Teacher, compact: true);
        }

        private void BuildContinueCard(RectTransform content)
        {
            var lesson = Learning.Lesson;
            var active = lesson.ActiveModule;
            if (active != null && lesson.CurrentStep != null)
            {
                var card = UIKit.Card(content, fill: ColorRole.PrimarySoft, borderOverride: P.Primary);
                UIKit.Label(card, Loc.T("Lanjutkan pelajaran", "Continue lesson"), TextStyle.Overline, ColorRole.OnPrimarySoft);
                UIKit.Label(card, active.Title, TextStyle.Heading, ColorRole.OnPrimarySoft);
                UIKit.Label(card, $"{lesson.StepLabel} · {lesson.CurrentStep.type.DisplayName()}", TextStyle.Body, ColorRole.OnPrimarySoft);
                UIKit.ProgressBar(card, (float)lesson.StepIndex / Mathf.Max(1, active.lessonSteps.Count));
                UIKit.Button(card, Loc.T("Lanjutkan", "Continue"), Learning.ContinueLesson, ButtonVariant.Primary, Icons.Play);
                return;
            }

            var rec = Learning.Modules.Recommended;
            if (rec == null) return;
            var state = Saved.StateOf(rec);
            var c2 = UIKit.Card(content);
            UIKit.Label(c2, state == ModuleState.InProgress ? Loc.T("Lanjutkan modul", "Continue module") : Loc.T("Rekomendasi untukmu", "Recommended for you"), TextStyle.Overline, ColorRole.TextSecondary);
            var r = UIKit.HStack(c2, DesignTokens.Dp(12));
            UIKit.IconTile(r, rec.iconName, P.Category(HardwareCategory.Processing), P.OnCategory);
            var t = UIKit.Flex(UIKit.VStack(r, DesignTokens.Dp(2)));
            UIKit.Label(t, $"{Loc.T("Modul", "Module")} {rec.order}: {rec.Title}", TextStyle.BodyStrong);
            UIKit.Label(t, rec.Description, TextStyle.Caption, ColorRole.TextSecondary);
            UIKit.ProgressBar(c2, Saved.ModuleProgress01(rec));
            UIKit.Button(c2, state == ModuleState.InProgress ? Loc.T("Lanjutkan", "Continue") : Loc.T("Mulai Modul", "Start Module"), () =>
            {
                Learning.Modules.Select(rec);
                Go(AppConstants.Scenes.ModuleDetail);
            }, ButtonVariant.Primary, Icons.Forward, iconRight: true);
        }

        private void MenuCard(Transform row, string title, string caption, string icon, Color color, Action onClick)
        {
            var card = UIKit.Card(row, onClick, spacing: DesignTokens.Dp(12), name: "MenuCard " + title);
            // Row wrapper keeps the tile at its fixed size, left-aligned, inside the vertical card.
            var tileRow = UIKit.HStack(card, 0);
            UIKit.IconTile(tileRow, icon, color, P.OnCategory);
            UIKit.Label(card, title, TextStyle.Heading);
            UIKit.Label(card, caption, TextStyle.Caption, ColorRole.TextSecondary);
        }

        private string ModulesCaption()
        {
            int done = Learning.Modules.CompletedCount;
            return Loc.T($"{done}/{Content.Modules.Count} modul selesai", $"{done}/{Content.Modules.Count} modules done");
        }

        private string ProgressCaption()
        {
            var (correct, answered, _) = Saved.OverallQuizScore();
            return answered == 0 ? Loc.T("Belum ada skor", "No score yet") : Loc.T("Skor latihan", "Practice score") + $" {Mathf.RoundToInt(100f * correct / answered)}%";
        }
    }
}
