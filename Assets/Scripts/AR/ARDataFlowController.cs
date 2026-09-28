using System;
using ComputerExplorer.Accessibility;
using UnityEngine;

namespace ComputerExplorer.AR
{
    /// <summary>
    /// Visual data-flow sequence on the Von Neumann model: a data packet travels Input → Memory → CPU → Output.
    /// Reduced motion replaces smooth travel with discrete steps (the learning content is preserved).
    /// </summary>
    public class ARDataFlowController : MonoBehaviour
    {
        public static readonly string[] StepCaptions =
        {
            "Input › Memori: data dari keyboard disimpan di RAM",
            "Memori › CPU: CPU mengambil instruksi dan data (fetch)",
            "CPU › Output: hasil proses dikirim ke monitor",
            "Output › Input: siklus berulang untuk data berikutnya"
        };

        public event Action<int> StepChanged;
        public int Step { get; private set; }
        public string Caption => StepCaptions[Step];

        private Transform packet;
        private Vector3[] points;
        private float progress;
        private const float Speed = 0.45f; // model units per second
        private const float DiscreteStepSeconds = 2.2f;

        public void Init(Transform modelSpace)
        {
            points = new[]
            {
                HardwareModelFactory.VnInput, HardwareModelFactory.VnMemory, HardwareModelFactory.VnCpu, HardwareModelFactory.VnOutput
            };
            packet = HardwareModelFactory.Part(modelSpace, PrimitiveType.Sphere, "DataPacket", points[0], Vector3.one * 0.06f,
                HardwareModelFactory.C("FACC15"));
        }

        private void Update()
        {
            if (packet == null) return;
            var from = points[Step];
            var to = points[(Step + 1) % points.Length];
            if (MotionController.Reduced)
            {
                progress += Time.deltaTime / DiscreteStepSeconds;
                packet.localPosition = from;
            }
            else
            {
                float length = Vector3.Distance(from, to);
                progress += Time.deltaTime * Speed / Mathf.Max(0.01f, length);
                var p = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, Mathf.Clamp01(progress)));
                p.y += Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI) * 0.12f; // arc above the board
                packet.localPosition = p;
            }
            if (progress >= 1f)
            {
                progress = 0f;
                Step = (Step + 1) % points.Length;
                StepChanged?.Invoke(Step);
            }
        }

        public void SetVisible(bool visible)
        {
            if (packet != null) packet.GetComponent<Renderer>().enabled = visible;
            enabled = visible;
        }
    }
}
