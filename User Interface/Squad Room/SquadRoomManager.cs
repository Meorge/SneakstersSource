using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Photon.Pun;
using Photon.Realtime;

namespace Sneaksters.UI.Menus
{
    public class SquadRoomManager : MonoBehaviourPunCallbacks
    {
        #region Singleton
        public static SquadRoomManager Instance = null;
        public static bool IsActive { get; private set; } = false;

        public static List<Player> Players { get => Instance.players; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else {
                Destroy(gameObject);
            }

            SetUpLabels();
        }

        private void OnDestroy() {
            labels.Clear();
        }
        #endregion

        #region References
        [Header("References")]
        [SerializeField] SquadRoomPlayerLabel labelPrefab = null;
        [SerializeField] RectTransform labelContainer = null;
        [SerializeField] int numberOfLabels = 12;
        [SerializeField] List<SquadRoomPlayerLabel> labels = new List<SquadRoomPlayerLabel>();
        [SerializeField] List<Player> players = new List<Player>();
        [SerializeField] PlayableDirector playableDirector = null;
        [SerializeField] Button moveOutButton = null;
        [SerializeField] SimpleLocalizeText moveOutButtonLabel = null;
        #endregion

        #region Assets
        [Header("Assets")]
        [SerializeField] Sprite gemSprite = null;
        [SerializeField] Sprite compSprite = null;
        [SerializeField] Sprite thinkingSprite = null;
        [SerializeField] Sprite unknownSprite = null;
        #endregion

        #region Asset Getters
        public static Sprite GemSprite { get => Instance.gemSprite; }
        public static Sprite ComputerSprite { get => Instance.compSprite; }
        public static Sprite ThinkingSprite { get => Instance.thinkingSprite; }
        public static Sprite UnknownSprite { get => Instance.unknownSprite; }
        #endregion

        private void SetUpLabels()
        {
            // Initialize the labels
            labels = new List<SquadRoomPlayerLabel>();
            for (int i = 0; i < numberOfLabels; i++)
            {
                SquadRoomPlayerLabel newLabel = Instantiate(labelPrefab);

                newLabel.Index = i;

                newLabel.transform.SetParent(labelContainer.transform, false);

                labels.Add(newLabel);
            }
        }

        public static void ShowSquadRoom()
        {
            
            ActiveCamera.SetActiveCamera("squadRoom");
            // Debug.Log("NEW SQUAD ROOM");
            Instance.UpdatePlayerList();
            foreach (SquadRoomPlayerLabel label in Instance.labels)
            {
                label.AnimateIn();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Instance.StartCoroutine(Instance.ActivateMoveOutButton("head_out", ShowLevelSelect));
            }
            else
            {
                Instance.CheckIfConfirmReady();
            }

            IsActive = true;
        }

        public static void HideSquadRoom()
        {
            IsActive = false;
            foreach (SquadRoomPlayerLabel label in Instance.labels)
            {
                label.AnimateOut();
            }

            Instance.moveOutButton.onClick.RemoveAllListeners();

            Instance.moveOutButton.gameObject.SetActive(false);
            MainMenuManager.instance.lastSelectedGameObject = null;
            MainMenuManager.instance.savedSelectedGameObject = null;
            EventSystem.current.SetSelectedGameObject(null);
            HideMoveOutButton();
        }

        public static void AnimateIntoElevator() {
            ActiveCamera.SetActiveCamera("doorFocus");
            Instance.playableDirector.Play();

        }

        IEnumerator ActivateMoveOutButton(string dispString = "null", UnityEngine.Events.UnityAction onClick = null)
        {
            yield return new WaitForEndOfFrame();

            moveOutButtonLabel.textID = dispString;

            moveOutButton.onClick.RemoveAllListeners();
            moveOutButton.onClick.AddListener(onClick);
            moveOutButton.gameObject.SetActive(true);
            HighlightMoveOutButton();
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            if (IsActive) CheckIfConfirmReady();
        }

        void CheckIfConfirmReady()
        {
            bool levelHasBeenChosen = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("levelID");
            bool isNotMasterClient = !PhotonNetwork.LocalPlayer.IsMasterClient;
            bool hasNotConfirmedYet = !(bool)PhotonNetwork.LocalPlayer.CustomProperties["ready"];

            if (levelHasBeenChosen && isNotMasterClient && hasNotConfirmedYet)
            {
                StartCoroutine(ActivateMoveOutButton("confirm_ready", ConfirmReadyForLevel));
            }
        }

        void ConfirmReadyForLevel()
        {
            // Hide the button
            HideMoveOutButton();

            // Prevent backing out
            MainMenuManager.instance.backButton.SetActive(false);

            // Update player status to be ready
            PhotonGameManager.Instance.ConfirmReadyForLevel();
        }


        public static void ShowLevelSelect()
        {
            LevelSelectManager.Initialize();
        }

        public static void HideMoveOutButton()
        {
            Instance.moveOutButton.gameObject.SetActive(false);
        }

        public static void HighlightMoveOutButton() {
            if (Instance.moveOutButton.gameObject.activeInHierarchy)
            {
                Instance.moveOutButton.Select();
                MainMenuManager.instance.lastSelectedGameObject = Instance.moveOutButton.gameObject;
            }
        }

        void UpdatePlayerList()
        {
            if (!IsActive) {
                // Debug.Log("SquadRoomManager - not active so return");
                return;
            }

            players = PhotonNetwork.PlayerList.ToList();
            players.Sort((a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

            string s = "Current players:\n";
            foreach (Player p in players)
            {
                s += $"{p.NickName} (id {p.ActorNumber}) - {p.CustomProperties.ToStringFull()}\n";
            }
            // Debug.Log(s);

            // Update labels
            foreach (SquadRoomPlayerLabel label in labels)
            {
                label.UpdateContents();
            }
        }

        #region Photon Functions
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Player joined, their ID is {newPlayer.ActorNumber}!");
            UpdatePlayerList();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"Player left, their ID was {otherPlayer.ActorNumber}!!");
            UpdatePlayerList();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            UpdatePlayerList();
        }
        #endregion
    }
}