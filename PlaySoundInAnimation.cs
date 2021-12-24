using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters
{
    public class PlaySoundInAnimation : MonoBehaviour
    {
        public void PlaySound(string path) {
            FMODUnity.RuntimeManager.PlayOneShot(path);
        }
    }
}