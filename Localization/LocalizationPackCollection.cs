using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters.Localization
{
    [CreateAssetMenu(fileName = "New Localization Pack Collection", menuName = "Localization/Localization Pack Collection", order = 1)]
    public class LocalizationPackCollection : ScriptableObject {
        public List<LocalizationPack> localizationPacks = new List<LocalizationPack>();

        public Sprite GetIconForRegion(string id) {
            foreach (LocalizationPack i in localizationPacks) {
                if (i.languageCode == id) return i.flagSymbol;
            }
            return localizationPacks[0].flagSymbol;
        }

    }
}