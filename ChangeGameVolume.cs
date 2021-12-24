using UnityEngine;
using Sneaksters.Gameplay;

namespace Sneaksters
{
    public class ChangeGameVolume : MonoBehaviour {
        public void MuteSFX() {
            // PhotonGameManager.gameManager.audioMixer.SetFloat("ingameSfxVol", -80f);
            if (PhotonCharacterController.localCharacterController != null) {
                PhotonCharacterController.localCharacterController.thiefStatus = PhotonCharacterController.ThiefStatus.WaitingToStart;
            }
        }

        public void UnmuteSFX() {
            // PhotonGameManager.gameManager.audioMixer.SetFloat("ingameSfxVol", 0f);
            if (PhotonCharacterController.localCharacterController != null) {
                PhotonCharacterController.localCharacterController.thiefStatus = PhotonCharacterController.ThiefStatus.Active;
            }
        }
    }
}