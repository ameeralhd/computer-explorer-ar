using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 14_TeacherGuide — objectives, recommended classroom sequence, preparation, AR target usage, contextual
    /// activity instructions, observation/evaluation points, inclusive-support tips and the theory mapping.
    /// </summary>
    public class TeacherGuideScreen : ScreenBase
    {
        protected override string Title => "Panduan Guru";

        protected override string ScreenNarration =>
            "Panduan guru. Berisi tujuan pembelajaran, urutan kegiatan kelas yang disarankan, persiapan, penggunaan kartu target AR, " +
            "aktivitas kontekstual, poin observasi, dan tips dukungan inklusif.";

        protected override void BuildContent(RectTransform content)
        {
            // Teacher control
            UIKit.ToggleRow(content, "Buka semua modul", "Izinkan siswa memilih modul mana pun tanpa urutan.",
                Saved.Data.unlockAllModules, v =>
                {
                    Saved.SetUnlockAll(v);
                    Render();
                }, Icons.Lock);

            Section(content, "Tujuan pembelajaran", Icons.Target, c =>
            {
                foreach (var m in Content.Modules)
                {
                    UIKit.Label(c, $"Modul {m.order}: {m.title}", TextStyle.BodyStrong);
                    for (int i = 0; i < m.learningObjective.Count; i++) UIKit.NumberedItem(c, i + 1, m.learningObjective[i]);
                }
            });

            Section(content, "Urutan kelas yang disarankan (2 × 40 menit)", Icons.ListOrdered, c =>
            {
                string[] steps =
                {
                    "Pembukaan (5 menit): tanyakan pengalaman siswa saat komputer lambat atau file hilang.",
                    "Diagram Von Neumann (10 menit): siswa menjelajahi komponen dan memutar alur data.",
                    "Eksplorasi AR berpasangan (20 menit): satu siswa memegang perangkat, satu siswa membaca/mendengarkan; bertukar peran.",
                    "Skenario kontekstual (15 menit): diskusikan jawaban kelompok sebelum menekan \"Periksa jawaban\".",
                    "Latihan individu (15 menit): gunakan hasil untuk umpan balik.",
                    "Refleksi & penutup (10 menit): minta 2–3 siswa membagikan refleksinya.",
                };
                for (int i = 0; i < steps.Length; i++) UIKit.NumberedItem(c, i + 1, steps[i]);
            });

            Section(content, "Persiapan", Icons.ListChecks, c =>
            {
                Bullet(c, "Cetak kartu target dari file Docs/ar-targets/print-targets.html (skala 100%, lebar 15 cm), satu set per kelompok.");
                Bullet(c, "Pastikan ruangan cukup terang; hindari kertas mengilap yang memantulkan cahaya.");
                Bullet(c, "Pasang paket suara Bahasa Indonesia pada Text-to-Speech perangkat (Pengaturan › Aksesibilitas › Output text-to-speech).");
                Bullet(c, "Uji satu kartu target di setiap perangkat sebelum pelajaran dimulai.");
                Bullet(c, "Siapkan earphone untuk siswa yang membutuhkan narasi tanpa mengganggu kelompok lain.");
            });

            Section(content, "Penggunaan target AR", Icons.Scan, c =>
            {
                Bullet(c, "Setiap kartu memunculkan satu model: CPU, RAM, Storage, Keyboard, Mouse, Monitor, Printer, Speaker, Von Neumann.");
                Bullet(c, "Letakkan kartu datar di meja; jarak kamera 20–40 cm.");
                Bullet(c, "Aktivitas AR dianggap selesai setelah siswa menjelajahi minimal dua bagian (hotspot).");
                Bullet(c, "Jika kamera tidak tersedia, gunakan Mode Simulasi atau \"Baca info tanpa kamera\".");
            });

            Section(content, "Aktivitas kontekstual", Icons.Globe, c =>
            {
                Bullet(c, "Setiap skenario: Situasi › Pertanyaan › Eksplorasi › Tugas › Umpan balik › Refleksi.");
                Bullet(c, "Minta siswa menceritakan pengalaman serupa sebelum tahap Eksplorasi.");
                Bullet(c, "Gunakan tahap Umpan balik sebagai bahan diskusi kelas.");
            });

            Section(content, "Poin observasi & evaluasi", Icons.Eye, c =>
            {
                Bullet(c, "Apakah siswa dapat menyebutkan fungsi komponen dengan kata-kata sendiri?");
                Bullet(c, "Apakah siswa menghubungkan komponen dengan situasi nyata (skenario)?");
                Bullet(c, "Skor latihan per modul dan tingkat keyakinan pada refleksi (layar Progres).");
                Bullet(c, "Kemandirian menggunakan AR dan kerja sama dalam kelompok.");
                Bullet(c, "Hambatan akses yang muncul (teks, audio, gerak) untuk penyesuaian berikutnya.");
            });

            Section(content, "Dukungan inklusif", Icons.Accessibility, c =>
            {
                Bullet(c, "Low vision: teks Besar/Sangat Besar dan Kontras Tinggi; aktifkan Bacakan otomatis.");
                Bullet(c, "Gangguan pendengaran: semua narasi tersedia sebagai teks di layar; umpan balik memakai ikon dan kata, bukan hanya suara.");
                Bullet(c, "Hambatan motorik: aktifkan Tombol besar; bagian model juga bisa dipilih dari daftar tanpa mengetuk titik kecil.");
                Bullet(c, "Kebutuhan fokus/kognitif: Mode Terpandu dan Kurangi gerakan; satu tugas per tahap.");
                Bullet(c, "Refleksi tidak wajib mengetik: pilihan tingkat pemahaman dan awal kalimat tersedia.");
            });

            Section(content, "Keterkaitan dengan kerangka teori", Icons.Book, c =>
            {
                Bullet(c, "UDL — Representasi: model 3D, narasi, ukuran teks, kontras. Aksi & ekspresi: sentuhan, soal interaktif. Keterlibatan: skenario, refleksi.");
                Bullet(c, "CTL — Pemodelan (AR, narasi), bertanya (latihan), konstruksi (skenario autentik), refleksi, penilaian autentik.");
                Bullet(c, "Mayer — Representasi multimedia, pemrosesan dua saluran (gambar + narasi), mengurangi beban tak perlu (panel singkat).");
                Bullet(c, "ADDIE Inklusif — analisis & desain (skenario), pengembangan (AR, narasi), implementasi (panduan guru), evaluasi (latihan, refleksi).");
            });
        }

        /// <summary>Card with an icon heading; every guide section uses the same structure.</summary>
        private static void Section(RectTransform content, string title, string icon, System.Action<RectTransform> body)
        {
            var card = UIKit.Card(content, spacing: DesignTokens.Dp(10));
            var head = UIKit.HStack(card, DesignTokens.Dp(12));
            UIKit.IconTile(head, icon, P.PrimarySoft, P.OnPrimarySoft, DesignTokens.Dp(40));
            UIKit.Flex(UIKit.Label(head, title, TextStyle.Heading));
            body(card);
        }

        private static void Bullet(Transform parent, string text)
        {
            var row = UIKit.HStack(parent, DesignTokens.Dp(10), align: TextAnchor.UpperLeft);
            var dot = UIKit.Box(row, ContrastController.Current.Primary, DesignTokens.RadiusPill, null, "Dot");
            float s = DesignTokens.Dp(8);
            UIKit.Layout(dot, minWidth: s, minHeight: s, preferredWidth: s, preferredHeight: s, flexibleWidth: 0);
            UIKit.Flex(UIKit.Label(row, text, TextStyle.Body));
        }
    }
}
