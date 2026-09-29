using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>13_Help — step-by-step tutorial: scanning, moving the device, hotspots, audio, tracking recovery.</summary>
    public class HelpScreen : ScreenBase
    {
        protected override string Title => Loc.T("Bantuan & Tutorial", "Help & Tutorial");

        protected override string ScreenNarration => Loc.T(
            "Bantuan. Enam langkah menggunakan AR: siapkan kartu target, buka AR Explorer, arahkan kamera, ketuk titik oranye, " +
            "dengarkan penjelasan, dan jika target hilang, perbaiki pencahayaan dan jarak.",
            "Help. Six steps for using AR: prepare a target card, open AR Explorer, point the camera, tap the orange dots, " +
            "listen to the explanation, and if the target is lost, improve the lighting and distance.");

        private static (string icon, string title, string body)[] Steps => new[]
        {
            (Icons.Printer, Loc.T("Siapkan kartu target", "Prepare a target card"),
                Loc.T("Cetak kartu target (lebar 15 cm) dari guru, atau tampilkan di layar lain. Letakkan di meja yang terang dan datar.",
                      "Print a target card (15 cm wide) from your teacher, or show it on another screen. Put it on a bright, flat table.")),
            (Icons.Scan, Loc.T("Buka AR Explorer", "Open AR Explorer"),
                Loc.T("Dari menu utama pilih AR Explorer, atau ikuti modul. Izinkan akses kamera jika diminta.",
                      "Choose AR Explorer from the main menu, or follow a module. Allow camera access if asked.")),
            (Icons.Phone, Loc.T("Arahkan & gerakkan perlahan", "Point and move slowly"),
                Loc.T("Pegang perangkat 20–40 cm di atas kartu. Seluruh kartu harus terlihat di dalam bingkai. Tahan stabil sekitar 2 detik.",
                      "Hold the device 20–40 cm above the card. The whole card must be inside the frame. Hold steady for about 2 seconds.")),
            (Icons.Hand, Loc.T("Ketuk titik oranye", "Tap the orange dots"),
                Loc.T("Setelah model 3D muncul, ketuk titik oranye untuk membuka penjelasan bagian. Kamu juga bisa memilih bagian dari daftar. Geser layar untuk memutar model.",
                      "When the 3D model appears, tap an orange dot to open the explanation of that part. You can also pick parts from the list. Swipe to rotate the model.")),
            (Icons.Volume, Loc.T("Gunakan audio", "Use audio"),
                Loc.T("Tekan \"Dengarkan\" untuk narasi. Tekan lagi untuk jeda, dan ikon putar ulang untuk mengulang.",
                      "Press \"Listen\" for narration. Press again to pause, and the replay icon to start over.")),
            (Icons.Refresh, Loc.T("Jika target hilang", "If the target is lost"),
                Loc.T("Tambah cahaya, hindari pantulan, dekatkan atau jauhkan perangkat, dan pastikan kartu tidak tertutup jari. Tekan Reset untuk mengembalikan model.",
                      "Add light, avoid reflections, move the device closer or further, and keep your fingers off the card. Press Reset to restore the model.")),
            (Icons.Globe, Loc.T("Ganti bahasa", "Change language"),
                Loc.T("Ketuk ikon bendera di bagian atas layar, atau buka Pengaturan, untuk beralih antara Bahasa Indonesia dan English.",
                      "Tap the flag icon at the top of the screen, or open Settings, to switch between English and Bahasa Indonesia.")),
        };

        private static (string q, string a)[] Faq => new[]
        {
            (Loc.T("Kamera tidak menyala?", "The camera doesn't start?"),
                Loc.T("Buka Pengaturan perangkat › Aplikasi › Computer Explorer › Izin, lalu izinkan Kamera. Jika tetap tidak bisa, gunakan Mode Simulasi di layar AR.",
                      "Open device Settings › Apps › Computer Explorer › Permissions and allow Camera. If it still fails, use Simulation Mode on the AR screen.")),
            (Loc.T("Narasi tidak bersuara?", "No narration sound?"),
                Loc.T("Periksa volume perangkat dan pengaturan Narasi. Suara memerlukan mesin Text-to-Speech dengan paket bahasa yang dipilih (Indonesia atau Inggris).",
                      "Check the device volume and the Narration setting. Speech needs a Text-to-Speech engine with the chosen language pack (Indonesian or English).")),
            (Loc.T("Teks terlalu kecil atau sulit dibaca?", "Text too small or hard to read?"),
                Loc.T("Buka Pengaturan untuk memperbesar teks atau menyalakan kontras tinggi.",
                      "Open Settings to make the text larger or turn on high contrast.")),
            (Loc.T("Tidak bisa memakai kamera sama sekali?", "Can't use the camera at all?"),
                Loc.T("Semua materi tetap bisa dipelajari: gunakan Jelajah Hardware, diagram Von Neumann, dan tombol \"Baca info tanpa kamera\" di skenario.",
                      "You can still learn everything: use the Hardware Explorer, the Von Neumann diagram and the \"Read info without camera\" button in scenarios.")),
        };

        protected override void BuildContent(RectTransform content)
        {
            UIKit.SectionHeader(content, Loc.T("Cara menggunakan AR", "How to use AR"),
                Loc.T("Ikuti langkah berikut. Setiap langkah bisa dibacakan.", "Follow these steps. Every step can be read aloud."));
            var steps = Steps;
            for (int i = 0; i < steps.Length; i++)
            {
                var (icon, title, body) = steps[i];
                var card = UIKit.Card(content, spacing: DesignTokens.Dp(10));
                var row = UIKit.HStack(card, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
                UIKit.IconTile(row, icon, P.PrimarySoft, P.OnPrimarySoft);
                var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(4)));
                UIKit.Label(col, Loc.T("Langkah", "Step") + $" {i + 1}", TextStyle.Overline, ColorRole.TextSecondary);
                UIKit.Label(col, title, TextStyle.Heading);
                UIKit.Label(card, body, TextStyle.Body);
                NarrationControl(card, $"{Loc.T("Langkah", "Step")} {i + 1}. {title}. {body}");
            }

            UIKit.Button(content, Loc.T("Coba pindai sekarang", "Try scanning now"), () =>
            {
                State.SelectHardware(null);
                State.ARReturnScene = AppConstants.Scenes.Help;
                Go(AppConstants.Scenes.ARScanner);
            }, ButtonVariant.Primary, Icons.Scan);

            UIKit.SectionHeader(content, Loc.T("Pertanyaan umum", "Frequently asked questions"));
            foreach (var (q, a) in Faq)
            {
                var card = UIKit.Card(content, spacing: DesignTokens.Dp(6));
                UIKit.Label(card, q, TextStyle.BodyStrong);
                UIKit.Label(card, a, TextStyle.Body, ColorRole.TextSecondary);
            }

            UIKit.Button(content, Loc.T("Buka pengaturan", "Open settings"), () => Go(AppConstants.Scenes.Accessibility),
                ButtonVariant.Secondary, Icons.Accessibility);
        }
    }
}
