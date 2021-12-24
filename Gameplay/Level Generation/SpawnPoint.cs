using UnityEngine;

namespace Sneaksters.Gameplay
{
    public class SpawnPoint : MonoBehaviour
    {
        public static SpawnPoint spawnPoint;

        public void Awake() {
            if (spawnPoint != null) Destroy(gameObject);
            else spawnPoint = this;
        }
    }
}