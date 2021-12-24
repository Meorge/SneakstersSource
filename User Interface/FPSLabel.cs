using UnityEngine;
using TMPro;

namespace Sneaksters.UI
{
    public class FPSLabel : MonoBehaviour
    {
        private TextMeshProUGUI text;

        [SerializeField]
        private Gradient colorGradient = null;

        [SerializeField]
        private Vector2Int bounds = Vector2Int.zero;

        [SerializeField]
        private int frequency = 10;
        private int count = 0;

        [SerializeField]
        private float alphaAmt = 0.7f;

        void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            if (count > frequency) {
                float approxFPS = Mathf.Round(1.0f / Time.deltaTime);
                text.text = $"{approxFPS} fps";
                Color col = colorGradient.Evaluate(Mathf.InverseLerp(bounds.x, bounds.y, approxFPS));
                col.a = alphaAmt;
                text.color = col;
                count = 0;
            }
            else {
                
                count++;
            }
        }
    }
}