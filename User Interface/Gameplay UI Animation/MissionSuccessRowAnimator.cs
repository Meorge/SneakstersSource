using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class MissionSuccessRowAnimator : MonoBehaviour
    {
        public TextMeshProUGUI left, middle, right;
        public bool doAnchorStuff = true;

        float rotateAmt = 20f;
        public void SetOut(float offset=-0.25f) {
            if (left != null) {
                left.SetAlpha(0f);
                if (doAnchorStuff) {
                    left.rectTransform.anchorMin = new Vector2(offset, 0.5f);
                    left.rectTransform.anchorMax = new Vector2(offset, 0.5f);
                    left.rectTransform.localEulerAngles = new Vector3(0f,0f,Random.Range(-rotateAmt, rotateAmt));
                }
            }

            if (middle != null) {
                middle.SetAlpha(0f);
                if (doAnchorStuff) {
                    middle.rectTransform.anchorMin = new Vector2(1+offset, 0.5f);
                    middle.rectTransform.anchorMax = new Vector2(1+offset, 0.5f);
                    middle.rectTransform.localEulerAngles = new Vector3(0f,0f,Random.Range(-rotateAmt, rotateAmt));
                }
            }

            if (right != null) {
                right.SetAlpha(0f);
                if (doAnchorStuff) {
                    right.rectTransform.anchorMin = new Vector2(1+offset, 0.5f);
                    right.rectTransform.anchorMax = new Vector2(1+offset, 0.5f);
                    right.rectTransform.localEulerAngles = new Vector3(0f,0f,Random.Range(-rotateAmt, rotateAmt));
                }
            }
        }

        public void Animate(ref Sequence sequence, float speed = 0.2f, float extraPauseBeforeRight = 0f) {
            if (left != null) {
                sequence
                    .Append(left.DOFade(1f, speed))
                    .Join(doAnchorStuff ? left.rectTransform.DOAnchorMin(new Vector2(0f, 0.5f), speed).SetEase(Ease.OutBack) : null)
                    .Join(doAnchorStuff ? left.rectTransform.DOAnchorMax(new Vector2(0f, 0.5f), speed).SetEase(Ease.OutBack) : null)
                    .Join(doAnchorStuff ? left.rectTransform.DOLocalRotate(Vector3.zero, speed).SetEase(Ease.OutBack) : null);
            }
            else sequence.AppendInterval(speed);

            if (middle != null) {
                sequence
                    .Append(middle.DOFade(1f, speed))
                    .Join(doAnchorStuff ? middle.rectTransform.DOAnchorMin(new Vector2(1f, 0.5f), speed).SetEase(Ease.OutBack) : null)
                    .Join(doAnchorStuff ? middle.rectTransform.DOAnchorMax(new Vector2(1f, 0.5f), speed).SetEase(Ease.OutBack) : null)
                    .Join(doAnchorStuff ? middle.rectTransform.DOLocalRotate(Vector3.zero, speed).SetEase(Ease.OutBack) : null);
            }
            else sequence.AppendInterval(speed);

            sequence.AppendInterval(extraPauseBeforeRight);

            if (right != null) {
                sequence
                    .Append(right.DOFade(1f, speed))
                    .Join(doAnchorStuff ? right.rectTransform.DOAnchorMin(new Vector2(1f, 0.5f), speed).SetEase(Ease.OutBack) : null)
                    .Join(doAnchorStuff ? right.rectTransform.DOAnchorMax(new Vector2(1f, 0.5f), speed).SetEase(Ease.OutBack) : null)
                    .Join(doAnchorStuff ? right.rectTransform.DOLocalRotate(Vector3.zero, speed).SetEase(Ease.OutBack) : null);
            }
            else sequence.AppendInterval(speed);
        }
    }
}