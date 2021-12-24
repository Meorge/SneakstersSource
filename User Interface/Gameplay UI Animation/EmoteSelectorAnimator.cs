using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class EmoteSelectorAnimator : MonoBehaviour
    {
        RectTransform rectTransform;
        public RectTransform emoteContainer;

        public SingleEmotePreviewAnimator oneNorth, oneEast, oneWest, oneSouth;
        public SingleEmotePreviewAnimator twoNorth, twoEast, twoWest, twoSouth;

        bool onMainPage = true;

        public bool ViewingPageOne {
            get { return onMainPage; }
        }

        public float flipDuration = 0.4f;
        public float animateOutDuration = 0.2f;

        void Start() {
            rectTransform = GetComponent<RectTransform>();
            SetToPageOne();
        }

        public void SwapPages () {
            if (onMainPage) {AnimateToPageTwo(); }
            else { AnimateToPageOne(); }
        }

        public void SetToPageOne() {
            // animate out the page one emotes
            oneNorth.SetIn();
            oneEast.SetIn();
            oneWest.SetIn();
            oneSouth.SetIn();

            // animate in the page two emotes
            twoNorth.SetOut();
            twoEast.SetOut();
            twoWest.SetOut();
            twoSouth.SetOut();

            onMainPage = true;
        }

        public void SetToPageTwo() {
            // animate in the page one emotes
            oneNorth.SetOut();
            oneEast.SetOut();
            oneWest.SetOut();
            oneSouth.SetOut();

            // animate out the page two emotes
            twoNorth.SetIn();
            twoEast.SetIn();
            twoWest.SetIn();
            twoSouth.SetIn();

            onMainPage = false;
        }


        [ContextMenu("Page One")]
        public void AnimateToPageOne() {
            DOTween.Sequence()
                .Append(emoteContainer.DOScaleX(0f, flipDuration / 2f).SetEase(Ease.InBack))
                .AppendCallback(SetToPageOne)
                .Append(emoteContainer.DOScaleX(1f, flipDuration / 2f).SetEase(Ease.OutBack));
        }

        [ContextMenu("Page Two")]
        public void AnimateToPageTwo() {
            DOTween.Sequence()
                .Append(emoteContainer.DOScaleX(0f, flipDuration / 2f).SetEase(Ease.InBack))
                .AppendCallback(SetToPageTwo)
                .Append(emoteContainer.DOScaleX(1f, flipDuration / 2f).SetEase(Ease.OutBack));
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            rectTransform.DOScale(1f, animateOutDuration).SetEase(Ease.OutBack);
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            rectTransform.DOScale(0f, animateOutDuration).SetEase(Ease.InBack);
        }
    }
}