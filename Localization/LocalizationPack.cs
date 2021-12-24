using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Sneaksters.Localization
{
    [CreateAssetMenu(fileName = "New Localization Pack", menuName = "Localization/Localization Pack", order = 1)]
    public class LocalizationPack : ScriptableObject {
        public TextAsset jsonAsset;
        public string languageCode = "";
        public string languageName = "";
        public Sprite flagSymbol;

        public TMP_FontAsset overrideFont;

        public List<LocalizationAsset> assets = new List<LocalizationAsset>();

        [System.Serializable]
        public class LocalizationAsset {
            public string key = "";
            public Object item;
        }
    }
}