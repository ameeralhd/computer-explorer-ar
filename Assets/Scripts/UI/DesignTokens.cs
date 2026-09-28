using ComputerExplorer.Accessibility;
using UnityEngine;

namespace ComputerExplorer.UI
{
    /// <summary>
    /// Single source of truth for the proportional layout system (blueprint §3, §14, §15).
    /// Canvas reference = 1080×1920 portrait, scale-with-screen-size, match 0.5.
    /// 1 dp = 3 canvas units (1080 units ≈ 360 dp, a standard phone width).
    /// Spacing follows an 8-point grid: 8, 16, 24, 32, 40, 48 dp.
    /// </summary>
    public static class DesignTokens
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;
        public const float MatchWidthOrHeight = 0.5f;
        public const float UnitsPerDp = 3f;

        public static float Dp(float dp) => dp * UnitsPerDp;

        // 8-point spacing scale (canvas units)
        public static readonly float Space1 = Dp(8);
        public static readonly float Space2 = Dp(16);
        public static readonly float Space3 = Dp(24);
        public static readonly float Space4 = Dp(32);
        public static readonly float Space5 = Dp(40);
        public static readonly float Space6 = Dp(48);

        /// <summary>Primary content margin (24 dp).</summary>
        public static readonly float ScreenMargin = Dp(24);
        /// <summary>Compact card padding (16 dp).</summary>
        public static readonly float CardPadding = Dp(16);

        // Corner radii
        public static readonly float RadiusCard = Dp(16);
        public static readonly float RadiusButton = Dp(12);
        public static readonly float RadiusSmall = Dp(8);
        public static readonly float RadiusPill = Dp(999);

        // Fixed chrome
        public static readonly float HeaderHeight = Dp(64);
        public static readonly float IconSize = Dp(24);
        public static readonly float IconSizeSmall = Dp(20);
        public static readonly float IconTile = Dp(48);

        /// <summary>Minimum touch target: 48 dp, or 64 dp with "large touch targets".</summary>
        public static float TouchTarget =>
            AccessibilityManager.Instance != null && AccessibilityManager.Instance.Settings.largeTouchTargets ? Dp(64) : Dp(48);

        /// <summary>Primary buttons are a little taller than the minimum for comfortable thumb reach.</summary>
        public static float ButtonHeight =>
            AccessibilityManager.Instance != null && AccessibilityManager.Instance.Settings.largeTouchTargets ? Dp(68) : Dp(56);

        public static float BorderWidth => Dp(ContrastController.Current.BorderDp);

        /// <summary>Maximum readable content width on tablets (text lines stay ~70 characters).</summary>
        public static readonly float MaxContentWidth = Dp(560);
    }

    /// <summary>Icon file names in Resources/Icons (generated from Lucide by tools/asset-gen).</summary>
    public static class Icons
    {
        public const string Menu = "menu", Settings = "settings", Volume = "volume-2", VolumeOff = "volume-x",
            Back = "arrow-left", Forward = "arrow-right", Chevron = "chevron-right", Book = "book-open",
            ScanLine = "scan-line", Scan = "scan", Cpu = "cpu", Memory = "memory-stick", Storage = "hard-drive",
            Keyboard = "keyboard", Mouse = "mouse", Monitor = "monitor", Printer = "printer", Speaker = "speaker",
            Workflow = "workflow", Globe = "globe", Pencil = "pencil-line", Chart = "chart-column", Help = "circle-help",
            Teacher = "graduation-cap", Accessibility = "accessibility", Play = "play", Pause = "pause",
            Reset = "rotate-ccw", ZoomIn = "zoom-in", ZoomOut = "zoom-out", Close = "x", Check = "check",
            CheckCircle = "circle-check", XCircle = "circle-x", Lock = "lock", Lightbulb = "lightbulb",
            Message = "message-square", Clock = "clock", ListChecks = "list-checks", Info = "info", Rotate3d = "rotate-3d",
            TextSize = "type", Contrast = "contrast", Move = "move", Hand = "hand", Phone = "smartphone", Star = "star",
            Refresh = "refresh-cw", Sparkles = "sparkles", Trophy = "trophy", Eye = "eye", Home = "house", Layers = "layers",
            Target = "target", Alert = "circle-alert", MapPin = "map-pin", ListOrdered = "list-ordered", Link = "link",
            TextInput = "text-cursor-input", Circuit = "circuit-board", Server = "server";
    }
}
