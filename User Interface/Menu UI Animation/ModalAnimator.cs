using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class ModalAnimator : MonoBehaviour
    {
        public GameObject container;
        public RectTransform modalWindowTransform;
        public Image modalWindowGrayBackground;
        public Image modalWindowGrayBackgroundHalftone;
        public float animationDuration = 0.15f;
        public bool ModalActive {
            get {
                return _modalActive;
            }
            set {
                SetActive(value);
            }
        }
        public IndeterminateProgressIndicator progressIndicator;

        void Awake() {
            SetActive(false);
        }
        
        private bool _modalActive = false;
        public void SetActive(bool active = true) {
            modalWindowTransform.anchorMin = Vector2.zero;
            if (active) {
                container.SetActive(true);
                modalWindowGrayBackground.SetAlpha(0.8f);
                modalWindowGrayBackgroundHalftone.SetAlpha(0.8f);
                modalWindowTransform.localEulerAngles = Vector3.zero;
                modalWindowTransform.anchorMax = Vector2.one;
                modalWindowTransform.anchoredPosition = new Vector2(50, 50);
                modalWindowTransform.sizeDelta = new Vector2(-100, -100);
            } else {
                modalWindowGrayBackground.SetAlpha(0f);
                modalWindowGrayBackgroundHalftone.SetAlpha(0f);
                modalWindowTransform.localEulerAngles = new Vector3(0f,0f,-80f);
                modalWindowTransform.anchorMax = Vector2.zero;
                modalWindowTransform.anchoredPosition = new Vector2(0, -300f);
                modalWindowTransform.sizeDelta = new Vector2(700, 350);
                progressIndicator.StopAnimation();
                container.SetActive(false);
            }

            _modalActive = active;
        }

        [ContextMenu("Animate in")]
        public void AnimateModalIn(TweenCallback onComplete) {
            Debug.Log("animate modal in");
            container.SetActive(true);
            progressIndicator.RestartAnimation();        
            Sequence seq = DOTween.Sequence();
            seq
                .Append(modalWindowGrayBackground.DOFade(0.8f, animationDuration))
                .Join(modalWindowGrayBackgroundHalftone.DOFade(0.8f, animationDuration))
                .Join(modalWindowTransform.DOAnchorMax(Vector2.one, animationDuration))
                .Join(modalWindowTransform.DOAnchorPos(new Vector2(50, 50), animationDuration))
                .Join(modalWindowTransform.DOSizeDelta(-100 * Vector2.one, animationDuration))
                .AppendCallback(onComplete)
                .Insert(animationDuration / 2f, modalWindowTransform.DOLocalRotate(Vector3.zero, animationDuration).SetEase(Ease.OutBack))
                .AppendCallback(() => {SetActive(true);});
        }

        public void AnimateModalIn() {
            AnimateModalIn(() => {});
        }

        [ContextMenu("Animate out")]
        public void AnimateModalOut(TweenCallback onComplete) {
            // modalWindowTransform.pivot = Vector2.zero;
            Sequence seq = DOTween.Sequence();
            seq
                .Append(modalWindowGrayBackground.DOFade(0f, animationDuration))
                .Join(modalWindowGrayBackgroundHalftone.DOFade(0f, animationDuration))
                .Join(modalWindowTransform.DOLocalRotate(new Vector3(0f,0f,-80f), animationDuration).SetEase(Ease.InBack))
                .Insert(animationDuration / 2f, modalWindowTransform.DOAnchorMax(Vector2.zero, animationDuration))
                .Join(modalWindowTransform.DOAnchorPos(new Vector2(0, -300f), animationDuration))
                .Join(modalWindowTransform.DOSizeDelta(new Vector2(700, 350), animationDuration))
                .AppendCallback(onComplete)
                .AppendCallback(() => {SetActive(false);});
        }

        public void AnimateModalOut() {
            AnimateModalOut(() => {});
        }

        [ContextMenu("Test animation")]
        public void TestAnimation() {
            StartCoroutine(AnimTest());
        }

        IEnumerator AnimTest() {
            AnimateModalIn();
            yield return new WaitForSeconds(animationDuration + 5f);
            AnimateModalOut();
            yield return new WaitForSeconds(animationDuration + 0.5f);
        }
    }
}