using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Boomlagoon.JSON;
using FMOD.Studio;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using DG.Tweening;

using Sneaksters.PostProcessing;
using Sneaksters.Gameplay;
using Sneaksters.Gameplay.Events;
using Sneaksters.Customization;
using Sneaksters.Emotes;
using Sneaksters.Discord;
using Sneaksters.Localization;
using Sneaksters.UI;
using Sneaksters.UI.Menus;
using Sneaksters.UI.Animation;
using Sneaksters.UI.Gameplay;
using UnityEditor;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Sneaksters
{
	public class PhotonGameManager : MonoBehaviourPunCallbacks, IPunObservable {
		public enum GameLocationSource {
			GameStartup,
			PreviousGameSession
		}

		public enum PlatformType {
			Desktop = 0,
			Mobile = 1
		}

		public PlatformType currentPlatformType;
		public GameLocationSource gameLocationSource;
		public LevelCatalog levelCatalog;
		public LevelObject currentLevelObject;

		public LocalizationPackCollection localizationPackCollection;

		public List<StringPack> stringPacks = new List<StringPack>();
		public int currentLanguageID;

		
		public HatCatalog currentHatCatalog;
		public string defaultPlayerName = "Player";
		
		[FormerlySerializedAs("computer_playerIDs")] public List<string> computerPlayerIDs = new List<string>();
		[FormerlySerializedAs("thief_playerIDs")] public List<string> thiefPlayerIDs = new List<string>();

		public List<Color> thiefColors = new List<Color>();
		private Hashtable _localPlayerProperties;

		public int playersInRoom;

		[FormerlySerializedAs("host_playerID")] public string hostPlayerID = "";

		public static PhotonGameManager Instance;

		public GameState gameState = GameState.InLobby;

		public GameObject playerPrefab;

		public bool joinedMidGame;

		[Header("Shader stuff")]
		public float halftoneScale = 100f;
		public IrisAnimator iris;

		[Header("FMOD Music")]
		EventInstance _musicInstance;


		[FormerlySerializedAs("SFX_ForwardEvent")]
		[Header("FMOD SFX")]
		[FMODUnity.EventRef]
		public string sfxForwardEvent = "";
		[FormerlySerializedAs("SFX_BackEvent")] [FMODUnity.EventRef]
		public string sfxBackEvent = "";
		[FormerlySerializedAs("SFX_TickEvent")] [FMODUnity.EventRef]
		public string sfxTickEvent = "";


		public PostProcessingManager postProcessingManager;

		[Header("Countdown stuff")]
		public GameObject countdownObject;
		public Animator countdownObjectAnimator;
		public TextMeshProUGUI countdownNumber;
		public int countdownStart = 5;
		public int countdownRemaining = 5;

		[Header("Disconnect modal stuff")]
		public GameManagerUIAnimator globalCanvasAnimator;
		[FormerlySerializedAs("disconnectModal_textLabel")] public TextMeshProUGUI disconnectModalTextLabel;
		[FormerlySerializedAs("disconnectModal_typeLabel")] public TextMeshProUGUI disconnectModalTypeLabel;
		[FormerlySerializedAs("disconnectModal_backToMenuButton")] public Button disconnectModalBackToMenuButton;


		[Header("Anti stealy measure")]
		public int expirationYear = 1960;
		public int expirationMonth = 1;
		public int expirationDay = 1;
		public bool antiStealyOverwridden;
		public bool withinTimeRange = true;

		public ErrorType lastErrorType;

		public enum ErrorType {
			Disconnect = 0,
			AntiStealy = 1
		}

		[Header("Bean mode")]
		public bool beanMode;

		[Header("Gemstone Appearance")]
		public GemstoneAppearanceCollection gemstoneAppearances;
		public GemstoneAppearanceEventCollection gemstoneAppearanceEvents;

		[Header("Emotes")]
		public GameObject emoteUIThing;
		public EmoteCollection emoteCollection;
		public EmoteCollection emoteCollectionEyes;

		[Header("Notifications")]
		public GameObject notificationPrefab;


		// Resolution
		[Header("Resolution")]
		public TextAsset resolutionsTextAsset;
		public List<ResolutionItem> listOfResolutions = new List<ResolutionItem>();


		[Header("Connection")]
		public string validRoomLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		
		public int lettersInRoomName = 4;
		
		private readonly Trigger _connectionFailedTrigger = new Trigger();
		private readonly Trigger _connectToRoomFailedTrigger = new Trigger();

		[Header("Control stuff")]
		public ControllerConfigCatalog controllerConfigCatalog;
		public GameObject controlButtonPrefab;
		public InputActionAsset inputActionsAsset;
		public SimpleStringDictionary controlButtonPromptStrings;
		public TMP_SpriteAsset buttonPromptSpriteAsset;

		void Awake() {
			if (Instance != null) {
				Destroy(gameObject);
				return;
			} else {
				Instance = this;
			}
			DontDestroyOnLoad(this.gameObject);

			// #if UNITY_EDITOR
			// EditorApplication.playModeStateChanged += PlayModeStateChanged;
			// #endif

			disconnectModalBackToMenuButton.onClick.AddListener(ReturnToMainScreen);

			foreach (var t in localizationPackCollection.localizationPacks) {
				var text = t.jsonAsset;
				var j = JSONObject.Parse(text.text);

				var newStringPack = new StringPack
				{
					language = j["language"].Str
				};

				foreach (KeyValuePair<string, JSONValue> pair in j["strings"].Obj) {
					newStringPack.Strings.Add(pair.Key, pair.Value.Str);
				}

				stringPacks.Add(newStringPack);
			}

			currentLanguageID = IntFromStringLanguageID(PlayerPrefs.GetString("language", "en"));

		}

		void Start() {
			Debug.developerConsoleVisible = false;
			ApplySettings();
			SetupResolution();
		}

		public PlayerInput GetPlayerInput() {
			if (MainMenuManager.instance != null) return MainMenuManager.instance.playerInput;
			else if (PhotonCharacterController.localCharacterController.playerInput != null) return PhotonCharacterController.localCharacterController.playerInput;
			else return null;
		}

		public bool GetBuildExpired() {
			withinTimeRange = true;
			return false;
			// System.DateTime expirationDate = new System.DateTime(expirationYear, expirationMonth, expirationDay);

			// if (System.IO.File.Exists("bingus bongus")) antiStealyOverwridden = true;
			// if (System.DateTime.Now > expirationDate && !antiStealyOverwridden) {
			// 	return true;
			// } else {
			// 	PhotonGameManager.gameManager.withinTimeRange = true;
			// 	return false;
			// }
		}

		public int GetDaysSinceExpired() {
			DateTime expirationDate = new DateTime(expirationYear, expirationMonth, expirationDay);
			return (int)(DateTime.Now - expirationDate).TotalDays;
		}

		// void PlayModeStateChanged(PlayModeStateChange c) {
		// }

		public void InitiateCountdownForEveryone(LevelObject levelObject) {
			var doBeanMode = false;
			if (Keyboard.current.bKey.isPressed) {
				doBeanMode = true;
				Debug.Log("BEAN MODE ACTIVATE");
			}

			var roomProperties = new Hashtable {{"levelID", levelObject.levelID}};
			PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

			photonView.RPC("StartLevelCountdown", RpcTarget.All, levelObject.levelID, doBeanMode);
			PhotonGameManager.Instance.currentLevelObject = levelObject;
		}

		public void PlayMusic(string path, Vector3 pos = default(Vector3))
		{
			// Debug.LogFormat("PlayMusic - with path {0}", path);
			_musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_musicInstance.release();
			_musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/" + path);

			_musicInstance.start();

			// PLAYBACK_STATE b;
			// _musicInstance.getPlaybackState(out b);
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("musicFade", 1f, true);
		}

		public void PauseMusic() {
			_musicInstance.setPaused(true);
		}

		public void ResumeMusic() {
			_musicInstance.setPaused(false);
		}

		public void SetMusicParameter(string param, float value, bool ignoreSeek = false) {
			_musicInstance.setParameterByName(param, value, ignoreSeek);
		}

		public void StopMusic() {
			_musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}

		public void LerpPostProcessing(int post1, int post2, float duration) {
			if (postProcessingManager != null) {
				postProcessingManager = GameObject.Find("PostProcessManager").GetComponent<PostProcessingManager>();
				postProcessingManager.LerpProfile(post1, post2, duration);
			}
		}

		public void SetCurrentLevelObject() {
			string levelID = (string)PhotonNetwork.CurrentRoom.CustomProperties["levelID"];

			currentLevelObject = levelCatalog.GetLevelObject(levelID);
		}

		public void SubtractOneFromCountdown() {
			countdownRemaining = Mathf.Clamp(countdownRemaining - 1, 0, countdownStart);
			countdownNumber.text = (countdownRemaining).ToString();

		}

		public void StartNextWait() {
			if (countdownRemaining == 0f && PhotonNetwork.IsMasterClient) {
				ActuallyLoadLevel();
			} else {
				StartCoroutine(LevelCountdownCoroutine());
			}
		}

		private IEnumerator LevelCountdownCoroutine() {
			yield return new WaitForSeconds(1f);

			if (countdownObjectAnimator != null) {
				if (countdownRemaining == 0f) {
					iris.AnimateIrisIn();
				} else {
					countdownObjectAnimator.SetTrigger("triggerNumChange");
				}
			}
			else {
				yield return null;
			}
		}



		private void StartGame() {
			gameState = GameState.InSession;
			
			Debug.Log("Master client of the room is " + hostPlayerID);
			Debug.Log("My ID is " + PhotonNetwork.LocalPlayer.UserId);

			photonView.RPC("GoIntoLevel", RpcTarget.All, currentLevelObject.levelID);

			print("PhotonGameManager.StartGame() - LOAD GAMEPLAY SCENE");
			PhotonNetwork.LoadLevel("Gameplay Scene");
		}

		public void SetLevelObjectFromID(int levelID) {
			currentLevelObject = levelCatalog.GetLevelObject(levelID);
		}

		private void ActuallyLoadLevel() {
			iris.AnimateIrisIn(StartGame);
		}

		public void OnLevelLoaded() {
			PhotonNetwork.Instantiate(playerPrefab.name, SpawnPoint.spawnPoint.transform.position, Quaternion.identity);

			postProcessingManager.SetProfile(0);

			if (PhotonNetwork.IsConnected) {
				SetDiscordStatusInMission();
			}
			
		}

		private void SetDiscordStatusInMission() {
			DiscordManager.SetActivityInSquadMission(
				missionName: currentLevelObject.levelName,
				squadID: PhotonNetwork.CurrentRoom.Name,
				currentNumberOfPlayers: playersInRoom,
				maxNumberOfPlayers: 12
			);
		}

		private void SetDiscordStatusInSquadIdle() {
			DiscordManager.SetActivityInSquadIdle(
				squadID: PhotonNetwork.CurrentRoom.Name,
				currentNumberOfPlayers: playersInRoom,
				maxNumberOfPlayers: 12
			);
		}

		public static uint GetCurrentUnixTimestamp() {
			DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (uint)(DateTime.UtcNow - epochStart).TotalSeconds;
		}

		public override void OnPlayerEnteredRoom(Player player) {
			Debug.Log("A player entered the room");
			playersInRoom = PhotonNetwork.PlayerList.Length;
			if (gameState == GameState.InSession && PhotonCharacterController.localCharacterController != null) {
				PhotonCharacterController.localCharacterController.AddNotification(player.NickName, NotificationItem.NotificationType.JoinedGame);

				SetDiscordStatusInMission();
			} else if (gameState == GameState.InLobby) {
				SetDiscordStatusInSquadIdle();
			}
		}

		public override void OnPlayerLeftRoom(Player player) {
			playersInRoom = PhotonNetwork.PlayerList.Length;
			if (gameState == GameState.InSession && PhotonCharacterController.localCharacterController != null) {
				PhotonCharacterController.localCharacterController.AddNotification(player.NickName, NotificationItem.NotificationType.LeftGame);

				SetDiscordStatusInMission();
			} else if (gameState == GameState.InLobby) {
				SetDiscordStatusInSquadIdle();
			}
		}

		public override void OnDisconnected(DisconnectCause cause) {
			MainMenuManager.instance.squadCodeAnimator.AnimateOut();
			Debug.LogWarning($"Disconnected from server. GameState is {gameState} and cause is {cause}");

			/*
			This gets set once we reload the scene and then it immediately triggers-
			so don't disconnect from the server once loading main scene??
			*/
			_connectionFailedTrigger.SetTrigger();

			SquadRoomManager.HideMoveOutButton();
			// if (MainMenuManager.instance != null)
			// 	// MainMenuManager.instance?.levelSelectButton.gameObject.SetActive(false);
				

			lastErrorType = ErrorType.Disconnect;

			if (gameState == GameState.InSession) {
				switch (cause) {
					case DisconnectCause.AuthenticationTicketExpired:
						break;
					case DisconnectCause.DisconnectByServerLogic:
						ShowDisconnectModal("The room disconnected you.", "DisconnectByServerLogic");
						break;
					case DisconnectCause.Exception:
						ShowDisconnectModal("An unknown error occurred.", "Exception");
						break;
					case DisconnectCause.ExceptionOnConnect:
						ShowDisconnectModal("The server is not available or the address is wrong.", "ExceptionOnConnect");
						break;
					case DisconnectCause.InvalidAuthentication:
						ShowDisconnectModal("The subscription with Exit Games needs to be updated.", "InvalidAuthentication");
						break;
					case DisconnectCause.InvalidRegion:
						ShowDisconnectModal("The region you are in is not supported.", "InvalidRegion");
						break;
					case DisconnectCause.MaxCcuReached:
						ShowDisconnectModal("The number of users online right now exceeds the server limit.", "MaxCcuReached");
						break;
					case DisconnectCause.None:
						ShowDisconnectModal("An unknown error occurred.", "Unknown");
						break;
					case DisconnectCause.OperationNotAllowedInCurrentState:
						ShowDisconnectModal("An unknown error occurred.", "OperationNotAllowedInCurrentState");
						break;
					case DisconnectCause.ClientTimeout:
						ShowDisconnectModal("You timed out.", "TimeoutDisconnect");
						break;
				}
			} else if (gameState == GameState.InLobby) {
				if (cause == DisconnectCause.DisconnectByClientLogic || cause == DisconnectCause.None)
				{
					// Client asked to disconnect, so there shouldn't be anything wrong here!
					Debug.LogWarning("Client disconnected from server, all is good (I think) :)");
				}
				else
				{
					MainMenuManager.instance.EnableModal(ModalMode.Error, "error_connecting");
				}
			}
		}

		private void ShowDisconnectModal(string text, string type) {
			FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/Error");
			disconnectModalTextLabel.text = text;
			disconnectModalTypeLabel.text = "Error type: " + type;
			Cursor.lockState = CursorLockMode.None;
			globalCanvasAnimator.AnimateIn();
		}


		// public void AntiStealy() {
		// 	StopMusic();
		// 	lastErrorType = ErrorType.AntiStealy;
		// 	ShowDisconnectModal("This build of Sneaksters is expired.", "Build expired");
		// 	MainMenuManager.instance.uiInputModule.enabled = false;
		// }

		public void ReturnToMainScreen() {
			if (lastErrorType == ErrorType.AntiStealy && Keyboard.current.spaceKey.isPressed && Keyboard.current.sKey.isPressed && Keyboard.current.qKey.isPressed) {
				antiStealyOverwridden = true;
			}
			Instance.gameLocationSource = GameLocationSource.GameStartup;

			// print("PhotonGameManager.ReturnToMainScreen() - LOAD MAIN MENU");
			PhotonNetwork.LoadLevel("Main Menu");

			postProcessingManager.SetProfile(-1);

			globalCanvasAnimator.AnimateOut();
		}

		// [PunRPC]
		// void UpdateLists(List<string> compIDs, List<string> thiefIDs, string hostID) {
		// 	computerPlayerIDs = compIDs;
		// 	thiefPlayerIDs = thiefIDs;
		//
		// 	hostPlayerID = hostID;
		// }
		//
		// public Player GetPlayerObjectFromPlayerID(string playerID) {
		// 	Player[] playerList = PhotonNetwork.PlayerList;
		// 	for (int i = 0; i < playerList.Length; i++) {
		// 		if (playerList[i].UserId == playerID) {
		// 			return playerList[i];
		// 		}
		// 	}
		// 	return null;
		// }


		public void Update() {
			Shader.SetGlobalFloat("_HalftoneScale", halftoneScale);
		}


		void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		}

		void SetupResolution() {
			JSONArray jsonArray = JSONObject.Parse(resolutionsTextAsset.text)["resolutions"].Array;

			listOfResolutions.Clear();
			foreach (JSONValue val in jsonArray) {
				ResolutionItem newResItem = new ResolutionItem((int)val.Obj["width"].Number, (int)val.Obj["height"].Number);
				listOfResolutions.Add(newResItem);
			}

			ResolutionItem current = listOfResolutions[3];

			if (PlayerPrefs.HasKey("resolution")) {
				current = listOfResolutions[PlayerPrefs.GetInt("resolution", 3)];
			} else {
				// The resolution has not been set, so let's find the best resolution and set to that.
				Resolution[] resolutions = Screen.resolutions;
				int largestWidth = resolutions[resolutions.Length - 1].width;
				Debug.Log($"The largest supported resolution by this monitor is {resolutions[resolutions.Length - 1].width}x{resolutions[resolutions.Length - 1].height}");
				// We now have the largest resolution width. Let's now go through
				// all of the game's supported resolutions, from biggest to smallest,
				// until we find the first one with a smaller width than this.
				for (int g = 0; g < listOfResolutions.Count; g++) {
					ResolutionItem i = listOfResolutions[g];
					if (i.width > largestWidth) continue;
					current = i;
					PlayerPrefs.SetInt("resolution", g);
					Debug.Log($"The largest game-supported resolution is {current.width}x{current.height}");
					break;
				}

			}

			// Default to full-screen mode
			FullScreenMode f = PlayerPrefs.GetInt("fullScreen", 1) != 0 ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
			Screen.SetResolution(current.width, current.height, f);


		}

		private int IntFromStringLanguageID(string id) {
			int i = 0;
			foreach (StringPack s in stringPacks) {
				if (s.language == id) return i;
				i++;
			}
			return i;
		}
		public void SetLanguageID(string id) {
			if (id == PlayerPrefs.GetString("language", "")) return;
			PlayerPrefs.SetString("language", id);
			currentLanguageID = IntFromStringLanguageID(PlayerPrefs.GetString("language", "en"));

			if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
			gameLocationSource = GameLocationSource.GameStartup;
			SceneManager.LoadScene(0);
		}

		public string GetStringFromID(string id) {
			if (!stringPacks[currentLanguageID].Strings.ContainsKey(id)) return "***";
			return stringPacks[currentLanguageID].Strings[id];
		}

		public TMP_FontAsset GetLanguageOverrideFont() {
			return localizationPackCollection.localizationPacks[currentLanguageID].overrideFont;
		}

		public Object GetLocalizedObject(string id) {
			List<LocalizationPack.LocalizationAsset> a = localizationPackCollection.localizationPacks[currentLanguageID].assets;
			foreach (LocalizationPack.LocalizationAsset b in a) {
				if (b.key == id) {
					return b.item;
				}
			}

			return null;

		}
		private void ApplySettings() {
			float musicVol = PlayerPrefs.GetFloat("musicVol", 0.5f);
			VCA musicVca = FMODUnity.RuntimeManager.GetVCA("vca:/Music");
			musicVca.setVolume(musicVol);

			float sfxVol = PlayerPrefs.GetFloat("sfxVol", 1f);
			VCA sfxVca = FMODUnity.RuntimeManager.GetVCA("vca:/SFX");
			sfxVca.setVolume(sfxVol);

			QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("quality", 3), true);

		}

		void EnableDiscord() {
			PlayerPrefs.SetInt("DiscordRichPresence", 1);
			Debug.Log("Enabling Discord RP");
			DiscordManager.SetActivityInMenus();
		}
		void DisableDiscord() {
			PlayerPrefs.SetInt("DiscordRichPresence", 0);
			Debug.Log("Disabling Discord RP");
			DiscordManager.ClearActivity();
		}

		void TurnOnInvertYMouselook() {
			PlayerPrefs.SetInt("invertYMouselook", 1);
		}

		void TurnOffInvertYMouselook() {
			PlayerPrefs.SetInt("invertYMouselook", 0);
		}


		public void UpdateDiscordRP(bool discordEnabled) {
			if (discordEnabled) EnableDiscord();
			else DisableDiscord();
		}

		public void UpdateMusicVol(float vol) {
			PlayerPrefs.SetFloat("musicVol", vol);
			VCA musicVca = FMODUnity.RuntimeManager.GetVCA("vca:/Music");
			musicVca.setVolume(vol);
		}

		public void UpdateSFXVol(float vol) {
			PlayerPrefs.SetFloat("sfxVol", vol);
			VCA sfxVca = FMODUnity.RuntimeManager.GetVCA("vca:/SFX");
			sfxVca.setVolume(vol);
		}

		public void UpdateInvertY(bool inv) {
			if (inv) TurnOnInvertYMouselook();
			else TurnOffInvertYMouselook();
		}

		public void UpdateMouseSensitivity(float sen) {
			PlayerPrefs.SetFloat("mouseSensitivity", sen);
		}


		public void UpdateQualityLevel(int value) {
			PlayerPrefs.SetInt("quality", value);
			QualitySettings.SetQualityLevel(value, true);
		}

		public void UpdateResolution(int value) {
			
			PlayerPrefs.SetInt("resolution", value);
			ResolutionItem newRes = listOfResolutions[value];

			Debug.Log($"Change resolution to {newRes.width}x{newRes.height}");


			FullScreenMode f = PlayerPrefs.GetInt("fullScreen", 0) != 0 ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;

			Screen.SetResolution(newRes.width, newRes.height, f, 0);
		}

		public void UpdateFullScreen(bool fullScreen) {
			PlayerPrefs.SetInt("fullScreen", fullScreen ? 1 : 0);
			Screen.fullScreenMode = fullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
		}

		public void UpdateHideCode(bool hideCode) {
			PlayerPrefs.SetInt("hideCode", hideCode ? 1 : 0);
		}

		public void UpdatePlayerName(string pName) {
			PlayerPrefs.SetString("playerName", pName);
		}

		public void UpdatePlayerHat(string hat) {
			PlayerPrefs.SetString("hat", hat);
		}

		public void UpdatePlayerColor(string colorID) {
			PlayerPrefs.SetString("playerColor", colorID);
		}



		// FUNCTIONS RELATED TO ONLINE ROOM CREATEY STUFF
		string MakeRoomName() {
			string roomName;
			string badRoomCodesJson = "{\"about\": \"\", \"credit\": \"\", \"warning\": \"\", \"codes\": []}";
			TextAsset badCodesAsset = Resources.Load("bad_codes") as TextAsset;
			if (badCodesAsset != null) badRoomCodesJson = badCodesAsset.text;
			else Debug.LogError("bad_codes.json couldn't be found, so not restricting codes.");

			BadCodes badCodesClass = JsonUtility.FromJson<BadCodes>(badRoomCodesJson);

			while (true) {
				roomName = "";
				for (int lettersSoFar = 0; lettersSoFar < lettersInRoomName; lettersSoFar++) {
					roomName += validRoomLetters[Random.Range(0,validRoomLetters.Length)].ToString();
				}

				Debug.Log("Generated room code " + roomName);
				if (badCodesClass.codes.Contains(roomName.ToUpper())) {
					Debug.LogError("The code " + roomName + " is not acceptable - will regenerate");
				} else {
					Debug.Log("This code is OK.");
					break;
				}
			}
			return roomName;
			

		}

		private void ConnectToInternet()
		{
			PhotonNetwork.OfflineMode = false;
			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.AutomaticallySyncScene = true;

			if (MainMenuManager.instance != null) {
				MainMenuManager.instance.EnableModal(ModalMode.Connecting);
			}
		}

		private IEnumerator ConnectToInternetCoroutine()
		{
			PhotonNetwork.OfflineMode = false;
			if (!PhotonNetwork.IsConnectedAndReady) {
                print("We are not connected and ready right now - so try to connect");
                ConnectToInternet();
				bool connectionFail = false;
				yield return new WaitForSeconds(0.25f);
				while (!PhotonNetwork.IsConnectedAndReady) {
					if (_connectionFailedTrigger.GetTrigger()) {
						connectionFail = true;
						MainMenuManager.instance.EnableModal(ModalMode.Error, "error_connecting");
						break;
					}
					yield return null;

				}

				if (!connectionFail) MainMenuManager.instance.DisableModal();
			} else {
				yield return null;
			}
		}

		public void TryConnectToInternet() {
			StartCoroutine(ConnectToInternetCoroutine());
		}

		public override void OnConnectedToMaster() {
			Debug.Log("Connected to server!");
            _connectionFailedTrigger.ClearTrigger();
        }

		public override void OnJoinedRoom() {
			SetDiscordStatusInSquadIdle();
		}

		public override void OnJoinRoomFailed(short returnCode, string message) {
			_connectToRoomFailedTrigger.SetTrigger();
		}

		public IEnumerator CreateRoom(GameMode gameMode = GameMode.GemHeist) {
			string roomName = MakeRoomName();

			if (!PhotonNetwork.OfflineMode)
				MainMenuManager.instance.EnableModal(ModalMode.Connecting, "creating_room");

			PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("playerName", defaultPlayerName);
			PhotonNetwork.NickName = PlayerPrefs.GetString("playerName", defaultPlayerName);

			PhotonNetwork.CreateRoom(roomName, roomOptions: new RoomOptions { PublishUserId = true});

			var roomProperties = new Hashtable {{"mode", gameMode}};

			yield return new WaitForSeconds(0.25f);

			StartCoroutine(HandleJoinRoomAttempt(roomProperties));
		}

		public IEnumerator CreateOfflineRoom(Action onBeforeCreateRoom = null)
		{
			if (PhotonNetwork.IsConnected)
			{
				PhotonNetwork.Disconnect();
				while (PhotonNetwork.IsConnected)
				{
					yield return null;
				}
			}
			PhotonNetwork.OfflineMode = true;
			onBeforeCreateRoom?.Invoke();
			yield return StartCoroutine(CreateRoom());
		}

		private void SetDefaultPlayerProperties() {
			print("Resetting player properties");
			_localPlayerProperties = new Hashtable
			{
				{"hat", PlayerPrefs.GetString("hat", "tophat")},
				{"playerColor", PlayerPrefs.GetString("playerColor", "000000")}
			};

			// If there are player properties from a previous session, keep the old job
			PlayerType job = PlayerType.Undecided;
			if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("job"))
			{
				job = (PlayerType)PhotonNetwork.LocalPlayer.CustomProperties["job"];
			}
			
			_localPlayerProperties.Add("job", job);
			_localPlayerProperties.Add("ready", false);
			_localPlayerProperties.Add("thiefStatus", PhotonCharacterController.ThiefStatus.Active);
			PhotonNetwork.LocalPlayer.SetCustomProperties(_localPlayerProperties);
		}

		private IEnumerator HandleJoinRoomAttempt(Hashtable roomProperties, bool joining=false) {
			var joinRoomFailed = false;
			while (!PhotonNetwork.InRoom) {
				if (_connectToRoomFailedTrigger.GetTrigger()) {
					joinRoomFailed = true;
					if (!joining) {
						MainMenuManager.instance.EnableModal(ModalMode.Error, "error_connecting");
					} else {
						MainMenuManager.instance.DisableModal();
						yield return new WaitForSeconds(0.5f);
						MainMenuManager.instance.codeInputAnimator.AnimateIncorrect();
						Debug.Log("Animate incorrect");
					}
					break;
				}
				yield return null;
			}

			if (joinRoomFailed) yield break;
			MainMenuManager.instance.DisableModal();
				
			if (joining) {
				yield return new WaitForSeconds(0.5f);
				MainMenuManager.instance.AnimateHorizontalStripeOut();
				MainMenuManager.instance.codeInputAnimator.AnimateOutRight();
				MainMenuManager.instance.codeInput.onValueChanged.RemoveAllListeners();
				MainMenuManager.instance.buttonCursor.Disable();
			}
			PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
				
			yield return new WaitForSeconds(1f);

			JoiningRoom();
		}

		public void JoinRoom(string code) {
			MainMenuManager.instance.EnableModal(ModalMode.Connecting, "joining_room", onAppear: () => {
				PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("playerName", defaultPlayerName);
				PhotonNetwork.NickName = PlayerPrefs.GetString("playerName", defaultPlayerName);

				PhotonNetwork.JoinRoom(code);

				StartCoroutine(HandleJoinRoomAttempt(null, joining: true)); // apparently GameMode can't be null so heck it
			});
		}

		public void JoiningRoom() {
			SetDefaultPlayerProperties();
			if (MainMenuManager.instance != null && MainMenuManager.instance.squadCodeLabel != null) {

				if (PlayerPrefs.GetInt("hideCode") == 1) {
					MainMenuManager.instance.squadCodeLabel.text = "????";
					MainMenuManager.instance.squadCodeVisible = false;
					MainMenuManager.instance.squadCodeTogglePrompt.SetActive(true);

				} else {
					MainMenuManager.instance.squadCodeLabel.text = PhotonNetwork.CurrentRoom.Name;
					MainMenuManager.instance.squadCodeVisible = true;
					MainMenuManager.instance.squadCodeTogglePrompt.SetActive(false);
				}
				if (PhotonNetwork.IsConnected && !PhotonNetwork.OfflineMode)
					MainMenuManager.instance.squadCodeAnimator.AnimateIn();
				MainMenuManager.instance.currentButton = null;
			}
			// Let the player choose their job!
			if ((GameMode) PhotonNetwork.CurrentRoom.CustomProperties["mode"] != GameMode.GemHeist) return;

			if (!PhotonNetwork.OfflineMode)
			{
				if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("job") || (PlayerType)PhotonNetwork.LocalPlayer.CustomProperties["job"] == PlayerType.Undecided)
				{
					ChooseJobManager.Initialize();
				} else
				{
					MainMenuManager.instance.ShowSquadRoom();
				}
			}
			else
			{
				Debug.Log("The player is playing offline so they have to be a Thief");
				SetLocalPlayerJob(PlayerType.Thief);
				LevelSelectManager.Initialize();
			}

		}

		public PlayerType GetLocalPlayerJob()
		{
			var props = PhotonNetwork.LocalPlayer.CustomProperties;
			return props.ContainsKey("job") ? (PlayerType)props["job"] : PlayerType.Undecided;
		}

		public void SetLocalPlayerJob(PlayerType pType) {
			if (_localPlayerProperties.ContainsKey("job")) _localPlayerProperties["job"] = pType;
			else _localPlayerProperties.Add("job", pType);

			PhotonNetwork.LocalPlayer.SetCustomProperties(_localPlayerProperties);
			SquadRoomManager.HighlightMoveOutButton();
		}

		public void SelectLevel(LevelObject o) {
			string app = "default";

			// Get appearance for events
			if (gemstoneAppearanceEvents != null) {
				app = gemstoneAppearanceEvents.GetCurrentGemstoneAppearanceID();
			}

			if (Keyboard.current.bKey.isPressed) {
				beanMode = true;
				app = "beans";
				Debug.LogError("BEAN TIME");
			} else if (Keyboard.current.pKey.isPressed) {
				Debug.LogError("PRESENT TIME");
				app = "present";
				beanMode = false;
			} else if (Keyboard.current.cKey.isPressed)
			{
				Debug.LogError("CANDY TIME");
				app = "candy";
				beanMode = false;
			}




			Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
			props.AddOrSet("gemAppearance", app);
			props.AddOrSet("levelID", o.sceneName);

			PhotonNetwork.CurrentRoom.SetCustomProperties(props);

			_localPlayerProperties["ready"] = true;
			PhotonNetwork.LocalPlayer.SetCustomProperties(_localPlayerProperties);


			MainMenuManager.instance.levelSelectAnimator.AnimateOut();
			SquadRoomManager.HideMoveOutButton();
			MainMenuManager.instance.backButton.SetActive(false);
		}

		public void ConfirmReadyForLevel() {
			_localPlayerProperties.AddOrSet("ready", true);
			PhotonNetwork.LocalPlayer.SetCustomProperties(_localPlayerProperties);
		}

		public bool LocalPlayerIsReadyForLevel()
		{
			return (bool)PhotonNetwork.LocalPlayer.CustomProperties["ready"];
		}

		public void LeaveRoom() {
			MainMenuManager.instance.squadCodeAnimator.AnimateOut();
			SquadRoomManager.HideMoveOutButton();

			SquadRoomManager.HideSquadRoom();
			PhotonNetwork.LeaveRoom();
		}

		public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps) {
			// TODO: when we change player properties in game, this causes the new level to be reloaded
			// That's kind of dumb. Make it so that it only does stuff if we're in the lobby I guess?

			base.OnPlayerPropertiesUpdate(target, changedProps);

			if (gameState == GameState.InLobby) {
				foreach (var p in PhotonNetwork.CurrentRoom.Players) {
					bool playerIsReady = (bool)p.Value.CustomProperties["ready"];
					if (playerIsReady == false) {
						return;
					}
				}

				print($"OnPlayerPropertiesUpdate() - Everyone is ready, let's go!");
				// made it this far and everyone has OK'd it, so let's do the level
				NewLoadLevel();
			}
		}

		public void ResetPlayerReady() {
			// Hashtable h = PhotonNetwork.LocalPlayer.CustomProperties;
			_localPlayerProperties["ready"] = false;
			PhotonNetwork.LocalPlayer.SetCustomProperties(_localPlayerProperties);

			if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;

			Hashtable g = PhotonNetwork.CurrentRoom.CustomProperties;

			// GROSS HACK
			// Fix this someday mmkay?
			g.AddOrSet("curScn", "Main Menu");
			g.Remove("levelID");
			PhotonNetwork.CurrentRoom.SetCustomProperties(g);
		}

		private void NewLoadLevel() {
			DOTween.Sequence()
				.AppendInterval(0.25f)
				.AppendCallback(() => {
					MainMenuManager.instance.squadCodeAnimator.AnimateOut();
					MainMenuManager.instance.backButton.SetActive(false);
					SquadRoomManager.HideSquadRoom();
					SquadRoomManager.AnimateIntoElevator();
					FMODUnity.RuntimeManager.StudioSystem.setParameterByName("musicFade", 0f);
				})
				.AppendInterval(3f)
				.AppendCallback(() => {
					print("PhotonGameManager.NewLoadLevel() - LOAD GAMEPLAY SCENE");
					if (PhotonNetwork.LocalPlayer.IsMasterClient) PhotonNetwork.LoadLevel("Gameplay");
				});
		}

		public bool CurrentlyInRoom() {return PhotonNetwork.InRoom;}
		public bool IsMasterClient() {return PhotonNetwork.LocalPlayer.IsMasterClient;}

		public string CurrentLevelFileName() {
			return (string)PhotonNetwork.CurrentRoom.CustomProperties["levelID"];
		}

		public void CleanCharacterControllerList() {
			PhotonCharacterController.characterControllers.Clear();
		}

		public ControllerConfigObject GetCurrentControllerConfigObject() {
			string id = "KeyboardMouse";
			if (Gamepad.current != null) {
				id = Gamepad.current.name;
			}

			foreach (ControllerConfigObject c in controllerConfigCatalog.configs) {
				if (c.ControllerSupported(id)) {
					return c;
				}
			}
			return null;
		}

		public string GetSquadCode() {
			if (!PhotonNetwork.InRoom) return "****";
			return PhotonNetwork.CurrentRoom.Name;
		}

		public override void OnLeftRoom() {
			PhotonNetwork.LocalPlayer.CustomProperties.Remove("job");
			DiscordManager.SetActivityInMenus();
		}
	}

	public enum GameState {
		InLobby,
		InSession
	}

	public enum GameMode {
		GemHeist = 0,
		HideAndSeek = 1
	}

	[Serializable]
	public class StringPack {
		public string language;
		public readonly Dictionary<string,string> Strings = new Dictionary<string, string>();
	}
}