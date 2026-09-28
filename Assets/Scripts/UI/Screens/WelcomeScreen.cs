using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>01_Welcome — purpose, mascot, language/audio indicator, accessibility shortcut, Start.</summary>
    public class WelcomeScreen : ScreenBase
    {
        protected override bool ShowHeader => false;

        protected override string ScreenNarration =>
            "Selamat datang di Computer Explorer. Di sini kamu akan belajar perangkat keras dan arsitektur komputer " +
            "dengan model tiga dimensi augmented reality, narasi berbahasa Indonesia, dan contoh dari kehidupan sehari-hari. " +
            "Tekan tombol Mulai Belajar untuk melanjutkan, atau atur aksesibilitas terlebih dahulu.";

        protected override void BuildContent(RectTransform content)
        {
            // Top row: language/audio indicator + accessibility shortcut
            var top = UIKit.HStack(content, DesignTokens.Space1);
            UIKit.Badge(top, AppConstants.NarrationLanguage + (AccessibilityManager.Instance.Settings.narrationEnabled ? " · Narasi aktif" : " · Narasi mati"),
                P.PrimarySoft, P.OnPrimarySoft, AccessibilityManager.Instance.Settings.narrationEnabled ? Icons.Volume : Icons.VolumeOff);
            UIKit.FlexSpacer(top);
            UIKit.IconButton(top, Icons.Accessibility, "Pengaturan aksesibilitas", () => Go(AppConstants.Scenes.Accessibility),
                ButtonVariant.Tonal);

            // The layout owns the holder; the mascot floats inside it so animation never fights the layout.
            var holder = UIKit.Rect(content, "MascotHolder");
            UIKit.Layout(holder, minHeight: DesignTokens.Dp(200), preferredHeight: DesignTokens.Dp(200));
            var mascot = UIKit.Picture(holder, UIAssets.Image("mascot"), DesignTokens.Dp(200));
            mascot.GetComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
            UIKit.Stretch(mascot.rectTransform);
            mascot.gameObject.AddComponent<FloatAnimator>().amplitude = DesignTokens.Dp(4);

            UIKit.Label(content, "Halo, aku Robi!", TextStyle.Label, ColorRole.Primary, TextAnchor.MiddleCenter);
            UIKit.Label(content, AppConstants.AppName, TextStyle.Display, ColorRole.TextPrimary, TextAnchor.MiddleCenter);
            UIKit.Label(content,
                "Belajar perangkat keras dan arsitektur komputer dengan model 3D augmented reality, narasi Bahasa Indonesia, " +
                "dan situasi nyata sehari-hari.",
                TextStyle.Body, ColorRole.TextSecondary, TextAnchor.MiddleCenter);

            var features = UIKit.Card(content, spacing: DesignTokens.Dp(12));
            Feature(features, Icons.Scan, "Jelajahi hardware 3D dalam AR", "Pindai kartu target untuk melihat CPU, RAM, dan lainnya.");
            Feature(features, Icons.Volume, "Dengarkan penjelasan", "Setiap materi bisa dibacakan dalam Bahasa Indonesia.");
            Feature(features, Icons.Accessibility, "Atur sesuai kebutuhanmu", "Ukuran teks, kontras tinggi, dan gerakan minimal.");

            NarrationControl(content, ScreenNarration, null, "Dengarkan sambutan");
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
            UIKit.Button(footer, returning ? "Lanjutkan" : "Mulai Belajar", () =>
            {
                Saved.MarkWelcomeSeen();
                Nav.Home();
            }, ButtonVariant.Primary, Icons.Forward, iconRight: true);
        }
    }
}
