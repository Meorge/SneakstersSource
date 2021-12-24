using UnityEngine;

namespace Sneaksters.Gameplay
{
	[CreateAssetMenu(fileName = "New Level Object", menuName = "Level Management/Level Object", order = 1)]
	public class LevelObject : ScriptableObject {
		public string levelName = "";
		public string sceneName = "level1-1";
		public int levelNumber = 1;

		public int levelID = 0;
		public int maxGems = 1;
	}
}