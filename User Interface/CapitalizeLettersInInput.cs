using UnityEngine;
using TMPro;

namespace Sneaksters.UI
{
    public class CapitalizeLettersInInput : MonoBehaviour
    {
        public TMP_InputField inputField;

        public void Start() {
            inputField = GetComponent<TMP_InputField>();
        }
        
        public void OnTextChanged() {
            inputField.text = inputField.text.ToUpper();
        }
    }
}