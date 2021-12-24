using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters.Customization
{
    [CreateAssetMenu(fileName = "New Hat Catalog", menuName = "Hats/Hat Catalog", order = 2)]
    public class HatCatalog : ScriptableObject
    {
        public List<HatItem> hats = new List<HatItem>();

        public HatItem GetHatItemFromID(string id) {
            foreach (HatItem h in hats) {
                if (h.id == id) return h;
            }
            return GetHatItemFromID("tophat");
        }
    }
}