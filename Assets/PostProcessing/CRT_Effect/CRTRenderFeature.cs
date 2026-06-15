using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#pragma warning disable CS0618

[System.Serializable]
public class CRTPostEffectSettings
{
    [Header("着色器")]
    public Shader crtShader;
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
            if (settings.crtShader != null && settings.crtShader.isSupported)
                crtMaterial = CoreUtils.CreateEngineMaterial(settings.crtShader);
            else
                Debug.LogError("[CRT] 着色器无效或当前平台不支持！");
            tempRT.Init("_TempCRTRenderTexture");
        }

        public void ReleaseResources()
        {
            CoreUtils.Destroy(crtMaterial);
            crtMaterial = null;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (crtMaterial == null) return;

            Camera currentCam = renderingData.cameraData.camera;
            if (renderingData.cameraData.isPreviewCamera) return;

            // 从当前相机读取独立 CRT 配置
            var camSettings = currentCam.GetComponent<CRTCameraSettings>();
            if (camSettings == null) return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            RenderTextureDescriptor rtDesc = renderingData.cameraData.cameraTargetDescriptor;
            rtDesc.depthBufferBits = 0;

            try
            {
                // 每相机独立参数
                crtMaterial.SetFloat("_PixelSize",  Mathf.Max(1, camSettings.pixelSize));
                crtMaterial.SetFloat("_Saturation", Mathf.Clamp(camSettings.saturation, 0, 3));
                crtMaterial.SetFloat("_Contrast",   Mathf.Clamp(camSettings.contrast, 0, 5));
                crtMaterial.SetColor("_EdgeColor",      camSettings.edgeColor);
                crtMaterial.SetFloat("_EdgeThickness",   Mathf.Clamp(camSettings.edgeThickness, 0, 0.5f));
                crtMaterial.SetFloat("_EdgeStrength",    Mathf.Clamp01(camSettings.edgeStrength));
                crtMaterial.SetFloat("_EdgeGradient",    Mathf.Clamp(camSettings.edgeGradient, 0, 2));
                crtMaterial.SetInt("_FlipUV",       camSettings.flipUV ? 1 : 0);

                RenderTargetIdentifier mainTarget = renderingData.cameraData.renderer.cameraColorTarget;

                cmd.GetTemporaryRT(tempRT.id, rtDesc, FilterMode.Bilinear);
                Blit(cmd, mainTarget, tempRT.Identifier(), crtMaterial, 0);
                Blit(cmd, tempRT.Identifier(), mainTarget);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CRT] 执行失败：{e.Message}");
            }
            finally
            {
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd != null)
                cmd.ReleaseTemporaryRT(tempRT.id);
        }
    }

    [SerializeField] private CRTPostEffectSettings settings = new CRTPostEffectSettings();
    private CRTPostProcessPass crtPass;

    public override void Create()
    {
        crtPass = new CRTPostProcessPass(settings);
        // AfterRenderingTransparents 对 Base / Overlay 相机都会触发，
        // 而 AfterRenderingPostProcessing 仅在 Base 相机存在
        crtPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.crtShader == null || crtPass == null)
            return;

        if (renderingData.cameraData.isPreviewCamera) return;

        // 仅当相机挂有 CRTCameraSettings 组件时才入队
        var cam = renderingData.cameraData.camera;
        if (cam == null || cam.GetComponent<CRTCameraSettings>() == null) return;

        renderer.EnqueuePass(crtPass);
    }

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
