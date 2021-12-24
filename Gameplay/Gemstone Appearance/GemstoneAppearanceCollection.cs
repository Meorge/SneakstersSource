using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters.Gameplay
{
    [CreateAssetMenu(fileName = "New Gemstone Appearance Collection", menuName = "Sneaksters/Gemstone Appearance Collection", order = 1)]
    public class GemstoneAppearanceCollection : ScriptableObject
    {
        public List<GemstoneAppearance> appearances = new List<GemstoneAppearance>();

        public GemstoneAppearance GetGemstoneAppearanceByID(string id = "default")
        {
            if (appearances == null)
            {
                Debug.LogError("GemstoneAppearanceCollection.GetGemstoneAppearanceByID() - appearances is null!");
                return null;
            }

            foreach (GemstoneAppearance app in appearances)
            {
                if (app.id == id) return app;
            }

            Debug.LogError($"GemstoneAppearanceCollection.GetGemstoneAppearanceByID() - Appearance with id \"{id}\" not found!");
            return null;
        }
    }
}