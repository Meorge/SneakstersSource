Shader "Unlit/Hologram Image"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _TintA ("Tint A", Color) = (0,0,0,1)
        [HDR] _TintB ("Tint B", Color) = (0,0,0,1)
        _LineDensity ("Line Density", Range(0, 1000)) = 50
        _AddAmount ("Add Amount", Range(0, 1)) = 0
        _LineMoveSpeed ("Line Movement Speed", Range(0, 100)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _TintA, _TintB;
            float _LineDensity, _AddAmount;

            float _LineMoveSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Apply a tint to the thing
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a *= i.uv.y + 0.25;


                // Do some kind of hologrammy effect?
                fixed lines = saturate((floor(_LineDensity * (i.uv.y + _Time.x * _LineMoveSpeed)) % 2));

                fixed3 coloredLines = lerp(_TintA, _TintB, lines);
                // col.rgb *= lerp(_TintA, _TintB, lines);
                col.rgb *= coloredLines;
                col.rgb += coloredLines * _AddAmount;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
