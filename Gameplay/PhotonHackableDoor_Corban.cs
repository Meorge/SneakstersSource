using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using Sneaksters.UI.Gameplay;

namespace Sneaksters.Gameplay
{
	public class PhotonHackableDoor_Corban : Photon.Pun.MonoBehaviourPunCallbacks {
		public Animator animator;

		public HackableDoorIcon iconOverlay;

		public GameObject iconPoint;

		public GameObject iconOverlayPrefab;

		public List<Light> doorActualLights;

		public Sprite doorUnlockedIcon;
		public Sprite doorLockedIcon;

		public float doorLockTimeLeft = 0f;

		public float doorLockMaxTime = 1f;

		public static int vID = 4;

		void Awake() {
			photonView.ViewID = vID;
			vID++;
		}
		// Use this for initialization
		void Start () {
			// Assigning the animator so that the script can open and close the door.
			animator = GetComponent<Animator>();

			// Set up icon overlay
			iconOverlay = Instantiate(iconOverlayPrefab).GetComponent<HackableDoorIcon>();
			iconOverlay.SetObjectToTrack(iconPoint);
			iconOverlay.SetParentToIconContainer();
		}

		public void OnMouseUp() {
			// Use this line of code to determine whether or not the current player is Mission Control.
			bool isMissionControl = PhotonCharacterController.localCharacterController.playerType == PlayerType.ComputerGuy ? true : false;

			// This line of code opens the door for everyone.

			if (isMissionControl && doorLockTimeLeft == 0f && PhotonCharacterController.localCharacterController.currentStamina > 0f) {
				StartCooldown();
				PhotonCharacterController.localCharacterController.currentStamina -= PhotonCharacterController.localCharacterController.initialStaminaDown * 4;
				photonView.RPC("ToggleDoor", RpcTarget.All);
			}
			
		}

		[Photon.Pun.PunRPC]
		void ToggleDoor() {
			// Debug.LogError("TOGGLE DOOR");
			animator.SetBool("doorIsOpen", !animator.GetBool("doorIsOpen"));
		}

		public void ForceDoorState(bool doorState) {
			photonView.RPC("ForceOpenDoor_RPC", RpcTarget.AllBufferedViaServer, doorState);
		}



		[Photon.Pun.PunRPC]
		void ForceOpenDoor_RPC(bool doorState) {
			if (doorState == true) {OpenDoor();}
			else {CloseDoor();}
		}


		void StartCooldown() {
			doorLockTimeLeft = doorLockMaxTime;
		}

		void Update() {
			if (doorLockTimeLeft > 0f) {
				//cooldownTimer.enabled = true;
				iconOverlay.SetCooldownTimerValue(doorLockTimeLeft / doorLockMaxTime);
				doorLockTimeLeft -= Time.deltaTime;

			} else {
				iconOverlay.SetCooldownTimerValue(0f);
				doorLockTimeLeft = 0f;
			}

			bool doorIsOpen = animator.GetBool("doorIsOpen");

			iconOverlay.SetDoorState(doorIsOpen);
		}

		void OpenDoor() {
			animator.SetBool("doorIsOpen", true);
		}

		void CloseDoor() {
			animator.SetBool("doorIsOpen", false);
		}
	}
}