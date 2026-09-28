using System;
using ComputerExplorer.Core;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.Accessibility
{
    /// <summary>
    /// Global accessibility state. Every screen listens to <see cref="Changed"/> and re-renders, so a
    /// setting changed anywhere applies everywhere immediately and is persisted.
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        public AccessibilitySettings Settings { get; private set; }

        public event Action<AccessibilitySettings> Changed;

        public bool ReducedMotion => Settings.reducedMotion;
        public bool IsGuided => Settings.guidance == GuidanceMode.Guided;
        public bool HighContrast => Settings.contrast == ContrastMode.High;

        private void Awake()
        {
            Instance = this;
            Settings = SaveSystem.Load<AccessibilitySettings>(AppConstants.Files.Accessibility) ?? new AccessibilitySettings();
        }

        /// <summary>Change one or more settings, persist them and notify all screens.</summary>
        public void Apply(Action<AccessibilitySettings> change)
        {
            change(Settings);
            Settings.narrationVolume = Mathf.Clamp01(Settings.narrationVolume);
            SaveSystem.Save(AppConstants.Files.Accessibility, Settings);
            Changed?.Invoke(Settings);
        }

        /// <summary>Raised for continuous values (volume slider) that must not re-render screens while dragging.</summary>
        public event Action<float> NarrationVolumeChanged;

        private float saveAt = -1f;

        /// <summary>Update narration volume without re-rendering; persisted shortly after the last change.</summary>
        public void SetNarrationVolume(float volume)
        {
            Settings.narrationVolume = Mathf.Clamp01(volume);
            NarrationVolumeChanged?.Invoke(Settings.narrationVolume);
            saveAt = Time.unscaledTime + 0.5f;
        }

        private void Update()
        {
            if (saveAt < 0f || Time.unscaledTime < saveAt) return;
            saveAt = -1f;
            SaveSystem.Save(AppConstants.Files.Accessibility, Settings);
        }

        public void ResetToDefaults() => Apply(s =>
        {
            var d = new AccessibilitySettings();
            s.textSize = d.textSize;
            s.contrast = d.contrast;
            s.narrationEnabled = d.narrationEnabled;
            s.narrationVolume = d.narrationVolume;
            s.autoNarrate = d.autoNarrate;
            s.reducedMotion = d.reducedMotion;
            s.guidance = d.guidance;
            s.largeTouchTargets = d.largeTouchTargets;
        });
    }
}
