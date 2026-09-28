using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI.Components
{
    /// <summary>
    /// Sizes a scroll area to its content up to a fraction of the screen height (bottom sheets, info panels):
    /// short content → no empty space, long content → scrolls.
    /// </summary>
    [RequireComponent(typeof(LayoutElement))]
    public class ClampHeightToContent : MonoBehaviour
    {
        public RectTransform content;
        public RectTransform reference;
        [Range(0.1f, 1f)] public float maxFraction = 0.6f;
        private LayoutElement element;

        private void Awake() => element = GetComponent<LayoutElement>();

        private void LateUpdate()
        {
            if (content == null || reference == null) return;
            float preferred = LayoutUtility.GetPreferredHeight(content);
            float target = Mathf.Min(preferred, reference.rect.height * maxFraction);
            if (Mathf.Abs(element.preferredHeight - target) > 0.5f)
            {
                element.preferredHeight = target;
                element.minHeight = Mathf.Min(target, DesignTokens.Dp(48));
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform.parent);
            }
        }
    }
}
