using ComputerExplorer.Core;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.Audio
{
    public enum FeedbackSound { Correct, Incorrect, TargetFound, Complete }

    /// <summary>
    /// Owns every AudioSource (music, UI, narration, instruction, feedback) so playback logic is not
    /// duplicated across buttons and screens. UI/feedback tones are synthesised at start-up, so the
    /// project needs no sound assets to run; drop real clips into the serialized fields to replace them.
    /// Music is ducked while narration plays so speech is never competing with other audio.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioClip musicClip;
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip incorrectClip;
        [SerializeField] private AudioClip targetFoundClip;
        [SerializeField] private AudioClip completeClip;

        public AudioSettings Settings { get; private set; }
        public NarrationController Narration { get; private set; }

        private AudioSource music, ui, feedback;
        internal AudioSource NarrationSource { get; private set; }
        internal AudioSource InstructionSource { get; private set; }

        private void Awake()
        {
            Instance = this;
            Settings = SaveSystem.Load<AudioSettings>(AppConstants.Files.Audio) ?? new AudioSettings();

            music = CreateSource("Music", loop: true);
            ui = CreateSource("UI Sounds");
            NarrationSource = CreateSource("Narration");
            InstructionSource = CreateSource("Instruction");
            feedback = CreateSource("Feedback");

            // Explicit null checks (not ??=) because Unity overrides == for destroyed/missing objects.
            if (clickClip == null) clickClip = ToneGenerator.Click();
            if (correctClip == null) correctClip = ToneGenerator.Chime(new[] { 660f, 880f });
            if (incorrectClip == null) incorrectClip = ToneGenerator.Chime(new[] { 392f, 330f });
            if (targetFoundClip == null) targetFoundClip = ToneGenerator.Chime(new[] { 523f, 784f }, 0.09f);
            if (completeClip == null) completeClip = ToneGenerator.Chime(new[] { 523f, 659f, 784f, 1046f }, 0.11f);

            Narration = gameObject.AddComponent<NarrationController>();

            if (musicClip != null)
            {
                music.clip = musicClip;
                music.Play();
            }
        }

        private AudioSource CreateSource(string sourceName, bool loop = false)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;
            return src;
        }

        private void Update()
        {
            // Duck background music under speech.
            float target = Settings.musicVolume * (Narration.IsSpeaking ? 0.2f : 1f);
            music.volume = Mathf.MoveTowards(music.volume, target, Time.unscaledDeltaTime * 2f);
        }

        public void PlayClick()
        {
            if (!Settings.uiSoundsEnabled || clickClip == null) return;
            ui.PlayOneShot(clickClip, Settings.uiVolume);
        }

        public void PlayFeedback(FeedbackSound sound)
        {
            var clip = sound switch
            {
                FeedbackSound.Correct => correctClip,
                FeedbackSound.Incorrect => incorrectClip,
                FeedbackSound.TargetFound => targetFoundClip,
                _ => completeClip
            };
            if (clip != null) feedback.PlayOneShot(clip, Settings.feedbackVolume);
        }

        public void ApplySettings(System.Action<AudioSettings> change)
        {
            change(Settings);
            SaveSystem.Save(AppConstants.Files.Audio, Settings);
        }
    }

    /// <summary>Synthesises short, soft UI tones so the prototype runs without audio assets.</summary>
    internal static class ToneGenerator
    {
        private const int SampleRate = 44100;

        public static AudioClip Click() => Build("click", new[] { 1200f }, 0.035f, 0.35f);

        public static AudioClip Chime(float[] notes, float noteLength = 0.13f) => Build("chime", notes, noteLength, 0.4f);

        private static AudioClip Build(string clipName, float[] notes, float noteLength, float gain)
        {
            int perNote = Mathf.CeilToInt(SampleRate * noteLength);
            var data = new float[perNote * notes.Length];
            for (int n = 0; n < notes.Length; n++)
            {
                for (int i = 0; i < perNote; i++)
                {
                    float t = (float)i / SampleRate;
                    float env = Mathf.Min(1f, i / (SampleRate * 0.005f)) * Mathf.Exp(-5f * i / perNote);
                    data[n * perNote + i] = Mathf.Sin(2 * Mathf.PI * notes[n] * t) * env * gain;
                }
            }
            var clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
