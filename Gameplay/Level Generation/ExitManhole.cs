using UnityEngine;

namespace Sneaksters.Gameplay
{
    public class ExitManhole : MonoBehaviour
    {
        public Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            PhotonStageManager.stageManager.exitManhole = this;
        }

        public void Open() {
            animator.SetBool("open", true);
        }

        public void OnTriggerEnter() {
            if (animator.GetBool("open")) PhotonStageManager.stageManager.EndGame();
        }
    }
}