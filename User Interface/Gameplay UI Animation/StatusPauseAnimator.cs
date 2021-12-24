using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class StatusPauseAnimator : MonoBehaviour
    {
        RectTransform rT;

        public RectTransform pauseButtonsContainer;
        public RectTransform thiefStatus, gemStatus;

        public TextMeshProUGUI missionNumLabel, missionNameLabel;
        public TextMeshProUGUI thievesRemLabel, gemsCollectedLabel;

        public StaminaBarAnimator staminaBarAnimator, compassAnimator;
        public float pauseDuration = 1f;
        public float scaleSpeed = 4f;

        bool currentlyPaused = false;

        public bool CurrentlyPaused { get { return currentlyPaused; } }

        void Start() { rT = GetComponent<RectTransform>(); SetUnpaused(); }

        public void TogglePaused() {
            if (currentlyPaused) AnimateOutPaused();
            else AnimateInPaused();
        }

        [ContextMenu("Animate pause in")]
        public void AnimateInPaused() {
            currentlyPaused = true;
            DOTween.Sequence()
                .AppendCallback(() => { staminaBarAnimator.AnimateOut(); compassAnimator.AnimateOut(); })
                .Append(rT.DOAnchorMin(Vector2.zero, pauseDuration).SetEase(Ease.OutBack))
                .Join(rT.DOAnchorMax(Vector2.one, pauseDuration).SetEase(Ease.OutBack))
                .Join(rT.DOSizeDelta(Vector2.zero, pauseDuration).SetEase(Ease.OutBack))
                .Join(rT.DOAnchorPos(Vector2.zero, pauseDuration).SetEase(Ease.OutBack))

                .Insert(0f, missionNumLabel.rectTransform.DOScale(1, pauseDuration).SetEase(Ease.OutBack))
                .Join(missionNumLabel.DOFade(1f, pauseDuration))
                .Join(missionNameLabel.rectTransform.DOScale(1, pauseDuration).SetEase(Ease.OutBack))
                .Join(missionNameLabel.DOFade(1f, pauseDuration))
                .Join(thievesRemLabel.rectTransform.DOScale(1, pauseDuration).SetEase(Ease.OutBack))
                .Join(thievesRemLabel.DOFade(1f, pauseDuration))
                .Join(gemsCollectedLabel.rectTransform.DOScale(1, pauseDuration).SetEase(Ease.OutBack))
                .Join(gemsCollectedLabel.DOFade(1f, pauseDuration))
                .Join(pauseButtonsContainer.DOScale(1f, pauseDuration).SetEase(Ease.OutBack))
                
                // gem status
                .Insert(0f, gemStatus.DOScale(2, pauseDuration))
                .Join(gemStatus.DOLocalRotate(new Vector3(0,0,8), pauseDuration))
                .Join(gemStatus.DOAnchorPos(new Vector2(-115, -135), pauseDuration))
                .Join(gemStatus.DOAnchorMin(Vector2.one, pauseDuration))
                .Join(gemStatus.DOAnchorMax(Vector2.one, pauseDuration))
                
                // thief status
                .Join(thiefStatus.DOAnchorPos(new Vector2(-310, -135), pauseDuration).SetEase(Ease.OutBack))
                .Join(thiefStatus.DOScale(2, pauseDuration).SetEase(Ease.OutBack))
                .Join(thiefStatus.DOAnchorMin(Vector2.one, pauseDuration).SetEase(Ease.OutBack))
                .Join(thiefStatus.DOAnchorMax(Vector2.one, pauseDuration).SetEase(Ease.OutBack))
                .Join(thiefStatus.DOLocalRotate(new Vector3(0,0,8), pauseDuration).SetEase(Ease.OutBack))
                ;
        }

        [ContextMenu("Animate pause out")]
        public void AnimateOutPaused() {
            currentlyPaused = false;
            DOTween.Sequence()
                .Append(rT.DOAnchorMin(new Vector2(1,0), pauseDuration).SetEase(Ease.InBack))
                .Join(rT.DOAnchorMax(new Vector2(1,0), pauseDuration).SetEase(Ease.InBack))
                .Join(rT.DOSizeDelta(Vector2.one * 100, pauseDuration).SetEase(Ease.InBack))
                .Join(rT.DOAnchorPos(new Vector2(-15,15), pauseDuration).SetEase(Ease.InBack))

                .Insert(0f, missionNumLabel.rectTransform.DOScale(0, pauseDuration).SetEase(Ease.InBack))
                .Join(missionNumLabel.DOFade(0f, pauseDuration))
                .Join(missionNameLabel.rectTransform.DOScale(0, pauseDuration).SetEase(Ease.InBack))
                .Join(missionNameLabel.DOFade(0f, pauseDuration))
                .Join(thievesRemLabel.rectTransform.DOScale(0, pauseDuration).SetEase(Ease.InBack))
                .Join(thievesRemLabel.DOFade(0f, pauseDuration))
                .Join(gemsCollectedLabel.rectTransform.DOScale(0, pauseDuration).SetEase(Ease.InBack))
                .Join(gemsCollectedLabel.DOFade(0f, pauseDuration))
                .Join(pauseButtonsContainer.DOScale(0f, pauseDuration).SetEase(Ease.InBack))

                // gem status
                .Insert(0f, gemStatus.DOAnchorPos(new Vector2(-50, 70), pauseDuration).SetEase(Ease.InBack))
                .Join(gemStatus.DOScale(1, pauseDuration).SetEase(Ease.InBack))
                .Join(gemStatus.DOAnchorMin(new Vector2(1f, 0f), pauseDuration).SetEase(Ease.InBack))
                .Join(gemStatus.DOAnchorMax(new Vector2(1f, 0f), pauseDuration).SetEase(Ease.InBack))
                .Join(gemStatus.DOLocalRotate(Vector3.zero, pauseDuration).SetEase(Ease.InBack))
                
                // thief status
                .Join(thiefStatus.DOAnchorPos(new Vector2(-50, 30), pauseDuration).SetEase(Ease.InBack))
                .Join(thiefStatus.DOScale(1, pauseDuration).SetEase(Ease.InBack))
                .Join(thiefStatus.DOAnchorMin(new Vector2(1f, 0f), pauseDuration).SetEase(Ease.InBack))
                .Join(thiefStatus.DOAnchorMax(new Vector2(1f, 0f), pauseDuration).SetEase(Ease.InBack))
                .Join(thiefStatus.DOLocalRotate(Vector3.zero, pauseDuration).SetEase(Ease.InBack))
                

                .AppendCallback(() => { staminaBarAnimator.AnimateIn(); compassAnimator.AnimateIn(); })
                ;
        }

        public void SetUnpaused() {
            rT.anchorMin = new Vector2(1,0);
            rT.anchorMax = new Vector2(1,0);
            rT.sizeDelta = Vector2.one * 100;
            rT.anchoredPosition = new Vector2(-15, 15);
            rT.localScale = Vector2.one;

            missionNumLabel.rectTransform.localScale = Vector2.zero;
            missionNumLabel.SetAlpha(0f);
            missionNameLabel.rectTransform.localScale = Vector2.zero;
            missionNameLabel.SetAlpha(0f);
            thievesRemLabel.rectTransform.localScale = Vector2.zero;
            thievesRemLabel.SetAlpha(0f);
            gemsCollectedLabel.rectTransform.localScale = Vector2.zero;
            gemsCollectedLabel.SetAlpha(0f);

            pauseButtonsContainer.localScale = Vector2.zero;

            gemStatus.anchoredPosition = new Vector2(-50, 70);
            gemStatus.localScale = Vector2.one;
            gemStatus.anchorMin = new Vector2(1,0);
            gemStatus.anchorMax = new Vector2(1,0);
            gemStatus.localEulerAngles = Vector3.zero;

            thiefStatus.anchoredPosition = new Vector2(-50, 30);
            thiefStatus.localScale = Vector2.one;
            thiefStatus.anchorMin = new Vector2(1,0);
            thiefStatus.anchorMax = new Vector2(1,0);
            thiefStatus.localEulerAngles = Vector3.zero;

            staminaBarAnimator.SetIn();
            compassAnimator.SetIn();

            currentlyPaused = false;
        }

        public void AnimateGone() {
            rT.DOScale(0f, pauseDuration).SetEase(Ease.InBack);
            currentlyPaused = false;
        }

        public void AnimateIn() {
            rT.DOScale(1f, pauseDuration).SetEase(Ease.OutBack);
            currentlyPaused = false;
        }

        public void SetGone() {
            rT.localScale = Vector2.zero;
            currentlyPaused = false;
        }
    }
}