using UnityEngine;
using UnityEngine.Animations;
using Photon.Pun;
using DG.Tweening;

using Sneaksters.UI;

namespace Sneaksters.Gameplay
{
	public class PhotonGemstone : MonoBehaviourPunCallbacks, IPunObservable {
		GameObject appearanceGameObject = null;
		public Tween appearanceRotTween = null;
		public Tween appearanceHoverTween = null;

		public SimpleIconOverlay iconOverlay;

		public GameObject iconOverlayPrefab;

		public ParentConstraint parentConstraint;

		public PhotonCharacterController characterHolding;

		public Vector3 pickedUpScale = new Vector3(0.35f, 0.35f, 0.35f);
		public Vector3 pickedUpOffset = new Vector3(0.28f, 0.44f, 0.08f);

		public Vector3 pickedUpRot = new Vector3(-60f, 90f, 0f);

		public Vector3 spawnPoint;

		public new Rigidbody rigidbody;

		public new Collider collider;

		public enum GemState {
			OnGround,
			BeingHeld,
			InSack
		}

		public GemState gemState;


		void Awake() {
			if (PhotonStageManager.stageManager != null && !PhotonStageManager.stageManager.gemstones.Contains(this)) {
				PhotonStageManager.stageManager.gemstones.Add(this);
			}
		}

		void OnDestroy() {
			if (PhotonStageManager.stageManager.gemstones.Contains(this)) PhotonStageManager.stageManager.gemstones.Remove(this);
		}


		// Use this for initialization
		void Start () {
			if (PhotonStageManager.stageManager == null) {return;}

			// Set up the icon overlay
			iconOverlay = Instantiate(iconOverlayPrefab).GetComponent<SimpleIconOverlay>();
			iconOverlay.SetObjectToTrack(gameObject);
			iconOverlay.SetParentToIconContainer();


			rigidbody = GetComponent<Rigidbody>();
			collider = GetComponent<Collider>();
			
			if (gemState != GemState.InSack) gemState = GemState.OnGround;


            if (LevelBuilder.levelBuilder != null) {
				//print("PhotonGemstone.Start() - parent the gemstone to the room container, NOT keeping its world position");
				transform.SetParent(LevelBuilder.levelBuilder?.roomsContainer.transform, false);
			} else {
				//print("PhotonGemstone.Start() - LevelBuilder is null so we can't parent the gem to the room container");
			}
			spawnPoint = transform.position;


			// Set up gemstone appearance
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

			// Instantiate the object prefab from that appearance and parent it to the gemstone object
			appearanceGameObject = Instantiate(appearance.objectPrefab);
			appearanceGameObject.transform.SetParent(transform, false);
			appearanceGameObject.transform.localPosition = Vector3.zero;

			// Set up tweens
			appearanceRotTween = appearanceGameObject.transform.DOLocalRotate(new Vector3(0f, 360, 0f), 16f, RotateMode.LocalAxisAdd)
				.SetEase(Ease.Linear)
				.SetLoops(-1, LoopType.Incremental);

			appearanceHoverTween = appearanceGameObject.transform.DOLocalMoveY(-0.2f, 6f)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo);

			// TODO: Update the icon to match
		}

		public void RunDropGemInSackRPC() {
			photonView.RPC("DropGemInSack", RpcTarget.AllBufferedViaServer);
		}

		[Photon.Pun.PunRPC]
		void DropGemInSack() {
			if (characterHolding != null) characterHolding.DropGemstone(PhotonStageManager.stageManager.gemSack.transform.position);
			gemState = GemState.InSack;

			PutInSack();

			PhotonStageManager.stageManager.CheckGemstones();

			PhotonStageManager.stageManager.gemSack.DropGemInSack();
		}
		
		// Update is called once per frame
		void Update () {
			
			if (PhotonGameManager.Instance != null && PhotonGameManager.Instance.gameState != GameState.InSession) {
				Debug.LogError("Game is no longer in session, so destroying the gemstone");
				PhotonNetwork.Destroy(photonView);
				//Destroy(gameObject);
				return;
			}
			
			if (gemState == GemState.InSack) {
				//Debug.LogError(string.Format("Gem {0} in sack", PhotonStageManager.stageManager.gemstones.IndexOf(this)));
				transform.localScale = Vector3.zero;
				transform.position = new Vector3(0f,-200f,0f);
				rigidbody.useGravity = false;
				collider.enabled = false;
				return;
			} else {
				if (parentConstraint.isActiveAndEnabled && characterHolding != null) {
					parentConstraint.SetTranslationOffset(0, pickedUpOffset);
					parentConstraint.SetRotationOffset(0, pickedUpRot);
					//xwDebug.Log("Gemstone is being held by " + characterHolding.playerName);
					//transform.localScale = pickedUpScale;
				} else {
					//Debug.Log("Gemstone is idle");

					if (transform.position.y < -100f) {
						transform.position = spawnPoint; // respawn if it gets too far down
                        print($"Return gemstone to {spawnPoint}");
                    }
				}
			}
		}

		[Photon.Pun.PunRPC]
		public void AttachToPlayer(string playerID) {
			characterHolding = null;
			bool foundAPlayer = false;
			foreach (PhotonCharacterController player in PhotonCharacterController.characterControllers) {
				if (player.userID == playerID) {
					characterHolding = player;
					Debug.LogWarning("The gemstone is being picked up by the player " + characterHolding.playerName);
					foundAPlayer = true;
					break;
				}
			}
			if (foundAPlayer == false) {Debug.LogError("No player found by this ID"); return;}
			//characterHolding = player.GetComponent<PhotonCharacterController>();

			ConstraintSource constraintSource = new ConstraintSource();
			constraintSource.sourceTransform = characterHolding.chestTransform;
			constraintSource.weight = 1;
			parentConstraint.SetSource(0, constraintSource);
			parentConstraint.constraintActive = true;
			rigidbody.useGravity = false;

			characterHolding.PickUpGemstone(this);
			transform.localScale = pickedUpScale;
			gemState = GemState.BeingHeld;
			collider.enabled = false;
			//photonView.TransferOwnership(characterHolding.photonView.Owner);
		}

		public void OnCharEnter(Collider other) {
			if (gemState == GemState.BeingHeld || characterHolding != null || gemState == GemState.InSack) {return;}
			if (other.gameObject.tag == "Player") {
				if (other.gameObject.GetComponent<PhotonCharacterController>().userID != PhotonNetwork.LocalPlayer.UserId && PhotonNetwork.IsConnected) {return;}
				if (other.gameObject.GetComponent<PhotonCharacterController>().gemstone != null) {return;}
				//Debug.Log("Gemstone colliding with" + other.gameObject.GetComponent<PhotonCharacterController>().playerName + " - let's transfer to them");
				if (PhotonNetwork.IsConnected) photonView.RPC("AttachToPlayer", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.UserId);
				else AttachToPlayer("player");
				
			}
		}



		public void DropGemstone(Vector3 dropPosition) {
			if (PhotonNetwork.IsConnected) photonView.RPC("GetDropped", RpcTarget.AllBufferedViaServer, dropPosition);
			else GetDropped(dropPosition);
		}

		[Photon.Pun.PunRPC]
		public void GetDropped(Vector3 dropPosition) {
			
			parentConstraint.constraintActive = false;
			transform.eulerAngles = Vector3.zero;

			if (collider != null)
				collider.enabled = true;

			if (rigidbody != null)
				rigidbody.useGravity = true;

			characterHolding = null;
			transform.position = dropPosition;
			
			if (gemState != GemState.InSack) {
				transform.localScale = Vector3.one;
				gemState = GemState.OnGround;
			}

			if (PhotonNetwork.IsConnected) photonView.TransferOwnership(PhotonNetwork.MasterClient);
			
			
			
			
		}

		public void PutInSack() {
			//GetDropped(PhotonStageManager.stageManager.stageSack.transform.position);
			gemState = GemState.InSack;
			
			if (rigidbody != null) rigidbody.useGravity = false;
			if (collider != null) collider.enabled = false;
			return;
		}

		void Photon.Pun.IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		}
	}
}