using UnityEngine;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class LogoAnimator : MonoBehaviour
    {
        [SerializeField]
        float duration = 0.8f;

        RectTransform rectTransform;

        public bool IsLarge {get; private set;}

        // Start is called before the first frame update
        void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        }

        [ContextMenu("Animate to small")]
        public void AnimateToSmall() {
            rectTransform.DOAnchorPos(new Vector2(50, -30), duration).SetEase(Ease.InOutBack);
            rectTransform.DOAnchorMin(Vector2.up, duration).SetEase(Ease.InOutBack);
            rectTransform.DOAnchorMax(Vector2.up, duration).SetEase(Ease.InOutBack);
            rectTransform.DOPivot(Vector2.up, duration).SetEase(Ease.InOutBack);
            rectTransform.DOScale(Vector3.one, duration).SetEase(Ease.InOutBack);
            IsLarge = false;
            // DOTween.Sequence()
            //     .Join(rectTransform.DOAnchorPos(new Vector2(50, -30), duration))
            //     .Join(rectTransform.DOAnchorMin(Vector2.up, duration))
            //     .Join(rectTransform.DOAnchorMax(Vector2.up, duration))
            //     .Join(rectTransform.DOPivot(Vector2.up, duration))
            //     .Join(rectTransform.DOScale(Vector3.one, duration))
            //     .SetEase(Ease.InOutBack)
            // ;
        }

        [ContextMenu("Set large")]
        public void SetLarge() {
            Awake();
            rectTransform.anchoredPosition = new Vector2(0,80);
            rectTransform.anchorMin = Vector2.one / 2f;
            rectTransform.anchorMax = Vector2.one / 2f;
            rectTransform.pivot = Vector2.one / 2f;
            rectTransform.localScale = new Vector3(2,2,1);
            IsLarge = true;
        }

        [ContextMenu("Set small")]
        public void SetSmall() {
            Awake();
            rectTransform.anchoredPosition = new Vector2(50, -30);
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            rectTransform.pivot = Vector2.up;
            rectTransform.localScale = Vector3.one;
            IsLarge = false;
        }
    }
}