using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using Sneaksters;
using Sneaksters.Localization;

namespace Sneaksters.UI
{
    public class SimpleLocalizeText : MonoBehaviour
    {
        [HideInInspector]
        public TextMeshProUGUI textpro;
        public string _textID = "null";

        public bool localizeText = true;
        public bool parseControllerPrompts = true;
        public bool dontShowPromptsForKeyboard = false;

        public string textID {
            get {
                return _textID;
            }

            set {
                _textID = value;
                LocalizeText();
                
            }
        }

        public void SetTextID(string newID) {
            textID = newID;
        }

        void Awake()
        {
            textpro = GetComponent<TextMeshProUGUI>();
        }
        
        void Start() {
            // textpro.spriteAsset = PhotonGameManager.gameManager.buttonPromptSpriteAsset;
            LocalizeText();
            // ControllerParse();
        }

        void Update() {
            LocalizeText();
            // ControllerParse();
        }

        public void LocalizeText() {
            if (localizeText) {
                if (PhotonGameManager.Instance == null) {
                    Debug.LogError("Game manager is null!");
                    return;
                }
                if (textpro == null) {
                    // Debug.LogError($"TextMeshPro object for label with text ID {_textID} is null!");
                    return;
                }
                TMP_FontAsset overrideFont = PhotonGameManager.Instance?.GetLanguageOverrideFont();
                if (overrideFont != null) textpro.font = overrideFont;
                
                if (_textID != "") {
                    // If the textID starts with an underscore, show the textID literally (not localized)
                    if (textID[0] == '_') textpro.text = textID.Substring(1);

                    // If it's localized and exists
                    else if (PhotonGameManager.Instance.stringPacks[PhotonGameManager.Instance.currentLanguageID].Strings.ContainsKey(_textID)) {
                        string stringOut = PhotonGameManager.Instance.GetStringFromID(_textID);
                        textpro.text = stringOut;
                    }

                    // If it's localized, but is not in the dictionary, just show the textID
                    else textpro.text = _textID;
                }
                // If the text ID is empty, this should be empty itself
                else textpro.text = "";
            }
        }

        [ContextMenu("Parse controller prompts")]
        void ControllerParse() {
            string stringIn = textpro.text;

            Regex regex = new Regex("@@{(.*?)}");

            var matches = regex.Matches(stringIn);


            string controlScheme = PhotonGameManager.Instance.GetPlayerInput().currentControlScheme;
            bool dontShowPrompts = controlScheme == "Keyboard" && dontShowPromptsForKeyboard;

            foreach (Match match in matches) {
                string buttonID = match.Groups[1].Value;

                InputAction action = PhotonGameManager.Instance.inputActionsAsset[buttonID];

                InputBinding bindingIndex = InputBinding.MaskByGroup(PhotonGameManager.Instance.GetPlayerInput().currentControlScheme);

                string dispString = action.GetBindingDisplayString(bindingIndex, options: InputBinding.DisplayStringOptions.DontOmitDevice | InputBinding.DisplayStringOptions.DontIncludeInteractions);

                // Only generate the button prompt stuff if we want to actually show prompts
                string correspondingText = "";
                if (!dontShowPrompts) {
                    foreach (string singleThing in dispString.Split(new string[] { " | " }, StringSplitOptions.None)) {
                        string singleCorrespondingString = PhotonGameManager.Instance.controlButtonPromptStrings.GetValueForKey(singleThing);

                        if (singleCorrespondingString != null) correspondingText += singleCorrespondingString;
                        else correspondingText += singleThing;

                        correspondingText += "/";
                    }
                    correspondingText = correspondingText.Substring(0, correspondingText.Length-1);
                }



                if (correspondingText != null) {
                    stringIn = stringIn.Replace("@@{" + buttonID + "}", correspondingText);
                } else {
                    stringIn = stringIn.Replace("@@{" + buttonID + "}", dispString);
                }
            }

            textpro.text = stringIn;
        }


        [ContextMenu("print all the things for the controller")]
        void DoThing() {
            PlayerInput playerInput = PhotonGameManager.Instance.GetPlayerInput();
            InputActionAsset inputActions = PhotonGameManager.Instance.inputActionsAsset;

            string b = $"{playerInput.currentControlScheme}\n---";
            foreach (InputAction action in inputActions) {
                b += $"Action = {action.name}\n";
                InputBinding bindingIndex = InputBinding.MaskByGroup(playerInput.currentControlScheme);

                string dispString = action.GetBindingDisplayString(bindingIndex, options: InputBinding.DisplayStringOptions.DontOmitDevice | InputBinding.DisplayStringOptions.DontIncludeInteractions);
                b += $"<{dispString}>\n\n";
            }
            Debug.Log(b);
        }
    }
}