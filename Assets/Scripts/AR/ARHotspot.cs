using ComputerExplorer.Accessibility;
using ComputerExplorer.Data;
using UnityEngine;

namespace ComputerExplorer.AR
{
    public enum HotspotState { Idle, Highlighted, Selected }

    /// <summary>
    /// Interactive component hotspot on a 3D model. Tapping it (via ARInteractionController) opens its explanation.
    /// States: idle (amber, gently pulsing), highlighted, selected (primary, larger). Explored hotspots turn green and
    /// their label gets a "dilihat" suffix — state is never shown by colour alone.
    /// </summary>
    public class ARHotspot : MonoBehaviour
    {
        public HotspotData Data { get; private set; }
        public HardwareData Hardware { get; private set; }
        public HotspotState State { get; private set; }
        public bool Explored { get; private set; }

        private Renderer rend;
        private ARLabel label;
        private float baseScale;
        private const float Size = 0.075f;

        private static readonly Color IdleColor = new Color(0.97f, 0.56f, 0.04f);
        private static readonly Color HighlightColor = new Color(1f, 0.84f, 0.2f);
        private static readonly Color ExploredColor = new Color(0.09f, 0.64f, 0.29f);

        public static ARHotspot Create(HardwareData hardware, HotspotData data, Transform modelSpace)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Hotspot_" + data.hotspotId;
            go.transform.SetParent(modelSpace, false);
            go.transform.localPosition = data.localPosition;
            go.transform.localScale = Vector3.one * Size;

            var col = go.GetComponent<SphereCollider>();
            // Generous hit area (about 2.4× the visible sphere); larger again with "large touch targets".
            col.radius = AccessibilityManager.Instance.Settings.largeTouchTargets ? 1.8f : 1.2f;

            var hs = go.AddComponent<ARHotspot>();
            hs.Data = data;
            hs.Hardware = hardware;
            hs.rend = go.GetComponent<Renderer>();
            hs.baseScale = Size;
            hs.label = ARLabel.Create(modelSpace, data.localPosition + Vector3.up * 0.11f, ShortLabel(data.label));
            hs.Apply();
            return hs;
        }

        public static string ShortLabel(string full)
        {
            int i = full.IndexOf('(');
            return i > 2 ? full.Substring(0, i).Trim() : full;
        }

        public void SetState(HotspotState state, bool explored)
        {
            State = state;
            Explored = explored;
            Apply();
        }

        public void SetVisible(bool visible)
        {
            rend.enabled = visible;
            GetComponent<Collider>().enabled = visible;
            label.SetVisible(visible);
        }

        private void Apply()
        {
            var p = ContrastController.Current;
            Color c = State == HotspotState.Selected ? p.Primary
                : State == HotspotState.Highlighted ? HighlightColor
                : Explored ? ExploredColor : IdleColor;
            if (p.IsHighContrast && State == HotspotState.Idle) c = Explored ? p.Success : Color.white;
            rend.sharedMaterial = HardwareModelFactory.MaterialFor(c);
            string text = ShortLabel(Data.label) + (Explored && State != HotspotState.Selected ? " · dilihat" : "");
            label.SetContent(text, State == HotspotState.Selected);
            transform.localScale = Vector3.one * baseScale * (State == HotspotState.Selected ? 1.35f : 1f);
        }

        private void Update()
        {
            if (State != HotspotState.Idle || Explored || MotionController.Reduced) return;
            float s = 1f + 0.15f * Mathf.Sin(Time.time * 4f);
            transform.localScale = Vector3.one * baseScale * s;
        }
    }
}
