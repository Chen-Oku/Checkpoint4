Shader "Custom/SpeedPulse"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "white" {}
        _Color ("Color", Color) = (0.2,0.6,1,1)
        _EmissionStrength ("Emission Strength", Float) = 1.0
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
        // Hidden culling property: 0=Back, 1=Front, 2=Off (matches Unity's Cull enum)
        _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            // allow material to choose culling (Back/Front/Off) via _Cull property
            Cull[_Cull]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _NoiseTex; float4 _NoiseTex_ST;
            float4 _Color;
            float _EmissionStrength;
            float _ScrollSpeed;
            float _TimeY = 0;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y * _ScrollSpeed;
                float2 nUV = i.uv + float2(t, t*0.3);
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                fixed4 noise = tex2D(_NoiseTex, nUV);
                float emiss = (sin(_Time.y * 8.0) * 0.5 + 0.5) * _EmissionStrength;
                fixed4 col = lerp(baseCol, _Color, noise.r);
                col.rgb += _Color.rgb * emiss;
                return col;
            }
            ENDCG
        }
    }
}
