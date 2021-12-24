using UnityEngine;
using Photon.Pun;

using Sneaksters.UI;

namespace Sneaksters.Gameplay
{
    public class PhotonGemSack : MonoBehaviourPunCallbacks, IPunObservable
    {

        public Animator animator;

        public SimpleIconOverlay iconOverlay;
        public GameObject iconOverlayPrefab;

        public MeshRenderer sackMeshRenderer = null;

        void Start() {
            if (PhotonStageManager.stageManager == null) {return;}
            PhotonStageManager.stageManager.gemSack = this;

            iconOverlay = Instantiate(iconOverlayPrefab).GetComponent<SimpleIconOverlay>();
            iconOverlay.SetObjectToTrack(gameObject);
            iconOverlay.SetParentToIconContainer();


            if (LevelBuilder.levelBuilder != null)
            {
                //print("PhotonGemSack.Start() - parent the gem sack to the room container, NOT keeping its world position");
                transform.SetParent(LevelBuilder.levelBuilder?.roomsContainer.transform, false);
            }
            else
            {
                //print("PhotonGemSack.Start() - LevelBuilder is null so we can't parent the gem sack to the room container");
            }

            SetUpAppearance();

        }

        void SetUpAppearance()
        {
            // Determine what the id of the current appearance is
            string appID = "default";

            if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gemAppearance"))
            {
                appID = PhotonNetwork.CurrentRoom.CustomProperties["gemAppearance"].ToString();
            } else
            {
                // Handle not being connected - maybe just don't do anything? idk
            }

            // Search the list of gemstone appearances to find the correct one
            GemstoneAppearance appearance = PhotonGameManager.Instance.gemstoneAppearances?.GetGemstoneAppearanceByID(appID);

            if (appearance == null)
            {
                Debug.LogError($"PhotonGemstone.SetUpAppearance() - appearance found for id \"{appID}\" is null! Trying to load default appearance...");
                appearance = PhotonGameManager.Instance.gemstoneAppearances?.GetGemstoneAppearanceByID("default");

                if (appearance == null)
                {
                    Debug.LogError($"PhotonGemstone.SetUpAppearance() - Couldn't load default appearance! That sucks");
                    return;
                }
            }

            // Set the texture of the gem sack
            sackMeshRenderer.material.SetTexture("_MainTex", appearance.gemSackTexture);
        }

        void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Player") {
                PhotonCharacterController thisChar = other.gameObject.GetComponent<PhotonCharacterController>();
                if (thisChar.userID != PhotonNetwork.LocalPlayer.UserId) {return;}

                if (thisChar.gemstone == null) {return;}
                Debug.Log(thisChar.playerName + " is dropping a gemstone in the sack!");

                int indexOfGemstone = PhotonStageManager.stageManager.gemstones.IndexOf(thisChar.gemstone);

                thisChar.gemstone.RunDropGemInSackRPC();
            }
        }

        public void DropGemInSack() {

            animator.SetTrigger("getGem");
        }

        void Photon.Pun.IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        }
    }
}