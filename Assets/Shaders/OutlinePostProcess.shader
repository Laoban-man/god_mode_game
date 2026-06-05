// OutlinePostProcess.shader — screen-space black edge detection for the
// "pastel with black edges" look. Sobel-style edge detection on depth + normals
// (via a replacement-rendered normal buffer or reconstructed depth). Attach via
// OutlineEffect.cs as an OnRenderImage post effect (Built-in pipeline).
//
// This is the stylized black outline that wraps cells and the planet silhouette.
Shader "DivineDrift/OutlinePostProcess"
{
    Properties
    {
        _MainTex     ("Source", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _DepthThreshold ("Depth Threshold", Range(0,1)) = 0.02
        _NormalThreshold("Normal Threshold", Range(0,1)) = 0.4
        _Thickness   ("Thickness (px)", Range(0,3)) = 1.0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraDepthNormalsTexture;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float  _DepthThreshold;
            float  _NormalThreshold;
            float  _Thickness;

            void SampleDN(float2 uv, out float depth, out float3 normal)
            {
                float4 dn = tex2D(_CameraDepthNormalsTexture, uv);
                DecodeDepthNormal(dn, depth, normal);
            }

            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 px = _MainTex_TexelSize.xy * _Thickness;

                float dC; float3 nC; SampleDN(i.uv, dC, nC);
                float dR; float3 nR; SampleDN(i.uv + float2(px.x, 0), dR, nR);
                float dU; float3 nU; SampleDN(i.uv + float2(0, px.y), dU, nU);
                float dRU;float3 nRU;SampleDN(i.uv + px, dRU, nRU);

                // Depth edges.
                float depthEdge = abs(dC - dR) + abs(dC - dU) + abs(dC - dRU);
                // Normal edges.
                float normalEdge = (1 - dot(nC, nR)) + (1 - dot(nC, nU)) + (1 - dot(nC, nRU));

                float edge = step(_DepthThreshold, depthEdge) +
                             step(_NormalThreshold, normalEdge);
                edge = saturate(edge);

                fixed4 src = tex2D(_MainTex, i.uv);
                return lerp(src, _OutlineColor, edge);
            }
            ENDCG
        }
    }
    FallBack Off
}
