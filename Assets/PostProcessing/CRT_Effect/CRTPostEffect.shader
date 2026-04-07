Shader "Custom/CRTPostEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(1, 100)) = 8
        _Saturation ("Saturation", Range(0, 3)) = 1.0
        _Contrast ("Contrast", Range(0, 5)) = 1.0
        _FlipUV ("Flip UV Y Axis", Int) = 1

        _FilmEnable ("Enable Old Film Effect", Range(0,1)) = 0
        _FlickerSpeed ("Flicker Speed", Range(0.5, 10)) = 2.5
        _FlickerPower ("Flicker Brightness", Range(0, 0.6)) = 0.2
        _NoiseDensity ("Noise Line Density", Range(50, 300)) = 120
        _NoisePower ("Film Noise Strength", Range(0, 0.25)) = 0.2
        _EdgeRange ("Screen Edge Range", Range(0.1, 0.5)) = 0.25 // 边缘范围
        _LineCurve ("Line Curvature", Range(0.1, 5)) = 1.2 // 弯曲度
        _LineLength ("Line Length", Range(0.01, 1)) = 0.1 // 线条长度
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

            // 老电影参数
            float _FilmEnable;
            float _FlickerSpeed;
            float _FlickerPower;
            float _NoiseDensity;
            float _NoisePower;
            float _EdgeRange;
            float _LineCurve;
            float _LineLength; // ✅ 线条长度

            // 随机函数
            float Random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453 + _Time.y);
            }

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
                // 原版CRT逻辑（完全不变）
                float2 pixelUV = PixelateUV(i.uv, _PixelSize);
                half4 col = tex2D(_MainTex, pixelUV);
                col.rgb = AdjustColor(col.rgb, _Saturation, _Contrast);

                if(_FilmEnable > 0.5)
                {
                    // 1. 屏闪效果
                    float flicker = sin(_Time.y * _FlickerSpeed) * 0.4 + 0.6;
                    float randomFlicker = Random(i.uv) * 0.15;
                    col.rgb *= 1.0 - (flicker + randomFlicker) * _FlickerPower;

                    // 2. ✅ EdgeRange 生效：控制线条边缘范围
                    float dist = length(i.uv - 0.5);
                    float edgeMask = smoothstep(_EdgeRange, _EdgeRange + 0.2, dist);

                    // 3. ✅ 核心：真正的线条（非点状）+ 长度可控
                    float timeSeed = _Time.y * 6.0;
                    float randDir = Random(i.uv + timeSeed);
                    float uvCoord = randDir > 0.5 ? i.uv.x : i.uv.y;
                    // ✅ LineCurve 生效：线条弯曲度
                    float wave = sin(uvCoord * 25 * _LineCurve + Random(i.uv * 10)) * 0.04;
                    
                    // ✅ LineLength 生效：控制线条长短（核心修复！）
                    float randomUV = (uvCoord + wave) * _NoiseDensity;
                    float lineShape = frac(randomUV);
                    float scratch = smoothstep(_LineLength, _LineLength + 0.02, lineShape);
                    scratch = 1 - scratch; // 反转成长条
                    scratch = saturate(scratch * 1.3);

                    // 4. 噪点合成
                    float grain = Random(i.uv * 12 + timeSeed) * 0.3;
                    float finalNoise = (scratch * 0.8 + grain * 0.2) * _NoisePower * edgeMask;
                    col.rgb = saturate(col.rgb - finalNoise);
                }

                return col;
            }
            ENDHLSL
        }
    }
}