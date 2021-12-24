using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI
{
    public class IrisAnimator : MonoBehaviour
    {
        [SerializeField]
        [Header("Parts")]
        private Image main = null;

        [SerializeField]
        private Image top = null, bottom = null, left = null, right = null;

        [Header("Triangle Parts")]
        [SerializeField]
        private Graphic topTriangle = null;
        [SerializeField]
        private Graphic bottomTriangle = null, leftTriangle = null, rightTriangle = null;

        [Header("Parameters")]
        [SerializeField]
        [Range(0,1)]
        private float IrisAmount = 0.0f;

        [SerializeField]
        private Vector2 MinSize = new Vector2(0,0), MaxSize = new Vector2(1000,1000);

        [SerializeField]
        private float MinRotation = 0f, MaxRotation = 270f;

        [SerializeField]
        private float PointToBeginFadeOut = 0.8f;

        private Vector2 currentSize = new Vector2();
        private float currentRotation = 0f;

        private RectTransform rectTransform = null;

        

        void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        void Update() {
            currentRotation = Mathf.Lerp(MinRotation, MaxRotation, IrisAmount);
            currentSize = Vector2.Lerp(new Vector2(0,0), MaxSize, IrisAmount);

            main.rectTransform.sizeDelta = currentSize;
            main.rectTransform.eulerAngles = new Vector3(0f,0f, currentRotation);

            float halfOfMainWidth = main.rectTransform.rect.width / 2f;
            float halfOfMainHeight = main.rectTransform.rect.height / 2f;

            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            float halfOfWholeWidth = rectTransform.rect.width / 2f;
            float halfOfWholeHeight = rectTransform.rect.height / 2f;

            // Width of the left and right sides should be
            //  wholeWidth / 2 - main.rect.width / 2
            Vector2 lrSizeDelta = new Vector2(halfOfWholeWidth - halfOfMainWidth, 0f);
            left.rectTransform.sizeDelta = lrSizeDelta;
            right.rectTransform.sizeDelta = lrSizeDelta;

            // Similarly, the heights of the top and bottom sides should be
            //  wholeHeight / 2 - main.rect.height / 2
            Vector2 tbSizeDelta = new Vector2(0f, halfOfWholeHeight - halfOfMainHeight);
            top.rectTransform.sizeDelta = tbSizeDelta;
            bottom.rectTransform.sizeDelta = tbSizeDelta;

            // Update the triangles
            UpdateTriangles();

            // Update colors and opacity
            UpdateOpacity();
        }

        void SetOpacity(float opacity) {
            main.SetAlpha(opacity);
            topTriangle.SetAlpha(opacity);
            bottomTriangle.SetAlpha(opacity);
            leftTriangle.SetAlpha(opacity);
            rightTriangle.SetAlpha(opacity);
            top.SetAlpha(opacity);
            bottom.SetAlpha(opacity);
            left.SetAlpha(opacity);
            right.SetAlpha(opacity);
        }

        void UpdateOpacity() {
            if (IrisAmount < PointToBeginFadeOut) {
                SetOpacity(1f);
            }
            float opacity = Mathf.InverseLerp(1f, PointToBeginFadeOut, IrisAmount);
            SetOpacity(opacity);
        }

        void UpdateTriangles() {
            topTriangle.rectTransform.eulerAngles = Vector3.zero;
            bottomTriangle.rectTransform.eulerAngles = Vector3.zero;
            leftTriangle.rectTransform.eulerAngles = Vector3.zero;
            rightTriangle.rectTransform.eulerAngles = Vector3.zero;

            float x = main.rectTransform.rect.width;
            float y = main.rectTransform.rect.height;


            Vector2 topBottomSizeDelta = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (90f - currentRotation)) * x, -x * Mathf.Sin(Mathf.Deg2Rad * currentRotation));
            Vector2 leftRightSizeDelta = new Vector2(-y * Mathf.Sin(Mathf.Deg2Rad * currentRotation), Mathf.Sin(Mathf.Deg2Rad * (90f - currentRotation)) * y);

            topTriangle.rectTransform.sizeDelta = topBottomSizeDelta;
            bottomTriangle.rectTransform.sizeDelta = topBottomSizeDelta;

            leftTriangle.rectTransform.sizeDelta = leftRightSizeDelta;
            rightTriangle.rectTransform.sizeDelta = leftRightSizeDelta;
        }

        [ContextMenu("Animate iris out (to visible)")]
        void TestIrisOut() { AnimateIrisOut(); }

        [ContextMenu("Animate iris in (to black)")]
        void TestIrisIn() { AnimateIrisIn(); }


        public void AnimateIrisOut(TweenCallback onComplete = null) {
            if (onComplete == null) onComplete = () => {};
            IrisAmount = 0f;
            DOTween.Sequence()
                .Append(DOTween.To(() => IrisAmount, (x) => IrisAmount = x, 1f, 1f)).SetEase(Ease.InQuad)
                .AppendCallback(onComplete);
        }


        public void AnimateIrisIn(TweenCallback onComplete = null) {
            if (onComplete == null) onComplete = () => {};
            IrisAmount = 1f;
            DOTween.Sequence()
                .Append(DOTween.To(() => IrisAmount, (x) => IrisAmount = x, 0f, 1f)).SetEase(Ease.OutQuad)
                .AppendCallback(onComplete);
        }

        public void SetIrisOut() {
            IrisAmount = 1f;
        }
        public void SetIrisIn() {
            IrisAmount = 0f;
        }
    }
}