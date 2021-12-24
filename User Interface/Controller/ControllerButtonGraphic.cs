using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sneaksters.UI
{
    public class ControllerButtonGraphic : MonoBehaviour
    {
        public string controlID = "";
        private Image imageComponent;
        private TextMeshProUGUI buttonOverlay;

        void Awake() {
            imageComponent = GetComponent<Image>();
            buttonOverlay = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }

        public void Start() {
            if (controlID != "") SetButton(controlID);
        }



        public void SetButton(string _ctrl) {
            controlID = _ctrl;

            ControllerConfigObject obj = PhotonGameManager.Instance.GetCurrentControllerConfigObject();
            Debug.LogFormat("Controller config object is {0}", obj);
            ControllerConfigObject_ControllerButton button = obj.GetControllerButtonGraphic(controlID);

            Debug.LogFormat("Value of button is {0}", button != null);

            if (button == null) {
                imageComponent.SetAlpha(0f);
                buttonOverlay.text = "";
                return;
            }

            imageComponent.SetAlpha(1f);
            if (button.graphic != null) {imageComponent.sprite = button.graphic; imageComponent.color = Color.white;}
            buttonOverlay.text = button.textOverlayID;
        }
    }
}