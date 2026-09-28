using UnityEngine;

namespace ComputerExplorer.Accessibility
{
    public enum TextStyle { Display, Title, Heading, Body, BodyStrong, Label, Caption, Button, Overline }

    /// <summary>
    /// Type scale (in dp, see UI.DesignTokens) multiplied by the learner's text-size preference.
    /// Layouts use layout groups, so larger text wraps and reflows instead of clipping.
    /// </summary>
    public static class TextSizeController
    {
        private const float MinimumDp = 12f;

        public static float Scale => ScaleFor(AccessibilityManager.Instance != null
            ? AccessibilityManager.Instance.Settings.textSize
            : TextSizeLevel.Medium);

        /// <summary>True for Large / Extra Large — screens switch multi-column grids to one column.</summary>
        public static bool IsLarge => Scale >= 1.25f;

        public static float ScaleFor(TextSizeLevel level) => level switch
        {
            TextSizeLevel.Small => 0.875f,
            TextSizeLevel.Large => 1.25f,
            TextSizeLevel.ExtraLarge => 1.5f,
            _ => 1f
        };

        public static string DisplayName(TextSizeLevel level) => level switch
        {
            TextSizeLevel.Small => "Kecil",
            TextSizeLevel.Medium => "Sedang",
            TextSizeLevel.Large => "Besar",
            TextSizeLevel.ExtraLarge => "Sangat Besar",
            _ => level.ToString()
        };

        public static float BaseDp(TextStyle style) => style switch
        {
            TextStyle.Display => 28f,
            TextStyle.Title => 22f,
            TextStyle.Heading => 18f,
            TextStyle.Body => 16f,
            TextStyle.BodyStrong => 16f,
            TextStyle.Label => 14f,
            TextStyle.Caption => 13f,
            TextStyle.Button => 16f,
            TextStyle.Overline => 12f,
            _ => 16f
        };

        public static bool IsBold(TextStyle style) =>
            style == TextStyle.Display || style == TextStyle.Title || style == TextStyle.Heading ||
            style == TextStyle.BodyStrong || style == TextStyle.Label || style == TextStyle.Button ||
            style == TextStyle.Overline;

        /// <summary>Font size in canvas units for the current preference.</summary>
        public static int Size(TextStyle style) =>
            Mathf.RoundToInt(UI.DesignTokens.Dp(Mathf.Max(MinimumDp, BaseDp(style) * Scale)));
    }
}
