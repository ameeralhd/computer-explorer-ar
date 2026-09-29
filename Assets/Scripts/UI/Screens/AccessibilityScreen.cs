using System;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 12_Accessibility — text size, contrast, narration + volume, auto-read, reduced motion, guided/standard mode,
    /// large touch targets. Every change applies instantly to all screens and is saved (blueprint §4.14, §9).
    /// </summary>
    public class AccessibilityScreen : ScreenBase
    {
        protected override string Title => Loc.T("Pengaturan & Aksesibilitas", "Settings & Accessibility");

        protected override string ScreenNarration =>
            Loc.T("Pengaturan. Kamu bisa memilih bahasa, mengubah ukuran teks, kontras tinggi, narasi dan volumenya, " +
                  "membacakan layar secara otomatis, mengurangi gerakan, memilih mode panduan, dan memperbesar tombol.",
                  "Settings. You can choose the language, change text size, high contrast, narration and its volume, " +
                  "read screens aloud automatically, reduce motion, choose the guidance mode and enlarge buttons.");

        private static AccessibilitySettings S => AccessibilityManager.Instance.Settings;
        private static void Apply(Action<AccessibilitySettings> change) => AccessibilityManager.Instance.Apply(change);

        protected override void BuildContent(RectTransform content)
        {
            UIKit.Label(content, Loc.T("Perubahan langsung berlaku di semua layar dan tersimpan otomatis.",
                "Changes apply to every screen immediately and are saved automatically."), TextStyle.Body, ColorRole.TextSecondary);

            // Language
            UIKit.SectionHeader(content, "Bahasa / Language",
                Loc.T("Seluruh aplikasi, materi, dan narasi mengikuti bahasa yang dipilih.", "The whole app, its lessons and narration follow the chosen language."));
            UIKit.LanguageSwitch(content);

            // Text size
            UIKit.SectionHeader(content, Loc.T("Ukuran teks", "Text size"));
            var sizes = (TextSizeLevel[])Enum.GetValues(typeof(TextSizeLevel));
            var labels = Array.ConvertAll(sizes, TextSizeController.DisplayName);
            UIKit.Segmented(content, labels, Array.IndexOf(sizes, S.textSize), i => Apply(s => s.textSize = sizes[i]),
                TextSizeController.IsLarge ? 2 : 4);
            var preview = UIKit.Card(content, fill: ColorRole.SurfaceAlt);
            UIKit.Label(preview, Loc.T("Contoh teks", "Sample text"), TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(preview, Loc.T("CPU memproses instruksi dari memori.", "The CPU processes instructions from memory."), TextStyle.Heading);
            UIKit.Label(preview, Loc.T("Teks panjang akan membungkus ke baris berikutnya dan tata letak menyesuaikan, tanpa terpotong.", "Long text wraps to the next line and the layout adjusts, without being cut off."), TextStyle.Body);

            // Contrast
            UIKit.SectionHeader(content, Loc.T("Kontras", "Contrast"), Loc.T("Kontras tinggi memakai latar hitam, teks putih, dan tombol kuning.", "High contrast uses a black background, white text and yellow buttons."));
            UIKit.Segmented(content, new[] { Loc.T("Standar", "Standard"), Loc.T("Tinggi", "High") }, S.contrast == ContrastMode.High ? 1 : 0,
                i => Apply(s => s.contrast = i == 1 ? ContrastMode.High : ContrastMode.Standard), 2);

            // Narration
            UIKit.SectionHeader(content, Loc.T("Audio & narasi", "Audio & narration"), Loc.T("Narasi pendidikan mengikuti bahasa aplikasi.", "Educational narration follows the app language."));
            UIKit.ToggleRow(content, Loc.T("Narasi", "Narration"), Loc.T("Tombol \"Dengarkan\" membacakan materi.", "The \"Listen\" button reads the lesson aloud."), S.narrationEnabled,
                v => Apply(s => s.narrationEnabled = v), Icons.Volume);
            if (S.narrationEnabled)
            {
                var vol = UIKit.Card(content, spacing: DesignTokens.Dp(4));
                var head = UIKit.HStack(vol, DesignTokens.Space1);
                UIKit.Flex(UIKit.Label(head, Loc.T("Volume narasi", "Narration volume"), TextStyle.BodyStrong));
                var pct = UIKit.Label(head, $"{Mathf.RoundToInt(S.narrationVolume * 100)}%", TextStyle.Label, ColorRole.TextSecondary,
                    TextAnchor.MiddleRight);
                UIKit.Slider(vol, S.narrationVolume, v =>
                {
                    AccessibilityManager.Instance.SetNarrationVolume(v);
                    pct.text = $"{Mathf.RoundToInt(v * 100)}%";
                });
                UIKit.Button(vol, Loc.T("Coba narasi", "Test narration"), () => Narration.Play(Loc.T("Halo! Ini contoh narasi dalam Bahasa Indonesia.", "Hello! This is a narration sample in English.")),
                    ButtonVariant.Tonal, Icons.Play, compact: true);
                UIKit.ToggleRow(content, Loc.T("Bacakan otomatis", "Read aloud automatically"), Loc.T("Materi dan umpan balik dibacakan tanpa perlu menekan tombol.", "Lessons and feedback are read aloud without pressing a button."),
                    S.autoNarrate, v => Apply(s => s.autoNarrate = v), Icons.Sparkles);
            }
            UIKit.ToggleRow(content, Loc.T("Suara tombol", "Button sounds"), Loc.T("Bunyi klik singkat saat menekan tombol.", "A short click when you press a button."),
                AudioManager.Instance.Settings.uiSoundsEnabled, v =>
                {
                    AudioManager.Instance.ApplySettings(a => a.uiSoundsEnabled = v);
                    Render();
                }, Icons.Speaker);

            // Motion & interaction
            UIKit.SectionHeader(content, Loc.T("Gerakan & interaksi", "Motion & interaction"));
            UIKit.ToggleRow(content, Loc.T("Kurangi gerakan", "Reduce motion"), Loc.T("Mematikan animasi dekoratif dan transisi.", "Turns off decorative animations and transitions."), S.reducedMotion,
                v => Apply(s => s.reducedMotion = v), Icons.Pause);
            UIKit.ToggleRow(content, Loc.T("Tombol besar", "Large buttons"), Loc.T("Memperbesar area sentuh semua tombol dan titik AR.", "Enlarges the touch area of every button and AR point."), S.largeTouchTargets,
                v => Apply(s => s.largeTouchTargets = v), Icons.Hand);

            // Guidance
            UIKit.SectionHeader(content, Loc.T("Mode panduan", "Guidance mode"));
            UIKit.Segmented(content, new[] { Loc.T("Terpandu", "Guided"), Loc.T("Standar", "Standard") }, S.guidance == GuidanceMode.Guided ? 0 : 1,
                i => Apply(s => s.guidance = i == 0 ? GuidanceMode.Guided : GuidanceMode.Standard), 2);
            UIKit.Label(content, S.guidance == GuidanceMode.Guided
                    ? Loc.T("Terpandu: setiap layar menampilkan petunjuk langkah demi langkah yang bisa dibacakan.", "Guided: every screen shows step-by-step tips that can be read aloud.")
                    : Loc.T("Standar: tampilan ringkas tanpa petunjuk tambahan, untuk pengguna berpengalaman.", "Standard: a compact view without extra tips, for experienced users."),
                TextStyle.Caption, ColorRole.TextSecondary);

            UIKit.Spacer(content, DesignTokens.Space1);
            UIKit.Button(content, Loc.T("Kembalikan ke pengaturan awal", "Reset to default settings"), () => ModalController.Instance.Confirm(Loc.T("Kembalikan pengaturan?", "Reset settings?"),
                Loc.T("Semua pengaturan aksesibilitas kembali ke bawaan (bahasa tidak berubah).", "All accessibility settings return to their defaults (the language stays the same)."), Loc.T("Kembalikan", "Reset"), AccessibilityManager.Instance.ResetToDefaults, Icons.Refresh),
                ButtonVariant.Secondary, Icons.Refresh);
        }

        protected override void BuildFooter(RectTransform footer)
        {
            UIKit.Button(footer, Loc.T("Selesai", "Done"), Nav.Back, ButtonVariant.Primary, Icons.Check);
        }
    }
}
