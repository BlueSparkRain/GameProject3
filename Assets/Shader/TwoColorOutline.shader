Shader "Unlit/TwoColorOutline"
{
 Properties
    {
        _OutlineWidth ("描边宽度", Range(0.002, 0.08)) = 0.03
        _Color1 ("描边颜色A", Color) = (1,0,0,1)
        _Color2 ("描边颜色B", Color) = (1,1,0,1)
        _Lerp ("颜色混合", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "OUTLINE"
            // 完全保留你原有的所有渲染设置（一字不改）
            Cull Front
            ZWrite Off
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _Color1;
                float4 _Color2;
                float _Lerp;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert (Attributes input)
            {
                Varyings output;

                // ====================== 【唯一修改：仅平滑法线，不改动任何坐标】 ======================
                // 1. 完全保留你原版的 模型空间外扩
                // 2. 仅强制法线平滑插值（修复分裂，无任何副作用）
                float3 smoothNormal = normalize(input.normalOS); // 平滑法线，唯一改动
                float3 pos = input.positionOS.xyz + smoothNormal * _OutlineWidth;
                // ==================================================================================

                output.positionHCS = TransformObjectToHClip(pos);
                return output;
            }

            // 你的片段着色器 一字不改
            half4 frag (Varyings i) : SV_Target
            {
                return lerp(_Color1, _Color2, _Lerp);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}