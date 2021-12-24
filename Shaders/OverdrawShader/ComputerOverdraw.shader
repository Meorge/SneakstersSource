Shader "Overdraw" {
Properties {
    _MainTex ("Base", 2D) = "white" {}
    _CompOverdrawColor ("Main Color", Color) = (0,0,0.01,0.2)
	[HDR] _InteractableColor ("Interactable color", Color) = (1,1,1,1)
	_AlphaAmt ("Alpha", Range(0,1)) = 1
	_UnlitAmt ("Amount of unlitness", Range(0,1)) = 1

}
 
SubShader {
    Fog { Mode Off }
	Tags {"RenderType"="Transparent" "Queue"="Transparent" "CustomType"="Environment"}
    ZWrite On
    ZTest Always
    //Blend One One // additive blending
	Blend OneMinusDstColor One

	Pass {
		Blend OneMinusDstColor One
		Cull Off
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
			float2 normal : NORMAL;
			float4 vertex : SV_POSITION;
			float3 viewDir : TEXCOORD3;
		};

		v2f vert (appdata_base v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.normal = v.normal;
			o.viewDir = normalize(UnityWorldSpaceViewDir(o.vertex));
			o.uv = v.texcoord;
			return o;
		}

		float4 _CompOverdrawColor;
		float4 _InteractableColor;
		float _AlphaAmt;
		float _UnlitAmt;

		fixed4 frag (v2f i) : SV_Target
		{
			fixed4 newcol_Dot = _InteractableColor * ((0.5-dot(i.normal, i.viewDir)) * _AlphaAmt);
			fixed4 newcol_Unlit = _InteractableColor * _AlphaAmt;
			//newcol_Unlit.a = _AlphaAmt;

			fixed4 outVal = lerp(newcol_Dot, newcol_Unlit, _UnlitAmt);
			return outVal;
		}

		ENDCG
	}
 
    // Pass {
    //     SetTexture[_MainTex] {
    //         constantColor [_Color]
    //         combine constant, constant
    //     }
    // }
}
}