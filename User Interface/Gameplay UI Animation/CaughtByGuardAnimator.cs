using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class CaughtByGuardAnimator : MonoBehaviour
    {
        public RectTransform youveBeenCapturedTextContainer;
        public TextMeshProUGUI youveBeenCapturedTopText, youveBeenCapturedBottomText;
        public RectTransform barsContainer;
        public HalvesTransitionAnimator halvesTransitionAnimator;


        public float initialCapturedTextScale = 200f;
        public float capturedTextSlamDuration = 0.25f;
        public float barsDownDuration = 0.75f;

        void Start() { SetOut(false); }

        public void SetOut(bool setHalvesOut = false) {
            youveBeenCapturedBottomText.SetAlpha(0f);
            youveBeenCapturedTopText.SetAlpha(0f);

            barsContainer.localScale = barsContainer.localScale.SetY(0f);

            youveBeenCapturedTextContainer.localScale = new Vector2(initialCapturedTextScale, initialCapturedTextScale);
            youveBeenCapturedTextContainer.localEulerAngles = new Vector3(0f,0f,-30f);

            if (setHalvesOut) halvesTransitionAnimator.SetOut();
        }

        [ContextMenu("Animate")]
        public void Animate(TweenCallback callback = null) {
            SetOut(true);

            DOTween.Sequence()
                .Append(youveBeenCapturedBottomText.DOFade(1f, 0.5f))
                .Join(youveBeenCapturedTopText.DOFade(1f, 0.5f))

                .Insert(0.2f, youveBeenCapturedTextContainer.DOScale(1.3f, capturedTextSlamDuration).SetEase(Ease.OutExpo))
                .InsertCallback(0f, () => FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Gameplay/Captured/Letter Slam"))
                .Join(youveBeenCapturedTextContainer.DOLocalRotate(new Vector3(0f,0f,5f), capturedTextSlamDuration))
                
                
                .AppendInterval(1.5f)

                .AppendCallback(() => FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Gameplay/Captured/Bars Down"))
                .Join(barsContainer.DOScaleY(1f, barsDownDuration).SetEase(Ease.InCirc))

                .AppendInterval(barsDownDuration + 1.5f)

                .AppendCallback(() => halvesTransitionAnimator.AnimateIn())
                .AppendCallback(() => FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Gameplay/Captured/Bars Close"))

                .AppendInterval(1f)

                .AppendCallback(callback);
        }

    }
}
