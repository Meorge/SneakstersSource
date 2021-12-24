using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using TMPro;

using Sneaksters.Gameplay;

namespace Sneaksters.UI.Menus
{
    public enum LevelSelectManagerState
    {
        ViewingLocations = 0,
        ViewingMissions = 1
    }

    public class LevelSelectManager : MonoBehaviour
    {
        public static LevelSelectManager Instance { get; private set; } = null;

        public static bool IsActive { get; private set; } = false;
        public static LevelSelectManagerState State { get; private set; } = LevelSelectManagerState.ViewingLocations;

        [SerializeField] UILevelSelectButton levelSelectButtonPrefab = null;
        [SerializeField] UILevelSelectMapBubbleAnimator mapBubblePrefab = null;

        public UIButtonCursorAnimator levelSelectCursor = null;

        [SerializeField] Transform buttonContainer = null, bubbleContainer = null;

        List<UILevelSelectButton> currentButtons = new List<UILevelSelectButton>();

        Dictionary<LevelCategory, UILevelSelectMapBubbleAnimator> currentMapBubbles = new Dictionary<LevelCategory, UILevelSelectMapBubbleAnimator>();

        LevelCategory currentCategory = null;

        [SerializeField] ShapesRingPulseAnimator locationSelectionRing = null;

        [SerializeField] TextMeshProUGUI areaDescription = null;


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            } else
            {
                Debug.LogError("More than one instance of LevelSelectManager, deleting the new one!");
                Destroy(gameObject);
                return;
            }
            Disable();
        }

        public static void Initialize()
        {
            ActiveCamera.SetActiveCamera("chooseLevel");

            IsActive = true;

            Instance.SetupListOfLocations();
            SquadRoomManager.HideMoveOutButton();
            Instance.locationSelectionRing.disc.enabled = true;
        }

        public static void Disable()
        {
            IsActive = false;
            Instance.ClearButtons();
            Instance.ClearMapBubbles();
            if (Instance.locationSelectionRing.disc != null) Instance.locationSelectionRing.disc.enabled = false;
            Instance.areaDescription.text = "";
        }


        void SetupListOfLocations(LevelCategory defaultSelect = null, bool setupMapPositions = true)
        {
            if (setupMapPositions)
            {
                SetupMapBubbles();
            }

            LevelCatalog catalog = PhotonGameManager.Instance.levelCatalog;
            SetupButtons<LevelCategory>(catalog.categories, (x) => x.name, (x) => SetupListOfMissions(x), (x) => HighlightLocationOnMap(x), defaultSelect);

            State = LevelSelectManagerState.ViewingLocations;


        }

        void SetupListOfMissions(LevelCategory category)
        {
            // currentCategory = category;

            SetupButtons<LevelObject>(category.levels, (x) => x.levelName, (x) => SelectMission(x), null, null);

            State = LevelSelectManagerState.ViewingMissions;
        }

        public static void BackOutToLocations()
        {
            Debug.Log($"Current category is {Instance.currentCategory.name}");
            Instance.SetupListOfLocations(Instance.currentCategory, false);
        }

        void SetupMapBubbles()
        {
            ClearMapBubbles();

            LevelCatalog catalog = PhotonGameManager.Instance.levelCatalog;
            foreach (LevelCategory category in catalog.categories)
            {
                // Set up new map bubble
                UILevelSelectMapBubbleAnimator bubble = Instantiate(mapBubblePrefab);
                bubble.transform.SetParent(bubbleContainer, false);

                bubble.transform.localPosition = new Vector3(category.mapPos.x, category.mapPos.y, 0f);

                bubble.SetInactive();
                currentMapBubbles.Add(category, bubble);
            }
        }

        void HighlightLocationOnMap(LevelCategory category)
        {
            Vector3 pos = locationSelectionRing.transform.localPosition;
            pos.x = category.mapPos.x;
            pos.y = category.mapPos.y;
            locationSelectionRing.transform.localPosition = pos;

            if (currentCategory != null) currentMapBubbles[currentCategory].AnimateToInactive();
            currentMapBubbles[category].AnimateToActive();

            currentCategory = category;

            areaDescription.text = category.description;
        }

        void SelectMission(LevelObject mission)
        {
            Debug.Log($"Mission selected: {mission.levelName}");
            PhotonGameManager.Instance.SelectLevel(mission);
            SquadRoomManager.ShowSquadRoom();
        }

        void ClearMapBubbles()
        {
            // clear previous bubbles
            foreach (KeyValuePair<LevelCategory, UILevelSelectMapBubbleAnimator> b in currentMapBubbles)
            {
                Destroy(b.Value.gameObject);
            }
            currentMapBubbles.Clear();
        }

        void ClearButtons()
        {
            // Clear buttons from currentButtons list
            foreach (UILevelSelectButton button in currentButtons)
            {
                Destroy(button.gameObject);
            }

            currentButtons.Clear();
        }

        void SetupButtons<T>(IEnumerable<T> enumerator, System.Func<T, string> nameGetter, System.Action<T> onClick, System.Action<T> onSelect, T defaultSelect)
        {
            ClearButtons();

            // Add a button for each location
            UILevelSelectButton previousButton = null;

            UILevelSelectButton defaultSelectButton = null;
            foreach (T item in enumerator)
            {
                // Add button for this category/location!
                UILevelSelectButton currentButton = AddButton<T>(item, nameGetter(item), onClick, onSelect);

                // Hook it up to the previous button, if there is a previous button
                if (previousButton != null)
                {
                    // Pressing down from previous button should lead to this button
                    var previousNav = previousButton.button.navigation;
                    previousNav.selectOnDown = currentButton.button;
                    previousButton.button.navigation = previousNav;

                    // Pressing up from this button should lead to previous button
                    var thisNav = currentButton.button.navigation;
                    thisNav.selectOnUp = previousButton.button;
                    currentButton.button.navigation = thisNav;
                }

                // Add to list
                currentButtons.Add(currentButton);

                // If this is the button we're supposed to have selected, select it
                if (item.Equals(defaultSelect)) defaultSelectButton = currentButton;

                // Keep track of the previous button
                previousButton = currentButton;
            }

            StartCoroutine(SelectButton(defaultSelectButton ?? currentButtons[0]));
        }

        IEnumerator SelectButton(UILevelSelectButton button)
        {
            yield return new WaitForEndOfFrame();
            button.Select();
        }

        UILevelSelectButton AddButton<T>(T obj, string text, System.Action<T> onClick, System.Action<T> onSelect)
        {
            UILevelSelectButton button = Instantiate(levelSelectButtonPrefab);
            button.transform.SetParent(buttonContainer, false);
            button.SetText(text);


            button.button.onClick.AddListener(() => onClick(obj));
            if (onSelect != null) button.onSelect += () => onSelect(obj);
            return button;
        }


    }
}