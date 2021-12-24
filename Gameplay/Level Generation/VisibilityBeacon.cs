using UnityEngine;
using Shapes;

namespace Sneaksters.Gameplay
{
    public class VisibilityBeacon : MonoBehaviour
    {
        public Animator animator;

        
        // public Transform rangeVis;
        public Disc rangeVis;
        public Disc rangeVisOutline;

        
        public int _radius;

        public int Radius {
            set {
                SetRadius(_radius);

            }

            get {
                return _radius;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            PhotonStageManager.stageManager.visibilityBeacons.Add(this);
        }

        void Update() {
            SetRadius(_radius);
        }

        public void SetRadius(int rad) {
            _radius = rad;

            if (rangeVis != null) {
                // rangeVis.transform.localScale = new Vector3(_radius * 2, _radius * 2, _radius * 2);
                rangeVis.Radius = _radius;
                rangeVisOutline.Radius = _radius;
            } else {
                Debug.LogError("RangeVis didn't exist");
            }
        }
    }
}