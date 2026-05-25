Shader "Unlit/HexMapShader"
{
    Properties
    {
        _MainTex ("大地图纹理", 2D) = "white" {}
        _MapSize ("地图总尺寸", Vector) = (100,100,0,0)
        _MapCenter ("地图中心", Vector) = (0,0,0,0)
        _HexRadius ("六边形半径", Float) = 1.0
        _Tiling ("纹理缩放", Vector) = (1,1,0,0) // 新增：缩放
        _Offset ("纹理偏移", Vector) = (0,0,0,0) // 新增：偏移
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float2 _MapSize;
            float3 _MapCenter;
            float _HexRadius;
            float2 _Tiling;  // 对应脚本参数
            float2 _Offset;  // 对应脚本参数

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offset = i.worldPos.xz - _MapCenter.xz;
                float2 uv = (offset / _MapSize) + 0.5;
                // 核心：应用缩放和偏移
                uv = uv * _Tiling + _Offset;
                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}