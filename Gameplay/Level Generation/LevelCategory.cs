using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters.Gameplay
{
	[CreateAssetMenu(fileName = "New Level Category", menuName = "Level Management/Level Category", order = 2)]
	public class LevelCategory : ScriptableObject {
        public new string name = "Category";
		public string description = "This is a description of an area";
		public Vector2 mapPos = Vector2.zero;
		public List<LevelObject> levels;
	}
}