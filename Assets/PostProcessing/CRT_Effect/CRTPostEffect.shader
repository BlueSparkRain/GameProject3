Shader "Custom/CRTPostEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(1, 100)) = 8
        _Saturation ("Saturation", Range(0, 3)) = 1.0
        _Contrast ("Contrast", Range(0, 5)) = 1.0
        _FlipUV ("Flip UV Y Axis", Int) = 1 // 内置UV翻转开关
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
            int _FlipUV; // UV翻转开关

            // 顶点着色器：内置UV翻转逻辑（不依赖Blit矩阵）
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // 仅在_FlipUV=1时翻转UV（适配主相机）
                if (_FlipUV == 1)
                {
                    o.uv.y = 1.0 - o.uv.y;
                }
                return o;
            }

            // 像素化处理
            float2 PixelateUV(float2 uv, float pixelSize)
            {
                float2 texelSize = _MainTex_TexelSize.xy * pixelSize;
                return floor(uv / texelSize) * texelSize;
            }

            // 饱和度/对比度调整
            float3 AdjustColor(float3 color, float saturation, float contrast)
            {
                color = (color - 0.5) * contrast + 0.5;
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                color = lerp(float3(luminance, luminance, luminance), color, saturation);
                return color;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 pixelUV = PixelateUV(i.uv, _PixelSize);
                half4 col = tex2D(_MainTex, pixelUV);
                col.rgb = AdjustColor(col.rgb, _Saturation, _Contrast);
                return col;
            }
            ENDHLSL
        }
    }
}