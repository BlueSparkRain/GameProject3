using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// UI面板类型枚举
/// </summary>
public enum E_UIPanelType
{
    TestTPanel,
    MessagePanel,
    ShopPanel,
    BattlePanel,
    NPCPanel,
    /// <summary>
    /// 技能配置买哪般
    /// </summary>
    SkillAssignPanel,
    MapTerrainEditorPanel,
    SettingsPanel,
    /// <summary>
    /// 技能奖励选择面板
    /// </summary>
    SkillSelectPanel,
    EquipmentPanel,
    UnknownEventPanel,

    MenuPanel,
    SkillDetailPanel,
    /// <summary>
    /// 神像奖励面板(三选一)
    /// </summary>
    RewardPanel,
}

/// <summary>
/// 面板实例模式
/// </summary>
public enum PanelInstanceMode
{
    Single,
    Multiple
}

/// <summary>
/// UI管理器（单例）
/// </summary>
public class UIManager : MonoGlobalManager
{
    protected override void Awake()
    {
        base.Awake();
        InitPanelRoot();
        InitPanelModeConfig();
    }

    #region 混合(单多例)模式缓存
    Transform _panelRoot;
    readonly string loadPath = "Prefab/UIPanel/";

    // 🔥 修复1：全局预制体缓存（不再重复加载，杜绝实例异常）
    Dictionary<E_UIPanelType, GameObject> _prefabCache = new();
    // 面板模式配置
    Dictionary<E_UIPanelType, PanelInstanceMode> _panelModeConfig = new();
    // 单实例缓存
    Dictionary<E_UIPanelType, UIPanelBase> _singlePanelCache = new();
    // 多实例缓存
    Dictionary<E_UIPanelType, List<UIPanelBase>> _multiPanelCache = new();
    // 全局ID映射
    Dictionary<string, UIPanelBase> _allPanelIDMap = new();
    // 多实例ID计数器
    Dictionary<E_UIPanelType, int> _multiPanelIDCounter = new();
    #endregion

    #region 初始化
    void InitPanelRoot()
    {
        CreateGlobalEventSystem();

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

    void InitPanelModeConfig(){
        _panelModeConfig[E_UIPanelType.MessagePanel] = PanelInstanceMode.Multiple; // 多实例
        _panelModeConfig[E_UIPanelType.MenuPanel] = PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.TestTPanel] = PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.ShopPanel] = PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.BattlePanel] = PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.NPCPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.SkillAssignPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.SkillSelectPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.MapTerrainEditorPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.SettingsPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.EquipmentPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.UnknownEventPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.SkillDetailPanel]= PanelInstanceMode.Single;
        _panelModeConfig[E_UIPanelType.RewardPanel]= PanelInstanceMode.Single;

        foreach (var type in Enum.GetValues(typeof(E_UIPanelType)))
            _multiPanelIDCounter[(E_UIPanelType)type] = 0;
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

    public T GetPanel<T>(E_UIPanelType type) where T : UIPanelBase
    {
        if (_singlePanelCache.ContainsKey(type))
            return _singlePanelCache[type] as T;
        else
            return null;
    }

    #region 核心：Open面板（多实例绝对隔离，不干扰其他面板）
    public T OpenPanel<T>(E_UIPanelType type, UnityAction<T> unityAction = null) where T : UIPanelBase
    {
        if (!_panelModeConfig.TryGetValue(type, out PanelInstanceMode mode))
            mode = PanelInstanceMode.Single;

        // 单实例逻辑（复用+置顶）
        if (mode == PanelInstanceMode.Single)
        {
            if (_singlePanelCache.TryGetValue(type, out UIPanelBase existPanel))
            {

                if (existPanel.canOpen)
                {
                    existPanel.transform.SetAsLastSibling();
                    existPanel.Show();
                    unityAction?.Invoke(existPanel as T);
                }
                
                return existPanel as T;
            }
            return CreateNewPanel<T>(type, GetSinglePanelID(type), unityAction);
        }

        // 🔥 修复2：多实例逻辑（纯新建，绝不操作已有面板，杜绝动画打断）
        string uniqueID = GetMultiPanelUniqueID(type);
        T newPanel = CreateNewPanel<T>(type, uniqueID, unityAction);

        if (!_multiPanelCache.ContainsKey(type))
            _multiPanelCache[type] = new List<UIPanelBase>();
        _multiPanelCache[type].Add(newPanel);

        return newPanel;
    }
    #endregion

    #region 多实例管理
    public void HidePanelByID(string panelID)
    {
        if (_allPanelIDMap.TryGetValue(panelID, out UIPanelBase panel))
            panel.Hide();
    }

    public void ClosePanelByID(string panelID)
    {
        if (!_allPanelIDMap.TryGetValue(panelID, out UIPanelBase panel)) return;

        if (_multiPanelCache.ContainsKey(panel.PanelType))
            _multiPanelCache[panel.PanelType].Remove(panel);

        _allPanelIDMap.Remove(panelID);
        panel.Close();
    }

    public void HideAllMultiPanel(E_UIPanelType type)
    {
        if (_multiPanelCache.TryGetValue(type, out List<UIPanelBase> panels))
            foreach (var p in panels) p.Hide();
    }

    public void CloseAllMultiPanel(E_UIPanelType type)
    {
        if (!_multiPanelCache.TryGetValue(type, out List<UIPanelBase> panels)) return;

        foreach (var p in panels)
        {
            _allPanelIDMap.Remove(p.PanelID);
            p.Close();
        }
        _multiPanelCache[type].Clear();
    }
    #endregion

    #region 单实例管理
    public void HidePanel(E_UIPanelType type)
    {
        if (_singlePanelCache.TryGetValue(type, out UIPanelBase panel))
            panel.Hide();
    }

    public void ClosePanel(E_UIPanelType type)
    {
        if (_singlePanelCache.TryGetValue(type, out UIPanelBase panel))
        {
            _singlePanelCache.Remove(type);
            _allPanelIDMap.Remove(panel.PanelID);
            panel.Close();
        }
    }
    #endregion

    #region 工具方法
    T CreateNewPanel<T>(E_UIPanelType type, string uniqueID, UnityAction<T> action = null) where T : UIPanelBase
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

        panel.Init(type, uniqueID);
        panel.transform.SetAsLastSibling();
        panel.Show();
        action?.Invoke(panel);

        _allPanelIDMap[uniqueID] = panel;
        if (_panelModeConfig[type] == PanelInstanceMode.Single)
            _singlePanelCache[type] = panel;

        return panel;
    }

    string GetSinglePanelID(E_UIPanelType type) => $"{type}_Single";
    string GetMultiPanelUniqueID(E_UIPanelType type)
    {
        _multiPanelIDCounter[type]++;
        return $"{type}_Multi_{_multiPanelIDCounter[type]}";
    }

    // 🔥 修复3：全局预制体缓存，只加载一次
    GameObject LoadPanelPrefab(E_UIPanelType type)
    {
        if (_prefabCache.TryGetValue(type, out GameObject prefab))
            return prefab;

        prefab = Resources.Load<GameObject>(loadPath + type);
        if (prefab != null)
            _prefabCache[type] = prefab;

        return prefab;
    }
    #endregion

    public override void MgrUpdate(float deltaTime) { }
}