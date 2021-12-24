using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Sneaksters.Emotes;

namespace Sneaksters.UI.Gameplay
{
    public class ThiefIcon : MonoBehaviour
    {
        SimpleIconOverlay iconOverlay;

        Image imageComponent;

        [SerializeField]
        TextMeshProUGUI nameLabel = null;

        [SerializeField]
        Shader thiefIconShader = null;

        Material thiefIconMaterial;

        [SerializeField]
        Sprite defaultThiefSprite = null;

        float emoteTimerMax = 5f;
        float emoteTimer = 0f;

        void Awake() {
            iconOverlay = GetComponent<SimpleIconOverlay>();
            imageComponent = GetComponent<Image>();
            imageComponent.sprite = null;
            imageComponent.color = Color.white;
        }

        void Update() {
            float emoteTimerBefore = emoteTimer;
            emoteTimer = Mathf.Clamp(emoteTimer - Time.deltaTime, 0f, Mathf.Infinity);
            if (emoteTimer == 0f && emoteTimerBefore != 0f) {
                Debug.Log("Emote timer is up, so set textures to defaults again");
                imageComponent.sprite = defaultThiefSprite;
                SetTextures(defaultThiefSprite.texture, Texture2D.blackTexture);
            }
        }

        public void SetObjectToTrack(GameObject g) { iconOverlay.SetObjectToTrack(g); }
        public void SetParent(Transform t) { iconOverlay.SetParent(t); }
        public void SetParentToIconContainer() { iconOverlay.SetParentToIconContainer(); }

        public void SetPlayerName(string name) {
            nameLabel.text = name;
        }

        public void SetUpImageForColor(Color color) {
            thiefIconMaterial = new Material(thiefIconShader);
            thiefIconMaterial.SetColor("_ReplaceBlackColor", color);
            thiefIconMaterial.SetFloat("_Alpha", 1f);
            imageComponent.sprite = defaultThiefSprite;
            SetTextures(defaultThiefSprite.texture, Texture2D.blackTexture);
        }

        public void SetOpacity(float opacity) {
            thiefIconMaterial.SetFloat("_Alpha", opacity);
            nameLabel.SetAlpha(opacity);
        }

        public void SetTextures(Texture2D main, Texture2D overlay) {
            Debug.Log($"SetTextures with main={main} and overlay={overlay}");
            thiefIconMaterial.SetTexture("_MainTex", main);
            thiefIconMaterial.SetTexture("_OverlayTex", overlay);
            imageComponent.material = thiefIconMaterial;
        }

        public void DoEmote(Emote em) {
            imageComponent.sprite = em.sprite;
            SetTextures(em.sprite.texture, em.overlaySprite.texture);
            emoteTimer = emoteTimerMax;
        }


    }
}