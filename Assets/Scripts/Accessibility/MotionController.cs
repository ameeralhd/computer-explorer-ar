using System.Collections;
using UnityEngine;

namespace ComputerExplorer.Accessibility
{
    /// <summary>
    /// All animation goes through here so reduced-motion mode can disable non-essential motion in one place.
    /// Essential state changes still happen — they just happen instantly.
    /// </summary>
    public static class MotionController
    {
        public static bool Reduced => AccessibilityManager.Instance != null && AccessibilityManager.Instance.ReducedMotion;

        public static float Duration(float seconds) => Reduced ? 0f : seconds;

        public static IEnumerator Fade(CanvasGroup group, float from, float to, float seconds)
        {
            if (group == null) yield break;
            seconds = Duration(seconds);
            if (seconds <= 0f)
            {
                group.alpha = to;
                yield break;
            }
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                if (group == null) yield break;
                group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, t / seconds));
                yield return null;
            }
            if (group != null) group.alpha = to;
        }

        public static IEnumerator Move(Transform target, Vector3 from, Vector3 to, float seconds)
        {
            seconds = Duration(seconds);
            if (seconds <= 0f)
            {
                if (target != null) target.localPosition = to;
                yield break;
            }
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                if (target == null) yield break;
                target.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t / seconds));
                yield return null;
            }
            if (target != null) target.localPosition = to;
        }
    }

    /// <summary>Decorative gentle bobbing (e.g. mascot). Disabled in reduced-motion mode.</summary>
    public class FloatAnimator : MonoBehaviour
    {
        public float amplitude = 12f;
        public float speed = 1.4f;
        private Vector2 origin;
        private RectTransform rt;
        private bool captured;

        private void OnEnable()
        {
            rt = transform as RectTransform;
            captured = false;
        }

        private void LateUpdate()
        {
            if (rt == null) return;
            if (!captured)
            {
                origin = rt.anchoredPosition;
                captured = true;
            }
            rt.anchoredPosition = MotionController.Reduced
                ? origin
                : origin + Vector2.up * Mathf.Sin(Time.unscaledTime * speed) * amplitude;
        }
    }

    /// <summary>Soft pulsing alpha to indicate "searching" states. Static when reduced motion is on.</summary>
    public class PulseAnimator : MonoBehaviour
    {
        public float speed = 2.2f;
        public float minAlpha = 0.45f;
        private CanvasGroup group;

        private void Awake()
        {
            if (!TryGetComponent(out group)) group = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            group.alpha = MotionController.Reduced
                ? 1f
                : Mathf.Lerp(minAlpha, 1f, (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f);
        }
    }

    /// <summary>Continuous rotation for loading indicators. Reduced motion shows a static indicator.</summary>
    public class SpinAnimator : MonoBehaviour
    {
        public float degreesPerSecond = -240f;

        private void Update()
        {
            if (!MotionController.Reduced) transform.Rotate(0f, 0f, degreesPerSecond * Time.unscaledDeltaTime);
        }
    }
}
