using ComputerExplorer.Accessibility;
using UnityEngine;

namespace ComputerExplorer.Core
{
    public enum AppLanguage { Indonesian, English }

    /// <summary>
    /// Bilingual text (Bahasa Indonesia / English). UI strings are written next to each other at the call site
    /// — <c>Loc.T("Kembali", "Back")</c> — so both languages stay in sync and nothing can go missing.
    /// Educational content keeps its English text in parallel "…En" fields on the ScriptableObjects.
    /// The language is part of the persisted learner settings; changing it re-renders every screen instantly.
    /// </summary>
    public static class Loc
    {
        public static AppLanguage Current =>
            AccessibilityManager.Instance != null ? AccessibilityManager.Instance.Settings.language : AppLanguage.Indonesian;

        public static bool IsEnglish => Current == AppLanguage.English;

        /// <summary>Pick the text for the current language (falls back to Indonesian when no English text exists).</summary>
        public static string T(string indonesian, string english) =>
            IsEnglish && !string.IsNullOrEmpty(english) ? english : indonesian;

        /// <summary>Pick a recorded clip for the current language; null means "use text-to-speech".</summary>
        public static AudioClip Clip(AudioClip indonesian, AudioClip english) => IsEnglish ? english : indonesian;

        public static string NativeName(AppLanguage language) =>
            language == AppLanguage.English ? "English" : "Bahasa Indonesia";

        public static string Code(AppLanguage language) => language == AppLanguage.English ? "EN" : "ID";

        /// <summary>Flag icon file in Resources/Icons.</summary>
        public static string FlagIcon(AppLanguage language) => language == AppLanguage.English ? "flag-en" : "flag-id";

        /// <summary>Text-to-speech locale (language, country).</summary>
        public static (string lang, string country) SpeechLocale =>
            IsEnglish ? ("en", "US") : ("id", "ID");

        public static void Set(AppLanguage language)
        {
            if (AccessibilityManager.Instance == null || Current == language) return;
            AccessibilityManager.Instance.Apply(s => s.language = language);
        }
    }
}
