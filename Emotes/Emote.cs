using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sneaksters.Emotes
{
    [CreateAssetMenu(fileName = "Emote", menuName = "Sneaksters/Emote", order = 0)]
    public class Emote : ScriptableObject {
        public Sprite sprite;
        public Sprite overlaySprite;
        public new string name = "";
        public int id = 0;
    }
}