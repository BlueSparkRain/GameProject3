using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// UI面板类型枚举（可根据项目拓展）
/// 枚举名应和实际的预制件名称保持一致
/// </summary>
public enum UIPanelType
{
    TestTPanel,    // 测试面板
    MessagePanel, // 提示面板
    ShopPanel,    // 商店面板
    // 新增面板类型直接在这里添加即可
}

/// <summary>
/// 面板实例模式（单实例/多实例）
/// </summary>
public enum PanelInstanceMode
{
    Single,   // 单实例（默认，复用）
    Multiple  // 多实例（可同时存在多个）
}

/// <summary>
/// UI管理器（单例），负责面板的加载、创建、管理、回收
/// </summary>
public class UIManager : MonoGlobalManager
{
    protected override void Awake()
    {
        base.Awake();
        InitPanelRoot();
        InitPanelModeConfig(); // 初始化面板模式配置
    }

    #region 核心配置（混合模式缓存）
    private Transform _panelRoot;
    private string loadPath = "Prefabs/UIPanels/";

    // 1. 面板模式配置（关键：标记哪些是多实例）
    private Dictionary<UIPanelType, PanelInstanceMode> _panelModeConfig = new();
    // 2. 单实例缓存（key=类型，value=单个面板）
    private Dictionary<UIPanelType, UIPanelBase> _singlePanelCache = new();
    // 3. 多实例缓存（key=类型，value=面板列表+唯一ID）
    private Dictionary<UIPanelType, List<UIPanelBase>> _multiPanelCache = new();
    // 4. 所有面板ID映射（通过ID快速找到面板）
    private Dictionary<string, UIPanelBase> _allPanelIDMap = new();
    // 5. 多实例ID计数器（保证ID唯一）
    private Dictionary<UIPanelType, int> _multiPanelIDCounter = new();
    #endregion

    #region 初始化
    private void InitPanelRoot()
    {
        // 创建全局EventSystem（不变）
        CreateGlobalEventSystem();

        // 创建全局Canvas（不变）
        GameObject canvasObj = new GameObject("GlobalUICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        _panelRoot = new GameObject("UIPanelRoot").transform;
        _panelRoot.SetParent(canvasObj.transform);
        _panelRoot.localPosition = Vector3.zero;
        _panelRoot.localScale = Vector3.one;
    }

    /// <summary>
    /// 初始化面板模式（你可以在这里配置哪些面板是多实例）
    /// </summary>
    private void InitPanelModeConfig()
    {
        // 配置规则：默认单实例，手动标记多实例
        _panelModeConfig[UIPanelType.TestTPanel] = PanelInstanceMode.Single;
        _panelModeConfig[UIPanelType.MessagePanel] = PanelInstanceMode.Multiple; // 多实例
        _panelModeConfig[UIPanelType.ShopPanel] = PanelInstanceMode.Single;


        // 初始化多实例ID计数器
        foreach (var type in Enum.GetValues(typeof(UIPanelType)))
        {
            _multiPanelIDCounter[(UIPanelType)type] = 0;
        }
    }

    void CreateGlobalEventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        if (es != null)
        {
            if (es.GetComponent<StandaloneInputModule>() != null)
                DestroyImmediate(es.GetComponent<StandaloneInputModule>());
            if (es.GetComponent<InputSystemUIInputModule>() == null)
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(es.gameObject);
            return;
        }

        GameObject eso = new GameObject("GlobalEventSystem");
        eso.AddComponent<EventSystem>();
        eso.AddComponent<InputSystemUIInputModule>();
        DontDestroyOnLoad(eso);
    }
    #endregion

    #region 核心：通用Open方法（自动判断单/多实例）
    /// <summary>
    /// 打开面板（自动适配单/多实例）
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="type">面板枚举</param>
    /// <returns>面板实例（多实例返回新建的，单实例返回复用的）</returns>
    public T OpenPanel<T>(UIPanelType type) where T : UIPanelBase
    {
        // 1. 获取面板模式
        if (!_panelModeConfig.TryGetValue(type, out PanelInstanceMode mode))
            mode = PanelInstanceMode.Single; // 默认单实例

        // 2. 单实例逻辑（复用已有）
        if (mode == PanelInstanceMode.Single)
        {
            if (_singlePanelCache.TryGetValue(type, out UIPanelBase existPanel))
            {
                existPanel.transform.SetAsLastSibling();
                existPanel.Show();
                return existPanel as T;
            }

            // 单实例首次创建
            return CreateNewPanel<T>(type, GetSinglePanelID(type));
        }

        // 3. 多实例逻辑（新建）
        string uniqueID = GetMultiPanelUniqueID(type);
        T newPanel = CreateNewPanel<T>(type, uniqueID);

        // 加入多实例缓存
        if (!_multiPanelCache.ContainsKey(type))
            _multiPanelCache[type] = new List<UIPanelBase>();
        _multiPanelCache[type].Add(newPanel);

        return newPanel;
    }
    #endregion

    #region 多实例专属方法（单独操作某一个）
    /// <summary>
    /// 隐藏指定ID的多实例面板
    /// </summary>
    public void HidePanelByID(string panelID)
    {
        if (_allPanelIDMap.TryGetValue(panelID, out UIPanelBase panel))
            panel.Hide();
    }

    /// <summary>
    /// 关闭指定ID的多实例面板（销毁+移除缓存）
    /// </summary>
    public void ClosePanelByID(string panelID)
    {
        if (!_allPanelIDMap.TryGetValue(panelID, out UIPanelBase panel))
            return;

        // 从多实例列表移除
        if (_multiPanelCache.ContainsKey(panel.PanelType))
            _multiPanelCache[panel.PanelType].Remove(panel);

        // 从ID映射移除
        _allPanelIDMap.Remove(panelID);

        // 销毁面板
        panel.Close();
    }

    /// <summary>
    /// 隐藏某类型所有多实例面板
    /// </summary>
    public void HideAllMultiPanel(UIPanelType type)
    {
        if (_multiPanelCache.TryGetValue(type, out List<UIPanelBase> panels))
        {
            foreach (var p in panels) p.Hide();
        }
    }

    /// <summary>
    /// 关闭某类型所有多实例面板
    /// </summary>
    public void CloseAllMultiPanel(UIPanelType type)
    {
        if (!_multiPanelCache.TryGetValue(type, out List<UIPanelBase> panels))
            return;

        // 销毁所有面板
        foreach (var p in panels)
        {
            _allPanelIDMap.Remove(p.PanelID);
            p.Close();
        }
        // 清空列表
        _multiPanelCache[type].Clear();
    }
    #endregion

    #region 单实例专属方法（兼容旧逻辑）
    /// <summary>
    /// 隐藏单实例面板
    /// </summary>
    public void HidePanel(UIPanelType type)
    {
        if (_singlePanelCache.TryGetValue(type, out UIPanelBase panel))
            panel.Hide();
    }

    /// <summary>
    /// 关闭单实例面板（销毁+移除缓存）
    /// </summary>
    public void ClosePanel(UIPanelType type)
    {
        if (_singlePanelCache.TryGetValue(type, out UIPanelBase panel))
        {
            _singlePanelCache.Remove(type);
            _allPanelIDMap.Remove(panel.PanelID);
            panel.Close();
        }
    }
    #endregion

    #region 通用工具方法
    /// <summary>
    /// 创建新面板（内部复用）
    /// </summary>
    private T CreateNewPanel<T>(UIPanelType type, string uniqueID) where T : UIPanelBase
    {
        GameObject prefab = LoadPanelPrefab(type);
        if (prefab == null)
        {
            Debug.LogError($"面板预制件不存在：{loadPath}{type}");
            return null;
        }

        GameObject go = Instantiate(prefab, _panelRoot);
        T panel = go.GetComponent<T>();
        if (panel == null)
        {
            Debug.LogError($"{type} 未挂载 UIPanelBase 子类");
            Destroy(go);
            return null;
        }

        // 初始化面板
        panel.Init(type, uniqueID);
        panel.transform.SetAsLastSibling();
        panel.Show();

        // 加入缓存
        _allPanelIDMap[uniqueID] = panel;
        if (_panelModeConfig[type] == PanelInstanceMode.Single)
            _singlePanelCache[type] = panel;

        return panel;
    }

    /// <summary>
    /// 获取单实例面板ID
    /// </summary>
    private string GetSinglePanelID(UIPanelType type)
    {
        return $"{type}_Single";
    }

    /// <summary>
    /// 获取多实例面板唯一ID
    /// </summary>
    private string GetMultiPanelUniqueID(UIPanelType type)
    {
        _multiPanelIDCounter[type]++;
        return $"{type}_Multi_{_multiPanelIDCounter[type]}";
    }

    /// <summary>
    /// 加载预制件
    /// </summary>
    private GameObject LoadPanelPrefab(UIPanelType type)
    {
        Dictionary<UIPanelType, GameObject> prefabCache = new Dictionary<UIPanelType, GameObject>();
        if (prefabCache.TryGetValue(type, out GameObject prefab))
            return prefab;

        prefab = Resources.Load<GameObject>(loadPath + type);
        if (prefab != null)
            prefabCache[type] = prefab;
        return prefab;
    }
    #endregion

    public override void MgrUpdate(float deltaTime) { }
}

