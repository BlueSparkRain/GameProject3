using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂到 UI Image / RawImage 上，对该元素单独应用 CRT 像素化效果。
/// 不影响同 Canvas 下的其他 UI 元素，也不需要额外的相机。
/// </summary>
[RequireComponent(typeof(Graphic))]
public class CRTImageEffect : MonoBehaviour
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

    [Header("着色器")]
    public Shader crtUIShader;

    Material _material;
    Graphic _graphic;

    static readonly int PixelSizeId    = Shader.PropertyToID("_PixelSize");
    static readonly int SaturationId   = Shader.PropertyToID("_Saturation");
    static readonly int ContrastId     = Shader.PropertyToID("_Contrast");
    static readonly int EdgeColorId    = Shader.PropertyToID("_EdgeColor");
    static readonly int EdgeThicknessId = Shader.PropertyToID("_EdgeThickness");
    static readonly int EdgeStrengthId  = Shader.PropertyToID("_EdgeStrength");
    static readonly int EdgeGradientId  = Shader.PropertyToID("_EdgeGradient");

    void Start()
    {
        _graphic = GetComponent<Graphic>();
        if (crtUIShader != null)
        {
            _material = new Material(crtUIShader);
            _graphic.material = _material;
        }
    }

    void Update()
    {
        if (_material == null) return;
        _material.SetFloat(PixelSizeId,   Mathf.Max(1, pixelSize));
        _material.SetFloat(SaturationId,  Mathf.Clamp(saturation, 0, 3));
        _material.SetFloat(ContrastId,    Mathf.Clamp(contrast, 0, 5));
        _material.SetColor(EdgeColorId,   edgeColor);
        _material.SetFloat(EdgeThicknessId, Mathf.Clamp(edgeThickness, 0, 0.5f));
        _material.SetFloat(EdgeStrengthId,  Mathf.Clamp01(edgeStrength));
        _material.SetFloat(EdgeGradientId,  Mathf.Clamp(edgeGradient, 0, 2));
    }

    void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}
