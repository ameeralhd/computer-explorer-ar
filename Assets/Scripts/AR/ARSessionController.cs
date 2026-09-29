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
    /// Starts the AR session. Uses Vuforia Engine when the package is installed (VUFORIA_ENGINE is defined
    /// automatically by the editor tooling); otherwise — or when the learner chooses it — runs a
    /// Simulation Mode: a virtual camera looking at a printed target card on a desk, so every AR lesson can be
    /// tested in the Editor and remains usable on devices without a working camera.
    /// </summary>
    public class ARSessionController : MonoBehaviour
    {
        [SerializeField] private Camera arCamera;
        [Tooltip("Always use Simulation Mode (useful for UI work in the Editor).")]
        [SerializeField] private bool forceSimulation;
        [Tooltip("Load targets from a Vuforia device database in StreamingAssets/Vuforia instead of the PNGs in Resources/ARTargets.")]
        [SerializeField] private bool useDeviceDatabase;
        [SerializeField] private string databaseName = "ComputerExplorer";

        /// <summary>Set by the "Gunakan Mode Simulasi" recovery button; lasts for the app session.</summary>
        public static bool PreferSimulation { get; set; }

        public bool IsSimulated { get; private set; }
        public event Action Ready;
        public event Action<string> Failed;

        private Transform simRoot;
        private Renderer simCard;

        public static bool VuforiaAvailable
        {
            get
            {
#if VUFORIA_ENGINE
                return true;
#else
                return false;
#endif
            }
        }

        public void Begin()
        {
            if (arCamera == null) arCamera = Camera.main;
#if VUFORIA_ENGINE
            if (!forceSimulation && !PreferSimulation)
            {
                BeginVuforia();
                return;
            }
            // Simulation inside a Vuforia scene: deactivate the AR camera (stops Vuforia) and use a plain camera.
            if (VuforiaBehaviour.Instance != null)
            {
                VuforiaBehaviour.Instance.gameObject.SetActive(false);
                // (The single AudioListener lives on the persistent AppManager.)
                arCamera = new GameObject("SimulationCamera") { tag = "MainCamera" }.AddComponent<Camera>();
            }
#endif
            BeginSimulation();
        }

        public ARTargetController CreateTarget(HardwareData h)
        {
#if VUFORIA_ENGINE
            if (!IsSimulated) return ARTargetController.CreateVuforia(h, useDeviceDatabase, databaseName);
#endif
            return ARTargetController.CreateSimulated(h, simRoot);
        }

        // ------------------------------------------------------------------ Vuforia
#if VUFORIA_ENGINE
        private void BeginVuforia()
        {
            IsSimulated = false;
            if (VuforiaBehaviour.Instance == null && arCamera != null)
                arCamera.gameObject.AddComponent<VuforiaBehaviour>();

            VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
            if (VuforiaApplication.Instance.IsRunning) Ready?.Invoke();
            else VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        }

        private void OnVuforiaInitialized(VuforiaInitError error)
        {
            if (error == VuforiaInitError.NONE) return;
            string code = error.ToString();
            string message =
                code.Contains("LICENSE") ? Loc.T("Kunci lisensi Vuforia belum diatur atau tidak valid. Minta guru/pengembang mengisi License Key di Vuforia Configuration.",
                                                 "The Vuforia license key is missing or invalid. Ask your teacher/developer to enter the License Key in Vuforia Configuration.") :
                code.Contains("PERMISSION") || code.Contains("CAMERA") ? Loc.T("Aplikasi tidak dapat membuka kamera. Izinkan akses kamera di Pengaturan perangkat, lalu coba lagi.",
                                                                               "The app cannot open the camera. Allow camera access in your device Settings, then try again.") :
                code.Contains("DEVICE") ? Loc.T("Perangkat ini belum didukung oleh Vuforia AR.", "This device is not supported by Vuforia AR.") :
                Loc.T($"AR tidak dapat dimulai ({code}).", $"AR could not start ({code}).");
            Failed?.Invoke(message);
        }

        private void OnVuforiaStarted()
        {
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
            Ready?.Invoke();
        }

        private void OnDestroy()
        {
            if (VuforiaApplication.Instance == null) return;
            VuforiaApplication.Instance.OnVuforiaInitialized -= OnVuforiaInitialized;
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        }
#endif

        // ------------------------------------------------------------------ Simulation
        private void BeginSimulation()
        {
            IsSimulated = true;
            simRoot = new GameObject("SimulatedTargets").transform;

            if (arCamera != null)
            {
                arCamera.clearFlags = CameraClearFlags.SolidColor;
                arCamera.backgroundColor = new Color(0.06f, 0.08f, 0.13f);
                arCamera.fieldOfView = 55f;
                arCamera.nearClipPlane = 0.01f;
                arCamera.farClipPlane = 20f;
                arCamera.transform.position = new Vector3(0f, 0.3f, -0.26f);
                arCamera.transform.LookAt(new Vector3(0f, 0.015f, 0.01f));
            }

            // Desk and printed card, sized like the real 15 cm target.
            HardwareModelFactory.Part(simRoot, PrimitiveType.Cube, "Desk", new Vector3(0, -0.006f, 0.1f), new Vector3(1.4f, 0.01f, 1f),
                HardwareModelFactory.C("C8B6A0"));
            float w = AppConstants.TargetPrintedWidthMeters;
            var card = GameObject.CreatePrimitive(PrimitiveType.Quad);
            card.name = "TargetCard";
            Destroy(card.GetComponent<Collider>());
            card.transform.SetParent(simRoot, false);
            card.transform.localPosition = new Vector3(0, 0.0005f, 0);
            card.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            card.transform.localScale = new Vector3(w, w, 1f);
            simCard = card.GetComponent<Renderer>();
            simCard.material = new Material(simCard.sharedMaterial);

            if (FindAnyObjectByType<Light>() == null)
            {
                var light = new GameObject("Sim Light").AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            Ready?.Invoke();
        }

        /// <summary>Show a target's printed card on the simulated desk.</summary>
        public void ShowSimulatedCard(HardwareData h)
        {
            if (simCard == null) return;
            var tex = h != null ? ARTargetController.LoadTargetTexture(h) : null;
            simCard.material.mainTexture = tex;
            simCard.material.color = tex != null ? Color.white : new Color(0.9f, 0.9f, 0.9f);
        }
    }
}
