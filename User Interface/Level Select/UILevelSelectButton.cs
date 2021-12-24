using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sneaksters.UI.Menus
{
    public class UILevelSelectButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        RectTransform rectTransform = null;
        public Button button { get; private set; } = null;

        [SerializeField] TextMeshProUGUI label = null;

        public event System.Action onSelect; 

        void Awake()
        {
            button = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
        }

        // public void OnButtonClick() {
        //     FMODUnity.RuntimeManager.PlayOneShot(PhotonGameManager.gameManager.SFX_ForwardEvent, transform.position);
        //     print($"clicked button with label {label?.text ?? "null"}");
        // }

        public void OnSelect(BaseEventData eventData) {
            // find out if we need to scroll
            if (button.interactable) ButtonSelected();
        }

        public void OnPointerEnter(PointerEventData eventData = null) {
            if (button.interactable) button.Select();
        }

        public void Select()
        {
            if (button.interactable) button.Select();
        }

        void ButtonSelected() {
            LevelSelectManager.Instance.levelSelectCursor.Enable();
            LevelSelectManager.Instance.levelSelectCursor.TweenToButton(rectTransform);

            onSelect?.Invoke();
        }

        // TODO: Localization support
        public void SetText(string text)
        {
            label.text = text;
        }
    }
}