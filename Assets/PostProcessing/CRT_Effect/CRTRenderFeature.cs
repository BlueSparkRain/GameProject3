using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CRTPostEffectSettings
{
    [Header("CRT效果参数")]
    [Range(1, 100)] public int pixelSize = 8;
    [Range(0, 3)] public float saturation = 1.0f;
    [Range(0, 5)] public float contrast = 1.0f;
    public Shader crtShader;

    [Header("主相机识别（适配相机堆栈）")]
    public string mainCameraTag = "MainCamera";
    public bool flipMainCameraUV = true;            // UV翻转开关（面板可配）

    [Header("老电影复古老化效果")]
    [Range(0, 1)] public float filmEnable = 0;
    [Range(0.5f, 10)] public float flickerSpeed = 2.5f;
    [Range(0, 0.6f)] public float flickerPower = 0.2f;
    [Range(50, 300)] public float noiseDensity = 100; // 降低默认密度
    [Range(0, 0.25f)] public float noisePower = 0.1f;
    [Range(0.1f, 0.5f)] public float edgeRange = 0.3f;     // 边缘范围
    [Range(0.1f, 5f)] public float lineCurve = 0.8f;       // 线条弯曲度
    [Range(0.01f, 1f)] public float lineLength = 0.1f;   // 线条长度
}

public class CRTRenderFeature : ScriptableRendererFeature
{
    private class CRTPostProcessPass : ScriptableRenderPass
    {
        private CRTPostEffectSettings settings;
        private Material crtMaterial;
        private RenderTargetHandle tempRT;
        private string profilerTag = "CRTPostProcess";

        public CRTPostProcessPass(CRTPostEffectSettings settings)
        {
            this.settings = settings;
            // 初始化材质（增加空值检查）
            if (settings.crtShader != null && settings.crtShader.isSupported)
            {
                crtMaterial = CoreUtils.CreateEngineMaterial(settings.crtShader);
            }
            else
            {
                Debug.LogError("[CRT] 着色器无效或当前平台不支持！");
            }
            tempRT.Init("_TempCRTRenderTexture");
        }

        // 释放材质资源
        public void ReleaseResources()
        {
            CoreUtils.Destroy(crtMaterial);
            crtMaterial = null;
        }

        // 核心执行逻辑：仅在Execute中访问cameraColorTarget
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 多层安全检查：材质/后处理开关/主相机
            if (crtMaterial == null || !renderingData.cameraData.postProcessEnabled) return;

            Camera currentCam = renderingData.cameraData.camera;
            // 排除预览相机/非主相机
            if (currentCam.tag != settings.mainCameraTag || renderingData.cameraData.isPreviewCamera) return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            RenderTextureDescriptor rtDesc = renderingData.cameraData.cameraTargetDescriptor;
            rtDesc.depthBufferBits = 0; // 后处理不需要深度

            try
            {
                // ====================== 原有CRT参数传递（完全不变） ======================
                crtMaterial.SetFloat("_PixelSize", Mathf.Max(1, settings.pixelSize));
                crtMaterial.SetFloat("_Saturation", Mathf.Clamp(settings.saturation, 0, 3));
                crtMaterial.SetFloat("_Contrast", Mathf.Clamp(settings.contrast, 0, 5));
                crtMaterial.SetInt("_FlipUV", settings.flipMainCameraUV ? 1 : 0);

                // ====================== 新增：老电影效果参数传递 ======================
                crtMaterial.SetFloat("_FilmEnable", Mathf.Clamp01(settings.filmEnable));
                crtMaterial.SetFloat("_FlickerSpeed", Mathf.Clamp(settings.flickerSpeed, 0.5f, 10));
                crtMaterial.SetFloat("_FlickerPower", Mathf.Clamp01(settings.flickerPower));
                crtMaterial.SetFloat("_NoiseDensity", Mathf.Clamp(settings.noiseDensity, 200, 1000));
                crtMaterial.SetFloat("_NoisePower", Mathf.Clamp01(settings.noisePower));
                crtMaterial.SetFloat("_EdgeRange", Mathf.Clamp(settings.edgeRange, 0.1f, 0.5f));
                crtMaterial.SetFloat("_LineCurve", Mathf.Clamp(settings.lineCurve, 0.1f, 2f));
                crtMaterial.SetFloat("_LineLength", Mathf.Clamp(settings.lineLength, 0.01f, 0.3f)); 

                // 关键修复：仅在Execute中访问cameraColorTarget（此时生命周期合法）
                RenderTargetIdentifier mainTarget = renderingData.cameraData.renderer.cameraColorTarget;

                // 修复Blit流程：避免直接覆盖主目标
                cmd.GetTemporaryRT(tempRT.id, rtDesc, FilterMode.Bilinear);
                // 第一步：源 -> 临时RT（应用CRT+老电影效果）
                Blit(cmd, mainTarget, tempRT.Identifier(), crtMaterial, 0);
                // 第二步：临时RT -> 主目标（回写）
                Blit(cmd, tempRT.Identifier(), mainTarget);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CRT] 执行失败：{e.Message}");
            }
            finally
            {
                // 确保命令执行和释放
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        // 释放临时纹理（修复空值检查）
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd != null)
            {
                cmd.ReleaseTemporaryRT(tempRT.id);
            }
        }
    }

    [SerializeField] private CRTPostEffectSettings settings = new CRTPostEffectSettings();
    private CRTPostProcessPass crtPass;

    // 初始化：修复渲染时机
    public override void Create()
    {
        crtPass = new CRTPostProcessPass(settings);
        // 关键修复：后处理必须在所有渲染+内置后处理完成后执行
        crtPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // 入队Pass：移除直接访问cameraColorTarget的逻辑
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.crtShader == null || crtPass == null || !renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        // 仅主相机执行（此处只做过滤，不访问渲染目标）
        if (renderingData.cameraData.camera.tag != settings.mainCameraTag || renderingData.cameraData.isPreviewCamera)
        {
            return;
        }

        // 仅入队Pass，不在此阶段访问cameraColorTarget
        renderer.EnqueuePass(crtPass);
    }

    // 释放资源（增加空值检查）
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && crtPass != null)
        {
            crtPass.ReleaseResources();
            crtPass = null;
        }
    }
}