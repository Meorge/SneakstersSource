using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Sneaksters.Gameplay;

namespace Sneaksters.UI.Gameplay {
	public class IconOverlay : MonoBehaviour {
		public GameObject objectToOverlay;
		public PhotonCharacterController characterOverlay;
		public TextMeshProUGUI charName;

		public Image imageComponent;

		public RectTransform canvasRect;

		public List<Image> images = new List<Image>();

		public bool Visible {
			set {
				foreach (Image i in images) {
					i.enabled = value;
				}
				imageComponent.enabled = value;
				charName.enabled = value;
			}
		}

		public float Opacity {
			set {
				foreach (Image i in images) {
					if (i != null) i.SetAlpha(value);
				}

				if (imageComponent != null) {
					imageComponent.SetAlpha(value);
					imageComponent.material?.SetFloat("_Alpha", value);
				}
				if (charName != null) charName.SetAlpha(value);
			}
		}

		RectTransform rectTransform;
		// Use this for initialization
		void Start () {
			rectTransform = GetComponent<RectTransform>();
			GameObject computerCanvas = PhotonCharacterController.localCharacterController.computerCanvas;
			transform.SetParent(computerCanvas.transform.Find("CompIcons"), false);
			imageComponent = GetComponent<Image>();

			canvasRect = gameObject.transform.parent.parent.gameObject.GetComponent<RectTransform>();

		}
		
		// Update is called once per frame
		void LateUpdate () {
			// Hide icons when the object they're following disappears
			if (objectToOverlay == null) {
				foreach (Image i in images) {
					if (i != null) i.enabled = false;
				}

				if (imageComponent != null) {
					imageComponent.enabled = false;
				}
				if (charName != null) charName.enabled = false;	
				return;
			} else {
				foreach (Image i in images) {
					if (i != null) i.enabled = true;
				}

				if (imageComponent != null) {
					imageComponent.enabled = true;
				}
				if (charName != null) charName.enabled = true;
			}
			
			Vector2 viewportPoint = Camera.main.WorldToViewportPoint(objectToOverlay.transform.position);
			

			Vector2 finalViewpointPort = new Vector2(
				(viewportPoint.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
				(viewportPoint.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
			);

			if (PhotonCharacterController.localCharacterController != null) {
				if (PhotonCharacterController.localCharacterController.playerType == PlayerType.Thief) {
					if (characterOverlay != null && characterOverlay.emoteTimer > 0 && Vector3.Distance(objectToOverlay.transform.position, PhotonCharacterController.localCharacterController.transform.position) < 10f) {
						charName.rectTransform.localScale = Vector3.zero;
						rectTransform.localScale = rectTransform.localScale * 2f;
					} else {
						rectTransform.localScale = Vector3.zero;
					}
				} else {
					if (PhotonGameManager.Instance.currentPlatformType == PhotonGameManager.PlatformType.Mobile) {
						rectTransform.localScale = Vector3.one * 2.5f;
					} else {
						rectTransform.localScale = Vector3.one;
					}

				}
			}

			rectTransform.anchoredPosition = finalViewpointPort;
		}
	}
}