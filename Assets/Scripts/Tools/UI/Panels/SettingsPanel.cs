using Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置面板——画面(Windows)、音频、其他。
/// 支持 ESC 呼出/关闭，动画期间不响应重复操作。
/// </summary>
public class SettingsPanel : UIPanelBase{
    #region 显示设置
    [Header("显示")]
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    GameObject _displaySection;
    #endregion
    #region 音频设置
    [Header("音频")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    #endregion
    #region 其他
    [Header("其他")]
    public Toggle fpsToggle;
    #endregion
    [Header("按钮")]
    public Button closeButton;
    public Button exitButton;

    const string KEY_BGM = "Settings_BGM";
    const string KEY_SFX = "Settings_SFX";
    const string KEY_FULLSCREEN = "Settings_Fullscreen";
    const string KEY_VSYNC = "Settings_VSync";
    const string KEY_FPS = "Settings_FPS";

    /// <summary>
    /// 绑定到任意按钮：点击自动呼出/关闭 SettingsPanel。
    /// 用法：button.onClick.AddListener(SettingsPanel.Toggle);
    /// </summary>
    public static void Toggle()
    {
        var mgr = GameRoot.GetManager<UIManager>();
        var panel = mgr.GetPanel<SettingsPanel>(E_UIPanelType.SettingsPanel);
        if (panel != null && panel.gameObject.activeSelf)
        {
            if (panel.canOpen)
                panel.Hide();
            return;
        }
        // 防止 stuck 状态
        if (panel != null)
            panel.canOpen = true;
        mgr.OpenPanel<SettingsPanel>(E_UIPanelType.SettingsPanel);
    }

    protected override void OnInit()
    {
        base.OnInit();

        // 显示区仅 Windows 平台可见
        _displaySection = fullscreenToggle?.transform.parent?.gameObject;
        if (_displaySection != null)
        {
            bool isWindows = Application.platform == RuntimePlatform.WindowsPlayer
                          || Application.platform == RuntimePlatform.WindowsEditor;
            _displaySection.SetActive(isWindows);
        }

        LoadSettings();
        BindEvents();
    }

    void BindEvents(){
        if (closeButton != null)
                closeButton.onClick.AddListener(() => { if (canOpen) Hide(); });
        if (exitButton != null)
            exitButton.onClick.AddListener(() => Application.Quit());
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(_ => ApplyDisplay());
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(_ => ApplyDisplay());

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(v => ApplyBGM(v));
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(v => ApplySFX(v));

        if (fpsToggle != null)
            fpsToggle.onValueChanged.AddListener(v => ApplyFPS(v));
    }

    #region Load / Apply
    void LoadSettings()
    {
        if (_displaySection != null && _displaySection.activeSelf)
        {
            bool fs = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
            bool vs = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
            if (fullscreenToggle != null) fullscreenToggle.isOn = fs;
            if (vsyncToggle != null) vsyncToggle.isOn = vs;
            ApplyDisplay();
        }

        float bgm = PlayerPrefs.GetFloat(KEY_BGM, 0.8f);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        if (bgmSlider != null) bgmSlider.value = bgm;
        if (sfxSlider != null) sfxSlider.value = sfx;
        ApplyBGM(bgm);
        ApplySFX(sfx);

        bool fps = PlayerPrefs.GetInt(KEY_FPS, 0) == 1;
        if (fpsToggle != null) fpsToggle.isOn = fps;
        ApplyFPS(fps);
    }

    void ApplyDisplay()
    {
        if (_displaySection == null || !_displaySection.activeSelf) return;

        bool fs = fullscreenToggle != null && fullscreenToggle.isOn;
        bool vs = vsyncToggle != null && vsyncToggle.isOn;

        // 计算最适合当前显示器的 16:9 分辨率
        Resolution native = Screen.currentResolution;
        int targetW = Mathf.RoundToInt(native.height * 16f / 9f);

        int w, h;
        if (targetW <= native.width)
        {
            // 显示器 ≥16:9（16:9、21:9）→ 以高度为准
            w = targetW;
            h = native.height;
        }
        else
        {
            // 显示器比 16:9 更窄（16:10、4:3）→ 以宽度为准
            w = native.width;
            h = Mathf.RoundToInt(native.width * 9f / 16f);
        }

        // 全屏用独占模式，显卡驱动处理黑边/缩放
        Screen.SetResolution(w, h, fs ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed);
        QualitySettings.vSyncCount = vs ? 1 : 0;

        PlayerPrefs.SetInt(KEY_FULLSCREEN, fs ? 1 : 0);
        PlayerPrefs.SetInt(KEY_VSYNC, vs ? 1 : 0);
    }

    void ApplyBGM(float v)
    {
        var audio = GameRoot.GetManager<AudioManager>();
        audio?.SetBgmVolume(v);
        PlayerPrefs.SetFloat(KEY_BGM, v);
    }

    void ApplySFX(float v)
    {
        var audio = GameRoot.GetManager<AudioManager>();
        audio?.SetSfxVolume(v);
        PlayerPrefs.SetFloat(KEY_SFX, v);
    }

    void ApplyFPS(bool show)
    {
        var fps = GameRoot.GetManager<FPSManager>();
        fps?.ToggleFPS(show);
        PlayerPrefs.SetInt(KEY_FPS, show ? 1 : 0);
    }
    #endregion

    #region 动画安全
    protected override void BeforeFadeInAnimCallBack()
    {
        base.BeforeFadeInAnimCallBack();
        canOpen = false;
    }

    protected override void EnterAnimCallBack()
    {
        base.EnterAnimCallBack();
        canOpen = true;
    }

    protected override void BeforeFadeOutAnimCallBack()
    {
        base.BeforeFadeOutAnimCallBack();
        canOpen = false;
        PlayerPrefs.Save();
    }

    protected override void ExitAnimCallBack()
    {
        base.ExitAnimCallBack();
        canOpen = true;
    }
    #endregion
}
