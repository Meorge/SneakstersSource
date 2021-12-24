using UnityEngine;
using Sneaksters.Discord;

namespace Sneaksters.UI {
    public class DiscordRPIcon : MonoBehaviour
    {
        [Header("Opacity")]
        public float activeOpacity = 0.7f;
        public float inactiveOpacity = 0.2f;
        public float disabledOpacity = 0f;

        private UnityEngine.UI.Image image = null;
        
        // Start is called before the first frame update
        void Start()
        {
            image = GetComponent<UnityEngine.UI.Image>();
        }

        // Update is called once per frame
        void Update()
        {
            if (DiscordManager.Connected && DiscordManager.RichPresenceEnabled)
            {
                image.SetAlpha(activeOpacity);
            }
            else if (DiscordManager.RichPresenceEnabled)
            {
                image.SetAlpha(inactiveOpacity);
            }
            else
            {
                image.SetAlpha(disabledOpacity);
            }
        }
    }
}