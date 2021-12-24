using UnityEngine;
using UnityEngine.UI;

namespace Sneaksters.UI
{
	public class HalfTriangle : Graphic
	{
		[SerializeField]
		Vector2 pointA = new Vector2(), pointB = new Vector2(), pointC = new Vector2();
		public new void Start() {
			base.Start();
		}

		protected override void OnPopulateMesh( VertexHelper vh )
		{
			base.OnPopulateMesh( vh );
			Rect r = GetPixelAdjustedRect();
			var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
			Color32 color32 = color;
			vh.Clear();

			vh.AddVert( new Vector3(r.x * pointA.x, r.y * pointA.y), color32, new Vector2( 0f, 0f ) ); // bottom left
			vh.AddVert( new Vector3(r.x * pointB.x, (r.y + r.height) * pointB.y), color32, new Vector2( 0f, 1f ) ); // top left
			vh.AddVert( new Vector3((r.x + r.width) * pointC.x, (r.y + r.height) * pointC.y), color32, new Vector2( 1f, 1f ) ); // center right
			vh.AddTriangle( 0, 1, 2 );
		}
	}
}