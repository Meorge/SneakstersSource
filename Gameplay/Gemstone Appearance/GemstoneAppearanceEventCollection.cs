using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters.Gameplay
{
    [CreateAssetMenu(fileName = "New Gemstone Appearance Event Collection", menuName = "Sneaksters/Gemstone Appearance Events/Event Collection", order = 1)]
    public class GemstoneAppearanceEventCollection : ScriptableObject
    {
        public List<GemstoneAppearanceEvent> events = new List<GemstoneAppearanceEvent>();

        public string GetCurrentGemstoneAppearanceID() {
            foreach (GemstoneAppearanceEvent ev in events) {
                if (ev.EventIsActive()) return ev.targetAppearanceID;
            }
            return "default";
        }
    }
}