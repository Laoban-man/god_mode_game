// PastelToon.shader — Built-in Render Pipeline toon shading for the planet cells.
// Flat, banded pastel lighting with vertex colors used for per-cell tint
// (terrain base color blended with owning population's banner color).
// No clouds, no specular, no realism — soft stepped diffuse only.
Shader "DivineDrift/PastelToon"
{
    Properties
    {
        _Color        ("Tint", Color) = (1,1,1,1)
        _RampSteps    ("Toon Steps", Range(1,5)) = 3
        _AmbientFloor ("Ambient Floor", Range(0,1)) = 0.55
        _Desaturate   ("Pastel Desaturate", Range(0,1)) = 0.25
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            float4 _Color;
            float  _RampSteps;
            float  _AmbientFloor;
            float  _Desaturate;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color  : COLOR;   // per-cell tint baked into the mesh
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float3 normal : TEXCOORD0;
                float4 color  : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.color = v.color;
                return o;
            }

            // Pull a color toward gray to give it the soft pastel feel.
            float3 Pastelize(float3 c, float amount)
            {
                float gray = dot(c, float3(0.299, 0.587, 0.114));
                return lerp(c, float3(gray, gray, gray) * 0.5 + c * 0.5, amount);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.normal);
                float ndl = saturate(dot(n, normalize(_WorldSpaceLightPos0.xyz)));

                // Banded (toon) diffuse.
                float steps = max(1.0, _RampSteps);
                float banded = floor(ndl * steps) / steps;
                float light = lerp(_AmbientFloor, 1.0, banded);

                float3 baseCol = i.color.rgb * _Color.rgb;
                baseCol = Pastelize(baseCol, _Desaturate);

                float3 lit = baseCol * light * _LightColor0.rgb;
                lit += baseCol * _AmbientFloor * 0.5; // soft fill so shadow side stays pastel
                return fixed4(lit, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
