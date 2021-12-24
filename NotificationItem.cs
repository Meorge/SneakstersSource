using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Sneaksters.UI.Animation;

namespace Sneaksters.UI.Gameplay
{
    public class NotificationItem : MonoBehaviour
    {
        public enum NotificationType {
            JoinedGame = 0,
            LeftGame = 1,
            Connected = 2,
            Disconnected = 3
        }

        public TextMeshProUGUI label;
        public TextMeshProUGUI timeLabel;
        
        [SerializeField]
        private NotificationItemAnimator animator = null;

        void Start() {
            animator.Animate();
        }

        public void SetInformation(string playerName, NotificationType type) {
            string outtext = string.Format("<color=yellow>{0}</color> ", playerName);
            if (type == NotificationType.JoinedGame)
                label.text = outtext + PhotonGameManager.Instance.GetStringFromID("player_joined");
            else if (type == NotificationType.LeftGame)
                label.text = outtext + PhotonGameManager.Instance.GetStringFromID("player_left");
            else if (type == NotificationType.Connected)
                label.text = outtext + "connected!";
            else if (type == NotificationType.Disconnected)
                label.text = outtext + "disconnected!";
            else
                label.text = outtext + "did something...";

            System.DateTime now = System.DateTime.Now;

            string timeText = now.ToShortTimeString();
            timeLabel.text = timeText;
        }

        public void SetText(string text) {
            label.text = text;
            
            System.DateTime now = System.DateTime.Now;
            string timeText = now.ToShortTimeString();
            timeLabel.text = timeText;
        }
    }
}