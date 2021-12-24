using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

namespace Sneaksters.UI
{
    public class PingTimeLabel : MonoBehaviour
    {
        private TextMeshProUGUI text;

        [SerializeField]
        private Gradient colorGradient = null;

        [SerializeField]
        private Vector2Int bounds = Vector2Int.zero;

        [SerializeField]
        private float alphaAmt = 0.7f;

        void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.OfflineMode) {
                int ping = PhotonNetwork.GetPing();

                text.text = $"{ping} ms";
                // Color gradient
                Color col = colorGradient.Evaluate(Mathf.InverseLerp(bounds.x, bounds.y, ping));
                col.a = alphaAmt;
                text.color = col;

            } else {
                text.text = "--- ms";
                Color col = Color.white;
                col.a = alphaAmt;
                text.color = col;
            }
        }
    }
}