using Core;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// 全局FPS性能监控管理器（修复精准版）
/// 特性：高度解耦、无依赖、全局单例、低性能消耗、右上角悬浮显示
/// 显示指标：实时FPS、平均FPS、最小/最大FPS、内存占用
/// </summary>
public class FPSManager : MonoGlobalManager
{
    [Header("FPS显示配置")]
    [Tooltip("是否开启FPS显示")]
    public bool showFPS = true;
    [Tooltip("帧率刷新间隔(秒)，越小越流畅")]
    public float refreshRate = 0.5f;
    [Tooltip("UI字体大小")]
    public int fontSize = 35;
    [Tooltip("文字颜色")]
    public Color textColor = Color.cyan;
    [Tooltip("背景透明度")]
    public float bgAlpha = 0.7f;

    // 精准帧率计算核心
    private float _accumulatedTime;
    private int _frameCount;
    private float _currentFps;

    // 统计数据
    private float _totalFpsTime;
    private int _totalFrames;
    private float _minFps;
    private float _maxFps;

    // 内存数据
    private float _usedMemoryMB;

    // UI
    private GUIStyle _uiStyle;
    //private Texture2D _bgTexture;

    #region 管理器生命周期
    public override void MgrInit(GameRoot gameRoot)
    {
        base.MgrInit(gameRoot);
        ResetAllStats();
        InitUIStyle();
        DebugManager.Log(EDebugCategory.General, "[FPSManager] 全局性能监控初始化完成");
    }

    public override void MgrUpdate(float deltaTime)
    {
        if (!showFPS) return;
        CalculateFPS(deltaTime);
        CalculateMemory();
    }

    public override void MgrDispose()
    {
        //base.MgrDispose();
        //if (_bgTexture != null) Destroy(_bgTexture);
    }
    #endregion

    #region 初始化
    private void ResetAllStats()
    {
        _accumulatedTime = 0;
        _frameCount = 0;
        _currentFps = 0;

        _totalFpsTime = 0;
        _totalFrames = 0;
        _minFps = float.MaxValue;
        _maxFps = float.MinValue;
    }

    private void InitUIStyle()
    {
        //// 背景
        //_bgTexture = new Texture2D(1, 1);
        //_bgTexture.SetPixel(0, 0, new Color(0, 0, 0, bgAlpha));
        //_bgTexture.Apply();

        // 文字样式
        _uiStyle = new GUIStyle
        {
            fontSize = fontSize,
            normal = { textColor = textColor },
            padding = new RectOffset(8, 8, 5, 5),
            alignment = TextAnchor.UpperLeft
        };
    }
    #endregion

    #region 【修复】核心精准FPS计算
    private void CalculateFPS(float deltaTime)
    {
        // 实时帧率计算
        _accumulatedTime += deltaTime;
        _frameCount++;

        // 按刷新间隔更新
        if (_accumulatedTime >= refreshRate)
        {
            _currentFps = _frameCount / _accumulatedTime;

            // 统计总数据
            _totalFpsTime += _accumulatedTime;
            _totalFrames += _frameCount;

            // 更新极值
            if (_currentFps < _minFps) _minFps = _currentFps;
            if (_currentFps > _maxFps) _maxFps = _currentFps;

            // 重置当前周期
            _accumulatedTime = 0;
            _frameCount = 0;
        }
    }

    private void CalculateMemory()
    {
        _usedMemoryMB = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
    }
    #endregion

    #region 公共接口
    public void ToggleFPS(bool isShow) => showFPS = isShow;
    public void ResetStats() => ResetAllStats();
    #endregion

    #region UI绘制
    private void OnGUI()
    {
        if (!showFPS) return;

        // 自适应右上角位置
        float width = 320;
        float height = 120;
        Rect uiRect = new Rect(Screen.width - width - 10, 10, width, height);

        //GUI.DrawTexture(uiRect, _bgTexture);

        // 平均帧率
        float avgFps = _totalFrames > 0 ? _totalFrames / _totalFpsTime : 0;

        string info = $"实时 FPS: {_currentFps:F1}\n" +
                      $"平均 FPS: {avgFps:F1}\n" +
                      $"极值: {_minFps:F1} ~ {_maxFps:F1}\n" +
                      $"内存占用: {_usedMemoryMB:F1} MB";

        GUI.Label(uiRect, info, _uiStyle);
    }
    #endregion
}