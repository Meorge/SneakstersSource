using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class SquadRoomCodeAnimator : MonoBehaviour
    {
        public RectTransform background;
        public TextMeshProUGUI squadCodeLabel, squadCodeCaptionLabel;
        public float duration = 0.4f;
        public float scale = 0.8f;

        void Start() {
            background.localScale = Vector2.zero;
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            background.DOScale(scale, duration).SetEase(Ease.OutBack);
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            background.DOScale(0f, duration).SetEase(Ease.InBack);
        }
    }
}