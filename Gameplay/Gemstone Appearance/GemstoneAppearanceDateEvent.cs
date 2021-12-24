using UnityEngine;

namespace Sneaksters.Gameplay
{
    [CreateAssetMenu(fileName = "New Gemstone Appearance Date Event", menuName = "Sneaksters/Gemstone Appearance Events/Date Event", order = 3)]
    public class GemstoneAppearanceDateEvent : GemstoneAppearanceEvent
    {
        public int month = 1, day = 1;
        public int daysBeforeToActivate = 7;

        public override bool EventIsActive()
        {
            System.DateTime targetDateTime = new System.DateTime(System.DateTime.Now.Year, month, day);
            System.DateTime now = System.DateTime.Now;

            int daysRemaining = (int)(targetDateTime - now).TotalDays;

            Debug.Log($"{daysRemaining} days until {eventName} this year");

            return daysRemaining >= 0 && daysRemaining <= daysBeforeToActivate;
        }
    }
}