using UnityEngine;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class MissionSelectAnimator : MonoBehaviour
    {
        public GameObject container;
        RectTransform missionSelectBox;

        public float duration = 0.5f;
        public float outPos = -500f;

        public bool IsActive {
            get {
                return isActive;
            }
        }

        bool isActive = false;

        void Start() {
            missionSelectBox = GetComponent<RectTransform>();

            missionSelectBox.anchoredPosition = new Vector2(missionSelectBox.anchoredPosition.x, outPos);
            isActive = false;
            container.SetActive(false);
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            missionSelectBox.DOAnchorPosY(0f, duration).SetEase(Ease.OutBack);
            isActive = true;
            container.SetActive(true);
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            DOTween.Sequence()
                .Append(missionSelectBox.DOAnchorPosY(outPos, duration).SetEase(Ease.InBack))
                .AppendCallback(() => container.SetActive(false));
            isActive = false;
        }
    }
}