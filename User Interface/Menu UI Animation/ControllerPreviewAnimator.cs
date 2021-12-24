using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Animation {
    public class ControllerPreviewAnimator : MonoBehaviour
    {
        public GameObject container;
        public Image controllerPreviewImage;
        public Image controllerPreviewSilhouetteImage;
        RectTransform cPreview;
        RectTransform cPreviewSil;
        public float controllerAnimation_mainInDuration = 0.2f;
        public float controllerAnimation_silhouetteInDuration = 0.25f;

        void Start() {
            container.SetActive(false);
            cPreview = controllerPreviewImage.GetComponent<RectTransform>();
            cPreviewSil = controllerPreviewSilhouetteImage.GetComponent<RectTransform>();
        }
        public Sequence AnimateControllerIn() {
            SetControllerVisibility();

            Vector2 p = cPreview.anchoredPosition;
            p.x = -50f;
            cPreview.anchoredPosition = p;
            cPreviewSil.anchoredPosition = Vector2.zero;

            controllerPreviewImage.SetAlpha(0f);
            controllerPreviewSilhouetteImage.SetAlpha(0f);

            Sequence animateControllerSequence = DOTween.Sequence();
            animateControllerSequence.Append(cPreview.DOLocalMoveX(0f, controllerAnimation_mainInDuration))
                .Join(controllerPreviewImage.DOFade(1f, controllerAnimation_mainInDuration))
                .Insert(controllerAnimation_mainInDuration / 2.5f, cPreviewSil.DOLocalMoveX(20f, controllerAnimation_silhouetteInDuration))
                .Join(controllerPreviewSilhouetteImage.DOFade(1f, controllerAnimation_silhouetteInDuration));

            return animateControllerSequence;
        }

        public Sequence AnimateControllerOut() {
            Sequence animateControllerSequence = DOTween.Sequence();
            animateControllerSequence.Append(cPreview.DOLocalMoveX(-50f, controllerAnimation_mainInDuration))
                .Join(controllerPreviewImage.DOFade(0f, controllerAnimation_mainInDuration))
                .Join(cPreviewSil.DOLocalMoveX(0f, controllerAnimation_silhouetteInDuration))
                .Join(controllerPreviewSilhouetteImage.DOFade(0f, controllerAnimation_silhouetteInDuration))
                .AppendCallback(() => {SetControllerVisibility(false);});

            return animateControllerSequence;
        }

        public void SetControllerVisibility(bool v = true) {
            container.SetActive(v);
            controllerPreviewImage.enabled = v;
            controllerPreviewSilhouetteImage.enabled = v;

            
        }

        public void SetSprites(Sprite main, Sprite sil) {
            controllerPreviewImage.sprite = main;
            controllerPreviewSilhouetteImage.sprite = sil;
        }
    }
}