using UnityEngine;

/// <summary>
/// 挂到需要应用 CRT 后处理的相机上，每个相机独立配置参数。
/// RenderFeature 执行时自动从此组件读取并应用。
/// </summary>
[RequireComponent(typeof(Camera))]
public class CRTCameraSettings : MonoBehaviour
{
    [Header("像素化")]
    [Range(1, 100)] public int pixelSize = 8;

    [Header("色彩调整")]
    [Range(0, 3)] public float saturation = 1f;
    [Range(0, 5)] public float contrast = 1f;

    [Header("边缘线")]
    public Color edgeColor = Color.black;
    [Range(0, 0.5f)] public float edgeThickness = 0.1f;
    [Range(0, 1)] public float edgeStrength = 1f;
    [Range(0, 2)] public float edgeGradient = 0.5f;

    [Header("UV")]
    [HideInInspector]
    [Tooltip("翻转 UV Y 轴（通常主相机需要勾选）")]
    public bool flipUV = false;
}
