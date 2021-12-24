using UnityEngine;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class StaminaBarAnimator : MonoBehaviour
    {
        RectTransform rT;
        public float duration = 0.5f;

        void Start() { rT = GetComponent<RectTransform>(); }

        public void SetIn() {
            rT.DOScaleY(1f, 0f);
        }

        public void SetOut() {
            rT.DOScaleY(0f, 0f);
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            rT.DOScaleY(1f, duration).SetEase(Ease.OutBack);
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            rT.DOScaleY(0f, duration).SetEase(Ease.InBack);
        }
    }
}