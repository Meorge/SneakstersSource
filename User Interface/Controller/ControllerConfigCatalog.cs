using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Sneaksters.UI
{
    [CreateAssetMenu(fileName = "New Controller Config Catalog", menuName = "Controllers/Controller Config Catalog", order = 1)]
    public class ControllerConfigCatalog : ScriptableObject {
        public List<ControllerConfigObject> configs = new List<ControllerConfigObject>();
    }
}