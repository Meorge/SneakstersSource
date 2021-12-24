using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class GameManagerUIAnimator : MonoBehaviour
    {
        public RectTransform modal;
        public Image background;
        public float duration = 0.2f;


        void Start() {
            background.SetAlpha(0f);
            modal.localScale = Vector2.zero;
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            background.DOFade(0.5f, duration);
            modal.DOScale(1f, duration);
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            background.DOFade(0f, duration);
            modal.DOScale(0f, duration);
        }
    }
}