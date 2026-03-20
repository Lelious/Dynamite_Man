Shader "Unlit/SimpleFreshnelGlow"
{
    Properties
    {    
        [HDR]_CenterColor ("Center Color", Color) = (1,1,1,1)
        [HDR]_EdgeColor   ("Edge Color", Color)   = (0,0,0,1)
        _AlphaCenter ("Alpha Center", Range(0,1)) = 1.0
        _AlphaEdge   ("Alpha Edge", Range(0,1)) = 0.0
        _EdgeSharpness ("Edge Sharpness", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 50

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _CenterColor;
            fixed4 _EdgeColor;
            float _AlphaCenter;
            float _AlphaEdge;
            float _EdgeSharpness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float3 safe_normalize(float3 v)
            {
                float len = length(v);
                return (len > 1e-6) ? v / len : float3(0,0,1);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = safe_normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 normal  = safe_normalize(i.normalWS);

                float d = saturate(dot(viewDir, normal)); // 1 в центре, 0 на краях

                // плавный контроль резкости:
                // если sharpness=0 → просто d
                // если sharpness=1 → используем d^3 для резких краёв
                float soft = d;
                float sharp = d * d * d;
                float factor = lerp(soft, sharp, _EdgeSharpness);

                fixed3 col = lerp(_EdgeColor.rgb, _CenterColor.rgb, factor);
                float alpha = lerp(_AlphaEdge, _AlphaCenter, factor);

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
