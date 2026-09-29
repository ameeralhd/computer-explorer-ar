using System;
using UnityEngine;

namespace ComputerExplorer.Audio
{
    /// <summary>
    /// Thin wrapper over android.speech.tts.TextToSpeech. The voice follows the app language
    /// (id-ID for Bahasa Indonesia, en-US for English).
    /// Used as the narration fallback when a content item has no recorded AudioClip, so every piece
    /// of educational text is always available as audio. No-op outside Android devices.
    /// </summary>
    public sealed class AndroidTextToSpeech : IDisposable
    {
        public bool IsReady => initStatus == 0;
        public bool LanguageAvailable { get; private set; }

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject tts;
        private volatile int initStatus = int.MinValue;
        private int utteranceCounter;

        private sealed class InitListener : AndroidJavaProxy
        {
            private readonly AndroidTextToSpeech owner;
            public InitListener(AndroidTextToSpeech owner) : base("android.speech.tts.TextToSpeech$OnInitListener") => this.owner = owner;
            // Called on a Java thread: only store the status, never touch Unity APIs here.
            public void onInit(int status) => owner.initStatus = status;
        }

        public AndroidTextToSpeech()
        {
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new InitListener(this));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[TTS] Text-to-speech unavailable: " + e.Message);
                initStatus = -1;
            }
        }

        private string configuredLocale;

        /// <summary>Switch the voice to the given locale if it is not already active.</summary>
        private void EnsureLocale(string lang, string country)
        {
            string key = lang + "-" + country;
            if (configuredLocale == key) return;
            configuredLocale = key;
            try
            {
                using var locale = new AndroidJavaObject("java.util.Locale", lang, country);
                // LANG_MISSING_DATA = -1, LANG_NOT_SUPPORTED = -2
                LanguageAvailable = tts.Call<int>("setLanguage", locale) >= 0;
                if (!LanguageAvailable)
                    Debug.LogWarning($"[TTS] Voice {key} not installed. Install it in Android Settings > Text-to-speech.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[TTS] " + e.Message);
            }
        }

        public bool Speak(string text, float volume, float rate)
        {
            if (tts == null || !IsReady || string.IsNullOrWhiteSpace(text)) return false;
            try
            {
                var (lang, country) = Core.Loc.SpeechLocale;
                EnsureLocale(lang, country);
                tts.Call<int>("setSpeechRate", rate);
                using var bundle = new AndroidJavaObject("android.os.Bundle");
                bundle.Call("putFloat", "volume", Mathf.Clamp01(volume)); // TextToSpeech.Engine.KEY_PARAM_VOLUME
                return tts.Call<int>("speak", text, 0 /* QUEUE_FLUSH */, bundle, "ce_" + (++utteranceCounter)) == 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[TTS] " + e.Message);
                return false;
            }
        }

        public bool IsSpeaking
        {
            get
            {
                try { return tts != null && initStatus == 0 && tts.Call<bool>("isSpeaking"); }
                catch { return false; }
            }
        }

        public void Stop()
        {
            try { if (tts != null && initStatus == 0) tts.Call<int>("stop"); }
            catch { /* engine already released */ }
        }

        public void Dispose()
        {
            try { tts?.Call("shutdown"); } catch { /* ignore */ }
            tts?.Dispose();
            tts = null;
        }
#else
        private int initStatus = -1;
        public bool Speak(string text, float volume, float rate) => false;
        public bool IsSpeaking => false;
        public void Stop() { }
        public void Dispose() { }
#endif
    }
}
