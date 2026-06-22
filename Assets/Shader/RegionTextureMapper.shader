Shader "Custom/RegionTextureMapper"
{
    Properties
    {
        _MainTex ("Region Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0, 1)) = 1.0
        _RegionMin ("Region Min", Vector) = (0, 0, 0, 0)
        _RegionSize ("Region Size", Vector) = (10, 10, 0, 0)
        _Tiling ("Tiling", Vector) = (1, 1, 0, 0)
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _TransitionStartTime ("Transition Start Time", Float) = 0
        _FadeDuration ("Fade Duration", Float) = 0.5
        _FromOpacity ("From Opacity", Float) = 0
        _TargetOpacity ("Target Opacity", Float) = 1
        _MaxRandomDelay ("Max Random Delay", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "SRPBatcher" = "False"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float delay : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // 全部属性放入 UnityPerMaterial — 兼容 SRP Batcher
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Opacity;
                float _TransitionStartTime;
                float _FadeDuration;
                float _FromOpacity;
                float _TargetOpacity;
                float2 _RegionMin;
                float2 _RegionSize;
                float2 _Tiling;
                float2 _Offset;
                float _MaxRandomDelay;
            CBUFFER_END

            // 基于世界坐标的哈希，同一房间的所有顶点返回相同值
            float Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;

                float2 worldXZ = output.positionWS.xz;
                output.uv = (worldXZ - _RegionMin) / _RegionSize;
                output.uv = output.uv * _Tiling + _Offset;

                // 用顶点局部坐标反推物体 pivot 的世界 XZ
                // objPivotWS = vertexWS - (vertexOS.xz 在 XZ 平面的分量)
                // 对平放在 XZ 平面的面片，取 fragment 的世界坐标不如直接用顶点平均
                float3 objPivotWS = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1)).xyz;
                output.delay = Hash21(objPivotWS.xz) * _MaxRandomDelay;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float elapsed = _Time.y - _TransitionStartTime;
                float t = saturate((elapsed - input.delay) / _FadeDuration);
                float fade = lerp(_FromOpacity, _TargetOpacity, t);

                color.a *= _Opacity * fade;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
