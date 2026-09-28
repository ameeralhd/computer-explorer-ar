using UnityEngine;

namespace ComputerExplorer.UI.Components
{
    /// <summary>
    /// Spoken/readable name for an interactive element (icon buttons, toggles). Used by screen narration
    /// and automated UI tests, and the natural hook for a future platform screen-reader bridge.
    /// </summary>
    public class AccessibleLabel : MonoBehaviour
    {
        public string label;
    }
}
