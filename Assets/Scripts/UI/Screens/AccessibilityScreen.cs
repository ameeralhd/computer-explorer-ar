using System;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 12_Accessibility — text size, contrast, narration + volume, auto-read, reduced motion, guided/standard mode,
    /// large touch targets. Every change applies instantly to all screens and is saved (blueprint §4.14, §9).
    /// </summary>
    public class AccessibilityScreen : ScreenBase
    {
        protected override string Title => "Aksesibilitas";

        protected override string ScreenNarration =>
            "Pengaturan aksesibilitas. Kamu bisa mengubah ukuran teks, kontras tinggi, narasi dan volumenya, " +
            "membacakan layar secara otomatis, mengurangi gerakan, memilih mode panduan, dan memperbesar tombol.";

        private static AccessibilitySettings S => AccessibilityManager.Instance.Settings;
        private static void Apply(Action<AccessibilitySettings> change) => AccessibilityManager.Instance.Apply(change);

        protected override void BuildContent(RectTransform content)
        {
            UIKit.Label(content, "Perubahan langsung berlaku di semua layar dan tersimpan otomatis.", TextStyle.Body, ColorRole.TextSecondary);

            // Text size
            UIKit.SectionHeader(content, "Ukuran teks");
            var sizes = (TextSizeLevel[])Enum.GetValues(typeof(TextSizeLevel));
            var labels = Array.ConvertAll(sizes, TextSizeController.DisplayName);
            UIKit.Segmented(content, labels, Array.IndexOf(sizes, S.textSize), i => Apply(s => s.textSize = sizes[i]),
                TextSizeController.IsLarge ? 2 : 4);
            var preview = UIKit.Card(content, fill: ColorRole.SurfaceAlt);
            UIKit.Label(preview, "Contoh teks", TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(preview, "CPU memproses instruksi dari memori.", TextStyle.Heading);
            UIKit.Label(preview, "Teks panjang akan membungkus ke baris berikutnya dan tata letak menyesuaikan, tanpa terpotong.", TextStyle.Body);

            // Contrast
            UIKit.SectionHeader(content, "Kontras", "Kontras tinggi memakai latar hitam, teks putih, dan tombol kuning.");
            UIKit.Segmented(content, new[] { "Standar", "Tinggi" }, S.contrast == ContrastMode.High ? 1 : 0,
                i => Apply(s => s.contrast = i == 1 ? ContrastMode.High : ContrastMode.Standard), 2);

            // Narration
            UIKit.SectionHeader(content, "Audio & narasi", "Narasi pendidikan dalam Bahasa Indonesia.");
            UIKit.ToggleRow(content, "Narasi", "Tombol \"Dengarkan\" membacakan materi.", S.narrationEnabled,
                v => Apply(s => s.narrationEnabled = v), Icons.Volume);
            if (S.narrationEnabled)
            {
                var vol = UIKit.Card(content, spacing: DesignTokens.Dp(4));
                var head = UIKit.HStack(vol, DesignTokens.Space1);
                UIKit.Flex(UIKit.Label(head, "Volume narasi", TextStyle.BodyStrong));
                var pct = UIKit.Label(head, $"{Mathf.RoundToInt(S.narrationVolume * 100)}%", TextStyle.Label, ColorRole.TextSecondary,
                    TextAnchor.MiddleRight);
                UIKit.Slider(vol, S.narrationVolume, v =>
                {
                    AccessibilityManager.Instance.SetNarrationVolume(v);
                    pct.text = $"{Mathf.RoundToInt(v * 100)}%";
                });
                UIKit.Button(vol, "Coba narasi", () => Narration.Play("Halo! Ini contoh narasi dalam Bahasa Indonesia."),
                    ButtonVariant.Tonal, Icons.Play, compact: true);
                UIKit.ToggleRow(content, "Bacakan otomatis", "Materi dan umpan balik dibacakan tanpa perlu menekan tombol.",
                    S.autoNarrate, v => Apply(s => s.autoNarrate = v), Icons.Sparkles);
            }
            UIKit.ToggleRow(content, "Suara tombol", "Bunyi klik singkat saat menekan tombol.",
                AudioManager.Instance.Settings.uiSoundsEnabled, v =>
                {
                    AudioManager.Instance.ApplySettings(a => a.uiSoundsEnabled = v);
                    Render();
                }, Icons.Speaker);

            // Motion & interaction
            UIKit.SectionHeader(content, "Gerakan & interaksi");
            UIKit.ToggleRow(content, "Kurangi gerakan", "Mematikan animasi dekoratif dan transisi.", S.reducedMotion,
                v => Apply(s => s.reducedMotion = v), Icons.Pause);
            UIKit.ToggleRow(content, "Tombol besar", "Memperbesar area sentuh semua tombol dan titik AR.", S.largeTouchTargets,
                v => Apply(s => s.largeTouchTargets = v), Icons.Hand);

            // Guidance
            UIKit.SectionHeader(content, "Mode panduan");
            UIKit.Segmented(content, new[] { "Terpandu", "Standar" }, S.guidance == GuidanceMode.Guided ? 0 : 1,
                i => Apply(s => s.guidance = i == 0 ? GuidanceMode.Guided : GuidanceMode.Standard), 2);
            UIKit.Label(content, S.guidance == GuidanceMode.Guided
                    ? "Terpandu: setiap layar menampilkan petunjuk langkah demi langkah yang bisa dibacakan."
                    : "Standar: tampilan ringkas tanpa petunjuk tambahan, untuk pengguna berpengalaman.",
                TextStyle.Caption, ColorRole.TextSecondary);

            UIKit.Spacer(content, DesignTokens.Space1);
            UIKit.Button(content, "Kembalikan ke pengaturan awal", () => ModalController.Instance.Confirm("Kembalikan pengaturan?",
                "Semua pengaturan aksesibilitas kembali ke bawaan.", "Kembalikan", AccessibilityManager.Instance.ResetToDefaults, Icons.Refresh),
                ButtonVariant.Secondary, Icons.Refresh);
        }

        protected override void BuildFooter(RectTransform footer)
        {
            UIKit.Button(footer, "Selesai", Nav.Back, ButtonVariant.Primary, Icons.Check);
        }
    }
}
