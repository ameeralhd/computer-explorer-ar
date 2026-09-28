using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>13_Help — step-by-step tutorial: scanning, moving the device, hotspots, audio, tracking recovery.</summary>
    public class HelpScreen : ScreenBase
    {
        protected override string Title => "Bantuan & Tutorial";

        protected override string ScreenNarration =>
            "Bantuan. Enam langkah menggunakan AR: siapkan kartu target, buka AR Explorer, arahkan kamera, ketuk titik oranye, " +
            "dengarkan penjelasan, dan jika target hilang, perbaiki pencahayaan dan jarak.";

        private static readonly (string icon, string title, string body)[] Steps =
        {
            (Icons.Printer, "Siapkan kartu target", "Cetak kartu target (lebar 15 cm) dari guru, atau tampilkan di layar lain. Letakkan di meja yang terang dan datar."),
            (Icons.Scan, "Buka AR Explorer", "Dari menu utama pilih AR Explorer, atau ikuti modul. Izinkan akses kamera jika diminta."),
            (Icons.Phone, "Arahkan & gerakkan perlahan", "Pegang perangkat 20–40 cm di atas kartu. Seluruh kartu harus terlihat di dalam bingkai. Tahan stabil sekitar 2 detik."),
            (Icons.Hand, "Ketuk titik oranye", "Setelah model 3D muncul, ketuk titik oranye untuk membuka penjelasan bagian. Kamu juga bisa memilih bagian dari daftar. Geser layar untuk memutar model."),
            (Icons.Volume, "Gunakan audio", "Tekan \"Dengarkan\" untuk narasi Bahasa Indonesia. Tekan lagi untuk jeda, dan ikon putar ulang untuk mengulang."),
            (Icons.Refresh, "Jika target hilang", "Tambah cahaya, hindari pantulan, dekatkan atau jauhkan perangkat, dan pastikan kartu tidak tertutup jari. Tekan Reset untuk mengembalikan model."),
        };

        private static readonly (string q, string a)[] Faq =
        {
            ("Kamera tidak menyala?", "Buka Pengaturan perangkat › Aplikasi › Computer Explorer › Izin, lalu izinkan Kamera. Jika tetap tidak bisa, gunakan Mode Simulasi di layar AR."),
            ("Narasi tidak bersuara?", "Periksa volume perangkat dan pengaturan Narasi. Suara Bahasa Indonesia memerlukan mesin Text-to-Speech dengan paket Bahasa Indonesia terpasang."),
            ("Teks terlalu kecil atau sulit dibaca?", "Buka Aksesibilitas untuk memperbesar teks atau menyalakan kontras tinggi."),
            ("Tidak bisa memakai kamera sama sekali?", "Semua materi tetap bisa dipelajari: gunakan Jelajah Hardware, diagram Von Neumann, dan tombol \"Baca info tanpa kamera\" di skenario."),
        };

        protected override void BuildContent(RectTransform content)
        {
            UIKit.SectionHeader(content, "Cara menggunakan AR", "Ikuti langkah berikut. Setiap langkah bisa dibacakan.");
            for (int i = 0; i < Steps.Length; i++)
            {
                var (icon, title, body) = Steps[i];
                var card = UIKit.Card(content, spacing: DesignTokens.Dp(10));
                var row = UIKit.HStack(card, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
                UIKit.IconTile(row, icon, P.PrimarySoft, P.OnPrimarySoft);
                var col = UIKit.Flex(UIKit.VStack(row, DesignTokens.Dp(4)));
                UIKit.Label(col, $"Langkah {i + 1}", TextStyle.Overline, ColorRole.TextSecondary);
                UIKit.Label(col, title, TextStyle.Heading);
                UIKit.Label(card, body, TextStyle.Body);
                NarrationControl(card, $"Langkah {i + 1}. {title}. {body}");
            }

            UIKit.Button(content, "Coba pindai sekarang", () =>
            {
                State.SelectHardware(null);
                State.ARReturnScene = AppConstants.Scenes.Help;
                Go(AppConstants.Scenes.ARScanner);
            }, ButtonVariant.Primary, Icons.Scan);

            UIKit.SectionHeader(content, "Pertanyaan umum");
            foreach (var (q, a) in Faq)
            {
                var card = UIKit.Card(content, spacing: DesignTokens.Dp(6));
                UIKit.Label(card, q, TextStyle.BodyStrong);
                UIKit.Label(card, a, TextStyle.Body, ColorRole.TextSecondary);
            }

            UIKit.Button(content, "Buka pengaturan aksesibilitas", () => Go(AppConstants.Scenes.Accessibility), ButtonVariant.Secondary,
                Icons.Accessibility);
        }
    }
}
