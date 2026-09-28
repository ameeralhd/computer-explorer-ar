using System.Collections;
using System.Collections.Generic;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Data;
using UnityEngine;

namespace ComputerExplorer.AR
{
    /// <summary>
    /// Lifecycle and presentation of one 3D hardware object attached to a target: show/hide on tracking,
    /// user rotation and zoom, and hotspot states. Created once per target (no repeated instantiation).
    /// </summary>
    public class ARObjectController : MonoBehaviour
    {
        public HardwareData Hardware { get; private set; }
        public IReadOnlyList<ARHotspot> Hotspots => hotspots;
        public ARDataFlowController DataFlow { get; private set; }
        public float Zoom { get; private set; } = 1f;
        public bool Visible { get; private set; }

        private readonly List<ARHotspot> hotspots = new List<ARHotspot>();
        private Transform pivot;
        private float baseScale;
        private float yaw;
        private Coroutine intro;

        public const float MinZoom = 0.6f, MaxZoom = 2.5f;

        public static ARObjectController Create(HardwareData h, Transform target, float printedWidthMeters)
        {
            var root = new GameObject("AR_" + h.hardwareId);
            root.transform.SetParent(target, false);
            root.transform.localPosition = new Vector3(0, 0.002f, 0);
            var ctrl = root.AddComponent<ARObjectController>();
            ctrl.Build(h, printedWidthMeters);
            return ctrl;
        }

        private void Build(HardwareData h, float printedWidth)
        {
            Hardware = h;
            pivot = new GameObject("Pivot").transform;
            pivot.SetParent(transform, false);
            // Model space is ~1 unit wide; make it ~80% of the printed target width.
            baseScale = printedWidth * 0.8f;

            if (h.modelPrefab != null) Instantiate(h.modelPrefab, pivot, false);
            else HardwareModelFactory.Build(h, pivot);

            foreach (var data in h.hotspots) hotspots.Add(ARHotspot.Create(h, data, pivot));

            if (h.hardwareId == Core.AppConstants.HardwareIds.VonNeumann)
            {
                DataFlow = gameObject.AddComponent<ARDataFlowController>();
                DataFlow.Init(pivot);
            }
            ApplyTransform();
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
            foreach (var hs in hotspots) hs.SetVisible(visible);
            if (DataFlow != null) DataFlow.SetVisible(visible);
            if (visible)
            {
                if (intro != null) StopCoroutine(intro);
                intro = StartCoroutine(Intro());
            }
        }

        private IEnumerator Intro()
        {
            float d = MotionController.Duration(0.25f);
            for (float t = 0; t < d; t += Time.deltaTime)
            {
                pivot.localScale = Vector3.one * WorldCorrectedScale * Zoom * Mathf.Lerp(0.7f, 1f, Mathf.SmoothStep(0, 1, t / d));
                yield return null;
            }
            ApplyTransform();
        }

        public void Rotate(float degrees)
        {
            yaw += degrees;
            ApplyTransform();
        }

        public void SetZoom(float zoom)
        {
            Zoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
            ApplyTransform();
        }

        public void ResetView()
        {
            yaw = 0f;
            Zoom = 1f;
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            pivot.localScale = Vector3.one * WorldCorrectedScale * Zoom;
            pivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        /// <summary>
        /// Target transforms may carry the target width as their scale (Vuforia) or be unit-scaled (simulation);
        /// dividing by the parent's scale keeps the model's real-world size identical in both cases.
        /// </summary>
        private float WorldCorrectedScale
        {
            get
            {
                float parentScale = transform.parent != null ? Mathf.Abs(transform.parent.lossyScale.x) : 1f;
                return baseScale / Mathf.Max(0.0001f, parentScale);
            }
        }

        /// <summary>Update every hotspot's visual state from the selection and the explored set.</summary>
        public void RefreshHotspots(string selectedId, System.Func<string, bool> isExplored, string highlightedId = null)
        {
            foreach (var hs in hotspots)
            {
                var state = hs.Data.hotspotId == selectedId ? HotspotState.Selected
                    : hs.Data.hotspotId == highlightedId ? HotspotState.Highlighted
                    : HotspotState.Idle;
                hs.SetState(state, isExplored(hs.Data.hotspotId));
            }
        }

        public ARHotspot Find(string hotspotId) => hotspots.Find(h => h.Data.hotspotId == hotspotId);
    }
}
