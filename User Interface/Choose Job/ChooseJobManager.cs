using UnityEngine;
using Photon.Pun;
using Sneaksters.Gameplay;

namespace Sneaksters.UI.Menus
{
    public class ChooseJobManager : MonoBehaviourPunCallbacks
    {
        public static ChooseJobManager Instance { get; private set; }= null;
        [SerializeField] ChooseJobButton thiefJobButton = null, hackerJobButton = null;

        [SerializeField] public PosterReticle reticle = null;

        #pragma warning disable 414
        PlayerType currentSelection = PlayerType.Thief;
        #pragma  warning restore 414


        public static bool IsActive { get; private set; } = false;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Debug.LogError("More than one instance of ChooseJobManager, deleting the new one!");
                Destroy(gameObject);
            }
        }

        void Start() {
            Instance.thiefJobButton.manager = Instance;
            Instance.hackerJobButton.manager = Instance;

            Instance.thiefJobButton.button.onClick.AddListener(() => Confirm(PlayerType.Thief));
            Instance.hackerJobButton.button.onClick.AddListener(() => Confirm(PlayerType.ComputerGuy));
        }

        public static void Initialize()
        {
            Debug.Log("ChooseJobManager opened");
            ActiveCamera.SetActiveCamera("chooseJob");

            var selectedJob = PhotonGameManager.Instance.GetLocalPlayerJob();
            switch (selectedJob)
            {
                case PlayerType.ComputerGuy:
                    Instance.HighlightHacker();
                    break;
                default:
                    Instance.HighlightThief();
                    break;
            }

            Instance.reticle.Enable();

            IsActive = true;
        }

        public static void Disable()
        {
            IsActive = false;
            Instance.thiefJobButton.Deselect();
            Instance.hackerJobButton.Deselect();

            Instance.reticle.Disable();


        }

        public static void Confirm(PlayerType selectedType) {
            if (!IsActive) {
                print("This menu isn't active, so don't do anything");
                return;
            }

            Disable();

            IsActive = false;
            
            FMODUnity.RuntimeManager.PlayOneShot(PhotonGameManager.Instance.sfxForwardEvent, Vector3.zero);

            PhotonGameManager.Instance.SetLocalPlayerJob(selectedType);

            MainMenuManager.instance.ShowSquadRoom();
        }

        void HighlightThief()
        {
            currentSelection = PlayerType.Thief;

            if (thiefJobButton != null)
                thiefJobButton?.button.Select();

            if (hackerJobButton != null)
                hackerJobButton?.Deselect();
        }

        

        void HighlightHacker()
        {
            currentSelection = PlayerType.ComputerGuy;

            if (thiefJobButton != null)
                thiefJobButton?.Deselect();
            
            if (hackerJobButton != null)
                hackerJobButton?.button.Select();
        }
    }
}