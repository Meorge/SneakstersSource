Shader "Hidden/VignetteShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OutColor ("Out color", Color) = (0,0,0,0)
		_TargetPos ("Target position", Vector) = (0,0,0,0)
		_Distance ("Distance threshold", Range(0,10)) = 10
		_WidthRatio ("Width ratio", Range(0,10)) = 1
		_Hardness ("Hardness", Range(0,10)) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 screenPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _OutColor;
			float2 _TargetPos;
			float _Distance;
			float _WidthRatio;
			float _Hardness;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				i.uv.x *= _WidthRatio;
				_TargetPos.x *= _WidthRatio;


				float distanceToCenter = length(i.uv.xy - _TargetPos.xy) / abs(_Distance);

				col *= 1-distanceToCenter;

				//col = lerp(_OutColor, col, distanceToCenter);

				//col /= _Hardness;

				// if (distanceToCenter > _Distance)
				// 	col = _OutColor;
				return col;
			}
			ENDCG
		}
	}
}
