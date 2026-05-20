using Core;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

/// <summary>
/// 六边形路径绘制管理器（完整6邻居+鼠标绘制+自动最短路径+不可行路径可视化）
/// 新增功能：
/// 1. 开关控制：是否记录/可视化超出行动点数的地块
/// 2. 鼠标落点高亮：可行路径内/外使用不同材质
/// 3. 障碍地块检测（基于外部walkableDic）
/// 核心特性：解耦、高性能、不破坏原有逻辑
/// </summary>
public class HexPathFindingManager : MonoGlobalManager
{
    [Header("核心配置")]
    [Tooltip("最大行动点数（路径最大长度）")]
    public int currentActionPoints = 8;
    string walkableMatPath = "Material/HexRoom/Walkable_HexRoom";
    string playerRoomMatPath = "Material/HexRoom/Player_HexRoom";
    string unreachableMatPath = "Material/HexRoom/Diswalkable_HexRoom";

    /// <summary>
    /// 鼠标落点在可行路径内终点的材质
    /// </summary>
    string endPointValidMatPath = "Material/HexRoom/EndPoint_Valid";

    /// <summary>
    /// 鼠标落点在不可行路径内终点的材质
    /// </summary>
    string endPointInvalidMatPath = "Material/HexRoom/EndPoint_Invalid";

    [Tooltip("射线检测层（仅检测六边形地块）")]
    public LayerMask hexRoomLayer;

    [Tooltip("是否启用超出行动点数的路径记录+可视化")]
    public bool enableUnreachablePath = false;


    [Header("六边形地图适配（关键！）")]
    [Tooltip("六边形地图行偏移规则：奇数行右移（true）/偶数行右移（false）")]
    public bool isOddRowStaggered = true;
    [Tooltip("六边形邻居判断调试（开启后打印邻居信息）")]
    public bool debugNeighborCheck = true;

    [Header("自动最短路径配置")]
    [Tooltip("启用鼠标悬停自动最短路径预览")]
    public bool enableAutoShortestPath = true;

    [Header("调试配置")]
    public bool enableDebugLog = false;

    
    //private HexMapInteractManager _gridManager;

    private GameMapManager  _mapManager;
    private Dictionary<Vector2Int, bool> _walkableDic;

    // 材质对象
    private Material _walkablePathMat;
    private Material _playerRoomMat;
    private Material _unreachablePathMat;
    private Material _endPointValidMat;
    private Material _endPointInvalidMat;
    private Material _playerRoom_OriginMat;

    // 核心绘制数据
    private HexRoomTag _playerStartRoom;
    private HexRoomTag _currentDrawRoom;
    private List<HexRoomTag> _walkablePath;
    private List<HexRoomTag> _diswalkablePath;
    private Dictionary<HexRoomTag, Material> _originMatCache;

    // 自动路径模式数据
    private bool _isManualDrawing;
    private List<HexRoomTag> _autoFullPath;

    #region 管理器生命周期
    public override void MgrInit(GameRoot gameRoot)
    {
        base.MgrInit(gameRoot);
        InitDependencies();
        InitDrawData();

        if (enableDebugLog)
            Debug.Log("[HexPathDrawMgr] 初始化完成（全功能版）");
    }

    void InitDependencies()
    {
        _mapManager ??= GameRoot.GetManager<GameMapManager>();
        //_gridManager = GameRoot.GetManager<HexMapInteractManager>();
        if (_mapManager == null)
        {
            Debug.LogError("[GameMapManager] GameMapManager，功能禁用！");
            enabled = false;
            return;
        }

        // 获取障碍字典
        _walkableDic = _mapManager.WalkableDic;

        // 加载所有材质
        _walkablePathMat = Resources.Load<Material>(walkableMatPath);
        _playerRoomMat = Resources.Load<Material>(playerRoomMatPath);
        _unreachablePathMat = Resources.Load<Material>(unreachableMatPath);
        _endPointValidMat = Resources.Load<Material>(endPointValidMatPath);
        _endPointInvalidMat = Resources.Load<Material>(endPointInvalidMatPath);

        // 材质校验
        if (_walkablePathMat == null) Debug.LogError($"材质缺失：{walkableMatPath}");
        if (_unreachablePathMat == null) Debug.LogError($"材质缺失：{unreachableMatPath}");
        if (_endPointValidMat == null) Debug.LogError($"材质缺失：{endPointValidMatPath}");
        if (_endPointInvalidMat == null) Debug.LogError($"材质缺失：{endPointInvalidMatPath}");

        if (hexRoomLayer.value == 0) hexRoomLayer = ~0;
    }

    void InitDrawData()
    {
        _walkablePath = new List<HexRoomTag>();
        _originMatCache = new Dictionary<HexRoomTag, Material>();
        _diswalkablePath = new List<HexRoomTag>();
        _autoFullPath = new List<HexRoomTag>();
        _isManualDrawing = false;
        _playerStartRoom = null;
        _currentDrawRoom = null;
    }

    public  bool canPathFind=false;
    public override void MgrUpdate(float deltaTime)
    {
        if (!canPathFind)
            return;

        //if (canPathFind) { 
        
        
        
        //}

        if (!enabled || _playerStartRoom == null) return;

        UpdateCurrentMouseRoom();
        UpdateDrawPath();
        RefreshPathVisual();
    }

    public void SetPathFindState(bool _canPathFind,int remainActionPoints=0) { 
        canPathFind = _canPathFind;
        currentActionPoints = remainActionPoints;

        if (!_canPathFind) {
            //清空所有材质
            ClearPathVisual();
        }
    }

    public override void MgrDispose()
    {
        base.MgrDispose();
        ClearPathVisual();
        _originMatCache.Clear();
        _diswalkablePath.Clear();
        _autoFullPath.Clear();
    }
    #endregion

    #region 外部接口
    public void SetPlayerStartRoom(HexRoomTag room)
    {
        if (room == null)
        {
            Debug.LogWarning("[HexPathDrawMgr]---玩家起始地块为空！");
            return;
        }

        if (!IsRoomWalkable(room))
        {
            Debug.LogWarning("[HexPathDrawMgr]---玩家起始地块不可行走！");
            return;
        }
        _playerStartRoom = room;
      
        MeshRenderer renderer = _playerStartRoom.GetComponent<MeshRenderer>();
        _playerRoom_OriginMat = renderer.material;

        _walkablePath.Clear();
        _diswalkablePath.Clear();
        _autoFullPath.Clear();
        _isManualDrawing = false;
        _currentDrawRoom = null;
        ClearPathVisual();

        if (debugNeighborCheck)
        {
            List<HexRoomTag> startNeighbors = GetAllHexNeighbors(room);
        }

        if (enableDebugLog)
            Debug.Log($"[HexPathDrawMgr] 玩家起始地块已设置：({room.row},{room.col})");
    }

    public void UpdateMaxActionPoints(int newPoints)
    {
        currentActionPoints = Mathf.Max(newPoints, 1);
        RefreshPathOnActionPointChange();

        if (enableDebugLog)
            Debug.Log($"[HexPathDrawMgr] 最大行动点数更新为：{currentActionPoints}");
    }

    public List<HexRoomTag> GetDrawnPath()
    {
        return new List<HexRoomTag>(_walkablePath);
    }

    public List<HexRoomTag> GetUnreachablePath()
    {
        return new List<HexRoomTag>(_diswalkablePath);
    }
    #endregion

    #region 鼠标地块检测
    private void UpdateCurrentMouseRoom()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexRoomLayer))
        {
            _currentDrawRoom = null;
            return;
        }

        HexRoomTag newMouseRoom = hit.collider.GetComponent<HexRoomTag>();
        if (newMouseRoom != null && newMouseRoom != _currentDrawRoom)
        {
            _currentDrawRoom = newMouseRoom;

            if (enableDebugLog)
                Debug.Log($"[HexPathDrawMgr] 鼠标当前地块：({newMouseRoom.row},{newMouseRoom.col})");
        }
    }
    #endregion

    /// <summary>
    /// 检测地块是否允许行走
    /// </summary>
    private bool IsRoomWalkable(HexRoomTag room)
    {
        if (room == null || _walkableDic == null) return false;
        return _walkableDic.TryGetValue(new Vector2Int(room.row, room.col), out bool isWalkable) && isWalkable;
    }

    #region 高性能BFS最短路径算法
    private List<HexRoomTag> BFSFindShortestPath(HexRoomTag start, HexRoomTag target)
    {
        List<HexRoomTag> path = new List<HexRoomTag>();
        if (start == null || target == null || start == target) return path;

        Queue<HexRoomTag> queue = new Queue<HexRoomTag>();
        Dictionary<HexRoomTag, HexRoomTag> pathMap = new Dictionary<HexRoomTag, HexRoomTag>();
        HashSet<HexRoomTag> visited = new HashSet<HexRoomTag>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            HexRoomTag current = queue.Dequeue();
            if (current == target) break;

            foreach (HexRoomTag neighbor in GetAllHexNeighbors(current))
            {
                if (!IsRoomWalkable(neighbor)) continue;

                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    pathMap[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        HexRoomTag temp = target;
        while (pathMap.ContainsKey(temp))
        {
            path.Add(temp);
            temp = pathMap[temp];
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// 拆分路径（受开关控制）
    /// </summary>
    private void SplitPath(List<HexRoomTag> fullPath)
    {
        _walkablePath.Clear();
        _diswalkablePath.Clear();
        if (fullPath == null || fullPath.Count == 0) return;

        int reachableCount = Mathf.Min(currentActionPoints, fullPath.Count);
        _walkablePath.AddRange(fullPath.GetRange(0, reachableCount));

        // 开关控制：是否记录不可行路径
        if (enableUnreachablePath && fullPath.Count > currentActionPoints)
            _diswalkablePath.AddRange(fullPath.GetRange(reachableCount, fullPath.Count - reachableCount));
    }

    private void RefreshPathOnActionPointChange()
    {
        if (!enableAutoShortestPath || _autoFullPath.Count == 0)
        {
            if (_walkablePath.Count > currentActionPoints)
            {
                int overflow = _walkablePath.Count - currentActionPoints;
                if (enableUnreachablePath)
                    _diswalkablePath.InsertRange(0, _walkablePath.GetRange(currentActionPoints, overflow));
                _walkablePath.RemoveRange(currentActionPoints, overflow);
            }
            return;
        }
        SplitPath(_autoFullPath);
    }
    #endregion

    #region 核心路径绘制逻辑
    void UpdateDrawPath()
    {
        if (_currentDrawRoom == null) return;

        // 回到起点 → 重置所有状态
        if (_currentDrawRoom == _playerStartRoom)
        {
            _walkablePath.Clear();
            _diswalkablePath.Clear();
            _autoFullPath.Clear();
            _isManualDrawing = false;
            return;
        }

        // 自动最短路径模式
        if (enableAutoShortestPath && !_isManualDrawing)
        {
            _autoFullPath = BFSFindShortestPath(_playerStartRoom, _currentDrawRoom);
            SplitPath(_autoFullPath);
            return;
        }

        // 手动绘制模式
        //_isManualDrawing = true;
        //_autoFullPath.Clear();

        //HexRoomTag lastRoom = _walkablePath.Count > 0 ? _walkablePath[^1] : _playerStartRoom;

        //if (IsHexNeighbor(lastRoom, _currentDrawRoom) && IsRoomWalkable(_currentDrawRoom))
        //{
        //    if (_walkablePath.Count < currentActionPoints)
        //    {
        //        _walkablePath.Add(_currentDrawRoom);
        //        GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/mambo");
        //    }
        //    else
        //    {
        //        // 开关控制：是否添加到不可行列表
        //        if (enableUnreachablePath)
        //            _diswalkablePath.Add(_currentDrawRoom);
        //    }
        //}
    }
    #endregion

    #region 六边形邻居判断
    bool IsHexNeighbor(HexRoomTag a, HexRoomTag b)
    {
        if (a == null || b == null) return false;

        int rowA = a.row;
        int colA = a.col;
        int rowB = b.row;
        int colB = b.col;

        int dRow = rowB - rowA;
        int dCol = colB - colA;
        bool isNeighbor = false;

        if (dRow is 1 or -1){
            bool isOddRow = (rowA % 2 == 1) == isOddRowStaggered;
            isNeighbor = isOddRow ? (dCol is 0 or 1) : (dCol is 0 or -1);
        }
        else if (dRow == 0)
            isNeighbor = dCol is 1 or -1;
        return isNeighbor;
    }

    public List<HexRoomTag> GetAllHexNeighbors(HexRoomTag room)
    {
        List<HexRoomTag> neighbors = new List<HexRoomTag>();
        if (room == null || _mapManager == null) return neighbors;

        int row = room.row;
        int col = room.col;
        bool isOddRow = (row % 2 == 1) == isOddRowStaggered;

        List<Vector2Int> neighborOffsets = new List<Vector2Int>{
            new(row, col + 1), new(row, col - 1)};

        if (isOddRow){
            neighborOffsets.AddRange(new List<Vector2Int>
            {
                new(row + 1, col), new(row + 1, col + 1),
                new(row - 1, col), new(row - 1, col + 1)
            });
        }
        else{
            neighborOffsets.AddRange(new List<Vector2Int>
            {
                new(row + 1, col), new(row + 1, col - 1),
                new(row - 1, col), new(row - 1, col - 1)
            });
        }

        foreach (var offset in neighborOffsets){
            if (_mapManager.HexRoomMap.TryGetValue(offset, out HexRoomTag neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }
    #endregion

    #region 路径可视化（含终点高亮）
    private void RefreshPathVisual()
    {
        ClearPathVisual();
        if (_walkablePathMat == null) return;

        // 渲染玩家起点
        if (_playerStartRoom){
            MeshRenderer renderer = _playerStartRoom.GetComponent<MeshRenderer>();
            renderer.material = _playerRoomMat;
        }

        // 渲染可行路径
        foreach (var room in _walkablePath)
            ApplyMaterial(room, _walkablePathMat);

        // 开关控制：渲染不可行路径
        if (enableUnreachablePath && _unreachablePathMat != null)
            foreach (var room in _diswalkablePath)
                ApplyMaterial(room, _unreachablePathMat);

        // 新增：渲染鼠标终点高亮
        ApplyEndPointMaterial();
    }

    /// <summary>
    /// 应用材质（缓存原始材质）
    /// </summary>
    private void ApplyMaterial(HexRoomTag room, Material mat)
    {
        MeshRenderer renderer = room.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        if (!_originMatCache.ContainsKey(room))
            _originMatCache[room] = renderer.material;

        if (mat != null)
        {
            renderer.material = mat;
            renderer.enabled = true;
        }
    }

    /// <summary>
    /// 新增：鼠标落点高亮材质
    /// </summary>
    private void ApplyEndPointMaterial()
    {
        if (_currentDrawRoom == null || _endPointValidMat == null || _endPointInvalidMat == null)
            return;

        // 缓存原始材质
        ApplyMaterial(_currentDrawRoom, null);

        bool isInValidPath = _walkablePath.Contains(_currentDrawRoom);
        MeshRenderer renderer = _currentDrawRoom.GetComponent<MeshRenderer>();
        renderer.material = isInValidPath ? _endPointValidMat : _endPointInvalidMat;

        canTriggerMover = isInValidPath ? true : false;
        if (canTriggerMover)
            TargetMoverPath = new List<HexRoomTag>(_walkablePath);
    }

    //可以通过点击触发移动
    public bool canTriggerMover;
    public List<HexRoomTag> TargetMoverPath;

    public void EndOneTimeMove() {
        canTriggerMover = false;
        //TargetMoverPath.Clear();
    }

    private void ClearPathVisual()
    {
        foreach (var kvp in _originMatCache)
        {
            MeshRenderer renderer = kvp.Key.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = kvp.Value;
        }
        _originMatCache.Clear();
        ResetPlayer_currentRoom();
    }

    void ResetPlayer_currentRoom()
    {
        if (_playerRoom_OriginMat != null && _playerStartRoom)
        {
            _playerStartRoom.GetComponent<MeshRenderer>().material = _playerRoom_OriginMat;
        }
    }
    #endregion
}