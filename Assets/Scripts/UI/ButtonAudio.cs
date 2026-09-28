using ComputerExplorer.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI
{
    /// <summary>Plays the shared click sound for any Button. Added by UIKit so no button duplicates audio logic.</summary>
    [RequireComponent(typeof(Button))]
    public class ButtonAudio : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            });
        }
    }
}
