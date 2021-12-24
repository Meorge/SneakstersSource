using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class PressStartAnimator : MonoBehaviour
    {
        #region Private Variables
        Sequence animationSequence = null;
        RectTransform rT = null;
        TextMeshProUGUI label = null;
        #endregion

        void Awake() {
            rT = GetComponent<RectTransform>();
            label = GetComponent<TextMeshProUGUI>();
            SetOut();
        }

        public void AnimateIn() {
            rT.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(SetUpAnimation);
            animationSequence.Restart();
        }

        public void AnimateOut() {
            label.DOFade(0f, 0.15f)
                .OnComplete(() => animationSequence.Kill());
        }

        void SetUpAnimation() {
            animationSequence = DOTween.Sequence()
                .Append(rT.DOJumpAnchorPos(rT.anchoredPosition, 20f, 1, 0.5f))
                .Insert(0.05f, rT.DOPunchRotation(new Vector3(0,0,5), 0.4f, vibrato: 8))
                .AppendInterval(1.5f)
                .SetLoops(-1);
        }

        void SetOut() {
            transform.localScale = Vector3.zero;
        }
    }
}