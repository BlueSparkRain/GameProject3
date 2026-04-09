Shader "Unlit/TwoColorOutline"
{
 Properties
    {
        _OutlineWidth ("描边宽度", Range(0.002, 0.08)) = 0.03
        [HDR]_Color1 ("描边颜色A", Color) = (1,0,0,1)
        [HDR]_Color2 ("描边颜色B", Color) = (1,1,0,1)
        _Lerp ("颜色混合", Range(0,1)) = 0.5

        _LerpIntensity("水平插值强度",Range(-5,5))=1
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

                float _LerpIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv:TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float2 uv:TEXCOORD0;
            };

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.uv=input.uv;
                float3 smoothNormal = normalize(input.normalOS); // 平滑法线，唯一改动
                float3 pos = input.positionOS.xyz + smoothNormal * _OutlineWidth;
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);
                output.positionHCS = TransformObjectToHClip(pos);
                return output;
            }


            half4 frag (Varyings i) : SV_Target
            {
      
                return lerp(_Color1, _Color2,  TransformObjectToWorld( i.normalWS).y *_LerpIntensity );
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}