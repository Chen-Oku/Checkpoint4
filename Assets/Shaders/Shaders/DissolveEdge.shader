Shader "Custom/DissolveEdge"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "white" {}
        _DissolveThreshold ("Threshold", Range(0,1)) = 0
        _EdgeColor ("Edge Color", Color) = (1,0.2,0.2,1)
        _EdgeWidth ("Edge Width", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _NoiseTex; float4 _NoiseTex_ST;
            float _DissolveThreshold;
            float4 _EdgeColor;
            float _EdgeWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv);
                fixed4 noise = tex2D(_NoiseTex, i.uv);
                float d = noise.r;
                float alpha = saturate((d - _DissolveThreshold) / _EdgeWidth);
                // edge factor
                float edge = smoothstep(_DissolveThreshold, _DissolveThreshold + _EdgeWidth, d);
                fixed4 edgeCol = lerp(_EdgeColor, albedo, edge);
                edgeCol.a = alpha;
                return edgeCol;
            }
            ENDCG
        }
    }
}
