namespace Sneaksters
{
	[System.Serializable]
	public struct ResolutionItem {
		public int width;
		public int height;

		public ResolutionItem(int w, int h) {
			width = w;
			height = h;
		}
	}
}