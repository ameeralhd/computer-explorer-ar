using System;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.Audio
{
    public enum NarrationState { Off, Idle, Playing, Paused }

    /// <summary>
    /// Educational narration (Bahasa Indonesia or English, following the app language) with play / pause / replay.
    /// Source priority: recorded AudioClip → Android TTS (id-ID) → Editor simulation (logged + timed),
    /// so the flow can be tested anywhere. Only one narration/instruction plays at a time.
    /// </summary>
    public class NarrationController : MonoBehaviour
    {
        public event Action<NarrationState> StateChanged;

        public NarrationState State { get; private set; } = NarrationState.Idle;
        public string CurrentText { get; private set; }
        public bool IsSpeaking => State == NarrationState.Playing;

        private AudioManager audioManager;
        private AndroidTextToSpeech tts;
        private AudioClip currentClip;
        private AudioSource activeSource;
        private bool usingTts;
        private float simulatedEnd;
        private float ttsGraceUntil;

        private static AccessibilitySettings A11y => AccessibilityManager.Instance.Settings;

        private void Awake()
        {
            audioManager = GetComponent<AudioManager>();
            tts = new AndroidTextToSpeech();
            AccessibilityManager.Instance.Changed += OnAccessibilityChanged;
            AccessibilityManager.Instance.NarrationVolumeChanged += v =>
            {
                if (activeSource != null) activeSource.volume = v;
            };
        }

        private void Start()
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.SceneWillChange += _ => Stop();
            SetState(A11y.narrationEnabled ? NarrationState.Idle : NarrationState.Off);
        }

        private void OnDestroy()
        {
            if (AccessibilityManager.Instance != null) AccessibilityManager.Instance.Changed -= OnAccessibilityChanged;
            tts?.Dispose();
        }

        private AppLanguage lastLanguage = Loc.Current;

        private void OnAccessibilityChanged(AccessibilitySettings s)
        {
            // Never keep speaking the old language after a switch.
            if (s.language != lastLanguage)
            {
                lastLanguage = s.language;
                Stop();
            }
            if (!s.narrationEnabled)
            {
                Stop();
                SetState(NarrationState.Off);
            }
            else if (State == NarrationState.Off)
            {
                SetState(NarrationState.Idle);
            }
            if (activeSource != null) activeSource.volume = s.narrationVolume;
        }

        /// <summary>Play narration. Pass a recorded clip when available; text is always required (captions + TTS).</summary>
        public void Play(string text, AudioClip clip = null, bool isInstruction = false)
        {
            if (!A11y.narrationEnabled)
            {
                SetState(NarrationState.Off);
                return;
            }
            Stop();
            CurrentText = text;
            currentClip = clip;
            usingTts = false;

            if (clip != null)
            {
                activeSource = isInstruction ? audioManager.InstructionSource : audioManager.NarrationSource;
                activeSource.clip = clip;
                activeSource.volume = A11y.narrationVolume;
                activeSource.Play();
            }
            else if (tts.Speak(text, A11y.narrationVolume, audioManager.Settings.speechRate))
            {
                usingTts = true;
                ttsGraceUntil = Time.unscaledTime + 0.8f; // engine needs a moment before isSpeaking turns true
            }
            else
            {
                // Editor / no TTS engine: simulate duration so UI states can be tested.
                simulatedEnd = Time.unscaledTime + Mathf.Clamp(text.Length / 14f, 1.5f, 30f);
                Debug.Log($"[Narasi] {text}");
            }
            SetState(NarrationState.Playing);
        }

        /// <summary>Play if idle, pause if playing this text, resume if paused.</summary>
        public void Toggle(string text, AudioClip clip = null)
        {
            if (State == NarrationState.Playing && text == CurrentText) Pause();
            else if (State == NarrationState.Paused && text == CurrentText) Resume();
            else Play(text, clip);
        }

        public void Pause()
        {
            if (State != NarrationState.Playing) return;
            if (activeSource != null && activeSource.isPlaying) activeSource.Pause();
            if (usingTts) tts.Stop(); // Android TTS cannot pause: resume restarts the sentence.
            SetState(NarrationState.Paused);
        }

        public void Resume()
        {
            if (State != NarrationState.Paused) return;
            if (activeSource != null && activeSource.clip != null)
            {
                activeSource.UnPause();
                SetState(NarrationState.Playing);
            }
            else
            {
                Play(CurrentText, currentClip);
            }
        }

        public void Replay()
        {
            if (!string.IsNullOrEmpty(CurrentText)) Play(CurrentText, currentClip);
        }

        public void Stop()
        {
            if (activeSource != null) activeSource.Stop();
            activeSource = null;
            tts.Stop();
            usingTts = false;
            simulatedEnd = 0f;
            if (State == NarrationState.Playing || State == NarrationState.Paused) SetState(NarrationState.Idle);
        }

        private void Update()
        {
            if (State != NarrationState.Playing) return;
            bool finished;
            if (activeSource != null) finished = !activeSource.isPlaying;
            else if (usingTts) finished = Time.unscaledTime > ttsGraceUntil && !tts.IsSpeaking;
            else finished = Time.unscaledTime >= simulatedEnd;
            if (finished) SetState(NarrationState.Idle);
        }

        private void SetState(NarrationState s)
        {
            if (State == s) return;
            State = s;
            StateChanged?.Invoke(s);
        }
    }
}
