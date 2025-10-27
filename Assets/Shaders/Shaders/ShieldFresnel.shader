Shader "Custom/ShieldFresnel"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
        _Color ("Tint", Color) = (0.2,0.8,1,1)
        _FresnelPower ("Fresnel Power", Float) = 3.0
        _FresnelStrength ("Fresnel Strength", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; float3 worldNormal : TEXCOORD1; float3 worldPos : TEXCOORD2; };

            sampler2D _MainTex; float4 _MainTex_ST;
            float4 _Color;
            float _FresnelPower;
            float _FresnelStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fres = pow(1.0 - saturate(dot(viewDir, normalize(i.worldNormal))), _FresnelPower) * _FresnelStrength;
                fixed4 baseCol = tex2D(_MainTex, i.uv) * _Color;
                baseCol.rgb += _Color.rgb * fres;
                baseCol.a = saturate(fres * 0.8 + 0.2);
                return baseCol;
            }
            ENDCG
        }
    }
}
