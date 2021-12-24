using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

namespace Sneaksters.UI.Menus {
    public class UIMainMenuButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler, IMoveHandler
    {
        public RectTransform rectTransform = null;
        public MenuButton buttonData = new MenuButton();
        public SettingItemData.SettingType settingType = SettingItemData.SettingType.None;
        public SimpleLocalizeText textLocalizer;

        public Image background;
        public Button button;
        public Image iconHolder;

        private int index = 0;


        [Header("Visual Components")]
        public Image checkGraphic;
        public Image cursorImage;
        Sequence cursorAnimation;

        public bool choiceSelectSelected = false;

        public bool ChoiceSelectSelected {
            get {
                return choiceSelectSelected;
            }

            set {
                choiceSelectSelected = value;
                if (value) AnimateCheckMarkIn();
                else AnimateCheckMarkOut();
            }
        }
        public Image checkBackground;

        // Settings item stuff
        public Slider slider;
        public Toggle toggle;
        public TMP_Dropdown dropdown;
        public TMP_InputField stringEntry;

        public bool isSetting {
            get {
                return buttonData.settingID != null && buttonData.settingID != "";
            }
        }

        void Awake() {
            rectTransform = GetComponent<RectTransform>();
            cursorImage.SetAlpha(0f);
        }

        void Start() {
            // AnimateCursor();
            AnimateButtonIn();
        }

        void AnimateButtonIn() {
            Sequence seq = DOTween.Sequence();

            // set initial
            background.transform.localPosition = new Vector3(-150f, 0f, 0f);
            background.SetAlpha(0f);

            // animate
            seq.AppendInterval(index * 0.05f);
            seq.Append(background.transform.DOLocalMoveX(0f, 0.2f, false));
            seq.Join(background.DOFade(1f, 0.2f));

            // Animate the check mark graphic
            if (buttonData.itemType == MenuButton.MenuItemType.ChoiceSelect && choiceSelectSelected) {
                checkBackground.SetAlpha(0f);
                checkGraphic.SetAlpha(0f);

                seq.Join(checkBackground.DOFade(1f, 0.2f));
                seq.Join(checkGraphic.DOFade(1f, 0.2f));
            } else {
                checkBackground.SetAlpha(0f);
                checkGraphic.SetAlpha(0f);
            }
        }

        void AnimateCheckMarkIn() {
            checkBackground.SetAlpha(1f);
            checkGraphic.SetAlpha(1f);

            checkBackground.transform.localScale = Vector3.zero;
            checkGraphic.transform.localScale = Vector3.zero;

            checkBackground.transform.DOScale(1f, 0.2f);
            checkGraphic.transform.DOScale(1f, 0.2f);
        }

        void AnimateCheckMarkOut() {
            checkBackground.transform.DOScale(0f, 0.2f);
            checkGraphic.transform.DOScale(0f, 0.2f);
        }


        void AnimateCursor() {
            cursorAnimation = DOTween.Sequence()
                .Append(cursorImage.rectTransform.DOScale(1.04f, 0.2f))
                .AppendInterval(1f)
                .Append(cursorImage.rectTransform.DOScale(1f, 0.2f))
                .AppendInterval(1f)
                .SetLoops(-1);
        }

        void OnDestroy() {
            cursorAnimation.Kill();
        }

        void Update() {



            // if (buttonData.itemType == MenuButton.MenuItemType.ChoiceSelect) {
            //     animator.SetBool("checkEnabled", choiceSelectSelected);
            // }


            if (textLocalizer != null) textLocalizer.textID = buttonData.buttonTextID;

            if (buttonData.itemType == MenuButton.MenuItemType.Label) {
                background.SetAlpha(0f);
                background.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                background.enabled = false;
                // animator.SetBool("checkEnabled", false);
                button.interactable = false;
                button.enabled = false;
                return;
            }

            if (settingType == SettingItemData.SettingType.None) {
                background.color = new Color(0.4f, 0.4f, 0.4f, background.color.a);
                //background.color = buttonData.buttonColor.Desaturate(0.7f);
                //checkBackground.color = buttonData.buttonColor;
            }
            else {
                background.color = new Color(0.3f, 0.3f, 0.3f, background.color.a);
                // background.SetAlpha(0f);
            }
        }

        public void OnButtonClick() {
            FMODUnity.RuntimeManager.PlayOneShot(PhotonGameManager.Instance.sfxForwardEvent, transform.position);
            if (MainMenuManager.instance.backTimer > 0) return;
            MainMenuManager.instance.backTimer = 10;

            if (buttonData.itemType == MenuButton.MenuItemType.ChoiceSelect && !choiceSelectSelected) {
                MainMenuManager.instance.UncheckAllButtons();
                ChoiceSelectSelected = true;

                buttonData.onChoiceSelect();
                return;
            }

            else if (buttonData.itemType != MenuButton.MenuItemType.ChoiceSelect && !isSetting) {
                MainMenuManager.instance.PopulateMenuWithButtons(buttonData);
            }

            else if (settingType == SettingItemData.SettingType.Toggle) {
                toggle.isOn = !toggle.isOn;
            } 
        }

        public void OnMove(AxisEventData d) {
            if (settingType == SettingItemData.SettingType.Slider) {
                if (d.moveDir == MoveDirection.Left) {
                    slider.normalizedValue -= 0.05f;
                } else if (d.moveDir == MoveDirection.Right) {
                    slider.normalizedValue += 0.05f;
                }
            } else if (settingType == SettingItemData.SettingType.Dropdown) {
                if (d.moveDir == MoveDirection.Left) {
                    dropdown.value--;
                } else if (d.moveDir == MoveDirection.Right) {
                    dropdown.value++;
                }
            }
        }

        private bool doTick = true;
        public void ForceSelect() {
            MainMenuManager.instance.buttonCursor.instantSnap = true;
            doTick = false;
            button.Select();
            MainMenuManager.instance.buttonCursor.instantSnap = false;
            // OnSelect(null);
        }


        public void OnSelect(BaseEventData eventData) {
            // find out if we need to scroll
            MainMenuManager.instance.DoScrollingStuff(this);
            if (button.interactable) ButtonSelected();
        }

        public void OnDeselect(BaseEventData eventData) {
            cursorImage.SetAlpha(0f);
        }

        public void OnPointerEnter(PointerEventData eventData = null) {
            if (button.interactable) button.Select();
        }

        public void ButtonSelected() {
            MainMenuManager.instance?.buttonCursor.Enable();
            MainMenuManager.instance?.buttonCursor.TweenToButton(rectTransform);
            // cursorImage.SetAlpha(1f);
            MainMenuManager.instance.buttonDescription.textID = buttonData.buttonDescriptionID;

            if (buttonData.buttonID.StartsWith("CONTROL_")) MainMenuManager.instance.SetCurrentControllerPreview(buttonData.buttonID.TrimStart("CONTROL_".ToCharArray()));
            
            // Only do this if not coming from below
            ActiveCamera.SetActiveCamera(buttonData.hoverCameraID);

            if (doTick) {
                FMODUnity.RuntimeManager.PlayOneShot(PhotonGameManager.Instance.sfxTickEvent, transform.position);
            }
            doTick = true;
        }

        public void SetUpButton(MenuButton m, SettingItemData d = null, int buttonIndex = 0) {

            buttonData = m;
            buttonData.buttonComponent = this;

            index = buttonIndex;

            // if (m.itemType == MenuButton.MenuItemType.ChoiceSelect) animator.SetBool("checkExists", true);
            
            slider.gameObject.SetActive(false);
            toggle.gameObject.SetActive(false);
            dropdown.gameObject.SetActive(false);
            stringEntry.gameObject.SetActive(false);

            if (m.choiceSelectDefault) ChoiceSelectSelected = true;

            if (m.icon == null) iconHolder.enabled = false;
            else iconHolder.sprite = m.icon;

            if (m.itemType == MenuButton.MenuItemType.Label) {
                background.SetAlpha(0f);
                background.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                background.enabled = false;
                // animator.SetBool("checkEnabled", false);
                button.interactable = false;
                button.enabled = false;
                textLocalizer.textpro.enableWordWrapping = false;
            }


            if (d != null) {
                settingType = d.settingType;

                switch (d.settingType) {
                    case SettingItemData.SettingType.Toggle:
                        toggle.gameObject.SetActive(true);

                        if (PlayerPrefs.GetInt(d.playerPrefsID) != 0) toggle.isOn = true;
                        else toggle.isOn = false;

                        toggle.onValueChanged.AddListener(d.toggleEvent);
                        break;

                    case SettingItemData.SettingType.Slider:
                        slider.gameObject.SetActive(true);

                        slider.minValue = d.sliderMin;
                        slider.maxValue = d.sliderMax;

                        slider.value = PlayerPrefs.GetFloat(d.playerPrefsID, d.sliderDefault);
                        slider.onValueChanged.AddListener(d.sliderEvent);
                        break;

                    case SettingItemData.SettingType.Dropdown:
                        dropdown.gameObject.SetActive(true);

                        dropdown.ClearOptions();
                        dropdown.AddOptions(d.dropdownItems);
                        dropdown.SetValueWithoutNotify(PlayerPrefs.GetInt(d.playerPrefsID, 0));
                        dropdown.onValueChanged.AddListener(d.dropdownEvent);
                        break;

                    case SettingItemData.SettingType.String:
                        stringEntry.gameObject.SetActive(true);

                        stringEntry.text = PlayerPrefs.GetString(d.playerPrefsID, "");
                        stringEntry.onValueChanged.AddListener(d.stringEvent);
                        break;

                    default:
                        break;
                }
            }

        }
    }


    [System.Serializable]
    public class MenuButton {
        public string buttonID = "null";
        public string buttonTextID = "null";
        public string buttonDescriptionID = "";
        public string runFunction = "";
        public Color buttonColor = Color.black;
        public string parentButtonID;
        public string settingID = "";
        public UIMainMenuButton buttonComponent;

        public Sprite icon = null;

        public string hoverCameraID = "";
        public string selectCameraID = "";
        public List<string> subMenuIDs = new List<string>();

        public MenuButtonAppearance appearance = MenuButtonAppearance.Normal;
        public MenuItemType itemType = MenuItemType.Button;

        public UnityEngine.Events.UnityAction onChoiceSelect;
        public bool choiceSelectDefault = false;

        public enum MenuButtonAppearance {
            Normal = 0,
            BigSetting = 1,
            HorizontalStripe = 2,
            NoStripes = 3
        }

        public enum MenuItemType {
            Button = 0,
            Toggle = 1,
            Slider = 2,
            Dropdown = 3,
            ChoiceSelect = 4,
            Label = 5
        }
    }

    [System.Serializable]
    public class SettingItemData {
        public enum SettingType {
            None = 0,
            Toggle = 1,
            Slider = 2,
            Dropdown = 3,
            String = 4,
            ChoiceSelect = 5
        }
        public SettingType settingType;
        public string playerPrefsID;

        // For toggles
        public UnityEngine.Events.UnityAction<bool> toggleEvent;

        // For sliders
        public float sliderMin;
        public float sliderMax;
        public float sliderDefault;
        public UnityEngine.Events.UnityAction<float> sliderEvent;

        // For dropdowns
        public List<string> dropdownItems;
        public UnityEngine.Events.UnityAction<int> dropdownEvent;

        // For strings
        public UnityEngine.Events.UnityAction<string> stringEvent;

        // For choice select
        public UnityEngine.Events.UnityAction choiceSelectEvent;

        public static SettingItemData DefineToggleSetting(string _id, UnityEngine.Events.UnityAction<bool> _ev) {
            SettingItemData d = new SettingItemData();
            d.playerPrefsID = _id;
            d.toggleEvent = _ev;
            d.settingType = SettingType.Toggle;
            return d;
        }

        public static SettingItemData DefineSliderSetting(string _id, UnityEngine.Events.UnityAction<float> _ev, float min, float max, float def) {
            SettingItemData d = new SettingItemData();
            d.playerPrefsID = _id;
            d.sliderEvent = _ev;
            d.sliderMin = min;
            d.sliderMax = max;
            d.sliderDefault = def;
            d.settingType = SettingType.Slider;
            return d;
        }

        public static SettingItemData DefineDropdownSetting(string _id, UnityEngine.Events.UnityAction<int> _ev, List<string> i) {
            SettingItemData d = new SettingItemData();
            d.playerPrefsID = _id;
            d.dropdownEvent = _ev;
            d.dropdownItems = i;
            d.settingType = SettingType.Dropdown;
            return d;
        }

        public static SettingItemData DefineStringSetting(string _id, UnityEngine.Events.UnityAction<string> _ev) {
            SettingItemData d = new SettingItemData();
            d.playerPrefsID = _id;
            d.stringEvent = _ev;
            d.settingType = SettingType.String;
            return d;
        }
    }
}