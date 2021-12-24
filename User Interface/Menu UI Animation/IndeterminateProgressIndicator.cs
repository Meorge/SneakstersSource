using UnityEngine;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class IndeterminateProgressIndicator : MonoBehaviour
    {
        public RectTransform outer, inner;
        public RectTransform outerShadow, innerShadow;
        public float speed = 1f;

        Sequence seq;

        void Start() {
            PlayAnimation();
            StopAnimation();
        }

        public void RestartAnimation() {
            seq.Restart();
        }

        public void StopAnimation() {
            seq.Pause();
        }

        public void PlayAnimation() {
            seq = DOTween.Sequence()
                .Append(outer.DOLocalRotate(new Vector3(0f, 0f, -180f), speed, RotateMode.FastBeyond360)
                    .SetRelative()
                    .SetEase(Ease.InOutBack))
                .Join(outerShadow.DOLocalRotate(new Vector3(0f, 0f, -180f), speed, RotateMode.FastBeyond360)
                    .SetRelative()
                    .SetEase(Ease.InOutBack))
                .Join(inner.DOLocalRotate(new Vector3(0f, 0f, 180f), speed, RotateMode.FastBeyond360)
                    .SetRelative()
                    .SetEase(Ease.InOutBack))
                .Join(innerShadow.DOLocalRotate(new Vector3(0f, 0f, 180f), speed, RotateMode.FastBeyond360)
                    .SetRelative()
                    .SetEase(Ease.InOutBack))
                .AppendInterval(0.5f)
                .SetLoops(-1, LoopType.Restart);
        }


        public void Activate() {
            gameObject.SetActive(true);
            RestartAnimation();
        }

        public void Deactivate() {
            gameObject.SetActive(false);
            StopAnimation();
        }
        
    }
}