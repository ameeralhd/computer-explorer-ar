using UnityEngine;
using UnityEngine.EventSystems;

namespace ComputerExplorer.AR
{
    /// <summary>
    /// Tap and drag interactions on the AR view. Lives on a transparent full-screen UI layer behind the AR
    /// controls, so it works with either input system: tap → physics ray → hotspot; horizontal drag → rotate.
    /// </summary>
    public class ARInteractionController : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler
    {
        private const float DegreesPerPixel = 0.35f;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging || ARManager.Instance == null) return;
            var cam = Camera.main;
            if (cam == null) return;
            var ray = cam.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out var hit, 50f))
            {
                var hotspot = hit.collider.GetComponentInParent<ARHotspot>();
                if (hotspot != null) ARManager.Instance.SelectHotspot(hotspot);
            }
        }

        public void OnBeginDrag(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData)
        {
            if (ARManager.Instance != null) ARManager.Instance.Rotate(-eventData.delta.x * DegreesPerPixel);
        }
    }
}
