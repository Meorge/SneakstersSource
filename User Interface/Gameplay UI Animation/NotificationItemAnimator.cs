using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class NotificationItemAnimator : MonoBehaviour
    {
        [SerializeField]
        private RectTransform backgroundRect = null;

        [SerializeField]
        private TextMeshProUGUI label = null, timestampLabel = null;

        [SerializeField]
        private Image thiefIcon = null, background = null;


        public float outOffset = -200f;
        public float inDuration = 0.1f;
        public float waitDuration = 5f;
        public float outDuration = 0.2f;

        public void Animate() {
            backgroundRect.DOAnchorPosX(outOffset, 0);
            label.SetAlpha(0f);
            timestampLabel.SetAlpha(0f);
            background.SetAlpha(0f);
            thiefIcon.SetAlpha(0f);

            DOTween.Sequence()
                .Append(backgroundRect.DOAnchorPosX(0, inDuration).SetEase(Ease.OutBack))
                .Join(label.DOFade(1f, inDuration))
                .Join(timestampLabel.DOFade(1f, inDuration))
                .Join(background.DOFade(1f, inDuration))
                .Join(thiefIcon.DOFade(1f, inDuration))

                .AppendInterval(waitDuration)

                .Append(label.DOFade(0f, outDuration))
                .Join(timestampLabel.DOFade(0f, outDuration))
                .Join(background.DOFade(0f, outDuration))
                .Join(thiefIcon.DOFade(0f, outDuration))
                .AppendInterval(0.5f)
                .AppendCallback(() => Destroy(gameObject));

        }
    }
}