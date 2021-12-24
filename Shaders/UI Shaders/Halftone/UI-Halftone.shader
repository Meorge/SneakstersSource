Shader "Unlit/UI-Halftone"
{
    Properties
    {
        _MainTex ("Dot Texture", 2D) = "white" {}
        _TargetPos ("Target position", Vector) = (0,0,0,0)
        _Scale ("Scale", Range(0,100)) = 100
		_WidthRatio ("Width ratio", Range(0,10)) = 1
        _RotationAmt ("Other Scale Amount", Float) = 0.707

        _Offset ("Offset", Vector) = (0,0,0,0)
        _AnimateSpeed ("Animation Speed", Vector) = (0,0,0,0)


        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        // Blend SrcAlpha OneMinusSrcAlpha
        Blend DstColor Zero
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members screenPos)
#pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _TargetPos;
            float _WidthRatio;
            float _RotationAmt;
            float4 _MainTex_ST;
            float4 _Offset;
            float4 _AnimateSpeed;

            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 ratioScreenPos = i.screenPos.xy;
                ratioScreenPos.x *= _ScreenParams.x / _ScreenParams.y;

                float2 rotatedScreenPos = mul(float2x2(_RotationAmt, -_RotationAmt, _RotationAmt, _RotationAmt), ratioScreenPos);

                
                fixed4 col = tex2D(_MainTex, rotatedScreenPos + float2(_Time.y * _AnimateSpeed.x, 0) + float2(_Offset.x, _Offset.y));

                float distanceFromCenter = distance(_TargetPos.xy, ratioScreenPos);
                float threshold = smoothstep(0, _Scale, distanceFromCenter);
                
                
                col = col.r > threshold;
                // col = col * threshold;
                // col.a = 0;
                

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                col *= i.color;

                if (col.a == 0)
                    discard;





                ///////////////////////////////////////////////
                ///////////////////////////////////////////////

                // PROCEDURAL DOTS
                // float frequency = _Scale;
                // i.screenPos.x *= _WidthRatio;

                // i.screenPos.x += _Offset.x;
                // i.screenPos.y += _Offset.y;

                // float2 rotatedScreenPos = mul(float2x2(_RotationAmt, -_RotationAmt, _RotationAmt, _RotationAmt), i.screenPos.xy);
                // float2 nearest = 2.0 * frac(frequency * rotatedScreenPos) - 1.0;
                // float dist = distance(nearest, 0.0);

                // float distanceFromCenter = distance(_TargetPos.xy, i.screenPos.xy);

                // float radius = 1-(distanceFromCenter / 100);
                // float4 whiteColor = float4(1,1,1,1);
                // float4 blackColor = float4(0,0,0,1);

                // fixed4 col = lerp(whiteColor, blackColor, step(radius, dist));
                // if (col.r == 0)
                //     discard;
                // col *= _DotColor;

                return col;
            }
            ENDCG
        }
    }
}
