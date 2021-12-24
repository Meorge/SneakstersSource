using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Sneaksters.UI.Menus
{
    public class UIButtonCursorAnimator : MonoBehaviour
    {
        public bool instantSnap = false;

        RectTransform rectTransform = null;
        Sequence cursorAnimation = null;

        Tween movementTween = null;
        Image image = null;

        RectTransform currentButton = null;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            // AnimateCursor();
        }

        void AnimateCursor() {
            cursorAnimation = DOTween.Sequence()
                .Append(rectTransform.DOScale(1.04f, 0.2f))
                .AppendInterval(1f)
                .Append(rectTransform.DOScale(1f, 0.2f))
                .AppendInterval(1f)
                .SetLoops(-1);
        }

        public void Disable()
        {
            cursorAnimation.Pause();
            image.enabled = false;
        }

        public void Enable()
        {
            cursorAnimation.Play();
            image.enabled = true;
        }

        public void TweenToButton(RectTransform button)
        {
            currentButton = button;
        }

        void Update()
        {
            if (currentButton == null)
            {
                Disable();
                return;    
            }

            if (instantSnap)
            {
                transform.position = currentButton.transform.position;
                
                // change z position
                // var pos = transform.position;

            }
            else
            {
                float animTime = Time.deltaTime * 20f;
                transform.position = Vector3.Lerp(transform.position, currentButton.transform.position, animTime);
                rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, currentButton.sizeDelta, animTime);
            }
            
        }
    }
}