Shader "Custom/HexOutline_ExternalOnly"
{
    Properties
    {
        [HDR] _OutlineColorA ("Outline Color A", Color) = (0,3,5,1)
        [HDR] _OutlineColorB ("Outline Color B", Color) = (0,1,3,1)
        _OutlineWidth ("Outline Width", Range(0.0,3)) = 1
        _EdgeSharpness ("Edge Sharpness", Range(0.1,5.0)) = 1.5
        _OutlineAngle ("External Angle Threshold", Range(0,1)) = 0.9   // 越大越严格只显示最外轮廓
        _NormalOffset ("Normal Offset", Range(0, 0.1)) = 0.01

        [Header(Gradient)]
        _GradientType ("Gradient Type (0=Radial,1=Horizontal,2=Vertical)", Float) = 0
        _GradientCenter ("Gradient Center (0-1)", Vector) = (0.5,0.5,0,0)
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

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;        // 需要法线
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                float3 normalVS   : TEXCOORD1;     // 视角空间法线
            };

            struct G2F
            {
                float4 positionCS  : SV_POSITION;
                float3 barycentric : TEXCOORD0;
                float2 screenUV    : TEXCOORD1;
                float3 normalVS    : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColorA;
                float4 _OutlineColorB;
                float  _OutlineWidth;
                float  _EdgeSharpness;
                float  _OutlineAngle;
                float  _NormalOffset;
                float  _GradientType;
                float2 _GradientCenter;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                input.positionOS.xyz += input.normalOS * _NormalOffset;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                // 转换法线到视角空间（也可以世界空间，这里为减少计算用视角空间）
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.normalVS = TransformWorldToViewDir(normalWS, true);
                return output;
            }

            [maxvertexcount(3)]
            void geom(triangle Varyings input[3], inout TriangleStream<G2F> stream)
            {
                G2F o;

                o.positionCS = input[0].positionCS;
                o.barycentric = float3(1,0,0);
                o.screenUV = input[0].screenPos.xy / input[0].screenPos.w;
                o.normalVS = input[0].normalVS;
                stream.Append(o);

                o.positionCS = input[1].positionCS;
                o.barycentric = float3(0,1,0);
                o.screenUV = input[1].screenPos.xy / input[1].screenPos.w;
                o.normalVS = input[1].normalVS;
                stream.Append(o);

                o.positionCS = input[2].positionCS;
                o.barycentric = float3(0,0,1);
                o.screenUV = input[2].screenPos.xy / input[2].screenPos.w;
                o.normalVS = input[2].normalVS;
                stream.Append(o);
            }

            half4 frag(G2F input) : SV_Target
            {
                // 1. 边缘检测（同原Shader）
                float3 deltas = fwidth(input.barycentric);
                float3 edge = smoothstep(deltas * _OutlineWidth,
                                         deltas * (_OutlineWidth + _EdgeSharpness * 0.02),
                                         input.barycentric);
                float edgeFactor = 1.0 - min(edge.x, min(edge.y, edge.z));
                clip(edgeFactor - 0.001);

                // 2. 外轮廓检测：法线与视线夹角
                float3 normalVS = normalize(input.normalVS);
                float3 viewDirVS = float3(0,0,1);       // 在视角空间中，视线方向指向 +Z
                float ndotv = dot(normalVS, viewDirVS);
                float contour = saturate( (1.0 - ndotv) / (1.0 - _OutlineAngle) );
                // 当 ndotv < _OutlineAngle 时，认为处于外轮廓区域，contour > 0
                // 为了只保留最强外轮廓，可以适当提高对比度
                contour = pow(contour, 2.0);
                if (contour < 0.1) discard;   // 忽略内部区域

                // 3. 渐变因子（同原Shader）
                float gradientFactor;
                if (_GradientType < 0.5)
                    gradientFactor = length(input.screenUV - _GradientCenter) * 1.414;
                else if (_GradientType < 1.5)
                    gradientFactor = input.screenUV.x;
                else
                    gradientFactor = input.screenUV.y;
                gradientFactor = saturate(gradientFactor);

                half4 color = lerp(_OutlineColorA, _OutlineColorB, gradientFactor);
                color.a *= edgeFactor * contour;
                return color;
            }
            ENDHLSL
        }
    }    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}