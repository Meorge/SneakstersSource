Shader "Hidden/ComputerView"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[HDR] _Color ("Tint", Color) = (1,1,1,1)
		_ComputerBar ("Computer bars", 2D) = "white" {}
		_ComputerBarScale ("Bar scale", Range(1,100)) = 1
		_ComputerBarScrollScale("Bar scrolling scale", Range(0,100)) = 1
		_ComputerBarTint ("Bar tint", Color) = (1,1,1,1)

		_ComputerRedOffset ("Red offset", Range(-0.1,0.1)) = 0
		_ComputerGreenOffset ("Green offset", Range(-0.1,0.1)) = 0
		_ComputerBlueOffset ("Blue offset", Range(-0.1,0.1)) = 0

		_ComputerCloseAmount ("Screen turn off effect amount", Range(0,1)) = 0

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
			sampler2D _ComputerBar;
			float _ComputerBarScale;
			float _ComputerBarScrollScale;
			float4 _ComputerBarTint;
			float4 _Color;

			float _ComputerRedOffset;
			float _ComputerGreenOffset;
			float _ComputerBlueOffset;

			float _ComputerCloseAmount;

			// https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
			float map(float s, float a1, float a2, float b1, float b2)
			{
				float res = b1 + (s-a1)*(b2-b1)/(a2-a1);
				return clamp(res, b1, b2);
			}

			float astroid(float s) {
				float t = smoothstep(0, 2 * 3.14159265359, s);

				return pow(sin(t), 3);

			}

			fixed4 frag (v2f i) : SV_Target
			{

				// Computer turn off effect
				// The 0-1 Y scale of the screen should correspond to the 0-0.5 range of _ComputerCloseAmount
				float yScale = map(_ComputerCloseAmount, 0, 0.5, 1, 32);

				// The 0-1 X scale of the screen should correspond to the 0.5-1 range of _ComputerCloseAmount
				float xScale = map(_ComputerCloseAmount, 0.6, 1, 1, 256);

				float brightnessMap = map(_ComputerCloseAmount, 0, 0.5, 0, 1);

				float darknessMap = map(_ComputerCloseAmount, 1, 0.99, 0, 1);

				float2 newUV = i.uv;

				newUV.x = (xScale * i.uv.x) + 0.5 * (1 - xScale);
				newUV.y = (yScale * i.uv.y) + 0.5 * (1 - yScale);

				fixed4 colRed = (tex2D(_MainTex, newUV + _ComputerRedOffset) * _Color).r;
				fixed4 colGreen = (tex2D(_MainTex, newUV + _ComputerGreenOffset) * _Color).g;
				fixed4 colBlue = (tex2D(_MainTex, newUV + _ComputerBlueOffset) * _Color).b;

				fixed4 col = fixed4(0,0,0,0);
				col.r = colRed;
				col.g = colGreen;
				col.b = colBlue;


				fixed4 colBars = tex2D(_ComputerBar, newUV * _ComputerBarScale + (_Time.x * _ComputerBarScrollScale));
				colBars *= _ComputerBarTint;
				colBars.rgb *= colBars.a;
				col += colBars;
				col.rgb = saturate(col.rgb + (brightnessMap));

				col.rgb *= darknessMap;

				
				col.rgb *= (newUV.x <= 2 && newUV.y <= 2 && newUV.x >= -1 && newUV.y >= -1);
				col.rgb *= (1 - abs(newUV.y * 2) * brightnessMap);
				

				// col = float4(0,0,0,1);
				// col.r = astroid(0);
				
				return col;
			}
			ENDCG
		}
	}
}
