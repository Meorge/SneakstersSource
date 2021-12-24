using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using Shapes;

public class ShapesRingPulseAnimator : MonoBehaviour
{
    public Disc disc { get; private set; } = null;
    Sequence sequence = null;

    [SerializeField] float endRadius = 0.1f, duration = 1f;

    // Start is called before the first frame update
    void Start()
    {
        disc = GetComponent<Disc>();
        SetUpAnimation();
    }

    void SetUpAnimation()
    {
        // Set initial values
        disc.Radius = 0f;
        disc.Thickness = endRadius / 2f;

        sequence = DOTween.Sequence();
        sequence
            .Append(disc.DORadius(endRadius, duration))
            .Join(disc.DOThickness(0, duration))
            .AppendInterval(duration / 2f)
            .SetLoops(-1);

    }
}
