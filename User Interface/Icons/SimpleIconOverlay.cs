using UnityEngine;

using Sneaksters.Gameplay;

namespace Sneaksters.UI
{
    // All this should do is track a GameObject if it exists
    public class SimpleIconOverlay : MonoBehaviour
    {

        GameObject objectToTrack = null;

        RectTransform rectTransform;

        // Make sure the icon overlay's parent spans the whole screen
        RectTransform canvasRect;

        void Start() {
            rectTransform = GetComponent<RectTransform>();
            canvasRect = gameObject.transform.parent.parent.gameObject.GetComponent<RectTransform>();
        }

        public void SetObjectToTrack(GameObject g) { objectToTrack = g; }
        public void SetParent(Transform t) { transform.SetParent(t, false); }
        public void SetParentToIconContainer() { SetParent(PhotonCharacterController.localCharacterController.iconContainer); }

        void Update() {
            if (objectToTrack == null || canvasRect == null) return;

            Vector2 viewportPoint = Camera.main.WorldToViewportPoint(objectToTrack.transform.position);

            Vector2 finalViewpointPort = new Vector2(
                (viewportPoint.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                (viewportPoint.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
            );

            rectTransform.anchoredPosition = finalViewpointPort;

            // Hide the icon if it's behind the player
            float dot = Vector3.Dot(Camera.main.transform.forward, (objectToTrack.transform.position - Camera.main.transform.position).normalized);
            rectTransform.localScale = (dot > 0) ? Vector3.one : Vector3.zero;
        }
    }
}