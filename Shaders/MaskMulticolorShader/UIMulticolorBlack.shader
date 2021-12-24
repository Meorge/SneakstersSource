Shader "Unlit/UIMulticolorBlack"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OverlayTex ("Overlay Texture", 2D) = "white" {}
		_ReplaceBlackColor ("Replace Black color", Color) = (0,0,0,1)
		_Alpha ("Alpha amount", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent"}
		LOD 100

		ZWrite Off


		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _ReplaceBlackColor;
			float4 _MainTex_ST;


			sampler2D _OverlayTex;
			float4 _OverlayTex_ST;

			float _Alpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
				fixed4 colorMask = 1-col; // just the opposite - now the balck stuff is white, etc

				col.rgb = lerp(col.rgb, _ReplaceBlackColor, colorMask.r);


				fixed4 col2 = tex2D(_OverlayTex, i.uv);
				col = lerp(col, col2, col2.a);
				col.a = _Alpha * col.a;

				// Uncomment to have the Image graphic's opacity affect it
				// col *= i.color;
				
				return col;
			}
			ENDCG
		}
	}
}
