using UnityEngine;
using UnityEngine.UI;

using Sneaksters.UI;
using Sneaksters.UI.Animation;

namespace Sneaksters.Emotes
{
    public class EmoteGameObject : MonoBehaviour
    {

        public ControllerButtonGraphic controllerButtonGraphic;
        public Image emotePreview;
        public Image emotePreviewEffect;

        public Shader iconShader;
        public SingleEmotePreviewAnimator animator;

        void Start() {
        }

        public void SetupEmote(Emote emote, int key, Color col, bool useRecolorShader = true) {
            if (useRecolorShader) {
                // set up for main emote
                emotePreview.material = new Material(iconShader);
                // emotePreview.sprite = emote.sprite;
                emotePreview.material.SetTexture("_MainTex", emote.sprite.texture);
                if (emote.overlaySprite != null) emotePreview.material.SetTexture("_OverlayTex", emote.overlaySprite.texture);
                emotePreview.material.SetColor("_ReplaceBlackColor", col);


                // set up for emote preview
                emotePreviewEffect.material = new Material(iconShader);
                // emotePreviewEffect.sprite = emote.sprite;
                emotePreviewEffect.material.SetTexture("_MainTex", emote.sprite.texture);
                if (emote.overlaySprite != null) emotePreviewEffect.material.SetTexture("_OverlayTex", emote.overlaySprite.texture);
                emotePreviewEffect.material.SetColor("_ReplaceBlackColor", col);
                emotePreviewEffect.material.SetFloat("_Alpha", 0f);

            } else {
                emotePreview.sprite = emote.sprite;
            }
        }
    }
}