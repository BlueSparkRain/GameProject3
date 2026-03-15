Shader "Custom/2DToonHexPrismShader"
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
        _BlindRotation ("Blind Line Rotation (Deg)", Range(0, 360)) = 0 // 线条旋转角度
        
        // 风格化参数
        _StepCount ("Shading Step Count", Range(2, 8)) = 3 // 明暗阶梯数
        _OutlinePower ("Outline Power", Range(0, 0.1)) = 0.02 // 轮廓强度
        
        // 渐变配置（兼容原需求）
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
            "DisableBatching"="False" // 保留批处理
        }

        LOD 100
        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            // 属性变量
            fixed4 _MainColor;
            fixed4 _ShadowColor;
            half _BlindCount;
            half _BlindThickness;
            half _BlindIntensity;
            half _BlindRotation; // 百叶窗旋转角度
            half _StepCount;
            half _OutlinePower;
            half _GradientType;
            half _GradientIntensity;
            float2 _GradientCenter;

            // 顶点输入
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 顶点输出
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewNormal : TEXCOORD0; // 视角空间法线
                float2 screenUV : TEXCOORD1;   // 屏幕UV（渐变/百叶窗）
                float3 worldPos : TEXCOORD2;   // 世界坐标（轮廓计算）
                UNITY_FOG_COORDS(3)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // 顶点着色器（轻量化，保证批处理）
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 视角空间法线（平滑计算，无阈值）
                o.viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                
                // 屏幕UV（0-1范围）
                o.screenUV = o.pos.xy / o.pos.w;
                o.screenUV = (o.screenUV + 1) * 0.5;
                
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }

            // 片元着色器（核心风格化逻辑）
            fixed4 frag (v2f i) : SV_Target
            {
                // ========== 1. 基础明暗计算（适配六棱柱，无全黑） ==========
                // 视角法线Z分量（0~1），平滑映射，无阈值裁切
                half normalDot = saturate(i.viewNormal.z);
                
                // 阶梯化处理（动画风格明暗块）
                half stepValue = floor(normalDot * _StepCount) / (_StepCount - 0.01);
                half shadeFactor = stepValue;

                // ========== 2. 百叶窗阴影生成（支持旋转） ==========
                // 计算旋转后的UV（绕屏幕中心旋转）
                half rad = _BlindRotation * UNITY_PI / 180; // 角度转弧度
                half2x2 rotMatrix = half2x2(cos(rad), -sin(rad), sin(rad), cos(rad)); // 旋转矩阵
                half2 rotatedUV = mul(rotMatrix, i.screenUV - 0.5) + 0.5; // 绕中心(0.5,0.5)旋转
                
                // 基于旋转后的UV生成等间距线条（适配六棱柱的面）
                half blindUV = _GradientType < 1 ? rotatedUV.y : rotatedUV.x; // 横竖切换
                half blindLine = sin(blindUV * _BlindCount * UNITY_PI * 2);
                blindLine = smoothstep(0, _BlindThickness, abs(blindLine));
                blindLine = 1 - (1 - blindLine) * _BlindIntensity;
                
                // 仅在阴影区域应用百叶窗
                half blindFactor = lerp(1, blindLine, 1 - shadeFactor);
                shadeFactor *= blindFactor;

                // ========== 3. 渐变叠加（优化版） ==========
                half gradientFactor = 0;
                if (_GradientType < 0.5) // 径向渐变
                {
                    gradientFactor = length(i.screenUV - _GradientCenter) * _GradientIntensity;
                }
                else if (_GradientType < 1.5) // 水平渐变
                {
                    gradientFactor = i.screenUV.x * _GradientIntensity;
                }
                else // 竖直渐变
                {
                    gradientFactor = i.screenUV.y * _GradientIntensity;
                }
                gradientFactor = saturate(gradientFactor);
                
                // 混合主色和阴影色，并叠加渐变
                fixed4 baseColor = lerp(_MainColor, _ShadowColor, 1 - shadeFactor);
                baseColor = lerp(baseColor, baseColor * (1 - gradientFactor), gradientFactor);

                // ========== 4. 简易轮廓（强化2D风格） ==========
                // 基于世界坐标法线的边缘检测（性能友好）
                half3 worldNormal = normalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
                half outline = saturate(dot(worldNormal, normalize(_WorldSpaceCameraPos - i.worldPos)));
                outline = 1 - smoothstep(1 - _OutlinePower, 1, outline);
                baseColor = lerp(fixed4(0,0,0,1), baseColor, outline);

                // ========== 5. 最终处理 ==========
                UNITY_APPLY_FOG(i.fogCoord, baseColor);
                baseColor.a = 1; // 强制不透明
                
                return baseColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"

}