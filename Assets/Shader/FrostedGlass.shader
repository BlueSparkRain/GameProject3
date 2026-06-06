Shader "Custom/FrostedGlass"{
    Properties{
        _BaseColor ("Base Color", Color) = (1, 1, 1, 0.25)
        _BlurStrength ("Blur Strength", Range(0, 5)) = 1.5
        _FresnelPower ("Fresnel Power", Range(0.2, 8)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 1)) = 0.35
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Distortion ("Distortion", Range(0, 1)) = 0.25
    }
    SubShader{
        Tags{
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        Pass{
            Name "FrostedGlass"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _BlurStrength;
                float _FresnelPower;
                float _FresnelIntensity;
                float _Distortion;
            CBUFFER_END

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs posInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInput.positionCS;
                output.screenPos = ComputeScreenPos(output.positionCS);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(posInput.positionWS);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), 1.0);
                float2 distortion = normalTS.xy * _Distortion * 0.06;
                float2 uv = screenUV + distortion;

                float2 texelSize = _ScreenParams.zw;
                float blurRadius = _BlurStrength * 20.0;

                half4 acc = (half4)0;
                float d = blurRadius * texelSize.x;

                acc += half4(SampleSceneColor(uv + float2( d,  d)), 1);
                acc += half4(SampleSceneColor(uv + float2( 0,  d)), 1);
                acc += half4(SampleSceneColor(uv + float2(-d,  d)), 1);
                acc += half4(SampleSceneColor(uv + float2(-d,  0)), 1);
                acc += half4(SampleSceneColor(uv + float2( 0,  0)), 1);
                acc += half4(SampleSceneColor(uv + float2( d,  0)), 1);
                acc += half4(SampleSceneColor(uv + float2(-d, -d)), 1);
                acc += half4(SampleSceneColor(uv + float2( 0, -d)), 1);
                acc += half4(SampleSceneColor(uv + float2( d, -d)), 1);

                float d2 = blurRadius * 1.5 * texelSize.x;
                acc += half4(SampleSceneColor(uv + float2( d2,  d2)), 1);
                acc += half4(SampleSceneColor(uv + float2( d2, -d2)), 1);
                acc += half4(SampleSceneColor(uv + float2(-d2,  d2)), 1);
                acc += half4(SampleSceneColor(uv + float2(-d2, -d2)), 1);
                acc += half4(SampleSceneColor(uv + float2( 0,  d2 * 0.7)), 1);
                acc += half4(SampleSceneColor(uv + float2( 0, -d2 * 0.7)), 1);
                acc += half4(SampleSceneColor(uv + float2( d2 * 0.7,  0)), 1);
                acc += half4(SampleSceneColor(uv + float2(-d2 * 0.7,  0)), 1);

                half4 blurred = acc / 17.0;

                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);
                float NdotV = saturate(dot(N, V));
                float fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelIntensity;

                half3 color = blurred.rgb;
                color = lerp(color, _BaseColor.rgb, _BaseColor.a * 0.6);
                color += fresnel * _BaseColor.rgb * 0.5;

                half alpha = saturate(_BaseColor.a + fresnel * 0.35);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
