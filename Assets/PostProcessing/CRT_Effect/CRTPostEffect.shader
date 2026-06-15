Shader "Custom/CRTPostEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(1, 100)) = 8
        _Saturation ("Saturation", Range(0, 3)) = 1.0
        _Contrast ("Contrast", Range(0, 5)) = 1.0
        _FlipUV ("Flip UV Y Axis", Int) = 1
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
        _EdgeThickness ("Edge Thickness", Range(0, 0.5)) = 0.1
        _EdgeStrength ("Edge Strength", Range(0, 1)) = 1
        _EdgeGradient ("Edge Gradient", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float _PixelSize;
            float _Saturation;
            float _Contrast;
            int _FlipUV;
            float4 _EdgeColor;
            float _EdgeThickness;
            float _EdgeStrength;
            float _EdgeGradient;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                if (_FlipUV == 1)
                {
                    o.uv.y = 1.0 - o.uv.y;
                }
                return o;
            }

            float2 PixelateUV(float2 uv, float pixelSize)
            {
                float2 texelSize = _MainTex_TexelSize.xy * pixelSize;
                return floor(uv / texelSize) * texelSize;
            }

            float3 AdjustColor(float3 color, float saturation, float contrast)
            {
                color = (color - 0.5) * contrast + 0.5;
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                color = lerp(float3(luminance, luminance, luminance), color, saturation);
                return color;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy;

                // 像素块边界检测（在像素化之前计算）
                float2 pixelSizeUV = texelSize * _PixelSize;
                float2 blockCenter = (floor(i.uv / pixelSizeUV) + 0.5) * pixelSizeUV;
                float2 fromCenter = (i.uv - blockCenter) / pixelSizeUV; // [-0.5, 0.5]
                float2 toEdge = 0.5 - abs(fromCenter);                   // 距最近边缘
                float minEdge = min(toEdge.x, toEdge.y);
                float edgeFactor = 1.0 - smoothstep(0, max(0.001, _EdgeThickness), minEdge);

                float2 pixelUV = PixelateUV(i.uv, _PixelSize);
                half4 col = tex2D(_MainTex, pixelUV);
                col.rgb = AdjustColor(col.rgb, _Saturation, _Contrast);

                // 径向渐变遮罩：画面中心→0，边缘→1
                float distFromCenter = length(i.uv - 0.5) * 2.0;          // 0(中心) ~ 1.4(角落)
                float radialMask = smoothstep(0.001, max(0.001, _EdgeGradient), distFromCenter);

                // 边缘线叠加（强度 × 径向渐变）
                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edgeFactor * _EdgeStrength * radialMask);
                return col;
            }
            ENDHLSL
        }
    }
}