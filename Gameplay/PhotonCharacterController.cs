using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Sneaksters.Customization;
using Sneaksters.Emotes;
using Sneaksters.UI.Gameplay;
using Sneaksters.UI.Animation;

namespace Sneaksters.Gameplay
{
	/*
	SNEAKSTERS

	PhotonCharacterController
	This is the character controller for both the Thieves
	and the Eyes in the Sky.

	*/
	public class PhotonCharacterController : Photon.Pun.MonoBehaviourPunCallbacks {
		[Header("Static stuff")]
		public static List<PhotonCharacterController> characterControllers = new List<PhotonCharacterController>();
		public static PhotonCharacterController localCharacterController;

		[Header("Player data")]
		public string userID;
		public string playerName;
		public ThiefStatus thiefStatus = ThiefStatus.Active;
		public Color thiefColor = Color.black;
		
		[Header("Player objects")]
		public PlayerType playerType;
		public GameObject walkingParticles;
		public new Camera camera;
		public GameObject map_ui_camera;
		public MeshRenderer visibilityCone;
		public SkinnedMeshRenderer characterRend;
		public Transform chestTransform;

		public GameObject hatModel;
		public GameObject hatContainer;

		public float distanceAwayToShowThiefInfo = 10f;

		public Animator characterAnimator;

		[Header("Player movement parameters")]
		public CharacterController characterController;
		public PlayerInput playerInput;
		public Shader thiefSurfShader;
		private float moveSpeed = 40f;
		private float gemMoveSpeed = 30f;

		private float sprintAdder = 40f;
		private float tiredSpeed = 10f;

		public float maxStamina = 100f;
		public float currentStamina = 100f;
		public float mouseTurningMultiplier = 4f;
		float userMouseSensitivity = 1f;
		public float initialStaminaDown = 5f;

		public Color staminaNormalColor = Color.green;
		public Color staminaLowColor = Color.red * Color.yellow;
		public Color staminaOutColor = Color.red;
		private float staminaDepletionModifier = 40f;

		public bool recoveringFromStamina = false;

		public PhotonGemstone gemstone = null;

		public float verticalTurnAmt = 0f;

		[Header("User interface")]
		public Canvas thiefCanvas;
		public Canvas globalCanvas;
		public Transform iconContainer;
		public InputSystemUIInputModule globalCanvasUIInput;

		// Things that replace the global canvas animator
		public StatusPauseAnimator statusPauseAnimator;
		public CaughtByGuardAnimator caughtByGuardAnimator;
		public BustedScreenAnimator bustedScreenAnimator;
		public EmoteSelectorAnimator emoteSelectorAnimator;
		public DangerTapeAnimator dangerTapeAnimator;
		public bool bustedScreenShown = false;
		public TextMeshProUGUI bustedScreenTime;

		public Image staminaBar;
		public float gemTutorialTime = 10f;
		public float gemTutorialTimeLeft;
		public GemstoneTutorialAnimator gemTutorialAnimator;


		public float timeStandingStill = 0f;
		public float timeStandingToTutorial = 5f;

		[Header("Pauseish")]
		public GameObject pausishMenu;
		public Button resumeButton;
		public Button returnToMenuButton;
		public bool paused = false;
		int pauseishTimer = 0;
		public TextMeshProUGUI pauseMissionNoLabel;
		public TextMeshProUGUI pauseMissionNameLabel;

		[Header("Some other UI stuff")]

		public GameObject staminaGaugeStuff;
		public Image gemstoneIcon;
		public Image thiefIcon;
		public TextMeshProUGUI gemstonesTotalLabel;
		public TextMeshProUGUI gemstonesSoFarLabel;

		public TextMeshProUGUI thievesTotalLabel;
		public TextMeshProUGUI thievesLeftLabel;
		public HeadForExitLabelAnimator headForExitAnimator;


		public TextMeshProUGUI timeRemainingTextMesh;


		[Header("Finish stuff")]
		public MissionSuccessAnimator missionSuccessAnimator;
		// public GameObject finishStuff;
		public TextMeshProUGUI finish_timeTakenLabel;
		public TextMeshProUGUI finish_gemsGottenLabel;
		public TextMeshProUGUI finish_escapedThievesLabel;

		public TextMeshProUGUI finish_timeLeftToRoom;

		public TextMeshProUGUI finish_gemMoneyLabel;
		public TextMeshProUGUI finish_timeBonusLabel;
		public TextMeshProUGUI finish_thievesLostUnbonus;
		public TextMeshProUGUI finish_totalMoneyText;


		[Header("Trailer recording and debug")]
		public bool godModeActivated = false;
		

		

		[Header("Computer specific")]
		public Vector3 mousePrevious = new Vector3();

		public Vector3 originalTouch = new Vector3();

		public Vector3 touchDirection = new Vector3();
		public Vector3 hoveringPosition = new Vector3();

		public float previousDistanceBetweenTouches;
		public float panSpeed = 1f;
		public float panSpeedMobile = 0.025f;

		public float maxZoomDistance = 20f;
		public float minZoomDistance = 80f;

		public float zoomAmount = 0.001f;
		public ThiefIcon iconOverlay;
		public Sprite normalThiefIcon;
		public Sprite nothingTexture;
		public GameObject iconOverlayPrefab;
		public GameObject computerCanvas;
		public Shader thiefIconShader;

		public Animator reticleAnimator;

		public LayerMask computerInteractableLayerMask;
		public LayerMask computerVisibleLayerMask;

		public PhotonHackableDoor_Corban hoveringDoor;

		public PhotonCameraComputerView computerEffect;
		public PhotonCameraComputerView computerEffectUI;

		public GameObject eyeBeaconPrefab;

		[Header("Intro stuff")]
		public TextMeshProUGUI levelNameLabel;
		public TextMeshProUGUI levelIDLabel;


		[Header("Emote stuff")]
		public EmoteCollection emoteCollection;
		public GameObject emoteContainer;
		public int emoteTimer = 100;
		public int emoteOriginalTime = 100;
		public List<EmoteGameObject> emoteGameObjects = new List<EmoteGameObject>();
		public TextMeshProUGUI eyeEmote_name;
		public Image eyeEmote_emote;

		[Header("Notifications")]
		public Transform notificationCollection;

		[Header("New Input")]
		public InputActionAsset inputActionAsset;
		public Vector2 moveVec = new Vector2();
		public Vector2 lookVec = new Vector2();
		public bool sprintActive = false;
		public bool hoveringOverGround = false;
		public float zoomVal = 0f;

		public float normalFOV = 30f;
		public float sprintFOV = 50f;
		public float fovTransitionDuration = 0.25f;
		// InputAction moveAction;
		// InputAction cameraPanAction;
		// InputAction sprintAction;
		// InputAction dropGemAction;

		[Header("Compass")]
		public GameObject compassContainer;
		public RectTransform compassTransform;
		public float compassMultiplier = (2f/3f);

		public enum ThiefStatus {
			Active = 0,
			Escaped = 1,
			CurrentlyBeingArrested = 2,
			WasArrested = 3,
			WaitingToStart = 4
		}

		public static PhotonCharacterController GetCharacterControllerFromID(string id) {
			foreach (PhotonCharacterController aController in characterControllers) {
				if (aController.userID == id) {return aController;}
			}
			return null;
		}

		void Awake() {
			if (photonView.IsMine || !PhotonNetwork.IsConnected)
			{
				localCharacterController = this;
			}
			characterControllers.Add(this);
		}

		// Use this for initialization
		void Start () {
			// print($"PhotonCharacterController() - New character controller for \"{photonView.Owner.NickName}\" has been created!");
			// Assign the characterController variable to... the character controller
			characterController = GetComponent<CharacterController>();

			// Some setup stuff
			userID = PhotonNetwork.IsConnected ? photonView.Owner.UserId : "player";
			playerName = PhotonGameManager.Instance != null ? photonView.Owner.NickName : "Player";


			// Cache the user's mouse sensitivity
			userMouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1f);

			// By default, disable the thief canvas. Not sure why this has to be done here - 
			// probably so that it remains disabled if it's not the local character controller?
			thiefCanvas.gameObject.SetActive(false);

			// Default the thief color to black
			thiefColor = Color.black;

			if (PhotonNetwork.IsConnected) {
				if (photonView.IsMine) PhotonGameManager.Instance.joinedMidGame = false;
				// Code to run if this is another player, not the local one
				else SetupRemote();
			}

			if (PhotonGameManager.Instance == null || photonView.IsMine) {
				// If the player was undecided they get to be a thief I guess
				// (how much do you wanna bet this is gonna mess the whole game up)
				if ((PlayerType)photonView.Owner.CustomProperties["job"] == PlayerType.Undecided)
				{
					Hashtable t = photonView.Owner.CustomProperties;
					t["job"] = PlayerType.Thief;
					photonView.Owner.SetCustomProperties(t);
				}

				thiefColor = ColorExtensions.FromHex(PlayerPrefs.GetString("playerColor"));
				// The initialization code we run when this IS the local player depends on what job they have!
				if (!PhotonNetwork.IsConnected || (PlayerType)photonView.Owner.CustomProperties["job"] == PlayerType.Thief) {
					SetupThief();
				} else { // If either the character is a Computer Guy or they haven't been assigned a job, they're a Computer Guy
					if (!photonView.Owner.CustomProperties.ContainsKey("job")) PhotonGameManager.Instance.joinedMidGame = true;
					SetupComputerGuy();
				}

				// Disable the walking particle effect
				walkingParticles.SetActive(false);

				returnToMenuButton.onClick.AddListener(ReturnToMenu);

				SetupEmotes();

				// Set up the level intro animation.
				// TODO: Localization
				if (PhotonGameManager.Instance.currentLevelObject != null) {
					levelNameLabel.text = PhotonGameManager.Instance.currentLevelObject.levelName;
					string missionNoString = string.Format(PhotonGameManager.Instance.GetStringFromID("mission_no"), PhotonGameManager.Instance.currentLevelObject.levelID + 1);
					levelIDLabel.text = missionNoString;

					pauseMissionNameLabel.text = PhotonGameManager.Instance.currentLevelObject.levelName;
					pauseMissionNoLabel.text = missionNoString;

				}
			}

			// Find the correct hat for this player and put it on their head!
			// if the game manager is null, then we don't have a hatlist so don't worry about this
			if (PhotonGameManager.Instance != null) {
				string hatName = PhotonNetwork.IsConnected ? (string)photonView.Owner.CustomProperties["hat"] : "tophat";
				foreach (HatItem hat in PhotonGameManager.Instance.currentHatCatalog.hats) {

					// If this hat is the right hat, instantiate it, parent it to the Thief, and set its color.
					if (hatName == hat.id) {
						hatModel = Instantiate(hat.model);
						hatModel.transform.SetParent(hatContainer.transform, false);
						hatModel.GetComponent<HatGameObject>().meshRenderer.material.SetColor("_Color", thiefColor);

						// If this is the local player, disable it so that it doesn't get in the way of the camera.
						if (photonView.IsMine) {hatModel.SetActive(false);}
						break;
					}
				}

				// This code is specific to the desktop version of the game.
				if (photonView.IsMine && PhotonGameManager.Instance.currentPlatformType == PhotonGameManager.PlatformType.Desktop) {
					hatModel.SetActive(false);		
					staminaBar.color = Color.green;
				}
			}




		}

		// Exits the current game session and returns to the main menu.
		void ReturnToMenu() {
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("musicFade", 0f);
			PhotonGameManager.Instance?.iris?.AnimateIrisIn(() => {
				DOTween.Sequence()
					.AppendInterval(1f)
					.AppendCallback(() => {
                        // PhotonNetwork.Disconnect();
                        PhotonNetwork.LeaveRoom();
                        PhotonGameManager.Instance.ReturnToMainScreen();
					});
			});
		}

		// Sets up emotes
		void SetupEmotes() {
			// Similarly, set up the emotes to use their color.

			EmoteCollection c = PhotonGameManager.Instance.emoteCollection;
			
			if (playerType == PlayerType.Thief) {
				if (PhotonGameManager.Instance.emoteCollection == null) {
					Debug.LogError("Emote collection is null, so can't load emotes");
					return;
				}
			} else if (playerType == PlayerType.ComputerGuy) {
				if (PhotonGameManager.Instance.emoteCollectionEyes == null) {
					Debug.LogError("Emote collection is null, so can't load emotes");
					return;
				}
				c = PhotonGameManager.Instance.emoteCollectionEyes;
			}




			int key = 1;

			for (int _g = 0; _g < 8 && _g < c.emotes.Count; _g++) {
				Emote em = c.emotes[_g];
				EmoteGameObject emg = emoteGameObjects[_g];

				emg.SetupEmote(em, key, playerType == PlayerType.Thief ? thiefColor : Color.black, true);
				key++;
			}


		}


		// Set up a remote player.
		void SetupRemote() {
			// Disable some stuff that we only want to be enabled for the local player.
			camera.gameObject.SetActive(false);
			computerCanvas.SetActive(false);
			map_ui_camera.SetActive(false);

			// Just check to make sure that this player is a Thief - if so, set their color to the correct one.

			if ((PlayerType)photonView.Owner.CustomProperties["job"] == PlayerType.Thief)
				thiefColor = ColorExtensions.FromHex((string)photonView.Owner.CustomProperties["playerColor"]);


			// Set up the shaders for the Thief using their assigned color.
			characterRend.material.SetColor("_Color", thiefColor);

			// Set up the player's icon for the Eyes in the Sky.
			iconOverlay = Instantiate(iconOverlayPrefab).GetComponent<ThiefIcon>();
			iconOverlay.SetObjectToTrack(gameObject);
			iconOverlay.SetParentToIconContainer();
			iconOverlay.SetPlayerName(playerName);
			iconOverlay.SetUpImageForColor(thiefColor);
		}

		// Set up a local Thief player.
		void SetupThief() {
			// Tell everyone who's playing that you're a Thief.
			if (PhotonNetwork.IsConnected) photonView.RPC("SetPlayerTypeToThief", RpcTarget.AllBuffered);

			// Disable some stuff that should only be visible to other players.
			visibilityCone.enabled = false;
			characterRend.enabled = false;

			// Enable the Thief-specific canvas, so that you can see your info!
			thiefCanvas.gameObject.SetActive(true);
			
			// Since they're a Thief, they don't need the Eye in the Sky camera.
			map_ui_camera.SetActive(false);

			// Lock their cursor since they're in a first person view.
			Cursor.lockState = CursorLockMode.Locked;

			// Initialize the status UI (gem and thief count)
			SetupStatusUI();

			// Disable the special animator things by default
			dangerTapeAnimator.SetOut();
			headForExitAnimator.SetOut();

			// Disable the Eye in the Sky reticle.
			reticleAnimator.gameObject.SetActive(false);

			// Spawn the player in a random location near the spawn point. This should prevent players spawning in/on each other.
			if (SpawnPoint.spawnPoint != null) transform.position = SpawnPoint.spawnPoint.transform.position + new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y);
			return;
		}

		[PunRPC]
		void SetPlayerTypeToThief() {playerType = PlayerType.Thief;}

		[PunRPC]
		void SetPlayerTypeToComputer() {playerType = PlayerType.ComputerGuy;}

		void SetupComputerGuy(bool setupUI = true) {
			// Tell everyone you're an Eye in the Sky.
			photonView.RPC("SetPlayerTypeToComputer", RpcTarget.AllBuffered);

			// Move yourself 1000 units up above the spawn, so that you're nowhere near the game plane.
			transform.position = new Vector3(SpawnPoint.spawnPoint.transform.position.x, 1000f, SpawnPoint.spawnPoint.transform.position.z);

			// Set up the camera to look at the map.
			camera.transform.localPosition = new Vector3(0f, -950f, 0f);
			camera.transform.eulerAngles = new Vector3(90f, 0f, 0f);

			camera.cullingMask = computerVisibleLayerMask;
			camera.orthographic = true;

			// Enable the map UI camera
			map_ui_camera.SetActive(true);

			// Initialize the status UI (gem and thief count)
			if (setupUI) {SetupStatusUI();}

			// Do something with the camera I guess. It works, okay?
			computerCanvas.GetComponent<Canvas>().worldCamera = map_ui_camera.GetComponent<Camera>();

			// On mobile, the reticle isn't necessary, so disable it
			if (PhotonGameManager.Instance.currentPlatformType == PhotonGameManager.PlatformType.Mobile) {
				reticleAnimator.gameObject.SetActive(false);
			}

			// Also disable the compass
			compassContainer.SetActive(false);
			return;
		}

		// Use this function when you want to make a Thief into an Eye in the Sky mid-game.
		public void ChangeToComputerGuy() {
			transform.rotation = Quaternion.identity;
			camera.transform.rotation = Quaternion.identity;
			Cursor.lockState = CursorLockMode.None;
			SetupComputerGuy(false);
		}

		public int NumberOfThieves() {
			int no = 0;

			// if we're offline (aka in single player mode) there's only one thief
			if (!PhotonNetwork.IsConnected) {
				return 1;
			}

			foreach (KeyValuePair<int, Photon.Realtime.Player> p in PhotonNetwork.CurrentRoom.Players) {
				if (p.Value.CustomProperties.ContainsKey("job") && (PlayerType)p.Value.CustomProperties["job"] == PlayerType.Thief) no++;
			}

			return no;
		}

		// Sets up the UI for the number of Gems collected/remaining and Thieves caught/active.
		void SetupStatusUI() {
			gemstonesTotalLabel.text = "/" + PhotonStageManager.stageManager.gemstones.Count.ToString();
			gemstonesSoFarLabel.text = "0";

			thievesTotalLabel.text = "/" + NumberOfThieves().ToString();
			thievesLeftLabel.text = NumberOfThieves().ToString(); // TODO: Fix this so it counts the active thieves

			gemstoneIcon.fillAmount = 0;
			thiefIcon.fillAmount = 1;
		}

		// Update the Status UI specifically for the gemstones. (The Thief count is not updated.)
		public void UpdateStatusUI_Gemstones() {
			// Iterate through all of the level's gemstones and count the number of collected ones.
			int gemstonesGotten = 0;
			foreach (PhotonGemstone gem in PhotonStageManager.stageManager.gemstones) {
				if (gem.gemState == PhotonGemstone.GemState.InSack) {gemstonesGotten++;}
			}

			// Set the number of gemstones collected.
			gemstonesSoFarLabel.text = gemstonesGotten.ToString();
			gemstonesTotalLabel.text = "/" + PhotonStageManager.stageManager.gemstones.Count.ToString();
			
			// Set the gemstone icon to be filled to the percent gemstones collected.
			gemstoneIcon.fillAmount = (float)((float)gemstonesGotten / (float)PhotonStageManager.stageManager.gemstones.Count);

			// Once all gemstones are collected, show the "Head for the Exit!" text.
			// if (gemstoneIcon.fillAmount == 1) {headForExitAnimator.Animate();}
			if (gemstoneIcon.fillAmount > 0 && !headForExitAnimator.HasAnimated) {headForExitAnimator.Animate();}
		}

		public float GetThievesRemainingPercentage() {
			int thievesLeft = 0;
			int thievesAtAll = 0;
			foreach (PhotonCharacterController ch in characterControllers) {
				if (ch.playerType == PlayerType.Thief) thievesAtAll++;
				if ((ch.thiefStatus == ThiefStatus.Active || ch.thiefStatus == ThiefStatus.WaitingToStart) && ch.playerType == PlayerType.Thief) thievesLeft++;
			}

			return (float)thievesLeft / (float)thievesAtAll;
		}

		// Update the Status UI specifically for the Thieves. (The gemstone count is not updated.)
		public void UpdateStatusUI_Thieves() {
			// Iterate through the Thieves and count how many have not been captured.
			int thievesLeft = 0;
			int thievesAtAll = 0;
			foreach (PhotonCharacterController ch in characterControllers) {
				if (ch.playerType == PlayerType.Thief) thievesAtAll++;
				if ((ch.thiefStatus == ThiefStatus.Active || ch.thiefStatus == ThiefStatus.WaitingToStart) && ch.playerType == PlayerType.Thief) thievesLeft++;
			}

			// Set the number of Thieves remaining.
			thievesLeftLabel.text = thievesLeft.ToString();

			// print($"UpdateStatusUI_Thieves() - there are {thievesLeft} / {thievesAtAll} remaining");
		}

		[ContextMenu("Show Busted Screen")]
		// Coroutine to show the "Busted" screen
		public void ShowBustedScreen() {
			if (!photonView.IsMine) return;
			//print($"ShowBustedScreen() - The player's name is {photonView.Owner.NickName} and their job is {playerType}");
			PhotonGameManager.Instance.StopMusic();
			// Calculate how long the game went on for, and put it in the bustedScreenTime TMP object.
			System.TimeSpan timeInLevel = System.DateTime.Now - PhotonStageManager.stageManager.startTime;

			var templateString = PhotonGameManager.Instance.GetStringFromID("busted_squad_lasted");
			var formattedString = string.Format(templateString, timeInLevel.Minutes.ToString("00"), timeInLevel.Seconds.ToString("00"));
			bustedScreenTime.text = formattedString;
			

			// Turn off the normal/status UI.
			// globalCanvasAnimator.SetBool("statusStuffVisible", false);
			statusPauseAnimator.AnimateGone();
			emoteSelectorAnimator.AnimateOut();
			computerCanvas.SetActive(false);

			// The Thieves should wait around 4.5 seconds before starting the actual "Busted" animation.
			if (playerType == PlayerType.Thief) {
				DOTween.Sequence()
					.AppendInterval(0.35f)
					.AppendCallback(AnimateBustedScreenThenIrisIn);
			} else {
				// The Eyes in the Sky need to play the "computer shutting down" animation, so they only need to wait 2 seconds.
				DOTween.Sequence()
					.Append(computerEffect.computerMat.DOFloat(1f, "_ComputerCloseAmount", 0.25f))
					.AppendInterval(0.1f)
					.AppendCallback(AnimateBustedScreenThenIrisIn);
					
					
			}
		}

		void AnimateBustedScreenThenIrisIn() {
			TweenCallback goBackToMenu = () => {
				// Return the players to the squad room.
				PhotonGameManager.Instance.gameLocationSource = PhotonGameManager.GameLocationSource.PreviousGameSession;
				print("PhotonCharacterController.ShowBustedScreen() - iris in effect is complete");
				if (PhotonNetwork.IsMasterClient) {
					print("PhotonCharacterController.ShowBustedScreen() - attempt to load main menu scene");
					PhotonNetwork.LoadLevel(0);
				}
			};

			TweenCallback animateIrisInToMenu = () => {
				print($"PhotonCharacterController.ShowBustedScreen() - time to do iris in! gameManager={PhotonGameManager.Instance} and iris={PhotonGameManager.Instance.iris}");
				PhotonGameManager.Instance?.iris?.AnimateIrisIn(goBackToMenu);
			};

			bustedScreenAnimator.Animate(animateIrisInToMenu);
		}

		
		
		// Update is called once per frame
		void FixedUpdate () {
			if (photonView.IsMine) {
				// Update the status UI for both gems and Thieves.
				// TODO: Make these only update when something happens.
				UpdateStatusUI_Gemstones();
				// UpdateStatusUI_Thieves();

				// IDK what this does but it's probably only good for the local player.
				UpdateTextThing();
			}


			if ( photonView.IsMine && localCharacterController != this)
			{
				localCharacterController = this;
			}

			// Always decrement pausishTimer until it hits 0
			if (pauseishTimer > 0) pauseishTimer--;
			else pauseishTimer = 0;

			// Just makin' sure we always know the character animator!
			if (characterAnimator == null) characterAnimator = transform.Find("ThiefGuyV3").GetComponent<Animator>();

			// For the other character controllers.
			if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
			{
				if (localCharacterController == null) { return; }
				float dist = Vector3.Distance(transform.position, localCharacterController.transform.position);

				float op = ExtensionsCode.NewLerp(dist, (float)distanceAwayToShowThiefInfo - 2f, (float)distanceAwayToShowThiefInfo, 1f, 0f);
				iconOverlay.SetOpacity(localCharacterController.godModeActivated ? 0f : op);

				// Find out how visible the Thief's icon should be, if the local player is an Eye in the Sky.
				if (localCharacterController.playerType == PlayerType.ComputerGuy) {
					EvaluateThiefIconVisibility();
				}
				return;
			}

			// If this is the local player, run different update functions depending on whether they're a
			// Thief or an Eye in the Sky.
			if (playerType == PlayerType.Thief) {UpdateThief();}
			else if (playerType == PlayerType.ComputerGuy) {UpdateComputer();}
			ManageStamina();
		}

		

		void EvaluateThiefIconVisibility() {
			// First, we need to find the closest beacon
			float distanceToClosestBeacon = 99999f;
			VisibilityBeacon closestBeac = null;
			foreach (VisibilityBeacon beac in PhotonStageManager.stageManager.visibilityBeacons) {
				float distanceToBeacon = Vector3.Distance(transform.position, beac.transform.position);

				if (distanceToBeacon < distanceToClosestBeacon && distanceToBeacon < beac.Radius) {
					distanceToClosestBeacon = distanceToBeacon;
					closestBeac = beac;
				}
			}

			// If the closest beacon has indeed been found, figure out how visible the Thief's icon should be based on its radius
			// and set the opacity to that.
			if (closestBeac != null) {
				float op = ExtensionsCode.NewLerp(distanceToClosestBeacon, (float)closestBeac.Radius - 2f, (float)closestBeac.Radius, 1f, 0f);
				iconOverlay.SetOpacity(op);
			}

			// If there's no beacon, then the Thief should be invisible.
			else {
				iconOverlay.SetOpacity(0f);
			}
			return;

		}

		public void AddNotification(string playerName, NotificationItem.NotificationType type) {
			NotificationItem i = Instantiate(PhotonGameManager.Instance.notificationPrefab).GetComponent<NotificationItem>();
			i.SetInformation(playerName, type);
			i.transform.SetParent(notificationCollection, false);
		}

		public void AddNotification(string content) {
			NotificationItem i = Instantiate(PhotonGameManager.Instance.notificationPrefab).GetComponent<NotificationItem>();
			i.SetText(content);
			i.transform.SetParent(notificationCollection, false);
		}

		// Shows emote to other players
		[PunRPC]
		void DoEmote(int emoteID, Vector3 position) {
			Emote em = PhotonGameManager.Instance.emoteCollection.emotes[emoteID];
			Sprite sp = em.sprite;

			// Also display the emotes on the Eye in the Sky icons
			if (iconOverlay != null) {
				// iconOverlay.SetTextures(em.sprite.texture, em.overlaySprite.texture);
				iconOverlay.DoEmote(em);
			}
		}


		// Sets up the in-game timer, which is currently unsued.
		// This needs a better name.
		void UpdateTextThing() {
			if (PhotonStageManager.stageManager.timeLeft.Minutes > 0f) {
				timeRemainingTextMesh.text = "<mspace=1.2em>" + PhotonStageManager.stageManager.timeLeft.Minutes.ToString("00") + ":" + PhotonStageManager.stageManager.timeLeft.Seconds.ToString("00") + "</mspace>";
			} else {
				timeRemainingTextMesh.text = "<mspace=1.2em>" + PhotonStageManager.stageManager.timeLeft.Seconds.ToString("00") + "." + (PhotonStageManager.stageManager.timeLeft.Milliseconds / 10).ToString("00") + "</mspace>";
			}
		}


		void ManageStamina() {
			// This allows for the stamina bar animations (pulsing when low).
			// TODO: Reimplement stamina pulse animation

			// Deploy stamina punishment.
			if (recoveringFromStamina) {
				// Tell the animator your stamina is recharging, so it doesn't do the pulsing animation.

				// Set the color of the stamina bar to red to show you how much you suck.
				staminaBar.color = Color.red;

				// Update the current stamina slowly back to full.
				currentStamina = Mathf.Clamp(currentStamina + (Time.deltaTime * (staminaDepletionModifier/4)), 0, maxStamina);

				// Once you're done recharging, you're all good.
				if (currentStamina == maxStamina) {
					recoveringFromStamina = false;
					staminaBar.color = Color.green;
				}

			} else {
				// If you're not using stamina, then slowly replentish it.
				if (currentStamina < maxStamina) {
					currentStamina = Mathf.Clamp(currentStamina + (Time.deltaTime * (staminaDepletionModifier/2)), 0, maxStamina);
				}
				
				if (currentStamina <= 0) {
					// If you ran out of stamina, you must be PUNISHED
					recoveringFromStamina = true;
				}
			}

			// Set the visual stamina bar.
			staminaBar.fillAmount = (currentStamina / maxStamina);
		}

		// Update the Thief specifically.
		void UpdateThief() {
			
			// print($"UpdateThief() - current action map is {}");
			// We only want this code to run when the Thief is active,
			// not when they've been caught by a Guard or escaped.
			if (thiefStatus != ThiefStatus.Active) {
				characterAnimator.speed = 0f;
				// Make the sound footstep sound effect stop
				return;
			}

			
			// Toggle God Mode.
			// In God Mode, you can move freely around without collisions.
			if (Keyboard.current.gKey.wasPressedThisFrame) {
				godModeActivated = !godModeActivated; // should toggle
			}

			// When God Mode is active, disable the UI.
			// (This is because God Mode is mostly used for recording cinematic game footage.)
			if (godModeActivated) {
				thiefCanvas.gameObject.SetActive(false);
				globalCanvas.gameObject.SetActive(false);
				
			} else {
				thiefCanvas.gameObject.SetActive(true);
				globalCanvas.gameObject.SetActive(true);
			}

			// Similar thing for the gemstone dropping tutorial.
			if (gemTutorialAnimator.IsVisible) {
				gemTutorialTimeLeft -= Time.deltaTime;
				if (gemTutorialTimeLeft <= 0f) {
					gemTutorialAnimator.AnimateOut();
				}

			}

			// Do some stuff to figure out how the Thief will be moving.
			float facingStep = moveVec.y * Time.deltaTime * 5f;
			float sideStep = moveVec.x * Time.deltaTime * 5f;

			characterAnimator.SetFloat("runSpeed", facingStep);

			float turningXAmount = lookVec.x * mouseTurningMultiplier * userMouseSensitivity;
			float turningYAmount = lookVec.y * mouseTurningMultiplier * userMouseSensitivity;


			// Invert the Y mouse look, if it's set in settings.
			if (PlayerPrefs.GetInt("invertYMouselook") == 0) {turningYAmount = -turningYAmount;}

			
			// Rotate the camera based on the cursor, but only if the cursor is locked!
			if (Cursor.lockState == CursorLockMode.Locked) {
				verticalTurnAmt += turningYAmount;
				verticalTurnAmt = Mathf.Clamp(verticalTurnAmt, -90f, 90f);
				camera.transform.localRotation = Quaternion.identity;
				camera.transform.Rotate(verticalTurnAmt, 0f, 0f);
				transform.Rotate(0, turningXAmount, 0);
			}

			// Figure out the player's movement speed.
			float tempMoveSpeed = moveSpeed;

			// If the player is carrying a gemstone, they should get slowed down a bit.
			if (gemstone != null) {tempMoveSpeed = gemMoveSpeed;}

			// Determine how long the Thief has been standing still. If it's more than a few seconds,
			// display the general tutorial.
			bool moving = facingStep != 0f || sideStep != 0f;
			if (moving) {
				timeStandingStill = 0f;
			} else {
				timeStandingStill += Time.deltaTime;
			}

			// If holding down shift and moving, sprint
			if (sprintActive && moving && currentStamina > 0 && !recoveringFromStamina) {
				// Only works if you have stamina and aren't recovering

				// When the player first taps Shift, deplete an extra chunk of stamina.
				// This prevents players from effectively getting infinite stamina.
				// TODO: Make this happen in the correct input function!
				if (Keyboard.current.shiftKey.wasPressedThisFrame) {
					currentStamina -= initialStaminaDown;
				}

				// Deplete a little bit of stamina.
				currentStamina -= Time.deltaTime * staminaDepletionModifier;

				// Make the Thief run a little bit faster.
				tempMoveSpeed += sprintAdder;

				// Since the player is running, their animation should be faster.
				characterAnimator.speed = 1.5f;
			}
			else if (recoveringFromStamina)
			{
				// Slow the player down to the tired speed.
				tempMoveSpeed = tiredSpeed;
			}
			else {
				// The player isn't running, so make their animator speed normal
				characterAnimator.speed = 1f;
			}

			// // If the player is carrying a gemstone and they press space, drop the gemstone.
			// if (Input.GetKeyDown(KeyCode.Space) && gemstone != null) {
			// 	DropGemstone(transform.position - (transform.forward * 3.5f));
			// }

			// Use the C key to toggle the cursor lock state.
			if (Keyboard.current.cKey.wasPressedThisFrame && Cursor.lockState == CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.None;
			} else if (Keyboard.current.cKey.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None) {
				Cursor.lockState = CursorLockMode.Locked;
			}

			// The actual movement code.

			Vector3 movement = transform.TransformDirection(new Vector3(sideStep, 0, facingStep)) * tempMoveSpeed;
			walkingParticles.SetActive(movement != Vector3.zero && localCharacterController != this);
			characterController.SimpleMove(movement);

			// Update the compass.
			float currentCompassYPosition = transform.localEulerAngles.y * compassMultiplier;
			Vector2 compassPos = compassTransform.anchoredPosition;
			compassPos.x = -currentCompassYPosition;
			compassTransform.anchoredPosition = compassPos;

			// Respawn the Thief back to the spawn point if they fall off the map.
			if (transform.position.y <= -50f) {
				transform.position = (SpawnPoint.spawnPoint != null) ? SpawnPoint.spawnPoint.transform.position : Vector3.zero;
			}
		}

		public void OnDestroy() {
			Destroy(iconOverlay?.gameObject);
		}

		private enum PanMode {
			UsingLMBAndDrag,
			UsingMove,
			None
		}

		private PanMode currentPanMode = PanMode.None;

		public void OnMove(InputAction.CallbackContext context) {
			if (PhotonNetwork.IsConnected) {
				if (!photonView.IsMine) return;
				if (paused) return;
			}

			if (playerType == PlayerType.Thief) {
				moveVec = context.ReadValue<Vector2>();

				var facingStep = moveVec.y * Time.deltaTime * 5.0f;
				characterAnimator.SetFloat("runSpeed", facingStep);
			}
			else {
				/*
				If the user is currently panning using LMB and drag, then
				we don't want them to be able to also use WASD at the same time.
				*/
				if (currentPanMode == PanMode.UsingLMBAndDrag) return;

                if (context.phase == InputActionPhase.Canceled)
                {
                    currentPanMode = PanMode.None;
                    lookVec = Vector2.zero;
                    return;
                }

                currentPanMode = PanMode.UsingMove;
                lookVec = -context.ReadValue<Vector2>();
			}
		}


		public void OnLook(InputAction.CallbackContext context) {
			if (PhotonNetwork.IsConnected) {
				if (!photonView.IsMine) return;
				if (paused) return;
			}

			if (playerType == PlayerType.ComputerGuy && currentPanMode == PanMode.UsingLMBAndDrag || playerType == PlayerType.Thief) {
				lookVec = context.ReadValue<Vector2>();
			}
		}

		public void OnLMB(InputAction.CallbackContext context) {
			/*
			This function is run when the left mouse button is pressed.
			We use it only for detecting when the user is using the mouse
			to pan around the map as a Hacker.
			*/
			if (PhotonNetwork.IsConnected) {
				if (!photonView.IsMine) return;
				if (paused) return;
			}

			/*
			Only Hackers can do this stuff.
			I wonder if you're going to run into some really infurating
			bug in the future, where you're trying to get code working where
			a Thief clicks on something but it's not working and you have no idea why!!
			If that happened, just know that past you is totally laughing at you right now.
			*/
			if (playerType == PlayerType.Thief) return;

            /*
			Let's see if the player is clicking on a door
			*/
            var cusrorPos = Mouse.current.position.ReadValue();
            var cursorRay = camera.ScreenPointToRay(cusrorPos);

            RaycastHit hit;
            // Only collide with objects in "ComputerVisionInteractable"
            if (Physics.Raycast(cursorRay, out hit, 1000, 1 << 13)) {
                if (hit.collider.CompareTag("ComputerInteractable")) {
					if (context.phase == InputActionPhase.Performed) {
                        hit.collider.GetComponent<PhotonHackableDoor_Corban>()?.OnMouseUp();
                    }
                }
            }

            /*
			If the user is already panning using WASD, then we don't want them
			to also use LMB/look vector for panning at the same time.
			*/
            if (currentPanMode == PanMode.UsingMove) return;

			if (context.ReadValueAsButton()) {
                currentPanMode = PanMode.UsingLMBAndDrag;
            } else {
                currentPanMode = PanMode.None;
                lookVec = Vector2.zero;
            }
		}

		public void OnSprint(InputAction.CallbackContext context) {
			if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
			if (playerType != PlayerType.Thief) return;
			if (paused) return;
			
			if (context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed) {
				sprintActive = true;
				camera.DOFieldOfView(sprintFOV, fovTransitionDuration);
			} else {
				sprintActive = false;
				camera.DOFieldOfView(normalFOV, fovTransitionDuration);
			}
		}

		public void OnDropGemstone(InputAction.CallbackContext context) {
			if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
			if (paused) return;
			if (context.phase != InputActionPhase.Performed) return;

			if (playerType == PlayerType.Thief) {
				if (gemstone != null) {
					DropGemstone(transform.position - (transform.forward * 3.5f));
				}
				Debug.Log("DROP GEM");
			}
			else if (playerType == PlayerType.ComputerGuy) {
				if (hoveringDoor != null) {
					hoveringDoor.OnMouseUp();
				}
			}
		}

		public void OnZoom(InputAction.CallbackContext context) {
			if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
			if (paused) return;

			if (playerType == PlayerType.ComputerGuy) {
				zoomVal = context.ReadValue<float>();
			}
		}

		public void OnShowAlternateEmotes(InputAction.CallbackContext context) {
			if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
			if (playerType == PlayerType.Thief && thiefStatus != ThiefStatus.Active) return;
			if (paused) return;

			if (context.phase == InputActionPhase.Performed) emoteSelectorAnimator.SwapPages();
		}

		public void OnEmote(InputAction.CallbackContext context) {
			if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
			if (playerType == PlayerType.Thief && thiefStatus != ThiefStatus.Active) return;
			if (context.phase != InputActionPhase.Started) return;
			if (paused) return;

			string emote_no = context.action.name;
			bool useAlternateEmotes = !emoteSelectorAnimator.ViewingPageOne;
			int altEmoteAdder = useAlternateEmotes ? 4 : 0;
			EmoteCollection c = PhotonGameManager.Instance.emoteCollection;

			// Get emote ID
			int emoteIndex = altEmoteAdder;
			switch (emote_no) {
				case "Emote2": emoteIndex += 1; break;
				case "Emote3": emoteIndex += 2; break;
				case "Emote4": emoteIndex += 3; break;
				default: break;
			}

			int emoteID = c.emotes[emoteIndex].id;

			

			Vector3 pos = Vector3.zero;
			if (playerType == PlayerType.ComputerGuy) {
				pos = hoveringPosition;
				currentStamina -= initialStaminaDown * 8;
				if (PhotonNetwork.IsConnected) photonView.RPC("SpawnEyeBeacon", RpcTarget.All, hoveringPosition, emoteID);
				else SpawnEyeBeacon(hoveringPosition, emoteID);
			}

			else if (playerType == PlayerType.Thief) {
				emoteGameObjects[emoteIndex].animator.AnimateSelected();
				photonView.RPC("DoEmote", RpcTarget.All, emoteID, pos);
			}
		}

		public void OnPause(InputAction.CallbackContext context) {
			HandlePausishMenu();
		}

		public void ClosePausishMenu() {
			if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
			if (pauseishTimer != 0) return;

			paused = false;
			// globalCanvasAnimator.SetBool("pause", false);
			statusPauseAnimator.AnimateOutPaused();
			
			Cursor.lockState = CursorLockMode.Locked;
		}

		public void HandlePausishMenu() {
			if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

			if (pauseishTimer != 0) return;

			pauseishTimer = 50;

			statusPauseAnimator.TogglePaused();
			paused = statusPauseAnimator.CurrentlyPaused;

			resumeButton.Select();
			EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);


			if (paused) {
				Cursor.lockState = CursorLockMode.None;
				characterAnimator.SetFloat("runSpeed", 0f);
				moveVec = Vector2.zero;
				lookVec = Vector2.zero;
				sprintActive = false;
				
			}
			else Cursor.lockState = CursorLockMode.Locked;		
		}

		// Update code specific to the Eyes in the Sky
		void UpdateComputer() {
			// By default, the reticle should not be moving.
			reticleAnimator.speed = 0;
			RaycastHit compRay;

			hoveringOverGround = false;
			hoveringPosition = Vector3.zero;

			// do panning stuff, if we're panning the camera
			// TODO: I think this'll break for controllers
			if (currentPanMode != PanMode.None && lookVec != Vector2.zero) {

				// get the current position
				Vector3 camPos = camera.transform.position;

				// Add the look vector to it
				camPos += new Vector3(-lookVec.x, 0f, -lookVec.y);

				// print($"lookVec=({lookVec.x}, {lookVec.y})");

				// Clamp it to within the allowed range
				// TODO: make this code not terrible
				// (calcs only need to be run once to get the true offset, etc)
				Transform roomContainer = PhotonStageManager.stageManager.levelBuilder.roomsContainer.transform;
				Vector2 min2 = PhotonStageManager.stageManager.levelBuilder.minRoomPosition, max2 = PhotonStageManager.stageManager.levelBuilder.maxRoomPosition;

				Vector3 min = roomContainer.TransformPoint(new Vector3(min2.x, 0, min2.y));
				Vector3 max = roomContainer.TransformPoint(new Vector3(max2.x, 0, max2.y));
				camPos.x = Mathf.Clamp(camPos.x, min.x, max.x);
				camPos.z = Mathf.Clamp(camPos.z, min.z, max.z);

				// Assign it back
				camera.transform.position = camPos;
			}

			if (zoomVal != 0f) {
				camera.GetComponent<Camera>().orthographicSize = Mathf.Clamp(camera.GetComponent<Camera>().orthographicSize + (zoomVal * -0.35f), minZoomDistance, maxZoomDistance);
			}

			// Check if the reticle is over a computer-interactable object (like a door).
			if (Physics.Raycast(camera.transform.position, camera.transform.forward, out compRay, 1000f, computerInteractableLayerMask, QueryTriggerInteraction.Collide)) {
				if (compRay.transform.CompareTag("ComputerInteractable")) {
					reticleAnimator.speed = 1;

					PhotonHackableDoor_Corban possibleDoor = compRay.transform.gameObject.GetComponent<PhotonHackableDoor_Corban>();

					if (possibleDoor != null) {
						hoveringDoor = possibleDoor;
					} else {
						hoveringDoor = null;
					}
				} else {
					hoveringDoor = null;
					reticleAnimator.speed = 0.5f;
					hoveringOverGround = true;
					hoveringPosition = compRay.point;
				}
			} else {
				hoveringDoor = null;	
			}	
		}

		[PunRPC]
		public void SpawnEyeBeacon(Vector3 pos, int emoteID) {
			print($"Eye emote with ID {emoteID}");
			Emote em = PhotonGameManager.Instance.emoteCollectionEyes.emotes[emoteID];
			print($"This emote is {em.name}");

			EyeBeacon beac = Instantiate(eyeBeaconPrefab).GetComponent<EyeBeacon>();
			beac.transform.position = pos;

			beac.SetData(playerName, em);
		}

		public void PickUpGemstone(PhotonGemstone gem) {
			gemstone = gem;
			// DiscordRPController.discordController?.OnGetGem();

			if (characterAnimator != null) {
				characterAnimator.SetBool("holdingGem", true);
			}

			if (gemTutorialAnimator != null) {
				gemTutorialAnimator.AnimateIn();
			}
			gemTutorialTimeLeft = gemTutorialTime;
		}

		public void DropGemstone(Vector3 posToDrop) {
			if (characterAnimator != null) {
				characterAnimator.SetBool("holdingGem", false);
			}

			if (gemTutorialAnimator != null) {
				gemTutorialAnimator.AnimateOut();
			}
			gemTutorialTimeLeft = 0f;
			gemstone.DropGemstone(posToDrop);
			gemstone = null;
		}

		// Run when a Guard catches the local Thief
		public void GetArrested() {
			SetCurrentlyBeingArrested();
			PhotonStageManager.stageManager.LocalPlayerCaptured();

			// Play the "Caught" animation
			StartCoroutine(GetArrestedAnimation());
		}

		public IEnumerator GetArrestedAnimation() {
			//PhotonGameManager.gameManager.audioPlayer.Stop();
			PhotonGameManager.Instance.StopMusic();
			// globalCanvasAnimator.SetBool("statusStuffVisible", false);
			statusPauseAnimator.AnimateGone();
			emoteSelectorAnimator.AnimateOut();

			// globalCanvasAnimator.SetTrigger("caughtByGuard");
			caughtByGuardAnimator.Animate(() => {
				SetWasArrested();
			});

			if (gemstone != null) {
				DropGemstone(gemstone.spawnPoint);
			}
			
			yield return null;
		}

		public void GuardChasingYou() {
			// globalCanvasAnimator.SetBool("danger", true);
			dangerTapeAnimator.AnimateIn();
		}

		public void NoGuardChasingYou() {
			// globalCanvasAnimator.SetBool("danger", false);
			dangerTapeAnimator.AnimateOut();
		}

		public void SetLocalPlayerProperty(string key, object value) {
			if (localCharacterController != this) {
				Debug.LogError("SetLocalPlayerProperty() - attempting to set local player property on the not-local player!");
				return;
			}
			Hashtable t = photonView.Owner.CustomProperties;
			t.AddOrSet(key, value);
			photonView.Owner.SetCustomProperties(t);
		}

		public void SetCurrentlyBeingArrested() {
			
			SetLocalPlayerProperty("thiefStatus", ThiefStatus.CurrentlyBeingArrested);
		}

		public void SetWasArrested() {
			SetLocalPlayerProperty("thiefStatus", ThiefStatus.WasArrested);
		}

		public void SetEscaped() {
			SetLocalPlayerProperty("thiefStatus", ThiefStatus.Escaped);
		}

		public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, ExitGames.Client.Photon.Hashtable changedProps)
		{
			base.OnPlayerPropertiesUpdate(target, changedProps);
			if (target != photonView.Owner) return;

			thiefStatus = (ThiefStatus)changedProps["thiefStatus"];

			if (thiefStatus == ThiefStatus.CurrentlyBeingArrested) {
				PhotonCharacterController.localCharacterController.AddNotification($"{photonView.Owner.NickName} got caught!");
				PhotonCharacterController.localCharacterController.UpdateStatusUI_Thieves();
			}

			if (this == localCharacterController && thiefStatus == ThiefStatus.WasArrested && PhotonStageManager.stageManager.GetCombinedThiefStatus().Item2 == PhotonStageManager.CombinedThiefStatus.Active) {
				print("I got caught!");
				ChangeToComputerGuy();
				caughtByGuardAnimator.SetOut(true);
				statusPauseAnimator.AnimateIn();
				emoteSelectorAnimator.AnimateIn();
			}
		}
	}

	public enum PlayerType {
		Undecided = -1,
		Thief = 0,
		ComputerGuy = 1
	}
}