using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Sneaksters;
using Sneaksters.Localization;

namespace Sneaksters.UI
{
    public class SimpleLocalizeUIImage : MonoBehaviour
    {
        private Image image;
        public string _objectID;

        public string objectID {
            get {
                return _objectID;
            } set {
                _objectID = value;
                UpdateImage();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            image = GetComponent<Image>();
            UpdateImage();
        }

        void UpdateImage() {
            // get sprite object
            if (PhotonGameManager.Instance == null) return;
            Object obj = PhotonGameManager.Instance.GetLocalizedObject(_objectID);
            if (obj == null) return;

            Sprite sp = (Sprite)obj;

            image.sprite = sp;
        }
    }
}