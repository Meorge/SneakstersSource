using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class DangerTapeAnimator : MonoBehaviour
    {
        public Image topTape, bottomTape;
        public Image topTapeEffect, bottomTapeEffect;

        Sequence loopSeq;

        public float speed = 1f;
        public float pulseSpeed = 0.4f;
        public float pulseScale = 1.4f;
        public float pulseOpacity = 0.7f;

        bool isVisible = false;

        public bool IsVisible { get { return isVisible; } }

        void Awake() { SetUpLoopSeq(); SetOut(); }



        public void SetOut() {
            topTape.rectTransform.DOLocalMoveX(1500, 0);
            bottomTape.rectTransform.DOLocalMoveX(-1500, 0);

            topTapeEffect.SetAlpha(0f);
            bottomTapeEffect.SetAlpha(0f);
            loopSeq.Pause();
        }

        public void SetIn() {
            topTape.rectTransform.DOLocalMoveX(0f, 0f);
            bottomTape.rectTransform.DOLocalMoveX(0f, 0f);
            loopSeq.Restart();
        }

        [ContextMenu("Animate in")]
        public void AnimateIn() {
            if (isVisible) {SetIn(); return;}
            isVisible = true;
            // SetOut();
            DOTween.Sequence()
                .Append(topTape.rectTransform.DOLocalMoveX(0f, 0.2f).SetEase(Ease.Linear))
                .Join(bottomTape.rectTransform.DOLocalMoveX(0f, 0.2f).SetEase(Ease.Linear))
                .AppendCallback(() => {
                    if (!isVisible) AnimateOut();
                    else loopSeq.Restart();
                });
        }

        [ContextMenu("Animate out")]
        public void AnimateOut() {
            if (!isVisible) {SetOut(); return;}
            isVisible = false;
            DOTween.Sequence()
                .AppendCallback(() => loopSeq.Pause())
                .Append(topTape.rectTransform.DOLocalMoveX(-1500, 0.2f).SetEase(Ease.Linear))
                .Join(bottomTape.rectTransform.DOLocalMoveX(1500, 0.2f).SetEase(Ease.Linear))
                .AppendCallback(() => {
                    if (isVisible) AnimateIn();
                });
        }

        void SetUpLoopSeq() {
            loopSeq = DOTween.Sequence()
                .AppendCallback(AnimateTapePulse)
                .Join(topTape.rectTransform.DOLocalMoveX(-160f, speed).SetEase(Ease.Linear))
                .Join(bottomTape.rectTransform.DOLocalMoveX(160f, speed).SetEase(Ease.Linear))
                .AppendCallback(SetIn)
                .SetLoops(-1);

            loopSeq.Pause();
        }


        [ContextMenu("Animate tape pulse")]
        void AnimateTapePulse() {
            topTapeEffect.rectTransform.localScale = Vector2.one;
            bottomTapeEffect.rectTransform.localScale = Vector2.one;

            topTapeEffect.SetAlpha(0f);
            bottomTapeEffect.SetAlpha(0f);

            DOTween.Sequence()
                .Append(topTapeEffect.DOFade(pulseOpacity, pulseSpeed / 4f))
                .Join(bottomTapeEffect.DOFade(pulseOpacity, pulseSpeed / 4f))
                .Append(topTapeEffect.rectTransform.DOScaleY(pulseScale, pulseSpeed))
                .Join(topTapeEffect.DOFade(0f, pulseSpeed))
                .Join(bottomTapeEffect.rectTransform.DOScaleY(pulseScale, pulseSpeed))
                .Join(bottomTapeEffect.DOFade(0f, pulseSpeed));
        }


    }
}