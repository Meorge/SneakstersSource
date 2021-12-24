using UnityEngine;
using TMPro;

namespace Sneaksters.UI
{
    public class TextMeshProGameVersion : MonoBehaviour
    {
        TextMeshProUGUI label;

        void Start()
        {
            label = GetComponent<TextMeshProUGUI>();
            label.text = Application.version;
        }
    }
}