using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class MissionIntroAnimator : MonoBehaviour
    {
        public TextMeshProUGUI missionNumLabel, missionNameLabel;

        public float animationDuration = 0.7f;
        public float waitDuration = 1.5f;

        public HalvesTransitionAnimator halvesTransitionAnimator;
        void Awake() {
            Animate();
        }


        void SetInitialPositions() {
            halvesTransitionAnimator.SetIn();
            missionNumLabel.rectTransform.anchorMax = new Vector2(-0.25f, 0.5f);
            missionNumLabel.rectTransform.anchorMin = new Vector2(-0.25f, 0.5f);

            missionNameLabel.rectTransform.anchorMax = new Vector2(-0.25f, 0.5f);
            missionNameLabel.rectTransform.anchorMin = new Vector2(-0.25f, 0.5f);

            missionNameLabel.rectTransform.sizeDelta = new Vector2(500, 100);

            missionNumLabel.SetAlpha(0f);
            missionNameLabel.SetAlpha(0f);

        }

        [ContextMenu("Animate")]
        public void Animate() {
            SetInitialPositions();

            DOTween.Sequence()
                .AppendInterval(0.5f)
                .Append(missionNumLabel.rectTransform.DOAnchorMin(new Vector2(0.5f, 0.5f), animationDuration).SetEase(Ease.OutBack))
                    .Join(missionNumLabel.rectTransform.DOAnchorMax(new Vector2(0.5f, 0.5f), animationDuration).SetEase(Ease.OutBack))
                    .Join(missionNumLabel.DOFade(1f, animationDuration))
                .Insert(animationDuration / 1.5f, missionNameLabel.rectTransform.DOAnchorMin(new Vector2(0.5f, 0.5f), animationDuration).SetEase(Ease.OutBack))
                    .Join(missionNameLabel.rectTransform.DOAnchorMax(new Vector2(0.5f, 0.5f), animationDuration).SetEase(Ease.OutBack))
                    .Join(missionNameLabel.DOFade(1f, animationDuration))
                .AppendInterval(waitDuration)
                .Append(missionNumLabel.rectTransform.DOAnchorMin(new Vector2(1.25f, 0.5f), animationDuration).SetEase(Ease.InBack))
                    .Join(missionNumLabel.rectTransform.DOAnchorMax(new Vector2(1.25f, 0.5f), animationDuration).SetEase(Ease.InBack))
                    .Join(missionNumLabel.DOFade(0f, animationDuration))
                .Insert(animationDuration + waitDuration + animationDuration / 1.5f, missionNameLabel.rectTransform.DOAnchorMin(new Vector2(1.25f, 0.5f), animationDuration).SetEase(Ease.InBack))
                    .Join(missionNameLabel.rectTransform.DOAnchorMax(new Vector2(1.25f, 0.5f), animationDuration).SetEase(Ease.InBack))
                    .Join(missionNameLabel.DOFade(0f, animationDuration))
                    
                .InsertCallback(animationDuration + waitDuration + animationDuration / 1.5f + animationDuration / 1.5f, () => halvesTransitionAnimator.AnimateOut());
        }
    }
}