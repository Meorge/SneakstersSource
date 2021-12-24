using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class SingleEmotePreviewAnimator : MonoBehaviour
    {
        public Image mainImage, effectImage;
        public float duration = 0.4f;
        public float endScale = 1.2f;
        public float startOpacity = 0.4f;

        public float animateOutDuration = 0.1f;

        

        [ContextMenu("Animate selected")]
        public void AnimateSelected() {
            // effectImage.SetAlpha(startOpacity);

            effectImage.material.SetFloat("_Alpha", startOpacity);
            effectImage.rectTransform.DOScale(1f, 0f);

            DOTween.Sequence()
                .Append(effectImage.rectTransform.DOScale(endScale, duration))
                .Join(effectImage.material.DOFloat(0f, "_Alpha", duration));
                // .Join(effectImage.DOFade(0f, duration));
        }

        public void SetOut() {
            mainImage.material.SetFloat("_Alpha", 0f);
            effectImage.material.SetFloat("_Alpha", 0f);
            effectImage.rectTransform.DOScale(1f, 0f);
        }

        public void SetIn() {
            mainImage.material.SetFloat("_Alpha", 1f);
            effectImage.material.SetFloat("_Alpha", 0f);
            effectImage.rectTransform.DOScale(1f, 0f);
        }
    }
}