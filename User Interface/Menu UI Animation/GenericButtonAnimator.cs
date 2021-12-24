using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class GenericButtonAnimator : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        Sequence cursorAnimation;

        [SerializeField]
        Image cursorImage = null;

        Button button;

        void Awake() {
            button = GetComponent<Button>();
            cursorImage.SetAlpha(0f);
        }

        void Start() {
            AnimateCursor();
        }

        void AnimateCursor() {
            cursorAnimation = DOTween.Sequence()
                .Append(cursorImage.rectTransform.DOScale(1.04f, 0.2f))
                .AppendInterval(1f)
                .Append(cursorImage.rectTransform.DOScale(1f, 0.2f))
                .AppendInterval(1f)
                .SetLoops(-1);
        }

        void OnDestroy() {
            cursorAnimation.Kill();
        }

        public void OnSelect(BaseEventData eventData) {
            // find out if we need to scroll
            if (button.interactable) ButtonSelected();
        }

        public void OnDeselect(BaseEventData eventData) {
            cursorImage.SetAlpha(0f);
        }

        public void OnPointerEnter(PointerEventData eventData = null) {
            if (button.interactable) button.Select();
        }

        void ButtonSelected() {
            cursorImage.SetAlpha(1f);
        }
    }
}