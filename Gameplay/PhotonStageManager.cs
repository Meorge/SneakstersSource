using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Sneaksters.Gameplay
{
	public class PhotonStageManager : MonoBehaviourPunCallbacks, IInRoomCallbacks {

		// public void OnPlayerEnteredRoom(Player player) {}
		// public void OnPlayerLeftRoom(Player player) {}
		// public void OnMasterClientSwitched(Player player) {}
		// public void OnRoomPropertiesUpdate(Hashtable newProps) {}

		public static PhotonStageManager stageManager;

		public List<PhotonGemstone> gemstones = new List<PhotonGemstone>();

		public List<VisibilityBeacon> visibilityBeacons = new List<VisibilityBeacon>();

		public ExitManhole exitManhole;

		public PhotonGemSack gemSack;
		

		public System.DateTime startTime;

		public int moneyPerGem = 500;

		public float originalSecs = 60f * 2f;
		float timeLeftSecs;

		public System.TimeSpan timeLeft = new System.TimeSpan();

		public LevelBuilder levelBuilder;

		public bool guardChasingLocalPlayer = false;


		// Event stuff
		public SneakstersEvent[] events = new SneakstersEvent[32];


		void Awake() {
			if (stageManager != null) {
				Destroy(gameObject);
			} else {
				stageManager = this;

				// PhotonNetwork.NetworkingClient.AddCallbackTarget(this);
			}
		}

		// void OnDestroy() {
		// 	PhotonNetwork.NetworkingClient.RemoveCallbackTarget(this);
		// }

		// Use this for initialization
		void Start () {
			if (PhotonGameManager.Instance == null) return;

			// print("PhotonStageManager.Start() - initializing stage manager");
			PhotonGameManager.Instance.gameState = GameState.InSession;
			levelBuilder = GetComponent<LevelBuilder>();

			// Load the current level object
			PhotonGameManager.Instance.SetCurrentLevelObject();

			
			levelBuilder.BuildLevelV2(PhotonGameManager.Instance.currentLevelObject.sceneName, PhotonNetwork.IsMasterClient);
			PhotonGameManager.Instance.OnLevelLoaded();
			startTime = System.DateTime.Now;

			timeLeftSecs = originalSecs;


			string[] potentialMusicTracks = new string[] {"Gameplay1", "Gameplay2"};

			int indexOfMusicTrackToPlay = Random.Range(0, potentialMusicTracks.Length);
			string musicTrackToPlay = potentialMusicTracks[indexOfMusicTrackToPlay];
			
			PhotonGameManager.Instance.PlayMusic($"Gameplay/{musicTrackToPlay}");
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("musicFade", 1f);
			PhotonGameManager.Instance?.iris?.SetIrisOut();
		}



		void Update() {
			timeLeftSecs -= Time.deltaTime;
			if (timeLeftSecs <= 0f) {timeLeftSecs = 0f;}

			timeLeft = System.TimeSpan.FromSeconds(timeLeftSecs);
		}

		public void CheckGemstones() {
			PhotonCharacterController.localCharacterController.UpdateStatusUI_Gemstones();
			
			bool allGemsCollected = true;

			AllGemsCollected();
			
			foreach (PhotonGemstone gem in gemstones) {
				if (gem.gemState != PhotonGemstone.GemState.InSack) {
					allGemsCollected = false;
					break;
				}
			}

			if (allGemsCollected) {AllGemsCollected();}
		}

		void AllGemsCollected() {
			// Debug.LogError("ALL GEMS COLLECTED!!");
			exitManhole.Open();
		}

		public void EndGame() {
			PhotonCharacterController.localCharacterController.SetEscaped();
			Cursor.lockState = CursorLockMode.None;
			// photonView.RPC("CheckIfAllPlayersDone", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId);

			PhotonCharacterController.localCharacterController.ChangeToComputerGuy();
			PhotonGameManager.Instance.StopMusic();
		}

		public void LocalPlayerCaptured() {
		}

		List<string> GetAllThiefIDs() {
			List<string> a = new List<string>();

			foreach (KeyValuePair<int, Photon.Realtime.Player> p in PhotonNetwork.CurrentRoom.Players) {
				if (p.Value.CustomProperties.ContainsKey("job") && (PlayerType)p.Value.CustomProperties["job"] == PlayerType.Thief) {
					a.Add(p.Value.UserId);
				}
			}

			return a;
		}
		void ShowFinishScreen() {
			PhotonCharacterController character = PhotonCharacterController.localCharacterController;
			character.missionSuccessAnimator.Animate();
			System.TimeSpan durationOfGame = System.DateTime.Now - startTime;
			string hourPart = "";
			if (durationOfGame.Hours != 0) {
				hourPart = durationOfGame.Hours.ToString() + ":";
			}
			string durationString = hourPart + durationOfGame.Minutes.ToString() + ":" + durationOfGame.Seconds.ToString() + "." + durationOfGame.Milliseconds.ToString();

			character.finish_timeTakenLabel.text = durationString;


			string gemsEarned = gemstones.Count.ToString() + "/" + gemstones.Count.ToString();
			character.finish_gemsGottenLabel.text = gemsEarned;

			string escapedThieves = PhotonCharacterController.characterControllers.Count.ToString() + "/" + PhotonCharacterController.characterControllers.Count.ToString();
			character.finish_escapedThievesLabel.text = escapedThieves;


			MoneyInfo moneyInfo = CalculateMoney((int)durationOfGame.TotalSeconds);

			character.finish_gemMoneyLabel.text = moneyInfo.moneyFromGems.ToString();
			character.finish_timeBonusLabel.text = "+" + moneyInfo.moneyFromTimeBonus.ToString();
			character.finish_thievesLostUnbonus.text = "-" + moneyInfo.moneyMinusFromLostThieves.ToString();

			int totalMoneyEarned = moneyInfo.moneyFromGems + moneyInfo.moneyFromTimeBonus - moneyInfo.moneyMinusFromLostThieves;
			character.finish_totalMoneyText.text = totalMoneyEarned.ToString();
			

			StartCoroutine(FinishCountdown());
		}

		IEnumerator FinishCountdown() {
			int timeRemaining = 10;

			while (timeRemaining >= 0) {
				PhotonCharacterController.localCharacterController.finish_timeLeftToRoom.text = timeRemaining.ToString();
				timeRemaining--;
				yield return new WaitForSeconds(1f);
			}
			PhotonGameManager.Instance.gameLocationSource = PhotonGameManager.GameLocationSource.PreviousGameSession;

			print("PhotonStageManager.FinishCountdown() - LOAD MAIN MENU");
			PhotonNetwork.LoadLevel("Main Menu");
			yield return null;
		}

		MoneyInfo CalculateMoney(int timeTaken) {
			MoneyInfo moneyInfo = new MoneyInfo();
			moneyInfo.moneyFromGems = gemstones.Count * moneyPerGem;
			moneyInfo.moneyFromTimeBonus = timeTaken / 10;

			moneyInfo.moneyMinusFromLostThieves = 0;
			return moneyInfo;
		}

		public void GuardStartChasing(PhotonPatrolGuard thisG) {
			// We need to check if at least one other guard is chasing the player
			// If not, then it's cool to activate post/music
			bool atLeastOneOtherGuardChasingPlayer = false;
			foreach (PhotonPatrolGuard g in PhotonPatrolGuard.guards) {
				if (g.guardState == GuardState.Chase && g.playerPursuing == PhotonCharacterController.localCharacterController && g != thisG) {
					// There's a guard chasing the player!
					atLeastOneOtherGuardChasingPlayer = true;
					break;
				}
			}

			if (!atLeastOneOtherGuardChasingPlayer) {
				// TODO: Resume the music with the stuff
				PhotonGameManager.Instance.ResumeMusic();
				PhotonGameManager.Instance.SetMusicParameter("chase", 1f);
				PhotonGameManager.Instance.LerpPostProcessing(0, 1, 0.3f);
				PhotonCharacterController.localCharacterController.GuardChasingYou();
			}
		}

		[PunRPC]
		public void GuardGiveUp() {
			// We need to check if at least one guard is chasing the player
			// If not, then it's cool to turn off post
			bool atLeastOneGuardChasingPlayer = false;
			foreach (PhotonPatrolGuard g in PhotonPatrolGuard.guards) {
				if (g.guardState == GuardState.Chase && g.playerPursuing == PhotonCharacterController.localCharacterController) {
					// There's a guard chasing the player!
					atLeastOneGuardChasingPlayer = true;
					break;
				}
			}

			if (!atLeastOneGuardChasingPlayer) {
				PhotonGameManager.Instance.SetMusicParameter("chase", 0f);
				PhotonGameManager.Instance.LerpPostProcessing(1, 0, 0.3f);
				PhotonCharacterController.localCharacterController.NoGuardChasingYou();
			}
		}

		public void UpdateGuardFX() {
			bool atLeastOneGuardChasingPlayer = false;
			foreach (PhotonPatrolGuard g in PhotonPatrolGuard.guards) {
				if (g.guardState == GuardState.Chase && g.playerPursuing == PhotonCharacterController.localCharacterController) {
					// There's a guard chasing the current player,
					// so either turn on or keep on the post and music.

					// Guard post wasn't on before, so turn it on
					if (!guardChasingLocalPlayer) {
						
						PhotonGameManager.Instance.SetMusicParameter("chase", 1f);
						PhotonGameManager.Instance.LerpPostProcessing(0, 1, 0.3f);
						guardChasingLocalPlayer = true;

						atLeastOneGuardChasingPlayer = true;
					}
					// Guard post was on before, so don't do anything
					else {
						guardChasingLocalPlayer = true;
						atLeastOneGuardChasingPlayer = true;
					}
					break;
				}
			}

			// There were no guards chasing the player, so stop the music and post.
			if (!atLeastOneGuardChasingPlayer) {
				PhotonGameManager.Instance.SetMusicParameter("chase", 0f);
				PhotonGameManager.Instance.LerpPostProcessing(1, 0, 0.3f);
				guardChasingLocalPlayer = false;
			}
		}


		public enum CombinedThiefStatus {
			Active = 0,
			AnyEscaped = 1,
			AllCaptured = 2
		}
		// How the thief-escaping logic should work:
		// If ANY thieves are active, then the game as a whole is active
		// If NO thieves are active:
		//	If ANY thieves have escaped, then the game is won
		//	Otherwise, the game is lost
		public (string, CombinedThiefStatus) GetCombinedThiefStatus() {
			string debug = "GetCombinedThiefStatus():\n";

			// First, let's see if any Thieves are active.
			bool anyThievesEscaped = false;
			foreach (PhotonCharacterController player in PhotonCharacterController.characterControllers) {
				// We only care about the status of Thieves.
				if ((PlayerType)player.photonView.Owner.CustomProperties["job"] != PlayerType.Thief) {
					debug += $"\"{player.photonView.Owner.NickName}\" isn't a Thief, so don't worry about them\n";
					continue;
				}

				PhotonCharacterController.ThiefStatus thiefStatus = (PhotonCharacterController.ThiefStatus)player.photonView.Owner.CustomProperties["thiefStatus"];
				debug += $"\"{player.photonView.Owner.NickName}\" status is {thiefStatus}";
				// Let's see if this Thief is active (aka they haven't been arrested)
				if (thiefStatus == PhotonCharacterController.ThiefStatus.Active || thiefStatus == PhotonCharacterController.ThiefStatus.CurrentlyBeingArrested) {
					// This Thief has not been arrested, so the group as a whole must still be active.
					debug += $" - active\n";
					return (debug, CombinedThiefStatus.Active);
				} else {
					// This Thief has been arrested, but this doesn't necessarily mean the group as a whole must be finished, so we'll continue to loop.
					debug += $" - not active";

					// Check if this Thief has escaped:
					if (thiefStatus == PhotonCharacterController.ThiefStatus.Escaped) {
						debug += " (escaped)\n";
						anyThievesEscaped = true;
					} else {
						debug += " (captured)\n";
					}
				}
			}
			// If we've made it here, that means that no one is active.
			// In that case, everyone must be either be arrested or have escaped. (Is that correct English?)
			// We also kept track of whether or not any Thieves had escaped, so we can easily check that.
			// At this point, if just a *single* person has escaped, then the mission was a success.
			return (debug, anyThievesEscaped ? CombinedThiefStatus.AnyEscaped : CombinedThiefStatus.AllCaptured);
		}
		// public bool AllThievesCaught() {
		// 	string debug_output = "AllThievesCaught():\n";
		// 	bool anyThievesNotCaptured = false;
		// 	foreach (PhotonCharacterController player in PhotonCharacterController.characterControllers) {
		// 		if ((PlayerType)player.photonView.Owner.CustomProperties["job"] != PlayerType.Thief) {
		// 			debug_output += $"\"{player.photonView.Owner.NickName}\" isn't a Thief, so don't worry about them\n";
		// 			continue;
		// 		}
		// 		if ((PhotonCharacterController.ThiefStatus)player.photonView.Owner.CustomProperties["thiefStatus"] != PhotonCharacterController.ThiefStatus.WasArrested) {
		// 			debug_output += $"\"{player.photonView.Owner.NickName}\" has NOT been captured";
		// 			anyThievesNotCaptured = true;
		// 			break;
		// 		} else {
		// 			debug_output += $"\"{player.photonView.Owner.NickName}\" has been captured\n";
		// 		}
		// 	}
		// 	return !anyThievesNotCaptured;
		// }

		void HandleAllCaptured() {
			PhotonCharacterController.localCharacterController?.AddNotification("All players have been captured!");
			PhotonGameManager.Instance.gameLocationSource = PhotonGameManager.GameLocationSource.PreviousGameSession;

			print("PhotonStageManager.OnPlayerPropertiesUpdate() - SHOW BUSTED SCREEN");
			PhotonCharacterController.localCharacterController?.ShowBustedScreen();
		}

		void HandleAnyEscaped() {
			PhotonCharacterController.localCharacterController?.AddNotification("No players are active, and at least one escaped!");
			PhotonGameManager.Instance.gameLocationSource = PhotonGameManager.GameLocationSource.PreviousGameSession;

			print("PhotonStageManager.OnPlayerPropertiesUpdate() - SHOW VICTORY SCREEN");
			ShowFinishScreen();		
		}

		public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
		{
			if (PhotonGameManager.Instance.gameState != GameState.InSession) {
				Debug.LogError($"OnPlayerPropertiesUpdate() in PhotonStageManager - gameState is {PhotonGameManager.Instance.gameState} but it should be InSession... that's weird");
				return;
			}

			// Count the number of remaining Thieves. If no thieves are remaining (i.e. everyone has status WasArrested), then the game is over
			(string, CombinedThiefStatus) combinedStatus = GetCombinedThiefStatus();
			// print(combinedStatus.Item1);
			switch (combinedStatus.Item2) {
				case CombinedThiefStatus.AllCaptured:
					HandleAllCaptured(); break;
				case CombinedThiefStatus.AnyEscaped:
					HandleAnyEscaped(); break;
				default:
					break;
			}
		}
	}

	public class SneakstersEvent {
		public bool activated = false;

		private List<bool> inputs = new List<bool>();


		// Event triggering
		public delegate void SneakstersEventSwitchFunction(bool status);
		public event SneakstersEventSwitchFunction OnSwitch;

		public void SetEventStatus(bool status) {
			inputs.Add(status);
		}

		public void UpdateEvent() {
			bool wasPreviouslyActivated = activated;

			activated = false;

			// If a single input is true, then the event is true
			foreach (bool b in inputs) {
				if (b) {
					activated = true;
					break;
				}
			}

			if (activated && !wasPreviouslyActivated) {
				OnSwitch(true);
			}

			else if (!activated && wasPreviouslyActivated) {
				OnSwitch(false);
			}

			// Clear inputs so we can process new ones next frame
			inputs.Clear();
		}

		public void ClearEventListeners() {
			foreach (System.Delegate d in OnSwitch.GetInvocationList()) {
				OnSwitch -= (SneakstersEventSwitchFunction)d;
			}
		}
	}

	public struct MoneyInfo {
		public int moneyFromGems;
		public int moneyFromTimeBonus;

		public int moneyMinusFromLostThieves;
	}

	public struct TimeLeftInfo {
		public int minutes;
		public int seconds;
		public int milliseconds;
	}
}