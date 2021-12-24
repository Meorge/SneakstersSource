// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SurfaceMulticolorBlack" {

	
	Properties {
		_Color ("Color", Color) = (0,0,0,1)
		_WhiteColor ("White Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_OutlineAmt ("Outline Amount", Range(0,10)) = 1

		// _HalftoneTex ("Halftone dot", 2D) = "white" {}
		// _HalftoneWidthRatio ("Halftone width ratio", Range(0,10)) = 1
        // _HalftoneRotationAmt ("Halftone rotation Amount", Float) = 0.707

        // _HalftoneOffset ("Halftone offset", Vector) = (0,0,0,0)


		_LumOffset ("Lumination Offset", Range(-1, 1)) = 0
		_LumMult ("Lumination Multiply", Range(0, 2)) = 1
		_DotOffset ("Dot Offset", Vector) = (1,1,0,0)
		_DotLumPush ("Dot Lumination Push", Range(-1,1)) = 0
		_SmoothTransition ("Smooth Transition", Range(0,1)) = 0
		_SizeOfDots ("Size of Dots", Float) = 10
		_DensityOfDots ("Density of Dots", Float) = 1

		_DotOpacity ("Dot opacity", Range(0,1)) = 1

		// [Toggle(HALFTONE_DEBUG)] _HalftoneDebug ("Halftone debug", Float) = 0

		[Toggle] _HalftoneColorConsistent ("Keep halftone color same as thief color", Float) = 0
	}



	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200



		Pass { // OUTLINE STUFF
			Name "Outline"
			Cull Front

			//CGINCLUDE
		

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			


			float _OutlineAmt;
			float4 _Color;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			
			struct v2f {
				float4 pos : POSITION;
				float4 color : COLOR;
			};
 

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(norm.xy);
			
				o.pos.xy += offset * o.pos.z * (_OutlineAmt / 100);
				o.color = _Color;
				return o;
			}

			half4 frag(v2f i) : COLOR {
				return _Color;
			}

			ENDCG

		}
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows

		#pragma surface surf NewHalftone fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 pos;
			float4 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _WhiteColor;

		half _HalftoneDebug;

		sampler2D _HalftoneTex;
		float _HalftoneScale;
		float _HalftoneWidthRatio;
		float _HalftoneRotationAmt;
		float _HalftoneOffset;

		float _HalftoneColorConsistent;




		// struct SurfaceOutputScreenPos // mostly ripped from https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
		// {
		// 	fixed3 Albedo;  // diffuse color
		// 	fixed3 Normal;  // tangent space normal, if written
		// 	fixed3 Emission;
		// 	half Specular;  // specular power in 0..1 range
		// 	fixed Gloss;    // specular intensity
		// 	fixed Alpha;    // alpha for transparencies
		// 	fixed4 screenPos; // screen position
		// };

		struct SurfOutWithPos // mostly ripped from https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
		{
			fixed3 Albedo;  // diffuse color
			fixed3 Normal;  // tangent space normal, if written
			fixed3 Emission;
			half Specular;  // specular power in 0..1 range
			fixed Gloss;    // specular intensity
			fixed Alpha;    // alpha for transparencies
			float2 Pos;
			float Distance;
		};

		float _SmoothTransition, _SizeOfDots, _DensityOfDots, _DotLumPush, _LumOffset, _LumMult, _DotOpacity;
		fixed4 _DotOffset;

		// Made by John10v10
		float dotty(float lum, float2 coords, float dist){
			float relSize = _SizeOfDots * dist;
			lum *= _LumMult;
			lum += _LumOffset;
			float2 crd = coords + _DotOffset.xy;
			float2 crd2 = coords + _DotOffset.xy + float2(0, relSize/2);
			float lighter_lum = 1;
			float darker_lum = 0;
			float transitionPointA = lerp(0.5, 0, _SmoothTransition);
			float transitionPointB = lerp(0.5, 1, _SmoothTransition);
			float transition = clamp((lum-transitionPointA)/(transitionPointB-transitionPointA),0,1);
			if(lum > transitionPointA){
				if(distance(crd-relSize/2, floor(crd/relSize)*relSize)<relSize*(1-lum + _DotLumPush)/2*_DensityOfDots){
					lighter_lum = 0;
				}
				else if(distance(crd, floor((crd+relSize/2)/relSize)*relSize)<relSize*(1-lum + _DotLumPush)/2*_DensityOfDots){
					lighter_lum = 0;
				}
			}
			if(lum <= transitionPointB){
				if(distance(crd2-relSize/2, floor(crd2/relSize)*relSize)<relSize*(lum + _DotLumPush)/2*_DensityOfDots){
					darker_lum = 1;
				}
				else if(distance(crd2, floor((crd2+relSize/2)/relSize)*relSize)<relSize*(lum + _DotLumPush)/2*_DensityOfDots){
					darker_lum = 1;
				}
			}

			return lerp(darker_lum, lighter_lum, transition);
		}



		// Also by John10v10
		half4 LightingNewHalftone(SurfOutWithPos s, half3 lightDir, half atten) {
			float4 c = dotty(dot(s.Normal, lightDir * atten), s.Pos, s.Distance);

			//s.Albedo = s.Pos;// * float4(1,0,0,1);
			// c *= _DotOpacity;
			c.rgb *= s.Albedo;
			c.rgb *= _LightColor0.rgb;
			c.rgb *= atten;



			// c.rgb = lightDir.rgb * atten;
			// s.Albedo = c.rgb * atten;
			return lerp(c, half4(0,0,0,0), _DotOpacity);
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfOutWithPos o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = lerp(_Color, _WhiteColor, tex2D (_MainTex, IN.uv_MainTex).r);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			o.Distance = 1;
			//o.Distance = -(length(UnityObjectToClipPos(IN.pos)) / 50) + 1;
			//o.Albedo = o.Distance;

			//o.Albedo = o.Distance;
			o.Pos = IN.screenPos.xy*_ScreenParams.xy/IN.screenPos.w;

			//o.Albedo = o.Distance;

			// for dev purposes

			// IN.screenPos.x *= _HalftoneWidthRatio;
			// float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			// float2 rotatedScreenPos = mul(float2x2(_HalftoneRotationAmt, -_HalftoneRotationAmt, _HalftoneRotationAmt, _HalftoneRotationAmt), screenUV);
			//float stepDown = step(0.75, rotatedScreenPos);

			//#if HALFTONE_DEBUG
			//o.Albedo = tex2D(_HalftoneTex, rotatedScreenPos);
			//#endif

			// o.Albedo = dotty(dot(IN.no))

		}
		ENDCG
	}
	FallBack "Diffuse"
}
