using UnityEngine;
using TMPro;

using Sneaksters.Emotes;

namespace Sneaksters.Gameplay
{
    public class EyeBeacon : MonoBehaviour
    {

        int timer = 300;
        public TextMeshPro nameLabel;
        public GameObject nameLabelContainer;

        [SerializeField] MeshRenderer emoteRenderer = null;

        public void SetData(string name, Emote em = null) {
            print($"This emote's name is {em.name} and its sprite is {em.sprite.name} (overlay {em.overlaySprite.name})");
            nameLabel.text = name;

            // set emote stuff
            Sprite sprite = em.overlaySprite;
            Texture2D texture = sprite.texture;

            print($"Setting texture to {texture.name} from sprite {sprite.name}");

            emoteRenderer.material.SetTexture("_MainTex", texture);
        }


        void Update()
        {
            nameLabelContainer.transform.LookAt(PhotonCharacterController.localCharacterController.camera.transform.position);
            timer--;
            if (timer < 0) {Destroy(gameObject);}
        }
    }
}
