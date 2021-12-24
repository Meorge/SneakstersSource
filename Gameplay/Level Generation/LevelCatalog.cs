using System.Collections.Generic;
using UnityEngine;

namespace Sneaksters.Gameplay
{
	[CreateAssetMenu(fileName = "New Level Catalog", menuName = "Level Management/Level Catalog", order = 2)]
	public class LevelCatalog : ScriptableObject {
		public List<LevelCategory> categories;


		public LevelObject GetLevelObject(string sceneName)
		{
			foreach (LevelCategory category in categories)
				foreach (LevelObject level in category.levels)
					if (level.sceneName == sceneName)
						return level;
			return null;
		}

		public LevelObject GetLevelObject(int id)
		{
			foreach (LevelCategory category in categories)
				foreach (LevelObject level in category.levels)
					if (level.levelID == id)
						return level;
			return null;
		}
	}
}