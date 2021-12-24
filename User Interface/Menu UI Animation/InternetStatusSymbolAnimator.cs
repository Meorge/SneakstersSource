using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class InternetStatusSymbolAnimator : MonoBehaviour
    {
        public RectTransform shadow, main;
        public float duration = 0.5f;
        Image shadowImg, mainImg;
        void Start() {
            shadowImg = shadow.GetComponent<Image>();
            mainImg = main.GetComponent<Image>();

            shadow.localScale = Vector2.zero;
            main.localScale = Vector2.zero;
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            shadow.DOScale(1f, duration).SetEase(Ease.OutBack);
            main.DOScale(1f, duration).SetEase(Ease.OutBack);
        }

        [ContextMenu("Animate Out")]
        public void AnimateOut() {
            shadow.DOScale(0f, duration).SetEase(Ease.InBack);
            main.DOScale(0f, duration).SetEase(Ease.InBack);
        }
    }
}