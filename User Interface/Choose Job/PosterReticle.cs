using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

namespace Sneaksters.UI.Menus
{
    public class PosterReticle : MonoBehaviour
    {
        [SerializeField] Image image = null;
        public static float tweenDuration = 0.2f;
        public static float overshoot = 1.05f;

        public void AlignToPoster(ChooseJobButton poster) {
            if (poster == null) {
                Debug.LogError("PosterReticle - the poster is null? which is weird because we're calling this method from the poster??");
                return;
            }
            
            Vector3 targetPos = poster.transform.position;
            Vector3 targetRot = poster.transform.eulerAngles;

            transform.DOMove(targetPos, tweenDuration).SetEase(Ease.OutBack, overshoot);
            transform.DORotate(targetRot, tweenDuration).SetEase(Ease.OutBack, overshoot);
        }

        public void Enable() {
            image.enabled = true;
        }

        public void Disable() {
            image.enabled = false;
        }
    }
}