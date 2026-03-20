using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器（单例），负责面板的加载、创建、管理、回收
/// </summary>
public class UIManager : MonoGlobalManager
{
    protected override void Awake()
    {
        base.Awake();
        // 初始化面板根节点
        InitPanelRoot();
    }

    #region 核心配置
    /// <summary>
    /// 面板根节点（所有面板的父物体）
    /// </summary>
    private Transform _panelRoot;

    /// <summary>
    /// 基础层级（所有面板的起始层级）
    /// </summary>
    private int _baseSortingOrder = 100;

    /// <summary>
    /// 当前最高层级（确保新面板在最上层）
    /// </summary>
    private int _currentMaxSortingOrder;

    /// <summary>
    /// 已打开的面板字典（Key：面板唯一ID）
    /// </summary>
    private Dictionary<string, UIPanelBase> _openedPanels = new Dictionary<string, UIPanelBase>();

    /// <summary>
    /// 同类型面板计数（用于生成唯一ID）
    /// </summary>
    private Dictionary<UIPanelType, int> _panelTypeCount = new Dictionary<UIPanelType, int>();

    /// <summary>
    /// 面板对象池（复用面板，减少GC）
    /// </summary>
    private Dictionary<UIPanelType, Stack<UIPanelBase>> _panelPool = new Dictionary<UIPanelType, Stack<UIPanelBase>>();

    /// <summary>
    /// 面板预制件缓存（避免重复加载Resources）
    /// </summary>
    private Dictionary<UIPanelType, GameObject> _panelPrefabCache = new Dictionary<UIPanelType, GameObject>();
    #endregion

    #region 初始化
    private void InitPanelRoot()
    {
        // 创建Canvas（UI根节点）
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        // 创建面板根节点（所有面板的父物体，便于管理）
        _panelRoot = new GameObject("UIPanelRoot").transform;
        _panelRoot.SetParent(canvasObj.transform);
        _panelRoot.localPosition = Vector3.zero;
        _panelRoot.localScale = Vector3.one;

        // 初始化层级
        _currentMaxSortingOrder = _baseSortingOrder;
    }
    #endregion

    #region 面板操作核心方法
    /// <summary>
    /// 打开面板（支持同类型多面板）
    /// </summary>
    /// <typeparam name="T">面板类型（继承UIPanelBase）</typeparam>
    /// <param name="panelType">面板枚举类型</param>
    /// <returns>打开的面板实例</returns>
    public T OpenPanel<T>(UIPanelType panelType) where T : UIPanelBase
    {
        // 1. 生成唯一ID
        if (!_panelTypeCount.ContainsKey(panelType))
        {
            _panelTypeCount[panelType] = 0;
        }
        _panelTypeCount[panelType]++;
        string uniqueID = $"{panelType}_{_panelTypeCount[panelType]}";

        // 2. 从对象池获取或创建面板
        UIPanelBase panel = GetPanelFromPool(panelType);

        // 3. 初始化面板
        if (panel == null)
        {
            // 加载预制件（优先从缓存获取）
            GameObject prefab = LoadPanelPrefab(panelType);
            if (prefab == null)
            {
                Debug.LogError($"面板预制件加载失败：Resources/Prefab/UIPanels/{panelType}");
                return null;
            }

            // 实例化面板
            GameObject panelObj = Instantiate(prefab, _panelRoot);
            panelObj.name = uniqueID;
            panel = panelObj.GetComponent<T>();
            if (panel == null)
            {
                Debug.LogError($"面板{panelType}未挂载UIPanelBase子类组件");
                Destroy(panelObj);
                return null;
            }

            // 初始化面板
            panel.Init(panelType, uniqueID);
        }
        else
        {
            // 复用面板，重新初始化唯一ID
            panel.PanelUniqueID = uniqueID;
            panel.Init(panelType, uniqueID);
        }

        // 4. 更新层级（最新面板在最上层）
        _currentMaxSortingOrder++;
        panel.Show(_currentMaxSortingOrder);

        // 5. 记录已打开面板
        _openedPanels[uniqueID] = panel;

        return panel as T;
    }

    /// <summary>
    /// 关闭指定唯一ID的面板
    /// </summary>
    /// <param name="uniqueID">面板唯一ID</param>
    public void ClosePanel(string uniqueID)
    {
        if (_openedPanels.TryGetValue(uniqueID, out UIPanelBase panel))
        {
            // 隐藏面板
            panel.Hide();

            // 从已打开字典移除
            _openedPanels.Remove(uniqueID);

            // 更新最高层级（如果关闭的是最上层面板）
            UpdateMaxSortingOrder();
        }
        else
        {
            Debug.LogWarning($"未找到要关闭的面板：{uniqueID}");
        }
    }

    /// <summary>
    /// 关闭指定类型的所有面板
    /// </summary>
    /// <param name="panelType">面板类型</param>
    public void CloseAllPanelsOfType(UIPanelType panelType)
    {
        List<string> toCloseIDs = new List<string>();
        foreach (var kvp in _openedPanels)
        {
            if (kvp.Value.PanelType == panelType)
            {
                toCloseIDs.Add(kvp.Key);
            }
        }

        foreach (string id in toCloseIDs)
        {
            ClosePanel(id);
        }

        // 重置该类型计数
        _panelTypeCount[panelType] = 0;
    }

    /// <summary>
    /// 关闭所有面板
    /// </summary>
    public void CloseAllPanels()
    {
        List<string> allIDs = new List<string>(_openedPanels.Keys);
        foreach (string id in allIDs)
        {
            ClosePanel(id);
        }

        // 重置计数
        _panelTypeCount.Clear();
        _currentMaxSortingOrder = _baseSortingOrder;
    }
    #endregion

    #region 对象池与资源加载
    /// <summary>
    /// 从对象池获取面板
    /// </summary>
    /// <param name="panelType">面板类型</param>
    /// <returns>面板实例（无则返回null）</returns>
    private UIPanelBase GetPanelFromPool(UIPanelType panelType)
    {
        if (!_panelPool.ContainsKey(panelType) || _panelPool[panelType].Count == 0)
        {
            return null;
        }

        UIPanelBase panel = _panelPool[panelType].Pop();
        panel.gameObject.SetActive(true);
        return panel;
    }

    /// <summary>
    /// 回收面板到对象池
    /// </summary>
    /// <param name="panel">要回收的面板</param>
    public void RecyclePanel(UIPanelBase panel)
    {
        if (!_panelPool.ContainsKey(panel.PanelType))
        {
            _panelPool[panel.PanelType] = new Stack<UIPanelBase>();
        }

        panel.gameObject.SetActive(false);
        _panelPool[panel.PanelType].Push(panel);

        // 从已打开字典移除
        if (_openedPanels.ContainsKey(panel.PanelUniqueID))
        {
            _openedPanels.Remove(panel.PanelUniqueID);
        }
    }

    /// <summary>
    /// 加载面板预制件（带缓存）
    /// </summary>
    /// <param name="panelType">面板类型</param>
    /// <returns>预制件</returns>
    private GameObject LoadPanelPrefab(UIPanelType panelType)
    {
        if (_panelPrefabCache.TryGetValue(panelType, out GameObject prefab))
        {
            return prefab;
        }

        // 从Resources加载预制件
        prefab = Resources.Load<GameObject>($"Prefab/UIPanels/{panelType}");
        if (prefab != null)
        {
            _panelPrefabCache[panelType] = prefab;
        }

        return prefab;
    }
    #endregion

    #region 层级更新
    /// <summary>
    /// 更新当前最高层级（关闭面板后调用）
    /// </summary>
    private void UpdateMaxSortingOrder()
    {
        _currentMaxSortingOrder = _baseSortingOrder;
        foreach (var panel in _openedPanels.Values)
        {
            if (panel.GetComponent<Canvas>().sortingOrder > _currentMaxSortingOrder)
            {
                _currentMaxSortingOrder = panel.GetComponent<Canvas>().sortingOrder;
            }
        }
    }

    /// <summary>
    /// 将指定面板置顶
    /// </summary>
    /// <param name="uniqueID">面板唯一ID</param>
    public void BringPanelToFront(string uniqueID)
    {
        if (_openedPanels.TryGetValue(uniqueID, out UIPanelBase panel))
        {
            _currentMaxSortingOrder++;
            panel.UpdateSortingOrder(_currentMaxSortingOrder);
        }
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 获取已打开的指定类型面板列表
    /// </summary>
    /// <param name="panelType">面板类型</param>
    /// <returns>面板列表</returns>
    public List<UIPanelBase> GetOpenedPanelsOfType(UIPanelType panelType)
    {
        List<UIPanelBase> panels = new List<UIPanelBase>();
        foreach (var panel in _openedPanels.Values)
        {
            if (panel.PanelType == panelType)
            {
                panels.Add(panel);
            }
        }
        return panels;
    }

    /// <summary>
    /// 检查面板是否已打开
    /// </summary>
    /// <param name="uniqueID">面板唯一ID</param>
    /// <returns>是否打开</returns>
    public bool IsPanelOpened(string uniqueID)
    {
        return _openedPanels.ContainsKey(uniqueID);
    }

    public override void MgrUpdate(float deltaTime)
    {
        throw new NotImplementedException();
    }
    #endregion
}