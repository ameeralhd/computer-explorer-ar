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
        protected override string Title => Loc.T("Panduan Guru", "Teacher Guide");

        protected override string ScreenNarration => Loc.T(
            "Panduan guru. Berisi tujuan pembelajaran, urutan kegiatan kelas yang disarankan, persiapan, penggunaan kartu target AR, " +
            "aktivitas kontekstual, poin observasi, dan tips dukungan inklusif.",
            "Teacher guide. It covers learning objectives, a suggested classroom sequence, preparation, using the AR target cards, " +
            "contextual activities, observation points and tips for inclusive support.");

        protected override void BuildContent(RectTransform content)
        {
            // Teacher control
            UIKit.ToggleRow(content, Loc.T("Buka semua modul", "Unlock all modules"),
                Loc.T("Izinkan siswa memilih modul mana pun tanpa urutan.", "Let students choose any module in any order."),
                Saved.Data.unlockAllModules, v =>
                {
                    Saved.SetUnlockAll(v);
                    Render();
                }, Icons.Lock);

            Section(content, Loc.T("Tujuan pembelajaran", "Learning objectives"), Icons.Target, c =>
            {
                foreach (var m in Content.Modules)
                {
                    UIKit.Label(c, $"{Loc.T("Modul", "Module")} {m.order}: {m.Title}", TextStyle.BodyStrong);
                    for (int i = 0; i < m.Objectives.Count; i++) UIKit.NumberedItem(c, i + 1, m.Objectives[i]);
                }
            });

            Section(content, Loc.T("Urutan kelas yang disarankan (2 × 40 menit)", "Suggested classroom sequence (2 × 40 minutes)"), Icons.ListOrdered, c =>
            {
                string[] steps =
                {
                    Loc.T("Pembukaan (5 menit): tanyakan pengalaman siswa saat komputer lambat atau file hilang.",
                          "Opening (5 min): ask students about times a computer was slow or a file was lost."),
                    Loc.T("Diagram Von Neumann (10 menit): siswa menjelajahi komponen dan memutar alur data.",
                          "Von Neumann diagram (10 min): students explore the components and play the data flow."),
                    Loc.T("Eksplorasi AR berpasangan (20 menit): satu siswa memegang perangkat, satu siswa membaca/mendengarkan; bertukar peran.",
                          "AR exploration in pairs (20 min): one student holds the device, the other reads/listens; then swap roles."),
                    Loc.T("Skenario kontekstual (15 menit): diskusikan jawaban kelompok sebelum menekan \"Periksa jawaban\".",
                          "Contextual scenario (15 min): discuss the group's answer before pressing \"Check answer\"."),
                    Loc.T("Latihan individu (15 menit): gunakan hasil untuk umpan balik.",
                          "Individual practice (15 min): use the results for feedback."),
                    Loc.T("Refleksi & penutup (10 menit): minta 2–3 siswa membagikan refleksinya.",
                          "Reflection & closing (10 min): invite 2–3 students to share their reflections."),
                };
                for (int i = 0; i < steps.Length; i++) UIKit.NumberedItem(c, i + 1, steps[i]);
            });

            Section(content, Loc.T("Persiapan", "Preparation"), Icons.ListChecks, c =>
            {
                Bullet(c, Loc.T("Cetak kartu target dari file Docs/ar-targets/print-targets.html (skala 100%, lebar 15 cm), satu set per kelompok.",
                                "Print the target cards from Docs/ar-targets/print-targets.html (100% scale, 15 cm wide), one set per group."));
                Bullet(c, Loc.T("Pastikan ruangan cukup terang; hindari kertas mengilap yang memantulkan cahaya.",
                                "Make sure the room is bright enough; avoid glossy paper that reflects light."));
                Bullet(c, Loc.T("Pasang paket suara Text-to-Speech untuk bahasa yang dipakai (Indonesia dan/atau Inggris) di Pengaturan perangkat.",
                                "Install the Text-to-Speech voice for the language you use (Indonesian and/or English) in the device Settings."));
                Bullet(c, Loc.T("Pilih bahasa aplikasi (ikon bendera) sesuai kelas: Bahasa Indonesia atau English.",
                                "Choose the app language (flag icon) for your class: English or Bahasa Indonesia."));
                Bullet(c, Loc.T("Uji satu kartu target di setiap perangkat sebelum pelajaran dimulai.",
                                "Test one target card on every device before the lesson starts."));
                Bullet(c, Loc.T("Siapkan earphone untuk siswa yang membutuhkan narasi tanpa mengganggu kelompok lain.",
                                "Have earphones ready for students who need narration without disturbing other groups."));
            });

            Section(content, Loc.T("Penggunaan target AR", "Using the AR targets"), Icons.Scan, c =>
            {
                Bullet(c, Loc.T("Setiap kartu memunculkan satu model: CPU, RAM, Storage, Keyboard, Mouse, Monitor, Printer, Speaker, Von Neumann.",
                                "Each card shows one model: CPU, RAM, Storage, Keyboard, Mouse, Monitor, Printer, Speaker, Von Neumann."));
                Bullet(c, Loc.T("Letakkan kartu datar di meja; jarak kamera 20–40 cm.", "Lay the card flat on the table; keep the camera 20–40 cm away."));
                Bullet(c, Loc.T("Aktivitas AR dianggap selesai setelah siswa menjelajahi minimal dua bagian (hotspot).",
                                "An AR activity counts as complete once the student explores at least two parts (hotspots)."));
                Bullet(c, Loc.T("Jika kamera tidak tersedia, gunakan Mode Simulasi atau \"Baca info tanpa kamera\".",
                                "If no camera is available, use Simulation Mode or \"Read info without camera\"."));
            });

            Section(content, Loc.T("Aktivitas kontekstual", "Contextual activities"), Icons.Globe, c =>
            {
                Bullet(c, Loc.T("Setiap skenario: Situasi › Pertanyaan › Eksplorasi › Tugas › Umpan balik › Refleksi.",
                                "Each scenario: Situation › Question › Explore › Task › Feedback › Reflection."));
                Bullet(c, Loc.T("Minta siswa menceritakan pengalaman serupa sebelum tahap Eksplorasi.",
                                "Ask students to share a similar experience before the Explore stage."));
                Bullet(c, Loc.T("Gunakan tahap Umpan balik sebagai bahan diskusi kelas.", "Use the Feedback stage as material for class discussion."));
            });

            Section(content, Loc.T("Poin observasi & evaluasi", "Observation & evaluation points"), Icons.Eye, c =>
            {
                Bullet(c, Loc.T("Apakah siswa dapat menyebutkan fungsi komponen dengan kata-kata sendiri?",
                                "Can students describe what each component does in their own words?"));
                Bullet(c, Loc.T("Apakah siswa menghubungkan komponen dengan situasi nyata (skenario)?",
                                "Do students connect the components to real situations (scenarios)?"));
                Bullet(c, Loc.T("Skor latihan per modul dan tingkat keyakinan pada refleksi (layar Progres).",
                                "Practice scores per module and confidence levels in reflections (Progress screen)."));
                Bullet(c, Loc.T("Kemandirian menggunakan AR dan kerja sama dalam kelompok.", "Independence with AR and teamwork in groups."));
                Bullet(c, Loc.T("Hambatan akses yang muncul (teks, audio, gerak) untuk penyesuaian berikutnya.",
                                "Any access barriers (text, audio, movement) to adjust for next time."));
            });

            Section(content, Loc.T("Dukungan inklusif", "Inclusive support"), Icons.Accessibility, c =>
            {
                Bullet(c, Loc.T("Low vision: teks Besar/Sangat Besar dan Kontras Tinggi; aktifkan Bacakan otomatis.",
                                "Low vision: Large/Extra Large text and High Contrast; turn on Read aloud automatically."));
                Bullet(c, Loc.T("Gangguan pendengaran: semua narasi tersedia sebagai teks di layar; umpan balik memakai ikon dan kata, bukan hanya suara.",
                                "Hearing impairment: all narration is also shown as on-screen text; feedback uses icons and words, not only sound."));
                Bullet(c, Loc.T("Hambatan motorik: aktifkan Tombol besar; bagian model juga bisa dipilih dari daftar tanpa mengetuk titik kecil.",
                                "Motor difficulties: turn on Large buttons; model parts can also be chosen from a list instead of tapping small dots."));
                Bullet(c, Loc.T("Kebutuhan fokus/kognitif: Mode Terpandu dan Kurangi gerakan; satu tugas per tahap.",
                                "Focus/cognitive needs: Guided mode and Reduce motion; one task per stage."));
                Bullet(c, Loc.T("Pembelajar bahasa: ganti bahasa kapan saja dengan ikon bendera untuk membandingkan istilah.",
                                "Language learners: switch language any time with the flag icon to compare terms."));
                Bullet(c, Loc.T("Refleksi tidak wajib mengetik: pilihan tingkat pemahaman dan awal kalimat tersedia.",
                                "Reflection doesn't require typing: understanding levels and sentence starters are available."));
            });

            Section(content, Loc.T("Keterkaitan dengan kerangka teori", "Link to the theoretical framework"), Icons.Book, c =>
            {
                Bullet(c, Loc.T("UDL — Representasi: model 3D, narasi, ukuran teks, kontras, dua bahasa. Aksi & ekspresi: sentuhan, soal interaktif. Keterlibatan: skenario, refleksi.",
                                "UDL — Representation: 3D models, narration, text size, contrast, two languages. Action & expression: touch, interactive questions. Engagement: scenarios, reflection."));
                Bullet(c, Loc.T("CTL — Pemodelan (AR, narasi), bertanya (latihan), konstruksi (skenario autentik), refleksi, penilaian autentik.",
                                "CTL — Modeling (AR, narration), questioning (practice), constructing (authentic scenarios), reflecting, authentic assessment."));
                Bullet(c, Loc.T("Mayer — Representasi multimedia, pemrosesan dua saluran (gambar + narasi), mengurangi beban tak perlu (panel singkat).",
                                "Mayer — Multimedia representation, dual-channel processing (images + narration), reducing extraneous load (short panels)."));
                Bullet(c, Loc.T("ADDIE Inklusif — analisis & desain (skenario), pengembangan (AR, narasi), implementasi (panduan guru), evaluasi (latihan, refleksi).",
                                "Inclusive ADDIE — analysis & design (scenarios), development (AR, narration), implementation (teacher guide), evaluation (practice, reflection)."));
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
