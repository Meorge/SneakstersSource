using UnityEngine;

namespace Sneaksters.Gameplay
{
    public class GemstoneAppearanceEvent : ScriptableObject
    {
        public string targetAppearanceID = "default";
        public string eventName = "Event";

        virtual public bool EventIsActive() { return false; }
    }
}