using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sneaksters.UI
{
    [CreateAssetMenu(fileName = "New Controller Config Object", menuName = "Controllers/Controller Config Object", order = 1)]
    public class ControllerConfigObject : ScriptableObject {
        public string controllerID = "";
        public bool isKeyboard = false;
        public Sprite controllerImage;
        public Sprite controllerSilImage;
        public List<ControllerConfigObject_ControllerButton> controllerButtons = new List<ControllerConfigObject_ControllerButton>();

        public bool ControllerSupported(string id) {
            List<string> split = new List<string>(controllerID.Split('/'));
            return split.Contains(id);
        }

        public ControllerConfigObject_ControllerButton GetControllerButtonGraphicNew(string controlID) {
            string controlPath = "*/{" + controlID + "}";
            InputControl c = InputControlPath.TryFindControl(Gamepad.current, "{Emote1}");
            Debug.Log(c);
            if (c != null) {
                string buttonMapped = c.path.Substring(c.path.IndexOf('/') + 1);
                foreach (ControllerConfigObject_ControllerButton buttonObj in controllerButtons) {
                    if (buttonObj.controlID == buttonMapped) return buttonObj;
                }
            }
            return null;
        }
        public ControllerConfigObject_ControllerButton GetControllerButtonGraphic(string controlID) {
            // Let's find the buttons mapped to this control ID

            List<string> parts = new List<string>(controlID.Split('/'));

            controlID = parts[1];
            string actionMapID = parts[0];
            InputActionMap m = PhotonGameManager.Instance.inputActionsAsset.FindActionMap(actionMapID);
            Debug.Log(m);

            if (m == null) {
                Debug.LogErrorFormat("InputActionMap is null");
                return null;
            }

            
            InputAction inputAction = m.FindAction(controlID);
            Debug.Log(controlID);
            Debug.Log(inputAction.bindings.Count);

            Debug.Log("-----");

            foreach (InputBinding b in inputAction.bindings) {
                Debug.LogFormat("Checking input binding {0}", b.path);
                if (!b.path.Contains("/")) continue;
                foreach (string _c in new List<string>(controllerID.Split('/'))) {
                    string controllerName = _c;
                    Debug.LogFormat("Looking for bindings using the controller {0}", controllerName);


                    if (controllerName == "KeyboardMouse") controllerName = "Keyboard";


                    
                    // First, let's make sure that this is the same input device:
                    if (b.path.Substring(0, b.path.IndexOf('/')) == string.Format("<{0}>", controllerName)) {

                        // Next, let's get the button mapped to this action
                        string buttonMapped = b.path.Substring(b.path.IndexOf('/') + 1);
                        Debug.LogFormat("Action {0} is mapped to {1}", controlID, buttonMapped);

                        if (!isKeyboard) {
                            foreach (ControllerConfigObject_ControllerButton buttonObj in controllerButtons) {
                                if (buttonObj.controlID == buttonMapped) return buttonObj;
                            }
                        } else {
                            ControllerConfigObject_ControllerButton buttonObj = new ControllerConfigObject_ControllerButton();

                            Debug.LogFormat("Looking for the button map /{0} in Keyboard.current", buttonMapped);
                            InputControl control = InputControlPath.TryFindControl(Keyboard.current, "<Keyboard>/" + buttonMapped);
                            Debug.LogFormat("The control found was {0}", control.name);

                            string outName = control.displayName.ToUpper();




                            switch (control.name) {
                                case "space": outName = "Space"; break;
                                case "tab": outName = "Tab"; break;
                                case "escape": outName = "Esc"; break;
                                // case "enter": outName = "Enter"; break;
                                // case "return": outName = "Return"; break;
                                default: break;

                            }
                            buttonObj.textOverlayID = string.Format("{0}", outName);
                            return buttonObj;
                        }
                    }

                }
            }
            return null;
        }
    }

    [System.Serializable]
    public class ControllerConfigObject_ControllerButton {
        public string controlID = "";
        public string textOverlayID = "";
        public Sprite graphic;
    }
}