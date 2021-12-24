using UnityEngine;
using UnityEngine.UI;

namespace Sneaksters.UI
{
	public class FunkyImage : Image
	{

		int gOrig = 2;
		int g;

		public new void Start() {
			base.Start();
			g = gOrig;
		}
		public void Update() {
			g -= 1;
			if (g <= 0) {
				g = gOrig;
				base.SetVerticesDirty();
			}
		}

		protected override void OnPopulateMesh( VertexHelper vh )
		{
				base.OnPopulateMesh( vh );
				var r = GetPixelAdjustedRect();
				var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
				Color32 color32 = color;
				vh.Clear();

				float skewX = Mathf.Cos(Time.time * 8) * 5;
				float skewY = Mathf.Sin(Time.time * 6) * 5;
				vh.AddVert( new Vector3( v.x + skewX, v.y + skewY), color32, new Vector2( 0f, 0f ) ); // bottom left
				vh.AddVert( new Vector3( v.x - skewY, v.w - skewX), color32, new Vector2( 0f, 1f ) ); // top left
				vh.AddVert( new Vector3( v.z + skewX, (v.w / 2) + skewY), color32, new Vector2( 1f, 0.5f ) ); // center right
				//vh.AddVert( new Vector3( v.z + skewX, v.w + skewY), color32, new Vector2( 1f, 1f ) ); // top right
				
				//vh.AddVert( new Vector3( v.z - skewY, v.y - skewX), color32, new Vector2( 1f, 0f ) ); // bottom right
				vh.AddTriangle( 0, 1, 2 );
				//vh.AddTriangle( 2, 3, 0 );
		}
	}
}