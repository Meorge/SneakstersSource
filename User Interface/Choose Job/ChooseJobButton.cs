using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sneaksters.UI.Menus
{
    public class ChooseJobButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        public Button button { get; private set; } = null;
        public bool selected { get; private set; } = false;

        public ChooseJobManager manager = null;

        private void Awake()
        {
            button = GetComponent<Button>();
            // TODO: Have button call method in SquadRoomManager when selected?

            // TODO: Disable the raycast target stuff on a bunch of the images
            // when they're not visible so we can click the buttons
        }

        public void Select() {
            selected = true;
            manager.reticle.AlignToPoster(this);
        }

        public void Deselect() {
            selected = false;
        }

        public void OnSelect(BaseEventData eventData) {
            if (button.interactable) Select();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (button.interactable) button.Select();
        }
    }
}