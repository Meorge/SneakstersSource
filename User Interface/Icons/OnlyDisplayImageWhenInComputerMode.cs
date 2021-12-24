using UnityEngine;
using UnityEngine.UI;

using Sneaksters.Gameplay;

namespace Sneaksters.UI
{
    public class OnlyDisplayImageWhenInComputerMode : MonoBehaviour
    {
        Image image;
        void Awake() {
            image = GetComponent<Image>();
        }
        void Update()
        {
            if (PhotonCharacterController.localCharacterController == null) { image.enabled = true; return; }
            image.enabled = PhotonCharacterController.localCharacterController.playerType == PlayerType.ComputerGuy;
        }
    }
}