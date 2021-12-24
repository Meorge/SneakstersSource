using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class SquadCodeEntryAnimator : MonoBehaviour
    {
        public GameObject container;

        public TextMeshProUGUI bigLabel, smallLabel, placeholderEntryText, entryText;
        public Image entryBackground;

        public float bigLabelInitialX, smallLabelInitialX, entryBoxInitialX;
        public float placeholderOpacity = 0.5f;
        public float duration = 1f;

        [Header("Incorrect shake")]
        public float shakeDuration = 0.5f;
        public float shakeStrength = 10f;
        public float shakeRandomness = 5f;
        public int shakeVibrato = 10;
        public Color incorrectColor = Color.red;

        void Start() {
            SetOut();
        }

        public void SetOut() {
            bigLabel.rectTransform.anchoredPosition = new Vector2(bigLabelInitialX, bigLabel.rectTransform.anchoredPosition.y);
            smallLabel.rectTransform.anchoredPosition = new Vector2(smallLabelInitialX, smallLabel.rectTransform.anchoredPosition.y);
            bigLabel.SetAlpha(0f);
            smallLabel.SetAlpha(0f);

            entryBackground.rectTransform.anchoredPosition = new Vector2(entryBoxInitialX, entryBackground.rectTransform.anchoredPosition.y);
            placeholderEntryText.SetAlpha(0f);
            entryText.SetAlpha(0f);
            entryBackground.SetAlpha(0f);
            container.SetActive(false);
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            container.SetActive(true);
            bigLabel.rectTransform.DOAnchorPosX(0f, duration).SetEase(Ease.OutBack);
            smallLabel.rectTransform.DOAnchorPosX(0f, duration).SetEase(Ease.OutBack);
            bigLabel.DOFade(1f, duration);
            smallLabel.DOFade(1f, duration);


            entryBackground.rectTransform.DOAnchorPosX(0f, duration).SetEase(Ease.OutBack);
            entryText.DOFade(1f, duration);
            placeholderEntryText.DOFade(placeholderOpacity, duration);
            entryBackground.DOFade(1f, duration);

        } 

        [ContextMenu("Animate incorrect entry")]
        public void AnimateIncorrect() {
            DOTween.Sequence()
                .Append(entryBackground.rectTransform.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness))
                .Join(entryBackground.DOColor(incorrectColor, shakeDuration / 2f))
                .Append(entryBackground.DOColor(Color.white, shakeDuration / 2f));
            
        }

        [ContextMenu("Animate out (Left)")]
        public void AnimateOutLeft() {
            bigLabel.rectTransform.DOAnchorPosX(bigLabelInitialX, duration).SetEase(Ease.InBack);
            smallLabel.rectTransform.DOAnchorPosX(smallLabelInitialX, duration).SetEase(Ease.InBack);
            bigLabel.DOFade(0f, duration);
            smallLabel.DOFade(0f, duration);


            entryBackground.rectTransform.DOAnchorPosX(entryBoxInitialX, duration).SetEase(Ease.InBack);
            entryText.DOFade(0f, duration);
            placeholderEntryText.DOFade(0f, duration);
            entryBackground.DOFade(0f, duration);

            DOTween.Sequence()
                .AppendInterval(duration)
                .AppendCallback(() => container.SetActive(false));
        }

        [ContextMenu("Animate out (Right)")]
        public void AnimateOutRight() {
            bigLabel.rectTransform.DOAnchorPosX(-bigLabelInitialX, duration).SetEase(Ease.InBack);
            smallLabel.rectTransform.DOAnchorPosX(-smallLabelInitialX, duration).SetEase(Ease.InBack);
            bigLabel.DOFade(0f, duration);
            smallLabel.DOFade(0f, duration);


            entryBackground.rectTransform.DOAnchorPosX(-entryBoxInitialX, duration).SetEase(Ease.InBack);
            entryText.DOFade(0f, duration);
            placeholderEntryText.DOFade(0f, duration);
            entryBackground.DOFade(0f, duration);

            DOTween.Sequence()
                .AppendInterval(duration)
                .AppendCallback(() => container.SetActive(false));
        }
    }
}