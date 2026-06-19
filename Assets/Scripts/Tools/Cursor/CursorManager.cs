using UnityEngine;

/// <summary>
/// 基于SO的高性能光标管理器（零运行时配置分配，直接读取SO数据）
/// </summary>
public class CursorManager : MonoGlobalManager{
    protected override void Awake(){
        base.Awake();
        // 核心：初始化时直接读取SO配置，零动态分配
        InitCursorSystem();

        // 禁用Update（零空轮询消耗）
        this.enabled = false;
        //LogInfo("[CursorManager]---光标管理器初始化完成");
    }
    CursorSettingsSO cursorSettings;
    #region 状态缓存（零冗余操作关键）
    CursorStyle _currentStyle;       // 当前光标样式
    CursorStyle _baseStyle;          // 基础样式（松开后恢复）
    bool _isCursorVisible;           // 显示状态
    bool _isMousePressed;            // 按下状态
    #endregion
    /// <summary>是否需要软件渲染（OnGUI）：调试模式 或 缩放≠1（硬件光标不支持缩放）</summary>
    bool UseSoftwareCursor => _showSystemCursor || Mathf.Abs(_cursorScale - 1f) > 0.001f;
    [Header("编辑器调试")]
    [Tooltip("热点偏移：叠加到 SO 的 hotSpot 上，方便实时微调（调好后复制到 SO 的 hotSpot 中保存）")]
    [SerializeField] Vector2 _hotSpotOffset = new Vector2(100, 60);
    [Tooltip("光标贴图缩放（1=原始大小），硬件光标模式下不支持缩放，非1时自动切换为软件渲染")]
    [SerializeField] float _cursorScale = 0.3f;
    [Tooltip("显示系统光标：开启后同时显示系统光标 + 自定义贴图，方便对照调整 hotSpot")]
    [SerializeField] bool _showSystemCursor = false;
    #region 初始化（仅执行一次，读取SO静态数据）
    /// <summary>
    /// 初始化光标系统（读取SO配置，构建缓存）
    /// </summary>
    void InitCursorSystem(){
        cursorSettings = Resources.Load<CursorSettingsSO>("SOData/CursorSettingsSO/CursorSettings");
        // 安全校验：SO未赋值则报错
        if (cursorSettings == null){
            Debug.LogError("[CursorManager]---SOData/CursorSettingsSO/CursorSettings下未查找到CursorSettingsSO文件！");
            return;
        }

        // 1. 初始化SO的字典缓存（零动态分配）
        cursorSettings.InitStyleDict();

        // 2. 读取SO的全局配置（直接读取，无复制）
        _isCursorVisible = cursorSettings.defaultShowCursor;
        Cursor.visible = _isCursorVisible;

        _baseStyle = cursorSettings.defaultCursorStyle;
        _currentStyle = _baseStyle;
        _isMousePressed = false;

        // 3. 应用默认光标样式（仅执行一次）
        ApplyCursorStyle(_baseStyle);
    }
    #endregion
    #region 核心逻辑（状态驱动）
    /// <summary>
    /// 应用光标样式（仅样式变化时执行）
    /// </summary>
    void ApplyCursorStyle(CursorStyle style){
        if (cursorSettings == null) return;

        // 从SO读取配置（O(1)查找）
        var config = cursorSettings.GetStyleConfig(style);

        // 容错：配置不存在则用默认样式
        if (config == null){
            //LogError($"样式{style}未配置，切换为默认样式");
            config = cursorSettings.GetStyleConfig(cursorSettings.defaultCursorStyle);

            // 极端容错：默认样式也不存在则用系统光标
            if (config == null){
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }
        }

        // 性能优化：仅样式/显示状态变化时更新
        if (_currentStyle != style || !_isCursorVisible){
            if (UseSoftwareCursor)
            {
                // 软件渲染（支持缩放）：OnGUI 绘制自定义贴图
                // 系统光标仅在 _showSystemCursor 开启时保留，否则隐藏
                if (_showSystemCursor)
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.visible = false;
                }
            }
            else
            {
                Cursor.SetCursor(
                    config.cursorTexture,
                    config.hotSpot + _hotSpotOffset,
                    CursorMode.Auto
                );
            }
            this.enabled = UseSoftwareCursor;
            _currentStyle = style;
        }
    }

    /// <summary>
    /// 更新鼠标按下状态（仅状态变化时处理）
    /// </summary>
    void UpdateMousePressedState(bool isPressed){
        if (_isMousePressed == isPressed) return;

        _isMousePressed = isPressed;
        var targetStyle = isPressed ? CursorStyle.Press : _baseStyle;
        ApplyCursorStyle(targetStyle);
    }
    #endregion
    #region 外部调用接口（保持简洁，高性能）
    /// <summary>
    /// 设置基础光标样式（松开状态）
    /// </summary>
    public void SetBaseCursorStyle(CursorStyle style){
        if (_baseStyle == style || cursorSettings == null) return;

        _baseStyle = style;
        if (!_isMousePressed) ApplyCursorStyle(style);
        //LogInfo($"设置基础样式：{style}");
    }

    /// <summary>
    /// 显示光标（仅隐藏时执行）
    /// </summary>
    public void ShowCursor(){
        if (_isCursorVisible) return;

        _isCursorVisible = true;
        Cursor.visible = true;
        ApplyCursorStyle(_isMousePressed ? CursorStyle.Press : _baseStyle);
        //LogInfo("显示光标");
    }

    /// <summary>
    /// 隐藏光标（仅显示时执行）
    /// </summary>
    public void HideCursor(){
        if (!_isCursorVisible) return;

        _isCursorVisible = false;
        Cursor.visible = false;
        //LogInfo("隐藏光标");
    }

    /// <summary>
    /// 切换光标显示/隐藏
    /// </summary>
    public void ToggleCursor(){
        if (_isCursorVisible) HideCursor();
        else ShowCursor();
    }

    /// <summary>
    /// 手动触发鼠标按下/松开
    /// </summary>
    public void TriggerMousePress(bool isPressed){
        UpdateMousePressedState(isPressed);
    }

    /// <summary>
    /// 启用自动鼠标检测（按需启用，零空轮询）
    /// </summary>
    public void EnableAutoMouseDetection(){
        this.enabled = true;
        //LogInfo("启用自动鼠标检测");
    }

    /// <summary>
    /// 禁用自动鼠标检测
    /// </summary>
    public void DisableAutoMouseDetection(){
        this.enabled = false;
        //LogInfo("禁用自动鼠标检测");
    }

    /// <summary>
    /// 重置为默认样式
    /// </summary>
    public void ResetToDefault(){
        if (cursorSettings == null) return;
        SetBaseCursorStyle(cursorSettings.defaultCursorStyle);
    }
    #endregion
    #region 辅助方法（日志+输入检测）
    void LogInfo(string msg){
        if (cursorSettings != null && cursorSettings.isDebugMode)
            DebugManager.Log(EDebugCategory.General, $"[CursorManager]---{msg}");
    }

    void LogError(string msg){
        if (cursorSettings != null && cursorSettings.isDebugMode)
            Debug.LogError($"[CursorManager]---{msg}");
    }
    #endregion
    #region 安全释放
    void OnDestroy(){
        if (cursorSettings == null) return;
        // 恢复系统默认光标
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
        //LogInfo("销毁：恢复系统默认光标");
    }

    public override void MgrUpdate(float deltaTime){
        //仅启用自动检测时执行（零空轮询）
        if (Input.GetMouseButtonDown(0)) UpdateMousePressedState(true);
        else if (Input.GetMouseButtonUp(0)) UpdateMousePressedState(false);
    }
    #endregion
    #region 软件渲染（缩放 / 双光标对照模式）
    void OnGUI(){
        if (!UseSoftwareCursor || cursorSettings == null) return;
        var config = cursorSettings.GetStyleConfig(_isMousePressed ? CursorStyle.Press : _baseStyle);
        if (config?.cursorTexture == null) return;
        Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // 鼠标不在 Game 窗口内时，恢复系统光标并跳过绘制
        bool insideWindow = mousePos.x > 0 && mousePos.x < Screen.width &&
                            mousePos.y > 0 && mousePos.y < Screen.height;
        Cursor.visible = _showSystemCursor || !insideWindow;

        if (!insideWindow) return;

        Vector2 offset = (config.hotSpot + _hotSpotOffset) * _cursorScale;
        var tex = config.cursorTexture;
        float w = tex.width * _cursorScale;
        float h = tex.height * _cursorScale;
        GUI.DrawTexture(
            new Rect(mousePos.x - offset.x, Screen.height - mousePos.y - offset.y, w, h),
            tex
        );
    }
    #endregion
}