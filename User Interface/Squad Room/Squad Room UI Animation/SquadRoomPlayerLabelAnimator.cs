using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation {
    public class SquadRoomPlayerLabelAnimator : MonoBehaviour
    {
        #region Animation Parameters
        [Header("Animation Parameters")]
        [Header("Main Box")]
        [SerializeField] float scaleDuration = 0.6f;
        [SerializeField] float shadowDuration = 0.8f, shadowOffset = 0.18f, overshoot = 2f, tagRestRotation = 0f, tagRotationPunch = -10f;
        [SerializeField] Ease inEase = Ease.OutBack, outEase = Ease.InBack;
        [SerializeField] AnimationCurve tagRotationCurve = new AnimationCurve(), jobPulseCurve = new AnimationCurve();
        [Header("Inactive Box")]
        [SerializeField] Color inactiveBackgroundColor = Color.black;
        [SerializeField] Color inactiveLabelColor = Color.white;
        [SerializeField] float inactiveBackgroundFadeDuration = 0.2f, initialOffset = -5f;
        #endregion

        #region Active Slot References
        [Header("Active Slot")]
        [SerializeField]
        RectTransform activeSlotTransform = null;
        [SerializeField]
        TextMeshProUGUI playerNameLabel = null;
        [SerializeField]
        Image shadow = null, background = null;
        [SerializeField]
        Image thiefIcon = null, jobIcon = null;
        [SerializeField]
        Image playerStatusBackground = null;
        [SerializeField]
        TextMeshProUGUI statusLabel = null;
        #endregion

        #region Inactive Slot References
        [Header("Inactive Slot")]
        [SerializeField]
        RectTransform inactiveSlotTransform = null;
        [SerializeField]
        Image inactiveBackground = null;
        [SerializeField]
        TextMeshProUGUI waitingLabel = null;
        #endregion

        public bool populated { get; private set; } = false;
        public bool tagVisible { get; private set; } = false;

        private void Awake()
        {
            SetWaiting();
            AnimateTagOut();
            AnimateWaitingOut();
        }

        [ContextMenu("Animate in")]
        public void AnimateIn()
        {
            populated = true;
            DOTween.Sequence()
                .Append(activeSlotTransform.DOScale(Vector3.one, scaleDuration)
                    .SetEase(inEase, overshoot)
                )
                .Insert(shadowOffset, shadow.rectTransform.DOLocalMove(new Vector2(5, -5), shadowDuration)
                    .SetEase(inEase, overshoot)
                )
                .Join(shadow.rectTransform.DOAnchorPos(new Vector2(5, -5), shadowDuration)
                    .SetEase(inEase, overshoot)
                )
                ;
        }

        [ContextMenu("Animate out")]
        public void AnimateToWaiting()
        {
            populated = false;
            DOTween.Sequence()
                .Append(activeSlotTransform.DOScale(Vector3.zero, scaleDuration)
                    .SetEase(outEase, overshoot)
                )
                .Insert(shadowOffset, shadow.rectTransform.DOLocalMove(Vector2.zero, shadowDuration)
                    .SetEase(outEase, overshoot)
                )
                .Join(shadow.rectTransform.DOAnchorPos(Vector2.zero, shadowDuration)
                    .SetEase(outEase, overshoot)
                )

                ;
        }

        [ContextMenu("Animate inactive in")]
        public void AnimateWaitingIn()
        {
            inactiveBackground.color = inactiveBackgroundColor.WithAlpha(0f);
            waitingLabel.color = inactiveBackgroundColor.WithAlpha(0f);

            inactiveSlotTransform.localPosition = new Vector2(initialOffset, 0);
            inactiveSlotTransform.anchoredPosition = new Vector2(initialOffset, 0);

            DOTween.Sequence()
                .Append(inactiveBackground.DOColor(inactiveBackgroundColor, inactiveBackgroundFadeDuration))
                .Join(waitingLabel.DOColor(inactiveLabelColor, inactiveBackgroundFadeDuration))
                .Join(inactiveSlotTransform.DOLocalMove(Vector2.zero, inactiveBackgroundFadeDuration).SetEase(Ease.OutBack))
                .Join(inactiveSlotTransform.DOAnchorPos(Vector2.zero, inactiveBackgroundFadeDuration).SetEase(Ease.OutBack))
                ;
        }

        [ContextMenu("Animate inactive out")]
        public void AnimateWaitingOut()
        {
            inactiveBackground.color = inactiveBackgroundColor;
            waitingLabel.color = inactiveBackgroundColor;

            inactiveSlotTransform.localPosition = Vector2.zero;
            inactiveSlotTransform.anchoredPosition = Vector2.zero;

            AnimateToWaiting();

            DOTween.Sequence()
                .Append(inactiveBackground.DOFade(0f, inactiveBackgroundFadeDuration))
                .Join(waitingLabel.DOFade(0f, inactiveBackgroundFadeDuration))
                .Join(inactiveSlotTransform.DOLocalMove(new Vector2(-initialOffset, 0), inactiveBackgroundFadeDuration).SetEase(Ease.InBack))
                .Join(inactiveSlotTransform.DOAnchorPos(new Vector2(-initialOffset, 0), inactiveBackgroundFadeDuration).SetEase(Ease.InBack))
                ;
        }


        [ContextMenu("Animate tag in")]
        public void AnimateTagIn()
        {
            tagVisible = true;

            AnimateTagUpdate();
            playerStatusBackground.rectTransform.DOScale(1f, scaleDuration).SetEase(Ease.OutBack);
        }

        [ContextMenu("Animate tag out")]
        public void AnimateTagOut()
        {
            tagVisible = false;

            AnimateTagUpdate();
            playerStatusBackground.rectTransform.DOScale(0f, scaleDuration).SetEase(Ease.InBack);
        }

        public void AnimateTagUpdate()
        {
            //DOTween.Sequence()
            //    .Append(playerStatusBackground.rectTransform.DOPunchRotation(new Vector3(0, 0, tagRotationPunch), scaleDuration))
            //    .Append(playerStatusBackground.rectTransform.DORotate(new Vector3(0, 0, tagRestRotation), 0f))
            //    ;
            playerStatusBackground.rectTransform.DOLocalRotate(new Vector3(0, 0, tagRotationPunch), scaleDuration).SetEase(tagRotationCurve);
        }

        [ContextMenu("Animate job update")]
        public void AnimateJobUpdate() {
            // jobIcon.rectTransform.DOScale(new Vector3(3f, 3f, 1f), scaleDuration).SetEase(jobPulseCurve);
        }
        
        public void SetPlayerName(string playerName = "NONE")
        {
            playerNameLabel.text = playerName;
        }

        public void SetPlayerTag(string playerTag = "")
        {
            string previousTag = statusLabel.text;

            statusLabel.text = playerTag;

            // animate tag in
            if (previousTag == "" && playerTag != "")
                AnimateTagIn();

            // animate tag out
            else if (previousTag != "" && playerTag == "")
                AnimateTagOut();

            // update tag
            else
                AnimateTagUpdate();
        }

        public void SetJobIcon(Sprite sprite = null)
        {
            AnimateJobUpdate();
            jobIcon.sprite = sprite;
        }

        void SetWaiting()
        {
            populated = false;

            activeSlotTransform.localScale = Vector2.zero;
            shadow.rectTransform.anchoredPosition = Vector2.zero;
            shadow.rectTransform.localPosition = Vector2.zero;

            playerStatusBackground.rectTransform.localScale = Vector2.zero;
        }

    }
}