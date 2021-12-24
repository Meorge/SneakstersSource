using UnityEngine;

namespace Sneaksters.Gameplay
{
    public class PlayFootsteps : MonoBehaviour
    {
        public void PlayFootstep() {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Gameplay/Footsteps", transform.position);
        }
    }
}