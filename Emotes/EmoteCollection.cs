using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sneaksters.Emotes
{
    [CreateAssetMenu(fileName = "Emote Collection", menuName = "Sneaksters/Emote Collection", order = 0)]
    public class EmoteCollection : ScriptableObject {
        public List<Emote> emotes = new List<Emote>();
    }
}