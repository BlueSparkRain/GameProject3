using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 光标样式枚举（保持不变，按需扩展）
/// </summary>
public enum CursorStyle
{
    Default,    // 默认/松开状态
    Press,      // 按下状态
    Hover,      // 悬停状态
    Loading     // 扩展样式
}

/// <summary>
/// 单个光标样式的配置项（序列化到SO中）
/// </summary>
[System.Serializable]
public class CursorStyleConfig
{
    [Tooltip("光标样式标识")]
    public CursorStyle style;
    [Tooltip("光标纹理（建议24x24/32x32，导入类型设为Cursor）")]
    public Texture2D cursorTexture;
    [Tooltip("光标热点（点击精准位置，如指针尖端）")]
    public Vector2 hotSpot = Vector2.zero;
}

/// <summary>
/// 光标全局配置SO（核心：所有光标参数存储在此，运行时直接读取）
/// </summary>
[CreateAssetMenu(fileName = "CursorSettings", menuName = "Game/Cursor Settings", order = 1)]
public class CursorSettingsSO : ScriptableObject
{
    [Header("全局基础配置")]
    [Tooltip("是否默认显示光标")]
    public bool defaultShowCursor = true;
    [Tooltip("默认/松开状态光标样式")]
    public CursorStyle defaultCursorStyle = CursorStyle.Default;
    [Tooltip("调试模式（发布时关闭）")]
    public bool isDebugMode = true;

    [Header("所有光标样式配置")]
    [Tooltip("配置所有光标样式，枚举新增后直接在此添加")]
    public List<CursorStyleConfig> cursorStyles = new List<CursorStyleConfig>();

    // 运行时缓存（仅初始化时构建，零动态分配）
    private Dictionary<CursorStyle, CursorStyleConfig> _styleDict;

    /// <summary>
    /// 初始化字典缓存（仅调用一次，零冗余）
    /// </summary>
    public void InitStyleDict()
    {
        if (_styleDict != null) return; // 避免重复构建

        _styleDict = new Dictionary<CursorStyle, CursorStyleConfig>();
        foreach (var config in cursorStyles)
        {
            // 跳过空纹理配置，避免运行时报错
            if (config.cursorTexture == null)
            {
                Debug.LogError($"[CursorSO] 样式{config.style}的纹理为空，已跳过！");
                continue;
            }

            // 避免重复配置覆盖
            if (!_styleDict.ContainsKey(config.style))
            {
                _styleDict.Add(config.style, config);
            }
            else
            {
                Debug.LogWarning($"[CursorSO] 样式{config.style}重复配置，保留第一个！");
            }
        }
    }

    /// <summary>
    /// 获取指定样式的配置（O(1)查找，零冗余）
    /// </summary>
    public CursorStyleConfig GetStyleConfig(CursorStyle style)
    {
        if (_styleDict == null) 
            InitStyleDict(); // 容错：未初始化时自动构建

        _styleDict.TryGetValue(style, out var config);
        return config;
    }
}