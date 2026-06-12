Shader "Unlit/MeltingChocolate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gloss ("Gloss", float) = 12
        _SpecIntensity ("Spec Intensity", float) = 0.5

        _FlowSpeed ("Flow Speed", float) = 0.15
        _MeltStrength ("Melt Strength", float) = 0.05
        _MeltFrequency ("Melt Frequency", float) = 8.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 world : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Gloss;
            float _SpecIntensity;
            float _FlowSpeed;
            float _MeltStrength;
            float _MeltFrequency;

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 uv = worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zw;

                uv.y -= _Time.y * _FlowSpeed * 0.5;

                uv.x += sin(uv.y * _MeltFrequency * 0.5 + _Time.y) * _MeltStrength * 0.5;

                o.uv = uv;

                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - o.world);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 texColor = tex2D(_MainTex, i.uv).rgb;

                float3 normal = float3(0, 1, 0);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(i.viewDir);

                float dotl = max(0.4, dot(normal, lightDir));

                float3 ambient = 0.3 * texColor;
                float3 lightColor = dotl * _LightColor0 + ambient;

                float3 reflected = reflect(-lightDir, normal);
                float spec = pow(max(0, dot(reflected, viewDir)), _Gloss);
                float3 specular = spec * _SpecIntensity * _LightColor0;

                float3 finalColor = texColor * lightColor + specular;

                return fixed4(finalColor, 0.8);
            }

            ENDCG
        }
    }
}