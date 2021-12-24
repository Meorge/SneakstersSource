using UnityEngine;

namespace Sneaksters.Gameplay {
    [CreateAssetMenu(fileName = "New Gemstone Appearance", menuName = "Sneaksters/Gemstone Appearance", order = 2)]
    public class GemstoneAppearance : ScriptableObject
    {
        public new string name = "Appearance";
        public string id = "appearance";
        public GameObject objectPrefab = null;
        public Sprite icon = null;
        public Texture2D gemSackTexture = null;
    }
}