using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class MissionSuccessAnimator : MonoBehaviour
    {
        public Image victoryBackground;
        public Image victoryTriangle;
        public TextMeshProUGUI victoryTriangleLabel;
        public RectTransform separatorBar;

        public float triangleInDuration = 1f;
        public float backgroundFadeInDuration = 0.4f;
        public float triangleShrinkDuration = 0.3f;

        public MissionSuccessRowAnimator cashEarnedAnimator, timeTakenAnimator, escapedThievesAnimator, totalAnimator;
        public MissionSuccessRowAnimator returningToBaseAnimator;
        public StatusPauseAnimator statusPauseAnimator;
        public DangerTapeAnimator dangerTapeAnimator;
        public GemstoneTutorialAnimator gemstoneTutorialAnimator;
        public EmoteSelectorAnimator emoteSelectorAnimator;

        public float rowFadeInDuration = 0.2f;

        void Start() {
            SetOut();
        }

        void SetOut() {
            victoryBackground.SetAlpha(0f);
            victoryTriangle.SetAlpha(0f);
            victoryTriangleLabel.SetAlpha(0f);

            // hide the separator bar
            separatorBar.DOScaleX(0f, 0f);

            // place the victory triangle out to the left
            victoryTriangle.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            victoryTriangle.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            victoryTriangle.rectTransform.anchoredPosition = new Vector2(-500f, 0f);
            victoryTriangle.rectTransform.pivot = Vector2.one / 2f;
            victoryTriangle.rectTransform.localScale = Vector2.one;
            victoryTriangle.rectTransform.localEulerAngles = new Vector3(0f,0f,40f);

            victoryTriangleLabel.rectTransform.anchoredPosition = new Vector2(-500f, 0f);
            // victoryTriangleLabel.rectTransform.localEulerAngles = new Vector3(0f,0f,40f);

            cashEarnedAnimator.SetOut();
            timeTakenAnimator.SetOut();
            escapedThievesAnimator.SetOut();
            totalAnimator.SetOut();

            returningToBaseAnimator.SetOut();
        }

        void SuppressOtherAnimators() {
            statusPauseAnimator.AnimateGone();
            emoteSelectorAnimator.AnimateOut();
            dangerTapeAnimator.AnimateOut();
            gemstoneTutorialAnimator.AnimateOut();
        }

        [ContextMenu("Animate")]
        public void Animate() {
            SetOut();

            Sequence seq = DOTween.Sequence()
                .AppendCallback(SuppressOtherAnimators)
                .Append(victoryTriangle.rectTransform.DOAnchorMin(Vector2.one / 2f, triangleInDuration).SetEase(Ease.OutBack))
                .Join(victoryTriangle.rectTransform.DOAnchorMax(Vector2.one / 2f, triangleInDuration).SetEase(Ease.OutBack))
                .Join(victoryTriangle.rectTransform.DOAnchorPos(Vector2.zero, triangleInDuration).SetEase(Ease.OutBack))
                .Join(victoryTriangle.rectTransform.DOScale(1.5f, triangleInDuration).SetEase(Ease.OutBack))
                .Join(victoryTriangle.DOFade(1f, triangleInDuration))
                .Join(victoryTriangle.rectTransform.DOLocalRotate(Vector3.zero, triangleInDuration).SetEase(Ease.OutBack))
                .Join(victoryTriangleLabel.DOFade(1f, triangleInDuration))
                .Join(victoryTriangleLabel.rectTransform.DOAnchorPosX(0f, triangleInDuration).SetEase(Ease.OutBack))
                // .Join(victoryTriangleLabel.rectTransform.DOLocalRotate(Vector3.zero, triangleInDuration))

                .AppendInterval(1.5f)


                .Append(victoryBackground.DOFade(1f, backgroundFadeInDuration))
                .Join(victoryTriangle.rectTransform.DOPivotY(1f, triangleShrinkDuration))
                .Join(victoryTriangle.rectTransform.DOAnchorMin(new Vector2(0.5f, 1f), triangleShrinkDuration).SetEase(Ease.InOutBack))
                .Join(victoryTriangle.rectTransform.DOAnchorMax(new Vector2(0.5f, 1f), triangleShrinkDuration).SetEase(Ease.InOutBack))
                .Join(victoryTriangle.rectTransform.DOAnchorPos(new Vector2(0f, -25f), triangleShrinkDuration).SetEase(Ease.InOutBack))
                .Join(victoryTriangle.rectTransform.DOScale(1f, triangleShrinkDuration).SetEase(Ease.InOutBack))

                .AppendInterval(0.5f);

            cashEarnedAnimator.Animate(ref seq, rowFadeInDuration);
            seq.AppendInterval(0.5f);
            timeTakenAnimator.Animate(ref seq, rowFadeInDuration);
            seq.AppendInterval(0.5f);
            escapedThievesAnimator.Animate(ref seq, rowFadeInDuration);
            seq.AppendInterval(0.5f);
            seq.Append(separatorBar.DOScaleX(1f, triangleShrinkDuration).SetEase(Ease.OutBack));
            seq.AppendInterval(0.75f);
            totalAnimator.Animate(ref seq, rowFadeInDuration, 0.25f);
            seq.AppendInterval(0.25f);
            returningToBaseAnimator.Animate(ref seq, rowFadeInDuration);
        }
    }
}