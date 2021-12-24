using UnityEngine;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class GemstoneTutorialAnimator : MonoBehaviour
    {
        RectTransform rectTransform;

        public float duration = 0.5f;

        bool isVisible = false;

        public bool IsVisible { get { return isVisible; } }

        void Start() { rectTransform = GetComponent<RectTransform>(); SetOut(); }

        void SetOut() {
            rectTransform.anchorMin = new Vector2(0.5f, -0.2f);
            rectTransform.anchorMax = new Vector2(0.5f, -0.2f);
            isVisible = false;
        }

        void SetIn() {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            isVisible = true;
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            // SetOut();
            rectTransform.DOAnchorMin(new Vector2(0.5f, 0f), duration).SetEase(Ease.OutBack);
            rectTransform.DOAnchorMax(new Vector2(0.5f, 0f), duration).SetEase(Ease.OutBack);
            isVisible = true;
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            // SetIn();
            rectTransform.DOAnchorMin(new Vector2(0.5f, -0.2f), duration).SetEase(Ease.InBack);
            rectTransform.DOAnchorMax(new Vector2(0.5f, -0.2f), duration).SetEase(Ease.InBack);
            isVisible = false;
        }
    }
}
