using System;
using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Audio;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;
using UnityEngine;

namespace ComputerExplorer.AR
{
    /// <summary>
    /// AR lesson coordination: decides which targets are active (the lesson's hardware, or all), creates one
    /// object per target, tracks detection state, hotspot selection and AR activity completion, and exposes a
    /// single <see cref="Changed"/> event the scanner UI renders from.
    ///
    /// Flow (blueprint §7.3): student selects CPU → ARManager activates the CPU target → target detected →
    /// prefab appears → hotspot tapped → HardwareData looked up → info panel + narration → progress update.
    /// </summary>
    public class ARManager : MonoBehaviour
    {
        public static ARManager Instance { get; private set; }

        [SerializeField] private ARSessionController session;

        public event Action Changed;

        public ARTrackingState Status { get; private set; } = ARTrackingState.Initializing;
        public string ErrorMessage { get; private set; }
        public bool IsSimulated => session != null && session.IsSimulated;
        /// <summary>Hardware chosen before opening the scanner (null = all targets).</summary>
        public HardwareData Focus { get; private set; }
        /// <summary>Hardware of the most recently detected target.</summary>
        public HardwareData Active { get; private set; }
        public HotspotData SelectedHotspot { get; private set; }
        public bool ShowRecoveryHelp { get; private set; }
        public IEnumerable<HardwareData> AvailableHardware => objects.Values.Select(o => o.Hardware);

        private readonly Dictionary<string, ARTargetController> targets = new Dictionary<string, ARTargetController>();
        private readonly Dictionary<string, ARObjectController> objects = new Dictionary<string, ARObjectController>();
        private readonly HashSet<string> announced = new HashSet<string>();
        private float lostSince;

        private static ContentLibrary Content => AppManager.Instance.Content;
        private static ProgressManager Saved => ProgressManager.Instance;

        public ARObjectController ActiveObject => Active != null && objects.TryGetValue(Active.hardwareId, out var o) ? o : null;

        private void Awake()
        {
            Instance = this;
            if (session == null) session = FindAnyObjectByType<ARSessionController>();
            if (session == null) session = gameObject.AddComponent<ARSessionController>();
        }

        private void Start()
        {
            Focus = Content.GetHardware(GameStateManager.Instance.SelectedHardwareId);
            session.Ready += OnSessionReady;
            session.Failed += OnSessionFailed;
            session.Begin();
            AccessibilityManager.Instance.Changed += OnSettingsChanged;
        }

        /// <summary>Language or contrast changed: refresh the 3D hotspot labels and colours.</summary>
        private void OnSettingsChanged(AccessibilitySettings s)
        {
            foreach (var obj in objects.Values)
                foreach (var hs in obj.Hotspots) hs.Refresh();
            RefreshHotspots();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (AccessibilityManager.Instance != null) AccessibilityManager.Instance.Changed -= OnSettingsChanged;
        }

        // ------------------------------------------------------------------ session
        private void OnSessionReady()
        {
            var list = Focus != null ? new List<HardwareData> { Focus } : Content.ARHardware.ToList();
            foreach (var h in list)
            {
                if (targets.ContainsKey(h.hardwareId)) continue;
                var target = session.CreateTarget(h);
                if (target == null) continue;
                targets[h.hardwareId] = target;
                objects[h.hardwareId] = ARObjectController.Create(h, target.transform, h.printedWidthMeters);
                target.TrackingChanged += OnTrackingChanged;
            }
            Status = targets.Count > 0 ? ARTrackingState.Searching : ARTrackingState.Error;
            if (targets.Count == 0) ErrorMessage = Loc.T("Tidak ada target AR yang dapat dimuat.", "No AR targets could be loaded.");
            if (IsSimulated) session.ShowSimulatedCard(Focus ?? list.FirstOrDefault());
            Changed?.Invoke();
        }

        private void OnSessionFailed(string message)
        {
            Status = ARTrackingState.Error;
            ErrorMessage = message;
            Changed?.Invoke();
        }

        private void OnTrackingChanged(ARTargetController target, bool tracked)
        {
            var obj = objects[target.Hardware.hardwareId];
            obj.SetVisible(tracked);

            if (tracked)
            {
                if (Active != target.Hardware) SelectedHotspot = null;
                Active = target.Hardware;
                Status = ARTrackingState.Detected;
                ShowRecoveryHelp = false;
                Saved.MarkHardwareViewed(Active.hardwareId);
                MarkScenarioExploration(Active);
                RefreshHotspots();
                if (announced.Add(Active.hardwareId))
                {
                    AudioManager.Instance.PlayFeedback(FeedbackSound.TargetFound);
                    if (AccessibilityManager.Instance.Settings.autoNarrate)
                        AudioManager.Instance.Narration.Play(Loc.T($"Target terdeteksi: {Active.hardwareName}. {Active.Function}", $"Target detected: {Active.hardwareName}. {Active.Function}"));
                }
            }
            else if (Active == target.Hardware)
            {
                Status = ARTrackingState.Lost;
                lostSince = Time.unscaledTime;
            }
            Changed?.Invoke();
        }

        private void Update()
        {
            if (Status == ARTrackingState.Lost && !ShowRecoveryHelp &&
                Time.unscaledTime - lostSince > AppConstants.TrackingLostHelpDelay)
            {
                ShowRecoveryHelp = true;
                Changed?.Invoke();
            }
        }

        private static void MarkScenarioExploration(HardwareData h)
        {
            var sc = GameStateManager.Instance.ActiveScenario;
            if (sc == null) return;
            var scenario = Content.GetScenario(sc.scenarioId);
            if (scenario != null && scenario.hardware == h) sc.explored = true;
        }

        // ------------------------------------------------------------------ hotspots
        public void SelectHotspot(ARHotspot hotspot) => SelectHotspot(hotspot.Hardware, hotspot.Data);

        public void SelectHotspot(HardwareData h, HotspotData data)
        {
            if (h == null || data == null) return;
            Active = h;
            SelectedHotspot = data;
            Saved.MarkHotspotExplored(h.hardwareId, data.hotspotId);
            if (IsActivityComplete(h)) Saved.MarkARActivityCompleted(h.hardwareId);
            RefreshHotspots();
            if (AccessibilityManager.Instance.Settings.autoNarrate)
                AudioManager.Instance.Narration.Play($"{data.Label}. {data.Description}", data.Clip);
            Changed?.Invoke();
        }

        public void ClearSelection()
        {
            SelectedHotspot = null;
            RefreshHotspots();
            Changed?.Invoke();
        }

        /// <summary>Selects the next hotspot, preferring ones not explored yet.</summary>
        public void NextHotspot()
        {
            if (Active == null || Active.hotspots.Count == 0) return;
            int start = SelectedHotspot != null ? Active.hotspots.IndexOf(SelectedHotspot) : -1;
            for (int k = 1; k <= Active.hotspots.Count; k++)
            {
                var candidate = Active.hotspots[(start + k) % Active.hotspots.Count];
                if (!IsExplored(Active, candidate.hotspotId))
                {
                    SelectHotspot(Active, candidate);
                    return;
                }
            }
            SelectHotspot(Active, Active.hotspots[(start + 1) % Active.hotspots.Count]);
        }

        public bool IsExplored(HardwareData h, string hotspotId) =>
            Saved.Data.exploredHotspots.Contains($"{h.hardwareId}/{hotspotId}");

        public int ExploredCount(HardwareData h) => h.hotspots.Count(x => IsExplored(h, x.hotspotId));

        /// <summary>An AR activity counts as done after exploring at least two hotspots (or all, if fewer).</summary>
        public int RequiredCount(HardwareData h) => Mathf.Min(2, h.hotspots.Count);

        public bool IsActivityComplete(HardwareData h) =>
            h != null && (Saved.Data.completedARActivities.Contains(h.hardwareId) || ExploredCount(h) >= RequiredCount(h));

        private void RefreshHotspots()
        {
            var obj = ActiveObject;
            if (obj == null) return;
            obj.RefreshHotspots(SelectedHotspot?.hotspotId, id => IsExplored(obj.Hardware, id));
        }

        // ------------------------------------------------------------------ view controls
        public void ResetView()
        {
            ActiveObject?.ResetView();
            ClearSelection();
        }

        public float CycleZoom()
        {
            var obj = ActiveObject;
            if (obj == null) return 1f;
            float next = obj.Zoom < 1.25f ? 1.5f : obj.Zoom < 1.75f ? 2f : 1f;
            obj.SetZoom(next);
            Changed?.Invoke();
            return next;
        }

        public void Rotate(float degrees) => ActiveObject?.Rotate(degrees);

        // ------------------------------------------------------------------ simulation
        public void SimulateDetection(HardwareData h)
        {
            if (!IsSimulated || h == null) return;
            foreach (var t in targets.Values)
                if (t.Hardware != h) t.SetTracked(false);
            session.ShowSimulatedCard(h);
            if (targets.TryGetValue(h.hardwareId, out var target)) target.SetTracked(true);
        }

        public void SimulateLoss()
        {
            if (!IsSimulated) return;
            foreach (var t in targets.Values) t.SetTracked(false);
        }

        /// <summary>Recovery path when the camera/AR cannot start: reload the scanner in Simulation Mode.</summary>
        public void SwitchToSimulation()
        {
            ARSessionController.PreferSimulation = true;
            SceneLoader.Instance.Load(AppConstants.Scenes.ARScanner);
        }
    }
}
