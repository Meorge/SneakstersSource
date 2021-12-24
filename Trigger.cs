using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters.Gameplay.Events
{
    public class Trigger
    {
        [SerializeField]
        private bool _triggered = false;

        // public bool Value {
        //     get {
        //         return _triggered;
        //         _triggered = false;
        //     }
        // }

        // public Trigger() {
        //     triggers.Add(this);
        // }

        public void SetTrigger() {
            _triggered = true;
        }

        public bool GetTrigger() {
            if (_triggered) {
                _triggered = false;
                return true;
            } else {
                return false;
            }
        }

        public void ClearTrigger() {
            _triggered = false;
        }

        // public static List<Trigger> triggers = new List<Trigger>();
        // public static void VeryLateUpdate() {
        //     foreach (Trigger t in triggers) {
        //         if (t != null) t._triggered = false;
        //     }
        // }
    }
}