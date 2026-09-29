using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.UI
{
    /// <summary>
    /// Hardware detail presented as a bottom sheet. Reused by the Hardware Explorer, Module Detail and the
    /// scenario exploration step (where it is also the non-AR alternative for learners who cannot use the camera).
    /// </summary>
    public static class HardwareInfoSheet
    {
        public static void Show(HardwareData h, string returnScene, System.Action onClosed = null)
        {
            var p = ContrastController.Current;
            ProgressManager.Instance.MarkHardwareViewed(h.hardwareId);
            bool hasAR = !string.IsNullOrEmpty(h.arTargetName);
            ModalController.Instance.Show(new ModalController.Options
            {
                Title = h.hardwareName,
                Icon = h.iconName,
                OnClosed = onClosed,
                BuildContent = c =>
                {
                    var head = UIKit.HStack(c, DesignTokens.Dp(12));
                    UIKit.IconTile(head, h.iconName, p.Category(h.category), p.OnCategory, DesignTokens.Dp(56));
                    var t = UIKit.Flex(UIKit.VStack(head, DesignTokens.Dp(2)));
                    UIKit.Label(t, h.FullName, TextStyle.BodyStrong);
                    UIKit.Label(t, h.category.DisplayName(), TextStyle.Caption, ColorRole.TextSecondary);

                    UIKit.Label(c, h.DetailedDescription, TextStyle.Body);
                    UIKit.Callout(c, Icons.Target, Loc.T("Fungsi", "Function"), h.Function);
                    UIKit.Callout(c, Icons.Globe, Loc.T("Contoh nyata", "Real-life example"), h.ContextualExample, ColorRole.Primary, ColorRole.PrimarySoft);
                    if (h.hotspots.Count > 0)
                    {
                        UIKit.Label(c, Loc.T("Bagian penting", "Key parts"), TextStyle.Heading);
                        foreach (var hs in h.hotspots)
                        {
                            var card = UIKit.Card(c, fill: ColorRole.SurfaceAlt, padding: DesignTokens.Dp(12));
                            UIKit.Label(card, hs.Label, TextStyle.BodyStrong);
                            UIKit.Label(card, hs.Description, TextStyle.Body);
                        }
                    }
                    UIKit.Button(c, Loc.T("Dengarkan penjelasan", "Listen to explanation"), () => AudioManager.Instance.Narration.Toggle(h.NarrationText, h.NarrationClip),
                        ButtonVariant.Tonal, Icons.Volume);
                },
                PrimaryLabel = hasAR ? Loc.T("Lihat dalam AR", "View in AR") : null,
                PrimaryIcon = Icons.Scan,
                OnPrimary = () =>
                {
                    GameStateManager.Instance.SelectHardware(h.hardwareId);
                    GameStateManager.Instance.ARReturnScene = returnScene;
                    NavigationController.Instance.GoTo(AppConstants.Scenes.ARScanner);
                },
                SecondaryLabel = Loc.T("Tutup", "Close")
            });
        }
    }
}
