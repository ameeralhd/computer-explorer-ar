using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 01_Welcome — language choice (Bahasa Indonesia / English), purpose, mascot, narration indicator,
    /// accessibility shortcut, Start.
    /// </summary>
    public class WelcomeScreen : ScreenBase
    {
        protected override bool ShowHeader => false;

        protected override string ScreenNarration => Loc.T(
            "Selamat datang di Computer Explorer. Di sini kamu akan belajar perangkat keras dan arsitektur komputer " +
            "dengan model tiga dimensi augmented reality, narasi, dan contoh dari kehidupan sehari-hari. " +
            "Pilih bahasa, lalu tekan tombol Mulai Belajar.",
            "Welcome to Computer Explorer. Here you will learn computer hardware and architecture with " +
            "three-dimensional augmented reality models, narration and everyday examples. " +
            "Choose your language, then press Start Learning.");

        protected override void BuildContent(RectTransform content)
        {
            // Top row: narration indicator + accessibility shortcut
            var top = UIKit.HStack(content, DesignTokens.Space1);
            bool narration = AccessibilityManager.Instance.Settings.narrationEnabled;
            UIKit.Badge(top, narration ? Loc.T("Narasi aktif", "Narration on") : Loc.T("Narasi mati", "Narration off"),
                P.PrimarySoft, P.OnPrimarySoft, narration ? Icons.Volume : Icons.VolumeOff);
            UIKit.FlexSpacer(top);
            UIKit.IconButton(top, Icons.Accessibility, Loc.T("Pengaturan aksesibilitas", "Accessibility settings"),
                () => Go(AppConstants.Scenes.Accessibility), ButtonVariant.Tonal);

            // Language choice — the label is shown in both languages so everyone can find it.
            var lang = UIKit.Card(content, spacing: DesignTokens.Dp(10));
            var head = UIKit.HStack(lang, DesignTokens.Space1);
            UIKit.Icon(head, Icons.Globe, DesignTokens.IconSize, ColorRole.Primary);
            UIKit.Flex(UIKit.Label(head, "Pilih bahasa / Choose language", TextStyle.BodyStrong));
            UIKit.LanguageSwitch(lang);

            // The layout owns the holder; the mascot floats inside it so animation never fights the layout.
            var holder = UIKit.Rect(content, "MascotHolder");
            UIKit.Layout(holder, minHeight: DesignTokens.Dp(170), preferredHeight: DesignTokens.Dp(170));
            var mascot = UIKit.Picture(holder, UIAssets.Image("mascot"), DesignTokens.Dp(170));
            mascot.GetComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
            UIKit.Stretch(mascot.rectTransform);
            mascot.gameObject.AddComponent<FloatAnimator>().amplitude = DesignTokens.Dp(4);

            UIKit.Label(content, Loc.T("Halo, aku Robi!", "Hi, I'm Robi!"), TextStyle.Label, ColorRole.Primary, TextAnchor.MiddleCenter);
            UIKit.Label(content, AppConstants.AppName, TextStyle.Display, ColorRole.TextPrimary, TextAnchor.MiddleCenter);
            UIKit.Label(content, Loc.T(
                    "Belajar perangkat keras dan arsitektur komputer dengan model 3D augmented reality, narasi, dan situasi nyata sehari-hari.",
                    "Learn computer hardware and architecture with 3D augmented reality models, narration and real-life situations."),
                TextStyle.Body, ColorRole.TextSecondary, TextAnchor.MiddleCenter);

            var features = UIKit.Card(content, spacing: DesignTokens.Dp(12));
            Feature(features, Icons.Scan, Loc.T("Jelajahi hardware 3D dalam AR", "Explore 3D hardware in AR"),
                Loc.T("Pindai kartu target untuk melihat CPU, RAM, dan lainnya.", "Scan target cards to see the CPU, RAM and more."));
            Feature(features, Icons.Volume, Loc.T("Dengarkan penjelasan", "Listen to explanations"),
                Loc.T("Setiap materi bisa dibacakan dalam Bahasa Indonesia atau English.", "Every lesson can be read aloud in English or Bahasa Indonesia."));
            Feature(features, Icons.Accessibility, Loc.T("Atur sesuai kebutuhanmu", "Adjust it to your needs"),
                Loc.T("Ukuran teks, kontras tinggi, dan gerakan minimal.", "Text size, high contrast and reduced motion."));

            NarrationControl(content, ScreenNarration, null, Loc.T("Dengarkan sambutan", "Listen to welcome"));
        }

        private static void Feature(Transform parent, string icon, string title, string body)
        {
            var row = UIKit.HStack(parent, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
            UIKit.IconTile(row, icon, P.PrimarySoft, P.OnPrimarySoft, DesignTokens.Dp(40));
            var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(2)));
            UIKit.Label(col, title, TextStyle.BodyStrong);
            UIKit.Label(col, body, TextStyle.Caption, ColorRole.TextSecondary);
        }

        protected override void BuildFooter(RectTransform footer)
        {
            bool returning = Saved.Data.hasSeenWelcome;
            UIKit.Button(footer, returning ? Loc.T("Lanjutkan", "Continue") : Loc.T("Mulai Belajar", "Start Learning"), () =>
            {
                Saved.MarkWelcomeSeen();
                Nav.Home();
            }, ButtonVariant.Primary, Icons.Forward, iconRight: true);
        }
    }
}
