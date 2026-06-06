Shader "Custom/HexOutline_ExternalOnly"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color", Color) = (0, 3, 5, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.02
        _NormalOffset ("Normal Offset", Range(0.0, 0.1)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        // Pass 1: 仅写入 Stencil，标记每个房间原始范围
        Pass
        {
            Name "StencilWrite"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _NormalOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }

        // Pass 2: 外轮廓（正面外扩 + 沿法线上推）
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _NormalOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                // XZ平面：从物体中心向外扩展顶点
                float3 dirXZ = normalize(float3(input.positionOS.x, 0, input.positionOS.z));
                input.positionOS.xyz += dirXZ * _OutlineWidth;
                // 沿法线向相机方向微推，确保顶视角可见
                input.positionOS.xyz += input.normalOS * _NormalOffset;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
