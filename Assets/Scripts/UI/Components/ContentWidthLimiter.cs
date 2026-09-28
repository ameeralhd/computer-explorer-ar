using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ComputerExplorer.UI.Components
{
    /// <summary>
    /// Keeps readable line lengths on tablets / landscape: horizontal padding grows so content never gets
    /// wider than DesignTokens.MaxContentWidth, while phones keep the standard 24 dp margin.
    /// </summary>
    [RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    public class ContentWidthLimiter : UIBehaviour
    {
        public float margin = DesignTokens.ScreenMargin;
        public float maxWidth = DesignTokens.MaxContentWidth;
        private int lastPad = -1;

        protected override void OnEnable()
        {
            base.OnEnable();
            Apply();
        }

        protected override void OnRectTransformDimensionsChange() => Apply();

        private void Apply()
        {
            var rt = transform as RectTransform;
            var group = GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (rt == null || group == null) return;
            float width = rt.rect.width;
            int pad = Mathf.RoundToInt(Mathf.Max(margin, (width - maxWidth) / 2f));
            if (pad == lastPad) return;
            lastPad = pad;
            group.padding.left = pad;
            group.padding.right = pad;
            LayoutRebuilder.MarkLayoutForRebuild(rt);
        }
    }
}
