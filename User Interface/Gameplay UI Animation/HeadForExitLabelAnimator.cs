using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Sneaksters.UI.Animation
{
    public class HeadForExitLabelAnimator : MonoBehaviour
    {
        RectTransform rT;
        TextMeshProUGUI tM;

        public float inDuration = 0.5f;
        public float waitDuration = 1f;
        public float outDuration = 0.5f;

        public float opacity = 0.4f;

        
        bool locked = false;

        bool hasAnimated = false;

        public bool HasAnimated { get { return hasAnimated; } }

        void Awake() {
            rT = GetComponent<RectTransform>();
            tM = GetComponent<TextMeshProUGUI>();
        }

        void Start() {SetOut();}

        public void SetOut() {
            tM.SetAlpha(0f);
            rT.anchorMin = new Vector2(-1, 0.5f);
            rT.anchorMax = new Vector2(-1, 0.5f);
            rT.localEulerAngles = new Vector3(0f, 0f, Random.Range(10, 20));
            rT.anchoredPosition = Vector2.zero;
        }

        [ContextMenu("Animate")]
        public void Animate() {
            if (locked) return;
            locked = true;
            hasAnimated = true;
            SetOut();
            DOTween.Sequence()
                .Append(tM.DOFade(opacity, inDuration))
                .Join(rT.DOAnchorMin(Vector2.one / 2f, inDuration).SetEase(Ease.OutBack))
                .Join(rT.DOAnchorMax(Vector2.one / 2f, inDuration).SetEase(Ease.OutBack))
                .Join(rT.DOLocalRotate(Vector3.zero, inDuration).SetEase(Ease.OutBack))

                .AppendInterval(waitDuration)

                .Append(tM.DOFade(0f, inDuration))
                .Join(rT.DOAnchorMin(new Vector2(2f, 0.5f), outDuration).SetEase(Ease.InBack))
                .Join(rT.DOAnchorMax(new Vector2(2f, 0.5f), outDuration).SetEase(Ease.InBack))
                .Join(rT.DOLocalRotate(new Vector3(0f,0f,Random.Range(10, 20)), inDuration).SetEase(Ease.InBack))
                
                .AppendCallback(() => {locked = false;});
        }

        [ContextMenu("Reset")]
        public void Reset() {
            hasAnimated = false;
        }
    }
}