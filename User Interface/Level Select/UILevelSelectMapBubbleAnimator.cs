using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UILevelSelectMapBubbleAnimator : MonoBehaviour
{
    [SerializeField] RectTransform circle = null, stem = null;
    [SerializeField] Image icon = null;

    [SerializeField] float duration = 0.1f;

    [ContextMenu("Animate active")]
    public void AnimateToActive()
    {
        circle.DOAnchorPosY(0.15f, duration)
            .SetEase(Ease.OutBack);
        stem.DOSizeDelta(new Vector2(0, 0.16f), duration)
            .SetEase(Ease.OutBack);

        transform.DOScale(1f, duration)
            .SetEase(Ease.OutBack);
    }

    [ContextMenu("Animate inactive")]
    public void AnimateToInactive()
    {
        circle.DOAnchorPosY(0f, duration)
            .SetEase(Ease.InBack);
        stem.DOSizeDelta(Vector2.zero, duration)
            .SetEase(Ease.InBack);

        transform.DOScale(0.75f, duration)
            .SetEase(Ease.InBack);
    }

    public void SetInactive()
    {
        circle.anchoredPosition = Vector2.zero;
        stem.sizeDelta = Vector2.zero;

        transform.localScale = Vector3.one * 0.75f;

    }
}
