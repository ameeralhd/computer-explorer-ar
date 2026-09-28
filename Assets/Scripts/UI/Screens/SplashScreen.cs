using System.Collections;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>00_Boot — app identity and a short loading indicator, then Welcome.</summary>
    public class SplashScreen : ScreenBase
    {
        protected override bool ShowHeader => false;
        protected override bool Scrollable => false;

        protected override void Start()
        {
            base.Start();
            StartCoroutine(Continue());
        }

        private IEnumerator Continue()
        {
            // Services are created by AppManager before this scene loads; keep the splash brief.
            yield return new WaitForSecondsRealtime(MotionController.Reduced ? 0.6f : 1.4f);
            Nav.Replace(AppConstants.Scenes.Welcome);
        }

        protected override void BuildContent(RectTransform content)
        {
            var col = UIKit.AddVertical(content, DesignTokens.Space2, DesignTokens.ScreenMargin, 0, TextAnchor.MiddleCenter);
            col.childForceExpandWidth = true;
            UIKit.Picture(content, UIAssets.Image("logo"), DesignTokens.Dp(120));
            UIKit.Label(content, AppConstants.AppName, TextStyle.Display, ColorRole.TextPrimary, TextAnchor.MiddleCenter);
            UIKit.Label(content, AppConstants.AppTagline, TextStyle.Body, ColorRole.TextSecondary, TextAnchor.MiddleCenter);
            UIKit.Spacer(content, DesignTokens.Space3);
            var row = UIKit.HStack(content, DesignTokens.Space1, align: TextAnchor.MiddleCenter);
            var spinner = UIKit.Icon(row, Icons.Refresh, DesignTokens.IconSize, ColorRole.Primary);
            spinner.gameObject.AddComponent<SpinAnimator>();
            UIKit.Label(row, "Menyiapkan…", TextStyle.Label, ColorRole.TextSecondary);
        }
    }
}
