using UnityEngine;
using UnityEngine.UI;

namespace Sneaksters.UI.Gameplay
{
    public class HackableDoorIcon : MonoBehaviour
    {
        SimpleIconOverlay iconOverlay;

        [SerializeField]
        Image cooldownTimer = null;

        Image imageComponent;

        [SerializeField]
        Sprite doorOpenSprite = null, doorClosedSprite = null;
        
        void Awake()
        {
            iconOverlay = GetComponent<SimpleIconOverlay>();
            imageComponent = GetComponent<Image>();
        }

        public void SetObjectToTrack(GameObject g) { iconOverlay.SetObjectToTrack(g); }
        public void SetParent(Transform t) { iconOverlay.SetParent(t); }
        public void SetParentToIconContainer() { iconOverlay.SetParentToIconContainer(); }

        public void SetCooldownTimerValue(float value) {
            if (cooldownTimer == null) return;
            cooldownTimer.fillAmount = value;
        }

        public void SetDoorState(bool open) {
            imageComponent.sprite = open ? doorOpenSprite : doorClosedSprite;
        }
    }
}