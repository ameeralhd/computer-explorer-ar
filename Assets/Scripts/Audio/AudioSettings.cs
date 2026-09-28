using System;

namespace ComputerExplorer.Audio
{
    /// <summary>
    /// Persisted audio preferences. Narration on/off and narration volume live in
    /// AccessibilitySettings because they are accessibility features (blueprint §4.14).
    /// Note: this is ComputerExplorer.Audio.AudioSettings, not UnityEngine.AudioSettings.
    /// </summary>
    [Serializable]
    public class AudioSettings
    {
        public bool uiSoundsEnabled = true;
        public float uiVolume = 0.6f;
        public float feedbackVolume = 0.8f;
        public float musicVolume = 0.25f;
        /// <summary>Text-to-speech rate (1 = normal). Slightly slower default aids comprehension.</summary>
        public float speechRate = 0.95f;
    }
}
