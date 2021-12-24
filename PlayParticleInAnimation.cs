using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters
{
    public class PlayParticleInAnimation : MonoBehaviour
    {
        public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        public void PlayAllParticles() {
            foreach (ParticleSystem p in particleSystems) p.Play();
        }

        public void StopAllParticles() {
            foreach (ParticleSystem p in particleSystems) p.Stop();
        }
    }
}