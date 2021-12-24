using UnityEngine;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class HalvesTransitionAnimator : MonoBehaviour
    {
        public RectTransform topHalf, bottomHalf;
        public float duration = 0.4f;

        // void Awake() { SetOut(); }
        public void SetIn() {
            // Debug.LogError("Halves set to in");
            topHalf.anchorMin = new Vector2(0, 0.5f);
            bottomHalf.anchorMax = new Vector2(1, 0.5f);
        }

        public void SetOut() {
            // Debug.LogError("Halves set to out");
            topHalf.anchorMin = new Vector2(0, 1f);
            bottomHalf.anchorMax = new Vector2(1, 0f);
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            SetOut();
            DOTween.Sequence()
                .Append(topHalf.DOAnchorMin(new Vector2(0, 0.5f), duration))
                    .Join(bottomHalf.DOAnchorMax(new Vector2(1, 0.5f), duration));
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            SetIn();
            // Debug.LogError("Animate out the things - initially set to in");
            DOTween.Sequence()
                .Append(topHalf.DOAnchorMin(new Vector2(0, 1f), duration))
                    .Join(bottomHalf.DOAnchorMax(new Vector2(1, 0f), duration));
        }
    }
}