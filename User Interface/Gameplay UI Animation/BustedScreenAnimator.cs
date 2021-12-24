using UnityEngine;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class BustedScreenAnimator : MonoBehaviour
    {
        public RectTransform bustedLabel;
        public RectTransform squadLastedLabel;

        public HalvesTransitionAnimator halvesAnimator = null;

        void Awake() { SetOut(); }
        void SetOut() {
            Vector2 initAnchor = new Vector2(0.5f, 1.25f);
            bustedLabel.anchorMin = initAnchor;
            bustedLabel.anchorMax = initAnchor;

            bustedLabel.anchoredPosition = Vector2.zero;
            bustedLabel.localEulerAngles = new Vector3(0f, 0f, Random.Range(-10f, 10f));
            bustedLabel.localScale = Vector2.zero;
            squadLastedLabel.DOScaleY(0f, 0f);
            
        }

        [ContextMenu("Animate")]
        public void Animate(TweenCallback onComplete = null) {
            if (onComplete == null) onComplete = () => {};
            SetOut();

            halvesAnimator?.SetIn();
            bustedLabel.localScale = Vector2.one;
            DOTween.Sequence()
                .AppendCallback(() => FMODUnity.RuntimeManager.PlayOneShot("event:/Music/Busted"))
                .Append(bustedLabel.DOAnchorMin(new Vector2(0.5f, 0.5f), 1f).SetEase(Ease.OutBounce))
                .Join(bustedLabel.DOAnchorMax(new Vector2(0.5f, 0.5f), 1f).SetEase(Ease.OutBounce))
                .Join(bustedLabel.DOLocalRotate(Vector3.zero, 1f).SetEase(Ease.OutBounce))
                .AppendInterval(3f)
                .Append(squadLastedLabel.DOScaleY(1f, 0.5f))
                .AppendInterval(3f)
                .AppendCallback(onComplete);

        }
    }
}