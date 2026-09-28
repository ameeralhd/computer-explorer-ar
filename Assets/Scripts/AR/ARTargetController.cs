using System;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using UnityEngine;
#if VUFORIA_ENGINE
using Vuforia;
#endif

namespace ComputerExplorer.AR
{
    /// <summary>
    /// Tracking state and events for one image target. Wraps a Vuforia ObserverBehaviour when Vuforia is
    /// installed, or a simulated target (Editor / devices without AR) otherwise, so the rest of the app never
    /// depends on Vuforia types.
    /// </summary>
    public class ARTargetController : MonoBehaviour
    {
        public HardwareData Hardware { get; private set; }
        public bool IsTracked { get; private set; }

        /// <summary>(target, isTracked)</summary>
        public event Action<ARTargetController, bool> TrackingChanged;

        public void SetTracked(bool tracked)
        {
            if (IsTracked == tracked) return;
            IsTracked = tracked;
            TrackingChanged?.Invoke(this, tracked);
        }

        public static ARTargetController CreateSimulated(HardwareData h, Transform parent)
        {
            var go = new GameObject("SimTarget_" + h.arTargetName);
            go.transform.SetParent(parent, false);
            var ctrl = go.AddComponent<ARTargetController>();
            ctrl.Hardware = h;
            return ctrl;
        }

        /// <summary>Loads the target image for runtime Image Target creation (Resources/ARTargets/&lt;name&gt;).</summary>
        public static Texture2D LoadTargetTexture(HardwareData h)
        {
            var tex = h.arTargetImage != null ? h.arTargetImage : Resources.Load<Texture2D>(AppConstants.ResourcePaths.ARTargets + h.arTargetName);
            if (tex == null) return null;
            if (tex.format == TextureFormat.RGBA32 || tex.format == TextureFormat.RGB24) return tex;
            // Vuforia needs an uncompressed RGB(A) texture; convert if the importer compressed it.
            try
            {
                var copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false) { name = tex.name };
                copy.SetPixels(tex.GetPixels());
                copy.Apply();
                return copy;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AR] Target texture '{tex.name}' is not readable. Run 'Computer Explorer > Setup Project'. {e.Message}");
                return tex;
            }
        }

#if VUFORIA_ENGINE
        private ObserverBehaviour observer;

        /// <summary>
        /// Creates a Vuforia Image Target at runtime. With a device database (Target Manager export imported into
        /// StreamingAssets/Vuforia) the target is loaded from it; otherwise it is created directly from the PNG,
        /// which requires no Target Manager setup.
        /// </summary>
        public static ARTargetController CreateVuforia(HardwareData h, bool useDeviceDatabase, string databaseName)
        {
            ObserverBehaviour obs;
            if (useDeviceDatabase)
            {
                obs = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget($"Vuforia/{databaseName}.xml", h.arTargetName);
            }
            else
            {
                var tex = LoadTargetTexture(h);
                if (tex == null)
                {
                    Debug.LogError($"[AR] No target image for {h.hardwareId} (Resources/ARTargets/{h.arTargetName}).");
                    return null;
                }
                obs = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(tex, h.printedWidthMeters, h.arTargetName);
            }
            if (obs == null) return null;
            var ctrl = obs.gameObject.AddComponent<ARTargetController>();
            ctrl.Hardware = h;
            ctrl.observer = obs;
            obs.OnTargetStatusChanged += ctrl.OnStatusChanged;
            return ctrl;
        }

        private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
        {
            // EXTENDED_TRACKED keeps the object anchored when the card briefly leaves the view.
            SetTracked(status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED);
        }

        private void OnDestroy()
        {
            if (observer != null) observer.OnTargetStatusChanged -= OnStatusChanged;
        }
#endif
    }
}
