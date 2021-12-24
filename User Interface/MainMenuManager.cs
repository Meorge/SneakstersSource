using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using DG.Tweening;
using Boomlagoon.JSON;
using Photon.Pun;
using TMPro;

using Sneaksters.Gameplay;
using Sneaksters.Customization;
using Sneaksters.Localization;
using Sneaksters.UI.Animation;

namespace Sneaksters.UI.Menus {
    public class MainMenuManager : MonoBehaviour
    {

        public static MainMenuManager instance = null;

        public PlayerInput playerInput;
        public InputSystemUIInputModule uiInputModule;
        public GameObject lastSelectedGameObject;
        public GameObject savedSelectedGameObject;
        public new Camera camera;
        public DoScreenIrisShader iris;

        public bool listIsPopulating = false;

        public GameObject logoObject;
        public LogoAnimator logoAnimator;
        public SimpleLocalizeText currentMenuLabel;

        public SimpleLocalizeText backButtonLabel;
        public GameObject backButton;

        public Color topLevelColor = Color.white;
        public RectTransform menuButtonContainer;
        public Vector2 menuButtonContainerNormalAnchoredPos;
        public ScrollRect menuButtonScrollRect;
        public Transform bigMenuButtonContainer;

        public MenuButton topLevelMenu;

        public List<MenuButton> allMenuButtons = new List<MenuButton>();
        public List<UIMainMenuButton> allMenuItems = new List<UIMainMenuButton>();
        public MenuButton currentButton;
        public GameObject mainMenuButtonPrefab;
        public GameObject mainMenuBigButtonPrefab;
        public UIButtonCursorAnimator buttonCursor = null;

        public bool atTopLevel = true;

        public TextAsset buttonJSON;

        public SimpleLocalizeText buttonDescription;

        [Header("Scrollin")]
        public int numberOfButtonsThatFit = 5;
        public int visiblePosition = 0;
        public int actualPosition = 0;
        
        [SerializeField] Button upButton = null, downButton = null;

        [Header("Control stuff")]
        public ControllerPreviewAnimator controlPreviewAnimator;

        [Header("Code Entry")]
        public TMP_InputField codeInput;
        public SquadCodeEntryAnimator codeInputAnimator;

        [Header("Modal Stuff")]
        public Modal modal;

        [Header("Background stuff")]
        public Image halftoneA;
        public Image halftoneB;
        public Image background;
        public Image buttonBackdrop;
        public Image buttonBackdropShadow;


        public Image horizBackdrop;
        public Image horizBackdropShadow;
        public bool verticalStripeIn = true;
        public bool horizontalStripeIn = false;


        public Image descriptionBackdrop;
        public Image descriptionBackdropShadow;
        public bool descriptionStripeIn = true;

        [Header("Squad room stuff")]
        public TextMeshProUGUI squadCodeLabel;
        public SquadRoomCodeAnimator squadCodeAnimator;
        public bool squadCodeVisible = true;
        public GameObject squadCodeTogglePrompt;

        [Header("Level Select")]
        public GameObject levelItemContainer;
        public GameObject levelItemPrefab;
        public MissionSelectAnimator levelSelectAnimator;
        public Button levelSelectButton;
        public MissionReadyAnimator confirmMissionAnimator;
        public Button confirmMissionButton;

        [Header("Credits")]
        public bool creditsActive = false;
        public TextAsset creditsJSON;
        public Dictionary<string, string> creditsNames;

        [Header("Title screen")]
        [SerializeField]
        PressStartAnimator pressKeyPrompt = null;

        public int backTimer = 0;
        public int noSelectTimer = 0;
        bool backButtonTriggered = false;

        void Awake() {
            if (instance) {
                Destroy(gameObject);
                return;
            } else {
                instance = this;
            }

            GenerateMenuButtonData(buttonJSON);
        }

        // Start is called before the first frame update
        void Start()
        {
            playerInput = uiInputModule.gameObject.GetComponent<PlayerInput>();

            Cursor.lockState = CursorLockMode.None;

            PopulateCreditsNameList();
            controlPreviewAnimator.SetControllerVisibility(false);

            // By default, remove the arrows
            upButton.gameObject.SetActive(false);
            downButton.gameObject.SetActive(false);

            // By default, disable reticle
            buttonCursor.Disable();

            // save this so we can reset it
            menuButtonContainerNormalAnchoredPos = menuButtonContainer.anchoredPosition;

            if (PhotonGameManager.Instance != null) PhotonGameManager.Instance.CleanCharacterControllerList();
            horizBackdrop.fillAmount = 0f;
            horizBackdropShadow.fillAmount = 0f;
            buttonBackdrop.fillAmount = 0f;
            buttonBackdropShadow.fillAmount = 0f;
            descriptionBackdrop.fillAmount = 0f;
            descriptionBackdropShadow.fillAmount = 0f;

            horizontalStripeIn = false;
            verticalStripeIn = false;
            descriptionStripeIn = false;

            codeInputAnimator.SetOut();

            confirmMissionButton.onClick.AddListener(
                delegate {
                    // confirmMissionAnimator.SetBool("active", false);
                    confirmMissionAnimator.AnimateOut();
                    PhotonGameManager.Instance.ConfirmReadyForLevel();
                }
            );

            SquadRoomManager.HideMoveOutButton();

            uiInputModule.cancel.action.performed += BackButtonFromInput;
            uiInputModule.submit.action.performed += GoToMainScreen;
            
            
            PhotonGameManager.Instance?.iris?.SetIrisIn();

            if (PhotonGameManager.Instance.gameLocationSource == PhotonGameManager.GameLocationSource.PreviousGameSession) {
                DOTween.Sequence()
                    .AppendInterval(1f)
                    .AppendCallback(() => {
                        PhotonGameManager.Instance.PlayMusic("Title");
                        PhotonGameManager.Instance?.iris?.AnimateIrisOut();
                    });
                

                // will probably maybe break with new modes??
                currentButton = LocateMenuButtonFromID("createRoom_gemHeist");
                PhotonGameManager.Instance.JoiningRoom();
                currentMenuLabel.gameObject.SetActive(false);
                currentMenuLabel.textID = "";
                logoObject.SetActive(false);
                logoAnimator.SetSmall();
                backButtonLabel.textID = "back_button";
            } else {
                InitMainMenu();
            }


        }

        public void Update() {
            if (backTimer > 0) backTimer--;

            if (listIsPopulating) return;

            if (EventSystem.current.currentSelectedGameObject == null) {
                
                if (noSelectTimer > 0) noSelectTimer--;
                else if (currentButton != null && currentButton.buttonID == "joinRoom") EventSystem.current.SetSelectedGameObject(codeInput.gameObject);

                else if (lastSelectedGameObject == null && allMenuItems.Count > 0) {
                    allMenuItems[0]?.ForceSelect();
                    noSelectTimer = 10;
                } else {
                    EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
                    noSelectTimer = 10;
                }
                
            } else {
                lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
        }

        public void SelectNextButtonDown()
        {
            buttonCursor.instantSnap = false;
            if (allMenuItems.Count - 1 >= actualPosition) allMenuItems[actualPosition].button.navigation.selectOnDown.Select();
        }

        public void SelectNextButtonUp()
        {
            buttonCursor.instantSnap = false;
            if (allMenuItems.Count - 1 >= actualPosition) allMenuItems[actualPosition].button.navigation.selectOnUp.Select();
        }

        private void GoToMainScreen(InputAction.CallbackContext c) {
            if (PhotonGameManager.Instance.GetBuildExpired()) return;
            if (currentButton == null) return;
            if (currentButton.buttonID == "title") {
                backTimer = 10;
                MainMenuManager.instance.PopulateMenuWithButtons(LocateMenuButtonFromID("root"));
                logoAnimator.AnimateToSmall();  
            }
        }


        private void AnimateVerticalStripeIn() {
            // buttonBackdrop.fillAmount = 0f;
            // buttonBackdropShadow.fillAmount = 0f;

            buttonBackdrop.fillOrigin = (int) Image.OriginVertical.Top;
            buttonBackdropShadow.fillOrigin = (int) Image.OriginVertical.Top;

            buttonBackdrop.DOFillAmount(1, 0.5f);
            buttonBackdropShadow.DOFillAmount(1, 0.5f);

            verticalStripeIn = true;
        }

        private void AnimateVerticalStripeOut() {
            // buttonBackdrop.fillAmount = 1f;
            // buttonBackdropShadow.fillAmount = 1f;

            buttonBackdrop.fillOrigin = (int) Image.OriginVertical.Bottom;
            buttonBackdropShadow.fillOrigin = (int) Image.OriginVertical.Bottom;

            buttonBackdrop.DOFillAmount(0, 0.5f);
            buttonBackdropShadow.DOFillAmount(0, 0.5f);

            verticalStripeIn = false;
        }


        private void AnimateHorizontalStripeIn() {
            // horizBackdrop.fillAmount = 0f;
            // horizBackdropShadow.fillAmount = 0f;

            horizBackdrop.fillOrigin = (int) Image.OriginVertical.Bottom;
            horizBackdropShadow.fillOrigin = (int) Image.OriginVertical.Bottom;

            horizBackdrop.DOFillAmount(1, 0.4f);
            horizBackdropShadow.DOFillAmount(1, 0.4f);

            horizontalStripeIn = true;
        }

        public void AnimateHorizontalStripeOut(bool toLeft = false) {
            // horizBackdrop.fillAmount = 1f;
            // horizBackdropShadow.fillAmount = 1f;

            int thing = toLeft ? (int) Image.OriginVertical.Bottom : (int) Image.OriginVertical.Top;
            horizBackdrop.fillOrigin = thing;
            horizBackdropShadow.fillOrigin = thing;

            horizBackdrop.DOFillAmount(0, 0.4f);
            horizBackdropShadow.DOFillAmount(0, 0.4f);

            horizontalStripeIn = false;
        }

        // FOR DESCRIPTION STRIPE
        public void AnimateDescriptionStripeIn() {
            //Debug.Log("Animate time");
            // descriptionBackdrop.fillAmount = 0f;
            // descriptionBackdropShadow.fillAmount = 0f;

            descriptionBackdrop.fillOrigin = (int) Image.OriginVertical.Bottom;
            descriptionBackdropShadow.fillOrigin = (int) Image.OriginVertical.Bottom;

            descriptionBackdrop.DOFillAmount(1, 0.4f);
            descriptionBackdropShadow.DOFillAmount(1, 0.4f);

            descriptionStripeIn = true;
        }

        public void AnimateDescriptionStripeOut(bool toLeft = false) {
            // descriptionBackdrop.fillAmount = 1f;
            // descriptionBackdropShadow.fillAmount = 1f;

            int thing = toLeft ? (int) Image.OriginVertical.Bottom : (int) Image.OriginVertical.Top;
            descriptionBackdrop.fillOrigin = thing;
            descriptionBackdropShadow.fillOrigin = thing;

            descriptionBackdrop.DOFillAmount(0, 0.4f);
            descriptionBackdropShadow.DOFillAmount(0, 0.4f);

            descriptionStripeIn = false;
        }

        public MenuButton LocateMenuButtonFromID(string id) {
            foreach (MenuButton b in allMenuButtons) {
                if (b.buttonID == id) return b;
            }
            return null;
        }

        public void GenerateMenuButtonData(TextAsset t) {
            topLevelMenu = new MenuButton();
            //Debug.Log(t.text);
            JSONObject j = JSONObject.Parse(t.text);

            foreach (JSONValue item in j["buttons"].Array) {
                MenuButton b = new MenuButton();
                if (item.Obj["buttonID"] != null) b.buttonID = item.Obj["buttonID"].Str;
                if (item.Obj["parentButtonID"] != null) b.parentButtonID = item.Obj["parentButtonID"].Str;
                if (item.Obj["buttonTextID"] != null) b.buttonTextID = item.Obj["buttonTextID"].Str;
                if (item.Obj["selectCameraID"] != null) b.selectCameraID = item.Obj["selectCameraID"].Str;
                if (item.Obj["hoverCameraID"] != null) b.hoverCameraID = item.Obj["hoverCameraID"].Str;

                if (item.Obj["buttonDescriptionID"] != null) b.buttonDescriptionID = item.Obj["buttonDescriptionID"].Str;
                if (item.Obj["runFunction"] != null) b.runFunction = item.Obj["runFunction"].Str;
                else b.runFunction = "";

                if (item.Obj["settingID"] != null) { // This is a setting button - let's find what it's a setting for
                    b.settingID = item.Obj["settingID"].Str;
                }

                if (item.Obj["appearance"] != null) {
                    switch (item.Obj["appearance"].Str) {
                        case "big_settings":
                            b.appearance = MenuButton.MenuButtonAppearance.BigSetting;
                            break;
                        case "horizontal_bar":
                            b.appearance = MenuButton.MenuButtonAppearance.HorizontalStripe;
                            break;
                        case "noStripes":
                            b.appearance = MenuButton.MenuButtonAppearance.NoStripes;
                            break;
                        default:
                            b.appearance = MenuButton.MenuButtonAppearance.Normal;
                            break;
                    }
                } else {
                    b.appearance = MenuButton.MenuButtonAppearance.Normal;
                }

                if (item.Obj["buttonColor"] != null) {
                    Color col = new Color((float)item.Obj["buttonColor"].Obj["r"].Number, (float)item.Obj["buttonColor"].Obj["g"].Number, (float)item.Obj["buttonColor"].Obj["b"].Number);
                    b.buttonColor = col;
                }

                if (item.Obj["subButtons"] != null) {
                    foreach (JSONValue subButton in item.Obj["subButtons"].Array) {
                        b.subMenuIDs.Add(subButton.Str);
                    }
                }

                allMenuButtons.Add(b);
            }

            string initialMenu = j["default_button"].Str;
            topLevelMenu = LocateMenuButtonFromID(initialMenu);
        }

        public void PopulateListOfControlSchemes() {
            Debug.Log("Populate list of control schemes");

            List<MenuButton> bs = new List<MenuButton>();

            foreach (Transform t in menuButtonContainer) {
                Destroy(t.gameObject);
            }

            MenuButton kb = new MenuButton();
            kb.parentButtonID = "settings_controls_customizecontrols";
            kb.buttonTextID = "_Keyboard & Mouse";
            kb.buttonID = "CONTROL_KeyboardMouse";
            Color buttonColor = currentButton.buttonColor;
            kb.buttonColor = buttonColor;
            bs.Add(kb);
            foreach (InputDevice d in InputSystem.devices) {
                if (d.name != "Keyboard" && d.name != "Mouse" && d.name != "Pen") {
                    MenuButton b = new MenuButton();
                    b.parentButtonID = "settings_controls_customizecontrols";
                    b.buttonTextID = string.Format("_{0}", d.displayName);
                    b.buttonID = string.Format("CONTROL_{0}", d.name);
                    b.buttonColor = buttonColor;

                    bs.Add(b);
                }
            }

            Debug.Log(bs.Count);

            StartCoroutine(PopulateButtonsCoroutine(bs, MenuButton.MenuButtonAppearance.Normal, false, "settings_controls_customizecontrols"));
            lastSelectedGameObject = allMenuItems[0].gameObject;
        }

        public void SetCurrentControllerPreview(string id) {
            ControllerConfigObject o = null;
            foreach (ControllerConfigObject c in PhotonGameManager.Instance.controllerConfigCatalog.configs) {
                if (c.ControllerSupported(id)) {
                    o = c;
                    break;
                }
            }

            if (o == null) {
                Debug.LogError(string.Format("Controller with ID {0} not found!", id));
                return;
            }



            controlPreviewAnimator.SetSprites(o.controllerImage, o.controllerSilImage);
            controlPreviewAnimator.AnimateControllerIn();
            controlPreviewAnimator.SetControllerVisibility(true);
        }

        public void PopulateListOfHats() {
            //Debug.LogFormat("Populate list of hats");
            List<MenuButton> hatIcons = new List<MenuButton>();

            foreach (Transform t in menuButtonContainer) {
                Destroy(t.gameObject);
            }

            foreach (HatItem hatItem in PhotonGameManager.Instance.currentHatCatalog.hats) {
                MenuButton hatButton = new MenuButton();
                hatButton.icon = hatItem.icon;
                hatButton.parentButtonID = "customize_mySneakID_playerhat";
                hatButton.buttonTextID = string.Format("_{0}", hatItem.name);
                hatButton.buttonID = string.Format("HAT_{0}", hatItem.id);
                hatButton.buttonColor = currentButton.buttonColor;
                hatButton.itemType = MenuButton.MenuItemType.ChoiceSelect;
                hatButton.onChoiceSelect += delegate { PhotonGameManager.Instance.UpdatePlayerHat(hatItem.id); };

                if (hatItem.id == PlayerPrefs.GetString("hat", "top_hat")) {
                    hatButton.choiceSelectDefault = true;
                }
                hatIcons.Add(hatButton);
            }

            currentMenuLabel.textID = "hat_select";

            

            StartCoroutine(PopulateButtonsCoroutine(hatIcons, MenuButton.MenuButtonAppearance.Normal, false, "customize_mySneakID_playerhat"));
            lastSelectedGameObject = allMenuItems[0].gameObject;
        }

        public void UncheckAllButtons() {
            foreach (UIMainMenuButton b in allMenuItems) {
                b.ChoiceSelectSelected = false;
            }
        }
        public void DoScrollingStuff(UIMainMenuButton newButton) {
            /*
            Scroll when:
            - Current selected is not the last in the list, but is at the bottom
            - Current selected is not the first in the list, but is at the top
            */

            // if there's not many buttons don't worry about it
            if (allMenuItems.Count <= numberOfButtonsThatFit) 
            {
                upButton.gameObject.SetActive(false);
                downButton.gameObject.SetActive(false);
                return;
            }

            int newActualPos = allMenuItems.IndexOf(newButton);

            VerticalScrollDirection scrollDir = VerticalScrollDirection.None;
            if (newActualPos < actualPosition) scrollDir = VerticalScrollDirection.Up;
            else if (newActualPos > actualPosition) scrollDir = VerticalScrollDirection.Down;

            int numSkipped = Mathf.Abs(newActualPos - actualPosition);

            actualPosition = newActualPos;

            Vector2 b = menuButtonContainer.anchoredPosition;

            // At top of current visible
            if (actualPosition >= 0 && visiblePosition == 0 && scrollDir == VerticalScrollDirection.Up) {
                // Scroll up
                menuButtonContainer.DOAnchorPosY(b.y - 50, 0.1f);
            }

            // At bottom of current visible
            else if (actualPosition <= allMenuItems.Count - 1 && visiblePosition >= numberOfButtonsThatFit - 1 && scrollDir == VerticalScrollDirection.Down) {
                // Scroll down
                menuButtonContainer.DOAnchorPosY(b.y + 50, 0.1f);
            }

            // Not at bottom or top, so let's change visible position
            else {
                if (scrollDir == VerticalScrollDirection.Down) {
                    visiblePosition = Mathf.Clamp(visiblePosition + numSkipped, 0, numberOfButtonsThatFit - 1);
                }
                else if (scrollDir == VerticalScrollDirection.Up) {
                    visiblePosition = Mathf.Clamp(visiblePosition - numSkipped, 0, numberOfButtonsThatFit - 1);
                }
            }

            // The down button should appear when the LAST VISIBLE BUTTON (NOT the selected button)
            // is NOT the same as the total last button.

            // How do we get this...?
            // First, we need to get the current visible buttons.
            // From there, we can the last item in that list.
            // I think we can map the visible buttons to the actual list of buttons, somehow?

            // Or, wait, maybe an easier way to do this.
            // We can see, for example, that the actual is 5/6 and the visible is 4/5.
            // The differences between the current and maximum possible values is the same, so I think
            // that means we're at the bottom?

            int actualDiff = (allMenuItems.Count - 1) - actualPosition;
            int visibleDiff = (numberOfButtonsThatFit - 1) - visiblePosition;

            if (actualDiff != visibleDiff)
            {
                downButton.gameObject.SetActive(true);
            } else 
            {
                downButton.gameObject.SetActive(false);
            }

            // Finding whether or not we're at the top should be easier - just check
            // if the current actualPosition and visiblePosition line up I think?
            if (actualPosition != visiblePosition)
            {
                upButton.gameObject.SetActive(true);
            } else
            {
                upButton.gameObject.SetActive(false);
            }
        }

        public void PopulateListOfLanguages() {
            foreach (Transform t in menuButtonContainer) {
                Destroy(t.gameObject);
            }

            List<MenuButton> langButtons = new List<MenuButton>();

            foreach (LocalizationPack p in PhotonGameManager.Instance.localizationPackCollection.localizationPacks) {
                MenuButton langButton = new MenuButton();
                langButton.parentButtonID = "settings_language";
                langButton.buttonTextID = $"_{p.languageName}";
                langButton.icon = PhotonGameManager.Instance.localizationPackCollection.GetIconForRegion(p.languageCode);
                langButton.buttonID = string.Format($"LANG_{p.languageCode.ToUpper()}");
                langButton.buttonColor = currentButton.buttonColor;
                langButton.itemType = MenuButton.MenuItemType.ChoiceSelect;
                langButton.onChoiceSelect += delegate { PhotonGameManager.Instance.SetLanguageID(p.languageCode); };
                if (PlayerPrefs.GetString("language", "en") == p.languageCode) {
                    langButton.choiceSelectDefault = true;
                }

                langButtons.Add(langButton);    
            }

            StartCoroutine(PopulateButtonsCoroutine(langButtons, MenuButton.MenuButtonAppearance.Normal, false, "settings_language"));

        }

        public void PopulateMenuWithButtons(MenuButton clickedButton, bool fromBelow = false) {
            // Set grid layout to have 1 column
            menuButtonContainer.GetComponent<GridLayoutGroup>().constraintCount = 1;

            // TODO: Need to make handling the title screen more robust
            backButton.SetActive(clickedButton.buttonID != "title");

            // By default, disable the scroll buttons
            upButton.gameObject.SetActive(false);
            downButton.gameObject.SetActive(false);

            if (PhotonGameManager.Instance.GetBuildExpired()) return;

            // Handle title screen
            if (clickedButton.buttonID == "title") {
                pressKeyPrompt.AnimateIn();
            } else {
                pressKeyPrompt.AnimateOut();
            }

            visiblePosition = 0;
            actualPosition = 0;

            buttonDescription.textID = "";
            SetBackgroundColors(clickedButton.buttonColor);
            
            codeInput.onValueChanged.RemoveAllListeners();

            // chooseJobObject.SetActive(false);

            ChooseJobManager.Disable();

            codeInput.text = "";

            if (clickedButton.runFunction == "joinRoom") codeInputAnimator.AnimateIn();
            else codeInputAnimator.AnimateOutLeft();

            if (clickedButton.runFunction != "") {
                switch (clickedButton.runFunction) {
                    case ("joinRoom"):
                        codeInput.onValueChanged.AddListener(AttemptToJoinRoom);
                        break;
                    case ("connectToInternet"):
                        // Let's create a new Gem Heist room...
                        if (!fromBelow) PhotonGameManager.Instance.TryConnectToInternet();
                        break;

                    case "singlePlayer":
                        Debug.Log("Single player time!");
                        StartCoroutine(PhotonGameManager.Instance.CreateOfflineRoom(
                            () =>
                            {
                                AnimateVerticalStripeOut();
                                AnimateHorizontalStripeOut();
                            }));
                        break;
                    
                    case ("createRoom_gemHeist"):
                        AnimateVerticalStripeOut();
                        AnimateDescriptionStripeOut();
                        StartCoroutine(PhotonGameManager.Instance.CreateRoom(GameMode.GemHeist));
                        break;

                    case ("customize_controls"):
                        currentButton = clickedButton;
                        PopulateListOfControlSchemes();
                        return;

                    case ("hat_list"):
                        currentButton = clickedButton;
                        PopulateListOfHats();
                        return;

                    case ("language"):
                        currentButton = clickedButton;
                        PopulateListOfLanguages();
                        return;

                    case ("credits"):
                        currentButton = clickedButton;
                        StartCredits();
                        return;
                }   
            }

            List<MenuButton> buttons = new List<MenuButton>();
            foreach (string s in clickedButton.subMenuIDs) {
                buttons.Add(LocateMenuButtonFromID(s));
            }

            string parentButton;
            if (currentButton != null)
                parentButton = currentButton.buttonID;
            else
                parentButton = "root";

            currentButton = clickedButton;

            if (clickedButton.runFunction != "customize_controls") {
                foreach (Transform t in menuButtonContainer) {
                    Destroy(t.gameObject);
                }

                foreach (Transform t in bigMenuButtonContainer) {
                    Destroy(t.gameObject);
                }
            }

            if (!fromBelow) {
                ActiveCamera.SetActiveCamera(clickedButton.selectCameraID);
            }


            StartCoroutine(PopulateButtonsCoroutine(buttons, clickedButton.appearance, fromBelow, parentButton));



            if (currentButton.buttonID == "root") {
                currentMenuLabel.gameObject.SetActive(false);
                logoObject.SetActive(true);
                if (logoAnimator.IsLarge) logoAnimator.AnimateToSmall();
                backButtonLabel.textID = "quit_button";
            } else {
                currentMenuLabel.gameObject.SetActive(true);
                currentMenuLabel.textID = currentButton.appearance == MenuButton.MenuButtonAppearance.NoStripes ? "" : currentButton.buttonTextID;
                if (currentButton.buttonID == "title") {
                    logoAnimator.SetLarge();
                }
                else {
                    logoObject.SetActive(false);
                }
                backButtonLabel.textID = "back_button";
            }

        }

        public SettingItemData GetSettingItem(MenuButton item) {
            SettingItemData result = null;
            switch (item.settingID) {
                case "discordrp":
                    result = SettingItemData.DefineToggleSetting("DiscordRichPresence", PhotonGameManager.Instance.UpdateDiscordRP);
                    break;

                case "server":
                    result = SettingItemData.DefineDropdownSetting("ServerRegion", null, new List<string> { "US" });
                    break;

                case "full_screen":
                    result = SettingItemData.DefineToggleSetting("fullScreen", PhotonGameManager.Instance.UpdateFullScreen);
                    break;


                case "quality_level":
                    List<string> qualLvls = new List<string>();

                    foreach (string q in QualitySettings.names) {
                        qualLvls.Add(q);
                    }

                    result = SettingItemData.DefineDropdownSetting("quality", PhotonGameManager.Instance.UpdateQualityLevel, qualLvls);
                    break;

                case "screen_res":
                    List<string> items = new List<string>();

                    foreach (ResolutionItem i in PhotonGameManager.Instance.listOfResolutions) {
                        items.Add(string.Format("{0}x{1}", i.width, i.height));
                    }
                    result = SettingItemData.DefineDropdownSetting("resolution", PhotonGameManager.Instance.UpdateResolution, items);
                    break;

                case "music_vol":
                    result = SettingItemData.DefineSliderSetting("musicVol", PhotonGameManager.Instance.UpdateMusicVol, 0.0001f, 1f, 0.5f);
                    break;

                case "sfx_vol":
                    result = SettingItemData.DefineSliderSetting("sfxVol", PhotonGameManager.Instance.UpdateSFXVol, 0.0001f, 1f, 0.5f);
                    break;

                case "controls_inverty":
                    result = SettingItemData.DefineToggleSetting("invertYMouselook", PhotonGameManager.Instance.UpdateInvertY);
                    break;

                case "controls_mouse_sensitivity":
                    result = SettingItemData.DefineSliderSetting("mouseSensitivity", PhotonGameManager.Instance.UpdateMouseSensitivity, 0f, 2f, 1f);
                    break;

                case "hidecode":
                    result = SettingItemData.DefineToggleSetting("hideCode", PhotonGameManager.Instance.UpdateHideCode);
                    break;

                case "playername":
                    result = SettingItemData.DefineStringSetting("playerName", PhotonGameManager.Instance.UpdatePlayerName);
                    break;

                case "playerhat":
                    result = SettingItemData.DefineStringSetting("hat", PhotonGameManager.Instance.UpdatePlayerHat);
                    break;

                case "playercolor":
                    result = SettingItemData.DefineStringSetting("playerColor", PhotonGameManager.Instance.UpdatePlayerColor);
                    break;

                default:
                    break;
            }
            return result;
        }

        public UIMainMenuButton CreateAndAddButton(MenuButton b, bool setButtonCurrent = false, int index = 0) {
            GameObject g = Instantiate(mainMenuButtonPrefab);
            UIMainMenuButton mainMenuButton = g.GetComponent<UIMainMenuButton>();
            mainMenuButton.SetUpButton(b, GetSettingItem(b), index);

            allMenuItems.Add(mainMenuButton);

            g.transform.SetParent(menuButtonContainer.transform, false);

            if (setButtonCurrent) {
                
                mainMenuButton.ForceSelect();
                // mainMenuButton.button.Select();
                lastSelectedGameObject = mainMenuButton.gameObject;
            }

            // mainMenuButton.button.interactable = !modalAnimator.GetBool("enabled");
            // mainMenuButton.button.interactable = !modalAnimator.ModalActive;
            return mainMenuButton;
        }

        public void SetupButtonRelatonships() {
            // Set the button relationships
            for (int i = 0; i < allMenuItems.Count; i++) {
                Button b = allMenuItems[i].button;
                var nav = b.navigation;
                if (i > 0) {
                    nav.selectOnUp = allMenuItems[i - 1].button;
                }
                if (i < allMenuItems.Count - 1) {
                    nav.selectOnDown = allMenuItems[i + 1].button;
                }

                b.navigation = nav;
            }
        }

        private IEnumerator PopulateButtonsCoroutine(List<MenuButton> buttons, MenuButton.MenuButtonAppearance appearance, bool fromBelow = false, string parentButton = "") {
            // Set grid layout to have 1 column
            menuButtonContainer.GetComponent<GridLayoutGroup>().constraintCount = 1;

            visiblePosition = 0;
            actualPosition = 0;
            allMenuItems.Clear();

            GameObject g;

            listIsPopulating = true;
            if (appearance == MenuButton.MenuButtonAppearance.Normal || appearance == MenuButton.MenuButtonAppearance.NoStripes) {
                if (appearance == MenuButton.MenuButtonAppearance.Normal) {
                    if (!verticalStripeIn) AnimateVerticalStripeIn();
                    if (horizontalStripeIn) AnimateHorizontalStripeOut(true);
                    if (!descriptionStripeIn) AnimateDescriptionStripeIn();
                } else {
                    if (horizontalStripeIn) AnimateHorizontalStripeOut();
                    if (verticalStripeIn) AnimateVerticalStripeOut();
                    if (descriptionStripeIn) AnimateDescriptionStripeOut();
                }

                foreach (MenuButton b in buttons) {
                    bool focusButton = (fromBelow && b.buttonID == parentButton) ||(!fromBelow && buttons.IndexOf(b) == 0);
                    UIMainMenuButton mainMenuButton = CreateAndAddButton(b, focusButton, buttons.IndexOf(b));
                }

                SetupButtonRelatonships();


            } else if (appearance == MenuButton.MenuButtonAppearance.HorizontalStripe) {
                if (!horizontalStripeIn) AnimateHorizontalStripeIn();
                if (verticalStripeIn) AnimateVerticalStripeOut();
                if (descriptionStripeIn) AnimateDescriptionStripeOut();
                foreach (MenuButton b in buttons) {
                    g = Instantiate(mainMenuButtonPrefab);
                    UIMainMenuButton mainMenuButton = g.GetComponent<UIMainMenuButton>();

                    allMenuItems.Add(mainMenuButton);
                    mainMenuButton.SetUpButton(b);
                    g.transform.SetParent(menuButtonContainer.transform, false);
                    yield return new WaitForSeconds(0.05f);
                }
            }

            noSelectTimer = 5;
            listIsPopulating = false;

            // set up the arrows (maybe issues? if it doesn't just go back to the first button always?)
            upButton.gameObject.SetActive(false);
            downButton.gameObject.SetActive(allMenuItems.Count > numberOfButtonsThatFit);
        }

        public void SetBackgroundColors(Color a) {
            Color newColorHalftoneA = a.GetColorForChangedBrightness(0.6f);
            Color newColorHalftoneB = a.GetColorForChangedBrightness(0.05f);
            Color newColorBackground = a.GetColorForChangedBrightness(0.1f);

            Color newColorButtonBackdrop = a.GetColorForChangedBrightness(0.7f);
            Color newColorButtonBackdropShadow = a.GetColorForChangedBrightness(0.2f);

            newColorButtonBackdrop = new Color(0.1f, 0.1f, 0.1f);
            newColorButtonBackdropShadow = new Color(0.03f, 0.03f, 0.03f);

            buttonBackdrop.color = newColorButtonBackdrop;
            buttonBackdropShadow.color = newColorButtonBackdropShadow;

            descriptionBackdrop.color = newColorButtonBackdrop.GetColorForChangedBrightness(0.9f);
            descriptionBackdropShadow.color = newColorButtonBackdropShadow.GetColorForChangedBrightness(0.9f);

            horizBackdrop.color = newColorButtonBackdrop;
            horizBackdropShadow.color = newColorButtonBackdropShadow;

            // Sequence colorSeq = DOTween.Sequence();
            // colorSeq.Append(halftoneA.DOColor(newColorHalftoneA, 0.6f));
            // colorSeq.Join(halftoneB.DOColor(newColorHalftoneB, 0.6f));
            // colorSeq.Join(background.DOColor(newColorBackground, 0.6f));
            // colorSeq.Join(buttonBackdrop.DOColor(newColorButtonBackdrop, 0.6f));
            // colorSeq.Join(buttonBackdropShadow.DOColor(newColorButtonBackdropShadow, 0.6f));

            // colorSeq.Join(horizBackdrop.DOColor(newColorButtonBackdrop, 0.6f));
            // colorSeq.Join(horizBackdropShadow.DOColor(newColorButtonBackdropShadow, 0.6f));

            // colorSeq.Join(descriptionBackdrop.DOColor(newColorButtonBackdrop.GetColorForChangedBrightness(0.9f), 0.6f));
            // colorSeq.Join(descriptionBackdropShadow.DOColor(newColorButtonBackdropShadow.GetColorForChangedBrightness(0.9f), 0.6f));
        }

        public void InitMainMenu() {
            backButton.SetActive(false);
            ActiveCamera.SetActiveCamera("title");

            currentMenuLabel.gameObject.SetActive(false);
            logoAnimator.SetLarge();
            backButtonLabel.textID = "quit_button";

            DOTween.Sequence()
                .AppendInterval(1f)
                .AppendCallback(() => {
                    // PhotonGameManager.gameManager?.iris?.AnimateIrisOut(() => StartCoroutine(CheckIfValid()));
                    PhotonGameManager.Instance?.iris?.AnimateIrisOut(CheckIfValid);
                })
                .AppendInterval(0.15f)
                .AppendCallback(() => PhotonGameManager.Instance.PlayMusic("Title"));
            

        }

        // Used to be an IEnumerator
        public void CheckIfValid() {
            bool buildExpired = PhotonGameManager.Instance.GetBuildExpired();
            if (buildExpired) {
                EnableModal(ModalMode.Error, string.Format(PhotonGameManager.Instance.GetStringFromID("build_expired"), PhotonGameManager.Instance.GetDaysSinceExpired()), null, null, null, false);
            }


            PopulateMenuWithButtons(topLevelMenu);
        }

        public void BackButtonFromInput(InputAction.CallbackContext context) {
            if (backTimer == 0) {
                backTimer = 10;
                BackButton();
            }

        }

        public void BackButton() {
            if (PhotonGameManager.Instance.GetBuildExpired()) return;
            FMODUnity.RuntimeManager.PlayOneShot(PhotonGameManager.Instance.sfxBackEvent, transform.position);
            backButtonTriggered = true;

            menuButtonContainer.anchoredPosition = menuButtonContainerNormalAnchoredPos;

            controlPreviewAnimator.AnimateControllerOut();


            // Level Select stuff
            // bool levelSelectOpen = levelSelectAnimator.IsActive;
            // if (levelSelectOpen) {
            //     levelSelectAnimator.AnimateOut();
            //     if (PhotonGameManager.gameManager.IsMasterClient()) {
            //         SquadRoomManager.HighlightMoveOutButton();
            //     }
            //     return;
            // }
            Debug.Log("Back button pressed");
            if (LevelSelectManager.IsActive)
            {

                if (LevelSelectManager.State == LevelSelectManagerState.ViewingLocations)
                {
                    LevelSelectManager.Disable();
                    if (!PhotonNetwork.OfflineMode)
                        SquadRoomManager.ShowSquadRoom();
                    else
                        EnableModal(
                            ModalMode.YesNo,
                            "confirm_disconnect",
                            onAppear: null,
                            confirmEvent: () => {
                                savedSelectedGameObject = null;
                                DisableModal(true);
                                PhotonGameManager.Instance.LeaveRoom();

                                ActiveCamera.SetActiveCamera("root");

                                lastSelectedGameObject = null;

                                PopulateMenuWithButtons(LocateMenuButtonFromID("root"), true);
                            }, 
                            denyEvent: () => {
                                DisableModal();
                                ChooseJobManager.Initialize();
                            }
                        );
                }
                else if (LevelSelectManager.State == LevelSelectManagerState.ViewingMissions)
                {
                    LevelSelectManager.BackOutToLocations();
                }
                return;
            }

            // Squad screen
            // Should go back to job choosing screen
            bool currentlyInRoom = PhotonGameManager.Instance.CurrentlyInRoom();
            if (currentlyInRoom) {

                // If on the choose job screen, ask if the user wants to disconnect
                bool onChooseJobScreen = ChooseJobManager.IsActive;
                if (onChooseJobScreen)
                {
                    ChooseJobManager.Disable();
                    EnableModal(
                        ModalMode.YesNo,
                        "confirm_disconnect",
                        onAppear: null,
                        confirmEvent: () => {
                            savedSelectedGameObject = null;
                            DisableModal(true);
                            PhotonGameManager.Instance.LeaveRoom();

                            ActiveCamera.SetActiveCamera("root");

                            lastSelectedGameObject = null;

                            PopulateMenuWithButtons(LocateMenuButtonFromID("root"), true);
                        }, 
                        denyEvent: () => {
                            DisableModal();
                            ChooseJobManager.Initialize();
                        }
                    );
                    return;
                }


                // If on the squad list screen, return to the choose job screen
                bool onSquadListScreen = SquadRoomManager.IsActive;
                bool playerHasNotConfirmedYet = !PhotonGameManager.Instance.LocalPlayerIsReadyForLevel();
                if (onSquadListScreen && playerHasNotConfirmedYet)
                {
                    SquadRoomManager.HideSquadRoom();
                    ChooseJobManager.Initialize();
                    return;
                }
            }
            
            
            else {
                if (currentButton.buttonID == "root") {
                    Debug.Log("Quit the game");
                    Application.Quit();
                } else {
                    PopulateMenuWithButtons(LocateMenuButtonFromID(currentButton.parentButtonID), true);
                    
                }
            }
        }

        public void AttemptToJoinRoom(string newVal) {
            if (codeInput.text.Length != 4) return;
            //Debug.LogFormat("codeInput.text={0}, newVal={1}", codeInput.text, newVal);
            // StartCoroutine(PhotonGameManager.gameManager.JoinRoom(codeInput.text));
            PhotonGameManager.Instance.JoinRoom(codeInput.text);
        }

        public void EnableModal(ModalMode mode, string text_id = "connecting_to_server", Modal.ModalEvent onAppear = null, Modal.ModalEvent confirmEvent = null, Modal.ModalEvent denyEvent = null, bool errorCloseButton = true) {
            codeInput.readOnly = true;
            // modalAnimator.AnimateModalIn();

            // by default, keep user input enabled
            uiInputModule.enabled = true;

            
            foreach (UIMainMenuButton b in allMenuItems) {
                if (b != null && b.button != null) b.button.interactable = false;
            }

            switch (mode) {
                case ModalMode.Connecting:
                    modal.ShowConnecting(text_id, onAppear);

                    // disable user input
                    uiInputModule.enabled = false;
                    break;

                case ModalMode.Error:
                    savedSelectedGameObject = lastSelectedGameObject;
                    modal.ShowError(text_id, "_Ok", errorCloseButton, onAppear, confirmEvent);
                    break;
                
                case ModalMode.YesNo:
                    savedSelectedGameObject = lastSelectedGameObject;
                    modal.ShowYesNo(text_id, "yes", "no", onAppear, confirmEvent, denyEvent);
                    break;
            }
            
        }

        public void DisableModal(bool clearLastSelected = false) {
            codeInput.readOnly = false;
            modal.Hide(() => {
                foreach (UIMainMenuButton b in allMenuItems) {
                    if (b != null && b.button != null) b.button.interactable = true;
                }

                if (!clearLastSelected) {
                    lastSelectedGameObject = savedSelectedGameObject;
                    EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
                }
                else {
                    lastSelectedGameObject = null;
                    EventSystem.current.SetSelectedGameObject(null);
                }

                // Allow user to do inputs again
                uiInputModule.enabled = true;
                savedSelectedGameObject = null;
            });
        }

        public IEnumerator FakeConnect() {
            yield return new WaitForSeconds(0.5f);
            EnableModal(ModalMode.Connecting, "joining_room");
            yield return new WaitForSeconds(3f);
            DisableModal();
            yield return new WaitForSeconds(0.5f);

            if (codeInput.text == "CODE") {
                // codeInputAnimator.SetBool("active", false);
                // codeInputAnimator.SetTrigger("confirmed");
                // codeInputAnimator.SetBool("active", false);
                codeInputAnimator.AnimateOutRight();
                AnimateHorizontalStripeOut(false);
            } else {
                // codeInputAnimator.SetTrigger("incorrectCode");
                codeInputAnimator.AnimateIncorrect();
            }
            codeInput.readOnly = false;
        }

        public void ShowChooseJob() {
            currentMenuLabel.textID = "";
            pressKeyPrompt.gameObject.SetActive(false);

            PhotonGameManager.Instance.OnJoinedRoom();
            PhotonGameManager.Instance.ResetPlayerReady();
            ChooseJobManager.Initialize();            
        }

        public void ShowSquadRoom() {
            currentMenuLabel.textID = "";
            pressKeyPrompt.gameObject.SetActive(false);

            

            
            // print("Show squad room!");
            PhotonGameManager.Instance.gameState = GameState.InLobby;
            SquadRoomManager.ShowSquadRoom();
            
            // PopulateLevels();
            // TODO: Populate the new level list!
        }

        public void ShowLevelSelect() {
            // levelSelectAnimator.SetBool("active", true);
            // print("show level select");
            // levelSelectAnimator.AnimateIn();
            // GameObject g = levelItemContainer.transform.GetChild(0).gameObject;
            // g.GetComponent<Button>().Select();
            // lastSelectedGameObject = g;
            
            LevelSelectManager.Initialize();

        }


        // public void PopulateLevels() {
        //     foreach (Transform t in levelItemContainer.transform) Destroy(t.gameObject);

        //     foreach (LevelObject l in PhotonGameManager.gameManager.levelCatalog.levels) {
        //         UILevelSelectButton b = Instantiate(levelItemPrefab).GetComponent<UILevelSelectButton>();
        //         // b.SetupWithLevelObject(l);
        //         b.transform.SetParent(levelItemContainer.transform, false);
        //     }


        //     // Set up navigation
        //     for (int i = 0; i < levelItemContainer.transform.childCount; i++) {
        //         // current button
        //         Button current = levelItemContainer.transform.GetChild(i).GetComponent<Button>();

        //         var nav = current.navigation;

        //         // Button to the left, if it exists
        //         if (i % 6 > 0) {
        //             Button left = levelItemContainer.transform.GetChild(i - 1).GetComponent<Button>();
        //             nav.selectOnLeft = left;
        //         }
        //         if (i + 1 % 6 > 0 && levelItemContainer.transform.childCount > i + 1) {
        //             Button right = levelItemContainer.transform.GetChild(i + 1).GetComponent<Button>();
        //             nav.selectOnRight = right;
        //         }
        //         current.navigation = nav;

        //     }
        // }

        public void ToggleCode(InputAction.CallbackContext context) {
            if (PlayerPrefs.GetInt("hideCode") != 1) return;

            if (squadCodeVisible) {
                squadCodeVisible = false;
                squadCodeLabel.text = "????";
            } else {
                squadCodeVisible = true;
                squadCodeLabel.text = PhotonGameManager.Instance.GetSquadCode();
            }
        }


        // CREDITS BOI

        public void PopulateCreditsNameList() {
            creditsNames = new Dictionary<string, string>();
            
            // Parse credits json
            JSONObject j = JSONObject.Parse(creditsJSON.text);

            foreach (JSONValue nameSet in j["nameValues"].Array) {
                creditsNames.Add(nameSet.Obj["nameID"].Str, nameSet.Obj["nameText"].Str);
            }
        }

        public void StartCredits() {
            StartCoroutine(CreditCoroutine());
        }


        public IEnumerator CreditCoroutine() {
            // First, parse out the credits...
            JSONObject j = JSONObject.Parse(creditsJSON.text);

            List<MenuButton> nameMBList = new List<MenuButton>();




            // timer stuff
            // (seconds)
            float creditsTimer = 0;
            float creditsTimerOrig = 5f;

            bool breakOut = false;

            creditsActive = true;

            foreach (JSONValue creditCat in j["credits"].Array) {
                string jobTextID = creditCat.Obj["jobTextID"].Str;
                currentMenuLabel.textID = jobTextID;
                ActiveCamera.SetActiveCamera(creditCat.Obj["cameraID"].Str);


                nameMBList.Clear();
                foreach (Transform t in menuButtonContainer) {
                    Destroy(t.gameObject);
                }
                foreach (JSONValue name in creditCat.Obj["names"].Array) {
                    // Create a button object

                    MenuButton nameMB = new MenuButton();
                    //Debug.Log(creditsNames[name.Str]);

                    if (name.Str.StartsWith("^")) {
                        nameMB.buttonTextID = name.Str.Substring(1);
                    } else {
                        nameMB.buttonTextID = string.Format("_{0}", creditsNames[name.Str]);
                    }
                    nameMB.itemType = MenuButton.MenuItemType.Label;

                    nameMBList.Add(nameMB);
                }

                yield return StartCoroutine(PopulateButtonsCoroutine(nameMBList, MenuButton.MenuButtonAppearance.NoStripes));

                // Set grid layout to have 2 columns
                menuButtonContainer.GetComponent<GridLayoutGroup>().constraintCount = 2;

                creditsTimer = creditsTimerOrig;

                while (creditsTimer > 0f) {
                    creditsTimer -= Time.deltaTime;
                    yield return null;
                    if (uiInputModule.cancel.ToInputAction().triggered || backButtonTriggered) {
                        // Cancel out
                        //PopulateMenuWithButtons(LocateMenuButtonFromID("settings"));
                        backButtonTriggered = false;
                        breakOut = true;
                        break;
                    }

                }

                if (breakOut) break;
                
            }

            // Set grid layout to have 1 column
            menuButtonContainer.GetComponent<GridLayoutGroup>().constraintCount = 1;

            // Return to settings
            if (!breakOut) PopulateMenuWithButtons(LocateMenuButtonFromID("settings"));

            creditsActive = false;

            yield return null;

        }

        public void OnDestroy() {
            // print("MainMenuManager.OnDestroy() - destroyed");
            uiInputModule.cancel.action.performed -= BackButtonFromInput;
            uiInputModule.submit.action.performed -= GoToMainScreen;
        }
    }

    [System.Serializable]
    public class MenuGameObjectItem {
        public string id = "";
        public GameObject gameObject;
    }

    [System.Serializable]
    public enum VerticalScrollDirection {
        None = 0,
        Down = 1,
        Up = 2
    }
}