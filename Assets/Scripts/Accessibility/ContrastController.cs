using ComputerExplorer.Data;
using UnityEngine;

namespace ComputerExplorer.Accessibility
{
    public enum ColorRole
    {
        Background, Surface, SurfaceAlt, Border, BorderStrong,
        Primary, PrimaryPressed, OnPrimary, PrimarySoft, OnPrimarySoft,
        TextPrimary, TextSecondary, TextDisabled,
        Success, SuccessSoft, Error, ErrorSoft, Warning, WarningSoft, Info, InfoSoft,
        Scrim, ARChrome, OnARChrome, Transparent
    }

    /// <summary>A full set of colors for one contrast mode. State is never communicated by color alone —
    /// every status also has an icon and a text label.</summary>
    public class Palette
    {
        public Color Background, Surface, SurfaceAlt, Border, BorderStrong;
        public Color Primary, PrimaryPressed, OnPrimary, PrimarySoft, OnPrimarySoft;
        public Color TextPrimary, TextSecondary, TextDisabled;
        public Color Success, SuccessSoft, Error, ErrorSoft, Warning, WarningSoft, Info, InfoSoft;
        public Color Scrim, ARChrome, OnARChrome;
        public bool IsHighContrast;
        /// <summary>Border thickness in dp; high contrast outlines every surface.</summary>
        public float BorderDp;

        public Color Get(ColorRole role) => role switch
        {
            ColorRole.Background => Background,
            ColorRole.Surface => Surface,
            ColorRole.SurfaceAlt => SurfaceAlt,
            ColorRole.Border => Border,
            ColorRole.BorderStrong => BorderStrong,
            ColorRole.Primary => Primary,
            ColorRole.PrimaryPressed => PrimaryPressed,
            ColorRole.OnPrimary => OnPrimary,
            ColorRole.PrimarySoft => PrimarySoft,
            ColorRole.OnPrimarySoft => OnPrimarySoft,
            ColorRole.TextPrimary => TextPrimary,
            ColorRole.TextSecondary => TextSecondary,
            ColorRole.TextDisabled => TextDisabled,
            ColorRole.Success => Success,
            ColorRole.SuccessSoft => SuccessSoft,
            ColorRole.Error => Error,
            ColorRole.ErrorSoft => ErrorSoft,
            ColorRole.Warning => Warning,
            ColorRole.WarningSoft => WarningSoft,
            ColorRole.Info => Info,
            ColorRole.InfoSoft => InfoSoft,
            ColorRole.Scrim => Scrim,
            ColorRole.ARChrome => ARChrome,
            ColorRole.OnARChrome => OnARChrome,
            _ => Color.clear
        };

        /// <summary>Accent per hardware category (standard mode) — used for icon tiles only, never as the sole cue.</summary>
        public Color Category(HardwareCategory c)
        {
            if (IsHighContrast) return Primary;
            return c switch
            {
                HardwareCategory.Processing => Hex("1D4ED8"),
                HardwareCategory.Memory => Hex("6941C6"),
                HardwareCategory.Storage => Hex("B54708"),
                HardwareCategory.Input => Hex("0E7C86"),
                HardwareCategory.Output => Hex("C11574"),
                HardwareCategory.Architecture => Hex("3538CD"),
                _ => Primary
            };
        }

        public Color OnCategory => IsHighContrast ? OnPrimary : Color.white;

        public static Color Hex(string hex, float alpha = 1f)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            c.a = alpha;
            return c;
        }
    }

    /// <summary>Provides the Standard and High Contrast palettes (text contrast ≥ 4.5:1 in both, ≥ 7:1 in High).</summary>
    public static class ContrastController
    {
        private static Palette standard, high;

        public static Palette Current =>
            AccessibilityManager.Instance != null && AccessibilityManager.Instance.HighContrast ? High : Standard;

        public static Palette Standard => standard ??= new Palette
        {
            Background = Palette.Hex("F4F6FB"),
            Surface = Palette.Hex("FFFFFF"),
            SurfaceAlt = Palette.Hex("EEF2FA"),
            Border = Palette.Hex("DDE3EE"),
            BorderStrong = Palette.Hex("98A2B3"),
            Primary = Palette.Hex("1D4ED8"),
            PrimaryPressed = Palette.Hex("1E3A8A"),
            OnPrimary = Palette.Hex("FFFFFF"),
            PrimarySoft = Palette.Hex("E0E9FF"),
            OnPrimarySoft = Palette.Hex("1E3A8A"),
            TextPrimary = Palette.Hex("101828"),
            TextSecondary = Palette.Hex("475467"),
            TextDisabled = Palette.Hex("98A2B3"),
            Success = Palette.Hex("067647"),
            SuccessSoft = Palette.Hex("DCFAE6"),
            Error = Palette.Hex("B42318"),
            ErrorSoft = Palette.Hex("FEE4E2"),
            Warning = Palette.Hex("B54708"),
            WarningSoft = Palette.Hex("FEF0C7"),
            Info = Palette.Hex("0E7C86"),
            InfoSoft = Palette.Hex("DDF6F4"),
            Scrim = Palette.Hex("101828", 0.55f),
            ARChrome = Palette.Hex("101828", 0.78f),
            OnARChrome = Palette.Hex("FFFFFF"),
            IsHighContrast = false,
            BorderDp = 1f
        };

        public static Palette High => high ??= new Palette
        {
            Background = Palette.Hex("000000"),
            Surface = Palette.Hex("000000"),
            SurfaceAlt = Palette.Hex("141414"),
            Border = Palette.Hex("FFFFFF"),
            BorderStrong = Palette.Hex("FFFFFF"),
            Primary = Palette.Hex("FFE14D"),
            PrimaryPressed = Palette.Hex("FFFFFF"),
            OnPrimary = Palette.Hex("000000"),
            PrimarySoft = Palette.Hex("000000"),
            OnPrimarySoft = Palette.Hex("FFE14D"),
            TextPrimary = Palette.Hex("FFFFFF"),
            TextSecondary = Palette.Hex("F2F2F2"),
            TextDisabled = Palette.Hex("A6A6A6"),
            Success = Palette.Hex("7CF29C"),
            SuccessSoft = Palette.Hex("000000"),
            Error = Palette.Hex("FF9C94"),
            ErrorSoft = Palette.Hex("000000"),
            Warning = Palette.Hex("FFE14D"),
            WarningSoft = Palette.Hex("000000"),
            Info = Palette.Hex("7FE7FF"),
            InfoSoft = Palette.Hex("000000"),
            Scrim = Palette.Hex("000000", 0.85f),
            ARChrome = Palette.Hex("000000", 0.92f),
            OnARChrome = Palette.Hex("FFFFFF"),
            IsHighContrast = true,
            BorderDp = 2f
        };
    }
}
