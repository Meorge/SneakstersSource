using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Shapes;

using Sneaksters.UI.Gameplay;

namespace Sneaksters.Gameplay
{
	public class PhotonPatrolGuard : MonoBehaviourPunCallbacks {

		public static List<PhotonPatrolGuard> guards = new List<PhotonPatrolGuard>();

		public float walkSpeed = 0.1f;

		public float chaseSpeed = 5f;

		public NavMeshAgent navMeshAgent;

		public List<Transform> stations = new List<Transform>();

		public int currentStationIndex = 0;

		public HackableDoorIcon iconOverlay;
		public GameObject iconOverlayPrefab;

		public GuardState guardState = GuardState.Patrol;
		public PhotonCharacterController playerPursuing;

		public Vector3 lastThiefPosSeen;

		public float doorWaitDuration = 4f;
		public float doorWaitSoFar = 0f;

		public GameObject headPosition;
		public LayerMask thievesOnly;

		public LayerMask noGemstones;
		public LayerMask doorsOnly;

		public AudioSource audioSource;
		public AudioClip thiefNoticeClip;
		public Animator animator;

		public GameObject bullseyeDebug;
		public bool useBullseye = false;

		[SerializeField] Line debugLine = null;


		// Use this for initialization
		void Start () {
			guards.Add(this);

			iconOverlay = Instantiate(iconOverlayPrefab).GetComponent<HackableDoorIcon>();
			iconOverlay.SetObjectToTrack(gameObject);
			iconOverlay.SetParentToIconContainer();

			navMeshAgent = GetComponent<NavMeshAgent>();

			if (LevelBuilder.levelBuilder != null)
			{
				//print("PhotonPatrolGuard.Start() - parent the guard to the room container, NOT keeping its world position");
				transform.SetParent(LevelBuilder.levelBuilder?.roomsContainer.transform, false);
			}
			else
			{
				//print("PhotonPatrolGuard.Start() - LevelBuilder is null so we can't parent the guard to the room container");
			}

			if (photonView != null && photonView.Owner != null) {
				if (!photonView.Owner.IsMasterClient) {
					navMeshAgent.enabled = false;
					return;
				} else if (stations.Count > 0) {
					navMeshAgent.Warp(stations[0].position);
					
					MoveToNextStation();
				}
			}

		}

		// [Photon.Pun.PunRPC]
		void UpdateBullseye() {
			// bullseyeDebug.transform.position = dest;
			// debugLine.
			if (photonView.Owner == PhotonNetwork.LocalPlayer) 
				debugLine.End = transform.InverseTransformPoint(navMeshAgent.destination);
		}
		
		// Update is called once per frame
		void Update () {
			// Only do navmesh calculations and movement for the game's owner
			if (photonView != null && photonView.Owner != null) {
				if (!photonView.IsMine) {
					navMeshAgent.enabled = false;
					return;
				}
			} else return;
			
			// Run the LookForThieves function()
			// Wow, these comments sure are useful!
			LookForThievesV2();

			switch (guardState) {
				case GuardState.Chase: // The Guard is currently chasing a Thief.
					// Make sure they're running, not walking!
					navMeshAgent.speed = chaseSpeed;

					// Make it so the Eyes in the Sky see their icon as chasing a Thief.
					iconOverlay.SetDoorState(true);

					// Check if the Thief they were chasing is still catchable.
					// If not, return to patrol.
					if (playerPursuing == null) {NoThiefSeen(0, false); return;}
					if (playerPursuing.thiefStatus != PhotonCharacterController.ThiefStatus.Active) NoThiefSeen(0, true);

					// Get the distance from the Thief they're chasing
					float distanceToThief = Vector3.Distance(transform.position, playerPursuing.transform.position);

					// If they are within the threshold, grab the Thief and arrest them!
					if (distanceToThief <= 4f) {

						RaycastHit hit;
						if (Physics.Raycast(transform.position, playerPursuing.transform.position - transform.position, out hit) && hit.collider.CompareTag("Player")) {
							ArrestThief();
						}
						

					}
					break;

				case GuardState.Patrol: // The Guard is currently patrolling.
					// Check if the Guard has reached their next station.
					if (navMeshAgent.remainingDistance < 0.01f) {

						// Check if they're at the end of their station list.
						if (currentStationIndex + 1 == stations.Count) {
							// If so, roll their current station back around to zero.
							currentStationIndex = 0;
						} else {
							// Otherwise, go to the next station in their list.
							currentStationIndex++;
						}

						// Start moving to the next station.
						MoveToNextStation();
					}
					break;

				default:
					break;
			}


			// Useful for debugging - this places a bullseye object where the Guard is walking or running towards.
			// When they're in pursuit of a Thief, the bullseye should follow the Thief's location (until they get out of
			// the Guard's line of sight.)
			if (lastThiefPosSeen != null && useBullseye && photonView.Owner == PhotonNetwork.LocalPlayer) {
				// photonView.RPC("UpdateBullseye", RpcTarget.All, navMeshAgent.destination);
			}
		}

		// Ends the game for the Thief the Guard is currently pursuing.
		void ArrestThief() {
			// First, let's make sure we even have a target:
			if (playerPursuing == null) return;

			// Next, let's check if the Thief is active (not captured or escaped)
			if (playerPursuing.thiefStatus != PhotonCharacterController.ThiefStatus.Active) return;

			print($"ArrestThief() - arrest thief named \"{playerPursuing.photonView.Owner.NickName}\"");

			// Run the GetArrested() function to play the "game over" stuff, but only if they're the local player
			if (playerPursuing == PhotonCharacterController.localCharacterController) playerPursuing.GetArrested();
			else photonView.RPC("ArrestRemoteThief", RpcTarget.Others, playerPursuing.userID);

			// Reset back to Patrol state
			guardState = GuardState.Patrol;

			// You've caught the Thief, so stop pursuing them.
			playerPursuing = null;

			// Reset lastThiefPosSeen
			lastThiefPosSeen = Vector3.zero;

			photonView.RPC("DoGuardGiveUp", RpcTarget.All);

			// Start patrol again
			FindClosestStation();

			return;
		}

		[PunRPC]
		void ArrestRemoteThief(string id) {
			if (id == PhotonCharacterController.localCharacterController.userID) {
				PhotonCharacterController.localCharacterController.GetArrested();
			}
		}

		[PunRPC]
		void DoGuardGiveUp() {
			// Stop the chase music and post processing
			PhotonStageManager.stageManager.GuardGiveUp();
		}

		// Opens door
		void OpenDoor(RaycastHit doorHit) {
			PhotonHackableDoor_Corban door = doorHit.transform.GetComponent<PhotonHackableDoor_Corban>();
			door?.ForceDoorState(true);
		}
		
		// Checks to see if a Thief is within the Guard's vision.
		// This version of the function is more consise.
		void LookForThievesV2() {
			RaycastHit hit;

			// Set up variables for the Guard visibility
			float maxDistance = 100f;

			Vector3 origin = transform.position;
			Vector3 direction = transform.forward;


			// Check for doors
			RaycastHit doorHit;
			if (Physics.Raycast(origin, direction, out doorHit, 2f, doorsOnly)) {
				// We hit a door!
				
				Vector3 doorForward = doorHit.transform.forward;

				// Find the dot product here
				Vector3 vectorToDoor = doorHit.transform.position - transform.position;

				Vector3 vectorToDestination = navMeshAgent.destination - transform.position;

				float dotProduct = Mathf.Abs(Vector3.Dot(vectorToDestination.normalized, doorForward));
				// print($"Dot={dotProduct}, door={vectorToDoor.sqrMagnitude}, dest={vectorToDestination.sqrMagnitude}");
				if (dotProduct >= 0.65f && vectorToDestination.sqrMagnitude > vectorToDoor.sqrMagnitude) {
					OpenDoor(doorHit);
				}
			}


			// Check if there's a Thief in front of the Guard (but not necessarily if there's a line of sight).
			if (Physics.SphereCast(origin, 2f, direction, out hit, maxDistance, thievesOnly)) {
				// There is a Thief in front of the Guard! Next, let's check if there's a line of sight:
				RaycastHit straightHit;

				Vector3 potentialPosition = hit.transform.position;

				Vector3 lineOfSight = potentialPosition - origin;

				if (Physics.Raycast(origin, lineOfSight, out straightHit, maxDistance, noGemstones)) {
					if (straightHit.collider.gameObject == hit.collider.gameObject) {
						// There is a line of sight to the Thief!

						// Make sure the Thief is catch-able:
						if (!hit.collider.gameObject.HasComponent<PhotonCharacterController>()) {return;}
						PhotonCharacterController ch = hit.collider.gameObject.GetComponent<PhotonCharacterController>();
						if (ch.thiefStatus != PhotonCharacterController.ThiefStatus.Active) {return;}

						// Let's check what the current state of the Guard is:

						switch (guardState) {
							case GuardState.Patrol: // The Guard is currently patrolling.
								// In this case, the Guard should play their "Thief seen" animation
								// and then begin to pursue the Thief.
								StartCoroutine(ThiefSeen(hit));
								break;

							case GuardState.Chase: // The Guard is currently chasing a Thief.
								// In this case, the Guard should immediately switch their target
								// to the Thief they've seen.
								
								if (playerPursuing == null || (playerPursuing.gameObject != hit.collider.gameObject)) SetPlayerPursuing(hit.collider.gameObject);
								lastThiefPosSeen = playerPursuing.transform.position;
								var destinationUpdateSuccess = navMeshAgent.SetDestination(lastThiefPosSeen);
								UpdateBullseye();
								break;

							default: // The Guard is currently in another state (opening a door or reacting to a Thief).
								// In these states, they shouldn't react to Thieves.
								break;
						}
					}
				} else { NoThiefSeen(1, false); }
			} else { NoThiefSeen(0, false); }
		}

		void SetPlayerPursuing(GameObject GO) {
			if (!GO.HasComponent<PhotonCharacterController>()) return;
			playerPursuing = GO.GetComponent<PhotonCharacterController>();

			photonView.RPC("GuardChaseFX", RpcTarget.All, playerPursuing.userID);
		}

		void NoThiefSeen(int source, bool forceStop) { // Run when a Thief is not seen
			// values for source
			// 0 - No Thieves nearby at all
			// 1 - A Thief is nearby but is not in the Guard's line-of-sight

			if (playerPursuing == null) return;

			if (guardState == GuardState.Chase) {
				// Let's see how close the Thief is to the last seen position.
				// If they were close enough, the Guard should continue to know where they are.

				float dThiefToLastSeen = Vector3.Distance(lastThiefPosSeen, playerPursuing.transform.position);
				if (dThiefToLastSeen <= 6f) {
					var destinationUpdateSuccess = navMeshAgent.SetDestination(playerPursuing.transform.position);
					UpdateBullseye();
				}
			}

			float d = Vector3.Distance(transform.position, navMeshAgent.destination);
			if ((d <= 1f && guardState == GuardState.Chase) || forceStop) {
				// Stop the chase music and post processing
				
				guardState = GuardState.Patrol;
				playerPursuing = null;

				photonView.RPC("DoGuardGiveUp", RpcTarget.All);

				
			}




		}

		IEnumerator ThiefSeen(RaycastHit hit) {
			// Update the Guard's state to AboutToChase. This effectively locks them
			// until their "Thief seen" animation is complete.
			guardState = GuardState.AboutToChase;

			// Stop the Guard from moving
			navMeshAgent.speed = 0f;

			// Play the "Thief seen" sound clip.
			// TODO: Tie this to the animation instead of calling via code.
			PhotonGameManager.Instance.PauseMusic();
			FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Gameplay/Guard Notice", transform.position);

			// Pause the music momentarily
			// PhotonGameManager.gameManager.audioPlayer.Pause();

			// Start the "Thief seen" animation.
			animator.SetTrigger("seeThief");

			// Wait 2 seconds for the "Thief seen" animation
			// to complete.
			yield return new WaitForSeconds(2f);

			// Resume the music
			// PhotonGameManager.gameManager.audioPlayer.Resume();

			// Make the Guard remember which Thief they're chasing.
			SetPlayerPursuing(hit.collider.gameObject);

			// Make the Guard remember the location they saw the Thief in.
			lastThiefPosSeen = playerPursuing.transform.position;

			// Start moving towards the last location they saw the Thief in.
			var destinationUpdateSuccess = navMeshAgent.SetDestination(lastThiefPosSeen);

			// Update the Guard's state to Chase. This unlocks them from the previous state
			// so they can begin to chase the Thief.
			guardState = GuardState.Chase;

			// Allow the Guard to move again, now at chase speed
			navMeshAgent.speed = chaseSpeed;

			yield return null;
		}

		[PunRPC]
		void GuardChaseFX(string playerID) {
			// Get the PhotonCharacterController from the id
			playerPursuing = PhotonCharacterController.GetCharacterControllerFromID(playerID);

			// If the player the Guard is pursuing is the local player, add
			// the special effects and music:
			if (playerPursuing == PhotonCharacterController.localCharacterController) {
				PhotonStageManager.stageManager.GuardStartChasing(this);
			}
		}


		void FindClosestStation() {
			// Determine the closest patrol station to the Guard's current location, and set that
			// to their current station index.
			int closestStationIndex = currentStationIndex;
			float distance;
			float closestDistance = 10000;
			for (int index = 0; index < stations.Count - 1; index++) {
				distance = Vector3.Distance(transform.position, stations[index].position);
				if (distance < closestDistance) {
					closestDistance = distance;
					closestStationIndex = index;
				}
			}

			currentStationIndex = closestStationIndex;
		}

		void MoveToNextStation() {
			// This hopefully shouldn't fire, but just in case it does, this prevents major explosions.
			if (stations == null) {
				print("Stations variable not defined??");
				return;
			}

			if (currentStationIndex < 0 || currentStationIndex >= stations.Count) {
				currentStationIndex = 0;
			}

			// Update the Guard's navmesh destination, so they'll walk to their station.
			var destinationUpdateSuccess = navMeshAgent.SetDestination(stations[currentStationIndex].position);

			// Begin walking towards their next station at the appropriate speed
			navMeshAgent.speed = walkSpeed;
			
			// Make sure their icon is normal for the Eyes in the Sky (not the pursuit version).
			if (PhotonCharacterController.localCharacterController.playerType == PlayerType.ComputerGuy && iconOverlay != null)
				iconOverlay.SetDoorState(false);
		}

		// void OnDrawGizmos()
		// {
		//     // Draw a yellow sphere at the transform's position
			
		// 	int i = 0;
		// 	foreach (Transform t in stations) {
		// 		if (i == currentStationIndex) {
		// 			Gizmos.color = Color.red;
		// 		} else {
		// 			Gizmos.color = Color.green;
		// 		}
		// 		Gizmos.DrawSphere(t.position, 1.5f);

		// 		i++;
		// 	}

		// 	if (navMeshAgent.destination != null) {
		// 		Gizmos.color = Color.yellow;
		// 		Gizmos.DrawSphere(navMeshAgent.destination, 1.5f);
		// 	}
		// }
	}


	public enum GuardState {
		Patrol,
		Chase,
		AboutToChase,
		OpeningDoor
	}
}