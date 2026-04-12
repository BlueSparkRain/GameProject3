Shader "Custom/2DToonHexPrismShader_HLSL"
{
    Properties
    {
        // 基础配色
        _MainColor ("Main Color", Color) = (0.9,0.9,0.9,1)
        _ShadowColor ("Shadow Color", Color) = (0.3,0.3,0.3,1)
        
        // 百叶窗阴影配置
        _BlindCount ("Blind Line Count", Range(5, 150)) = 100
        _BlindThickness ("Blind Line Thickness", Range(0.01, 0.5)) = 0.2
        _BlindIntensity ("Blind Intensity", Range(0, 3)) = 0.15
        _BlindRotation ("Blind Line Rotation (Deg)", Range(0, 360)) = 0
        
        // 风格化参数
        _StepCount ("Shading Step Count", Range(2, 8)) = 3
        _OutlinePower ("Outline Power", Range(0, 0.1)) = 0.02
        
        // 渐变配置
        _GradientType ("Gradient Type (0=Radial,1=Horizontal,2=Vertical)", Float) = 0
        _GradientIntensity ("Gradient Intensity", Range(0, 1)) = 0.5
        _GradientCenter ("Gradient Center (0-1)", Vector) = (0.5,0.5,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "DisableBatching"="False"
        }

        LOD 100
        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            // URP 核心头文件
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // 🔥 修复：手动定义圆周率常量（解决 UNITY_PI 未定义）
            #define PI 3.141592653589793

            // SRP Batcher 必备常量缓冲区
            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _ShadowColor;
                half _BlindCount;
                half _BlindThickness;
                half _BlindIntensity;
                half _BlindRotation;
                half _StepCount;
                half _OutlinePower;
                half _GradientType;
                half _GradientIntensity;
                float2 _GradientCenter;
            CBUFFER_END

            // 顶点输入结构
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            // 片元输入结构
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 normalVS     : TEXCOORD0;
                float2 screenUV     : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
                // UNITY_FOG_COORDS(3)
            };

            // 顶点着色器
            Varyings vert (Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.normalVS = TransformWorldToViewNormal(normalWS);
                output.normalVS = normalize(output.normalVS);

                output.screenUV = output.positionHCS.xy / output.positionHCS.w;
                output.screenUV = (output.screenUV + 1) * 0.5;

                // UNITY_TRANSFER_FOG(output, output.positionHCS);
                return output;
            }

            // 片元着色器
            half4 frag (Varyings input) : SV_Target
            {
                // 1. 卡通明暗阶梯
                half normalDot = saturate(input.normalVS.z);
                half stepValue = floor(normalDot * _StepCount) / (_StepCount - 0.01);
                half shadeFactor = stepValue;

                // 2. 旋转百叶窗效果
                half rad = _BlindRotation * PI / 180.0; // 🔥 使用自定义 PI
                half2x2 rotMatrix = half2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
                half2 rotatedUV = mul(rotMatrix, input.screenUV - 0.5) + 0.5;
                
                half blindUV = (_GradientType < 1.0) ? rotatedUV.y : rotatedUV.x;
                half blindLine = sin(blindUV * _BlindCount * PI * 2.0); // 🔥 使用自定义 PI
                blindLine = smoothstep(0.0, _BlindThickness, abs(blindLine));
                blindLine = 1.0 - (1.0 - blindLine) * _BlindIntensity;
                
                half blindFactor = lerp(1.0, blindLine, 1.0 - shadeFactor);
                shadeFactor *= blindFactor;

                // 3. 渐变效果
                half gradientFactor = 0.0;
                if (_GradientType < 0.5)
                    gradientFactor = length(input.screenUV - _GradientCenter) * _GradientIntensity;
                else if (_GradientType < 1.5)
                    gradientFactor = input.screenUV.x * _GradientIntensity;
                else
                    gradientFactor = input.screenUV.y * _GradientIntensity;
                gradientFactor = saturate(gradientFactor);

                // 颜色混合
                half4 finalColor = lerp(_MainColor, _ShadowColor, 1.0 - shadeFactor);
                finalColor = lerp(finalColor, finalColor * (1.0 - gradientFactor), gradientFactor);

                // 4. 轮廓线
                float3 worldNormal = normalize(cross(ddx(input.positionWS), ddy(input.positionWS)));
                half viewDot = saturate(dot(worldNormal, normalize(_WorldSpaceCameraPos - input.positionWS)));
                half outline = 1.0 - smoothstep(1.0 - _OutlinePower, 1.0, viewDot);
                finalColor = lerp(half4(0,0,0,1), finalColor, outline);

                // 雾化 + 不透明
                // UNITY_APPLY_FOG(input.fogCoord, finalColor);
                finalColor.a = 1.0;
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}