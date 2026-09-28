using System;

namespace ComputerExplorer.Accessibility
{
    public enum TextSizeLevel { Small, Medium, Large, ExtraLarge }
    public enum ContrastMode { Standard, High }
    public enum GuidanceMode { Guided, Standard }

    /// <summary>Persisted accessibility preferences (saved as JSON in persistentDataPath).</summary>
    [Serializable]
    public class AccessibilitySettings
    {
        public TextSizeLevel textSize = TextSizeLevel.Medium;
        public ContrastMode contrast = ContrastMode.Standard;
        public bool narrationEnabled = true;
        public float narrationVolume = 1f;
        public bool autoNarrate;
        public bool reducedMotion;
        public GuidanceMode guidance = GuidanceMode.Guided;
        public bool largeTouchTargets;

        public AccessibilitySettings Clone() => (AccessibilitySettings)MemberwiseClone();
    }
}
