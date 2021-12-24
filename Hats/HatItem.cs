using UnityEngine;

namespace Sneaksters.Customization
{
    [CreateAssetMenu(fileName = "New Hat Item", menuName = "Hats/Hat Item", order = 1)]
    public class HatItem : ScriptableObject
    {
        public new string name = "";
        public GameObject model;
        public string id = "";
        public Sprite icon;

        [Header("Offset stuff")]
        public Vector3 offset = new Vector3();
        public Vector3 rotation = new Vector3();
        public Vector3 scale = new Vector3();

        
    }
}