using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation {
    public class MissionReadyAnimator : MonoBehaviour
    {
        public GameObject container;

        [Header("Main animation")]
        public Image background;
        public TextMeshProUGUI weHaveMissionLabel;
        public Image missionGoButton, missionGoPulser;
        public TextMeshProUGUI missionGoText;

        public float duration = 0.5f;
        public float backgroundOpacity = 0.5f;
        public float invisibleLabelScale = 3f;


        Sequence missionGoPulserSequence;
        public float missionGoPulserFadeInDuration, missionGoPulserFadeInAmount, missionGoPulserLargeScale, missionGoPulserScaleDuration;

        void Awake() {
            SetupLoopingPulseAnimation();
            InitializeOut();
        }

        void InitializeOut() {
            weHaveMissionLabel.SetAlpha(0f);
            background.SetAlpha(0f);

            missionGoText.SetAlpha(0f);
            missionGoButton.SetAlpha(0f);

            weHaveMissionLabel.rectTransform.localScale = invisibleLabelScale * Vector2.one;
            missionGoButton.rectTransform.localScale = invisibleLabelScale * Vector2.one;
            missionGoPulser.SetAlpha(0f);

            container.SetActive(false);
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            InitializeOut();
            container.SetActive(true);
            background.DOFade(backgroundOpacity, duration);
            weHaveMissionLabel.rectTransform.DOScale(1f, duration).SetEase(Ease.OutBack);
            weHaveMissionLabel.DOFade(1f, duration);

            missionGoText.DOFade(1f, duration);
            missionGoButton.DOFade(1f, duration);
            missionGoButton.rectTransform.DOScale(1f, duration).SetEase(Ease.OutBack);
            
            missionGoPulserSequence.Restart();
        }

        public void SetupLoopingPulseAnimation() {
            missionGoPulserSequence = DOTween.Sequence();
            missionGoPulserSequence
                .AppendCallback(() => { missionGoPulser.rectTransform.localScale = Vector3.one; missionGoPulser.SetAlpha(0f); })
                .AppendInterval(1f)
                .Append(missionGoPulser.DOFade(missionGoPulserFadeInAmount, missionGoPulserFadeInDuration))
                .Join(missionGoPulser.rectTransform.DOScale(missionGoPulserLargeScale, missionGoPulserScaleDuration))
                .Join(missionGoPulser.DOFade(0f, missionGoPulserScaleDuration));

            missionGoPulserSequence.SetLoops(-1);
            missionGoPulserSequence.Pause();
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            background.DOFade(0f, duration);
            weHaveMissionLabel.DOFade(0f, duration);

            missionGoPulserSequence.Pause();

            Sequence seq = DOTween.Sequence();
            seq.Append(missionGoButton.rectTransform.DOScale(invisibleLabelScale / 1.5f, duration).SetEase(Ease.InBack))
                .Insert(duration / 2f, missionGoText.DOFade(0f, duration / 2f))
                .Join(missionGoButton.DOFade(0f, duration / 2f))
                .AppendCallback(() => container.SetActive(false));
        }
    }
}