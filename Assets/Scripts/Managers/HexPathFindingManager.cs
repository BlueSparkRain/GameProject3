using Core;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    /// <summary>
    /// 鼠标落点在可行路径非终点的材质
    /// </summary>
    string walkableMatPath = "Material/HexRoom/Walkable_HexRoom";
    string walkableMatPath_ring = "Material/HexRoom/HexRing/Walkable_HexRoom";
    /// <summary>
    /// 玩家当前所在的地块的材质
    /// </summary>
    string playerRoomMatPath = "Material/HexRoom/Player_HexRoom";
    string playerRoomMatPath_ring = "Material/HexRoom/HexRing/Player_HexRoom";
    /// <summary>
    /// 不可行路径地块的材质
    /// </summary>
    string unreachableMatPath = "Material/HexRoom/Diswalkable_HexRoom";
    string unreachableMatPath_ring = "Material/HexRoom/HexRing/Diswalkable_HexRoom";
    /// <summary>
    /// 鼠标落点在可行路径内终点的材质
    /// </summary>
    string endPointValidMatPath = "Material/HexRoom/EndPoint_Valid";
    string endPointValidMatPath_ring = "Material/HexRoom/HexRing/EndPoint_Valid";

    /// <summary>
    /// 鼠标落点在不可行路径内终点的材质
    /// </summary>
    string endPointInvalidMatPath = "Material/HexRoom/EndPoint_Invalid";
    string endPointInvalidMatPath_ring = "Material/HexRoom/HexRing/EndPoint_Invalid";

    [Tooltip("射线检测层（仅检测六边形地块）")]
    public LayerMask hexRoomLayer;

    [Tooltip("是否启用超出行动点数的路径记录+可视化")]
    public bool enableUnreachablePath = true;


    [Header("六边形地图适配（关键！）")]
    [Tooltip("六边形地图行偏移规则：奇数行右移（true）/偶数行右移（false）")]
    public bool isOddRowStaggered = true;
    [Tooltip("六边形邻居判断调试（开启后打印邻居信息）")]
    public bool debugNeighborCheck = true;

    [Header("自动最短路径配置")]
    [Tooltip("启用鼠标悬停自动最短路径预览")]
    public bool enableAutoShortestPath = true;

    [Header("悬浮配置")]
    [Tooltip("HexRoom自身Z轴上升高度")]
    public float roomFloatHeight = 1.0f;
    [Tooltip("上升/归位动画时长")]
    public float roomFloatDuration = 0.2f;
    [Tooltip("首个子物体透明度过渡时长")]
    public float childFadeDuration = 0.3f;

    [Header("调试配置")]
    public bool enableDebugLog = false;


    //private HexMapInteractManager _gridManager;

    private GameMapManager _mapManager;
    private Dictionary<Vector2Int, bool> _walkableDic;

    // 材质对象
    private Material _walkablePathMat;
    private Material _playerRoomMat;
    private Material _unreachablePathMat;
    private Material _endPointValidMat;
    private Material _endPointInvalidMat;
    private Material _playerRoom_OriginMat;

    // Ring材质（应用于子物体）
    private Material _walkablePathMat_ring;
    private Material _playerRoomMat_ring;
    private Material _unreachablePathMat_ring;
    private Material _endPointValidMat_ring;
    private Material _endPointInvalidMat_ring;
    private Material _playerRoom_ChildOriginMat;

    // 主材质 → Ring材质映射
    private Dictionary<Material, Material> _mainToRingMat;

    // 核心绘制数据
    private HexRoomTag _playerStartRoom;
    private HexRoomTag _currentDrawRoom;
    private List<HexRoomTag> _walkablePath;
    private List<HexRoomTag> _diswalkablePath;
    private Dictionary<HexRoomTag, Material> _originMatCache;
    private Dictionary<HexRoomTag, Material> _originChildMatCache;

    // 路径上浮状态追踪
    private Dictionary<HexRoomTag, float> _roomOrigY = new Dictionary<HexRoomTag, float>();      // 写一次永不删
    private HashSet<HexRoomTag> _floatingSet = new HashSet<HexRoomTag>();                       // 当前上浮中的房间
    private HashSet<HexRoomTag> _pendingDown = new HashSet<HexRoomTag>();                       // 正在下沉动画中

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

        // 加载Ring材质
        _walkablePathMat_ring = Resources.Load<Material>(walkableMatPath_ring);
        _playerRoomMat_ring = Resources.Load<Material>(playerRoomMatPath_ring);
        _unreachablePathMat_ring = Resources.Load<Material>(unreachableMatPath_ring);
        _endPointValidMat_ring = Resources.Load<Material>(endPointValidMatPath_ring);
        _endPointInvalidMat_ring = Resources.Load<Material>(endPointInvalidMatPath_ring);

        // 材质校验
        if (_walkablePathMat == null) Debug.LogError($"材质缺失：{walkableMatPath}");
        if (_unreachablePathMat == null) Debug.LogError($"材质缺失：{unreachableMatPath}");
        if (_endPointValidMat == null) Debug.LogError($"材质缺失：{endPointValidMatPath}");
        if (_endPointInvalidMat == null) Debug.LogError($"材质缺失：{endPointInvalidMatPath}");

        // 构建主材质→Ring材质映射
        _mainToRingMat = new Dictionary<Material, Material>
        {
            { _walkablePathMat, _walkablePathMat_ring },
            { _playerRoomMat, _playerRoomMat_ring },
            { _unreachablePathMat, _unreachablePathMat_ring },
            { _endPointValidMat, _endPointValidMat_ring },
            { _endPointInvalidMat, _endPointInvalidMat_ring },
        };

        if (hexRoomLayer.value == 0) hexRoomLayer = ~0;
    }

    void InitDrawData()
    {
        _walkablePath = new List<HexRoomTag>();
        _originMatCache = new Dictionary<HexRoomTag, Material>();
        _originChildMatCache = new Dictionary<HexRoomTag, Material>();
        _diswalkablePath = new List<HexRoomTag>();
        _autoFullPath = new List<HexRoomTag>();
        _isManualDrawing = false;
        _playerStartRoom = null;
        _currentDrawRoom = null;
    }

    public bool canPathFind = false;
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
    public void SetPathFindState(bool _canPathFind, int remainActionPoints = 0){
        canPathFind = _canPathFind;
        currentActionPoints = remainActionPoints;

        if (!_canPathFind)
        {
            ClearPathVisual();
            HoverDownAll();
        }
    }
    public override void MgrDispose(){
        base.MgrDispose();
        HoverDownAll();
        ClearPathVisual();
        _originMatCache.Clear();
        _originChildMatCache.Clear();
        _diswalkablePath.Clear();
        _autoFullPath.Clear();
    }
    #endregion

    #region 外部接口
    public void SetPlayerStartRoom(HexRoomTag room){
        if (room == null){
            // 重新检测：从MapMoverManager获取当前操作角色的所在房间
            var moverMgr = GameRoot.GetManager<MapMoverManager>();
            if (moverMgr != null && moverMgr.currentIMovable != null)
                room = moverMgr.currentIMovable.currentRoom;

            if (room == null){
                Debug.LogWarning("[HexPathDrawMgr]---玩家起始地块为空！");
                return;
            }
        }
        if (!IsRoomWalkable(room)){
            Debug.LogWarning("[HexPathDrawMgr]---玩家起始地块不可行走！");
            return;
        }
        _playerStartRoom = room;

        MeshRenderer renderer = _playerStartRoom.GetComponent<MeshRenderer>();
        _playerRoom_OriginMat = renderer.material;

        Renderer childRenderer = GetFirstChildRenderer(room);
        _playerRoom_ChildOriginMat = childRenderer != null ? childRenderer.material : null;

        _walkablePath.Clear();
        _diswalkablePath.Clear();
        _autoFullPath.Clear();
        _isManualDrawing = false;
        _currentDrawRoom = null;
        ClearPathVisual();

        if (debugNeighborCheck){
            List<HexRoomTag> startNeighbors = GetAllHexNeighbors(room);
        }
        if (enableDebugLog)
            Debug.Log($"[HexPathDrawMgr] 玩家起始地块已设置：({room.row},{room.col})");
    }
    public void UpdateMaxActionPoints(int newPoints){
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

        HexRoomTag newMouseRoom = hit.collider.GetComponentInParent<HexRoomTag>();
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
    /// <summary>
    /// 根据目标row/col计算六边形邻居坐标（不依赖HexRoomMap，纯坐标计算）
    /// </summary>
    List<Vector2Int> GetHexNeighborCoords(int row, int col)
    {
        var coords = new List<Vector2Int> { new(row, col + 1), new(row, col - 1) };
        bool isOdd = (row % 2 == 1) == isOddRowStaggered;
        if (isOdd)
        {
            coords.Add(new(row + 1, col)); coords.Add(new(row + 1, col + 1));
            coords.Add(new(row - 1, col)); coords.Add(new(row - 1, col + 1));
        }
        else
        {
            coords.Add(new(row + 1, col)); coords.Add(new(row + 1, col - 1));
            coords.Add(new(row - 1, col)); coords.Add(new(row - 1, col - 1));
        }
        return coords;
    }

    private List<HexRoomTag> BFSFindShortestPath(HexRoomTag start, HexRoomTag target)
    {
        List<HexRoomTag> path = new List<HexRoomTag>();
        if (start == null || target == null || start == target) return path;

        bool targetWalkable = IsRoomWalkable(target);

        Queue<HexRoomTag> queue = new Queue<HexRoomTag>();
        Dictionary<HexRoomTag, HexRoomTag> pathMap = new Dictionary<HexRoomTag, HexRoomTag>();
        HashSet<HexRoomTag> visited = new HashSet<HexRoomTag>();
        Dictionary<HexRoomTag, int> distance = new Dictionary<HexRoomTag, int>();

        queue.Enqueue(start);
        visited.Add(start);
        distance[start] = 0;

        while (queue.Count > 0)
        {
            HexRoomTag current = queue.Dequeue();

            if (targetWalkable && current == target) break;

            foreach (HexRoomTag neighbor in GetAllHexNeighbors(current))
            {
                if (!IsRoomWalkable(neighbor)) continue;

                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    pathMap[neighbor] = current;
                    distance[neighbor] = distance[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (targetWalkable)
        {
            HexRoomTag temp = target;
            while (pathMap.ContainsKey(temp))
            {
                path.Add(temp);
                temp = pathMap[temp];
            }
            path.Reverse();
        }
        else
        {
            // 不可行走目标：查目标的6个邻居坐标，O(6)固定开销
            HexRoomTag bestNeighbor = null;
            int bestDist = int.MaxValue;

            foreach (var offset in GetHexNeighborCoords(target.row, target.col))
            {
                if (_mapManager.HexRoomMap.TryGetValue(offset, out HexRoomTag neighbor)
                    && IsRoomWalkable(neighbor)
                    && distance.TryGetValue(neighbor, out int d)
                    && d < bestDist)
                {
                    bestDist = d;
                    bestNeighbor = neighbor;
                }
            }

            if (bestNeighbor != null)
            {
                HexRoomTag temp = bestNeighbor;
                while (pathMap.ContainsKey(temp))
                {
                    path.Add(temp);
                    temp = pathMap[temp];
                }
                path.Reverse();
                path.Add(target);
            }
        }

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

        // 始终记录不可行路径（上浮需要完整路径），开关仅控制材质
        if (fullPath.Count > currentActionPoints)
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
            if (enableDebugLog)
                Debug.Log($"[HexPathDrawMgr] BFS结果: fullPath={_autoFullPath.Count}, target walkable={IsRoomWalkable(_currentDrawRoom)}, target=({_currentDrawRoom.row},{_currentDrawRoom.col})");
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

        if (dRow is 1 or -1)
        {
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

        if (isOddRow)
        {
            neighborOffsets.AddRange(new List<Vector2Int>
            {
                new(row + 1, col), new(row + 1, col + 1),
                new(row - 1, col), new(row - 1, col + 1)
            });
        }
        else
        {
            neighborOffsets.AddRange(new List<Vector2Int>
            {
                new(row + 1, col), new(row + 1, col - 1),
                new(row - 1, col), new(row - 1, col - 1)
            });
        }

        foreach (var offset in neighborOffsets)
        {
            if (_mapManager.HexRoomMap.TryGetValue(offset, out HexRoomTag neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }
    #endregion

    #region 路径可视化（含终点高亮）
    private void RefreshPathVisual(){
        // 在ClearPathVisual之前标记即将下沉的房间，防止材质被提前恢复
        MarkPendingDown();
        ClearPathVisual();
        if (_walkablePathMat == null) return;
        // 渲染玩家起点
        if (_playerStartRoom){
            MeshRenderer renderer = _playerStartRoom.GetComponent<MeshRenderer>();
            renderer.material = _playerRoomMat;
            ApplyRingToChild(_playerStartRoom, _playerRoomMat);
        }

        // 渲染可行路径
        foreach (var room in _walkablePath)
            ApplyMaterial(room, _walkablePathMat);

        // 渲染不可行路径
        if (_unreachablePathMat != null)
            foreach (var room in _diswalkablePath)
                ApplyMaterial(room, _unreachablePathMat);

        // 新增：渲染鼠标终点高亮
        ApplyEndPointMaterial();

        // 同步路径房间上浮状态
        SyncPathFloat();
        if (enableDebugLog)
            Debug.Log($"[HexPathDrawMgr] RefreshPathVisual: walkablePath={_walkablePath.Count}, diswalkablePath={_diswalkablePath.Count}, floatingSet={_floatingSet.Count}, pendingDown={_pendingDown.Count}");
    }

    /// <summary>
    /// 提前标记本帧要下沉的房间，让ClearPathVisual跳过它们的材质恢复
    /// </summary>
    void MarkPendingDown()
    {
        var shouldFloat = new HashSet<HexRoomTag>();
        if (_playerStartRoom != null) shouldFloat.Add(_playerStartRoom);
        foreach (var room in _walkablePath) shouldFloat.Add(room);
        foreach (var room in _diswalkablePath) shouldFloat.Add(room);
        if (_currentDrawRoom != null) shouldFloat.Add(_currentDrawRoom);

        foreach (var room in _floatingSet)
        {
            if (!shouldFloat.Contains(room))
                _pendingDown.Add(room);
        }
    }

    Renderer GetFirstChildRenderer(HexRoomTag room)
    {
        if (room == null || room.transform.childCount == 0) return null;
        return room.transform.GetChild(0).GetComponent<Renderer>();
    }

    /// <summary>
    /// 应用材质（缓存原始材质），同时为子物体应用对应Ring材质
    /// </summary>
    private void ApplyMaterial(HexRoomTag room, Material mat)
    {
        MeshRenderer renderer = room.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            if (!_originMatCache.ContainsKey(room))
                _originMatCache[room] = renderer.material;

            if (mat != null)
                renderer.material = mat;
        }

        // 子物体应用对应Ring材质
        ApplyRingToChild(room, mat);
    }

    void ApplyRingToChild(HexRoomTag room, Material mainMat)
    {
        if (mainMat == null) return;
        if (!_mainToRingMat.TryGetValue(mainMat, out Material ringMat) || ringMat == null) return;

        Renderer childRenderer = GetFirstChildRenderer(room);
        if (childRenderer == null) return;

        if (!_originChildMatCache.ContainsKey(room))
            _originChildMatCache[room] = childRenderer.material;

        childRenderer.material = ringMat;
    }

    /// <summary>
    /// 鼠标落点高亮材质：终点必须在可行走路径内且未超出行动力才算有效终点
    /// </summary>
    private void ApplyEndPointMaterial()
    {
        if (_currentDrawRoom == null || _endPointValidMat == null || _endPointInvalidMat == null)
            return;

        // 终点有效 = 地块可行走 + 实际在可达路径内（未超出行动点数）
        bool isWithinReach = _walkablePath.Count > 0 && _walkablePath.Contains(_currentDrawRoom);
        bool isValid = IsRoomWalkable(_currentDrawRoom) && isWithinReach;
        Material endMat = isValid ? _endPointValidMat : _endPointInvalidMat;

        // 缓存原始材质（仅首次）
        if (!_originMatCache.ContainsKey(_currentDrawRoom))
        {
            MeshRenderer r = _currentDrawRoom.GetComponent<MeshRenderer>();
            if (r != null) _originMatCache[_currentDrawRoom] = r.material;
        }

        MeshRenderer renderer = _currentDrawRoom.GetComponent<MeshRenderer>();
        renderer.material = endMat;

        // 子物体Ring材质
        ApplyRingToChild(_currentDrawRoom, endMat);

        canTriggerMover = isValid ? true : false;
        if (canTriggerMover)
            TargetMoverPath = new List<HexRoomTag>(_walkablePath);
    }

    //可以通过点击触发移动
    public bool canTriggerMover;
    public List<HexRoomTag> TargetMoverPath;

    public void EndOneTimeMove()
    {
        canTriggerMover = false;
        //TargetMoverPath.Clear();
    }

    private void ClearPathVisual()
    {
        // 保留正在下沉动画中的房间的缓存，延后恢复
        var pendingMatCache = new Dictionary<HexRoomTag, Material>();
        var pendingChildCache = new Dictionary<HexRoomTag, Material>();
        foreach (var room in _pendingDown)
        {
            if (_originMatCache.TryGetValue(room, out var mat))
                pendingMatCache[room] = mat;
            if (_originChildMatCache.TryGetValue(room, out var childMat))
                pendingChildCache[room] = childMat;
        }

        foreach (var kvp in _originMatCache)
        {
            MeshRenderer renderer = kvp.Key.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = kvp.Value;
        }
        _originMatCache.Clear();
        // 恢复pending房间的缓存
        foreach (var kvp in pendingMatCache)
            _originMatCache[kvp.Key] = kvp.Value;

        foreach (var kvp in _originChildMatCache)
        {
            Renderer childRenderer = GetFirstChildRenderer(kvp.Key);
            if (childRenderer != null)
                childRenderer.material = kvp.Value;
        }
        _originChildMatCache.Clear();
        foreach (var kvp in pendingChildCache)
            _originChildMatCache[kvp.Key] = kvp.Value;

        if (!_pendingDown.Contains(_playerStartRoom))
            ResetPlayer_currentRoom();
    }

    void ResetPlayer_currentRoom()
    {
        if (_playerRoom_OriginMat != null && _playerStartRoom)
        {
            _playerStartRoom.GetComponent<MeshRenderer>().material = _playerRoom_OriginMat;
        }
        if (_playerRoom_ChildOriginMat != null && _playerStartRoom)
        {
            Renderer childRenderer = GetFirstChildRenderer(_playerStartRoom);
            if (childRenderer != null)
                childRenderer.material = _playerRoom_ChildOriginMat;
        }
    }

    /// <summary>
    /// 同步路径房间上浮：在路径内的上浮，不在路径内的归位（材质延迟到动画结束后恢复）
    /// </summary>
    void SyncPathFloat()
    {
        var shouldFloat = new HashSet<HexRoomTag>();
        if (_playerStartRoom != null) shouldFloat.Add(_playerStartRoom);
        foreach (var room in _walkablePath) shouldFloat.Add(room);
        foreach (var room in _diswalkablePath) shouldFloat.Add(room);
        if (_currentDrawRoom != null) shouldFloat.Add(_currentDrawRoom);

        // 不在路径内的 → 归位
        var toDown = new List<HexRoomTag>();
        foreach (var room in _floatingSet)
        {
            if (!shouldFloat.Contains(room))
                toDown.Add(room);
        }
        foreach (var room in toDown)
        {
            FloatDown(room);
        }

        // 新进入路径的 → 上浮
        foreach (var room in shouldFloat)
        {
            if (_floatingSet.Contains(room) || _pendingDown.Contains(room)) continue;

            if (!_roomOrigY.ContainsKey(room))
                _roomOrigY[room] = room.transform.localPosition.y;

            float origY = _roomOrigY[room];
            _floatingSet.Add(room);

            room.transform.DOKill();
            room.transform.DOLocalMoveY(origY + roomFloatHeight, roomFloatDuration).SetEase(Ease.OutQuad);
            FadeFirstChild(room, 1f);
        }
    }

    void FloatDown(HexRoomTag room)
    {
        if (!_floatingSet.Remove(room)) return;
        if (!_roomOrigY.TryGetValue(room, out float origY)) return;

        _pendingDown.Add(room);

        room.transform.DOKill();
        var tween = room.transform.DOLocalMoveY(origY, roomFloatDuration).SetEase(Ease.OutQuad);
        FadeFirstChild(room, 0f);

        // 动画结束后恢复材质
        tween.OnComplete(() =>
        {
            _pendingDown.Remove(room);
            // 恢复原始材质
            if (_originMatCache.TryGetValue(room, out var originMat))
            {
                var renderer = room.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.material = originMat;
                _originMatCache.Remove(room);
            }
            if (_originChildMatCache.TryGetValue(room, out var originChildMat))
            {
                var childRenderer = GetFirstChildRenderer(room);
                if (childRenderer != null)
                    childRenderer.material = originChildMat;
                _originChildMatCache.Remove(room);
            }
            // 恢复玩家起点材质
            if (room == _playerStartRoom)
                ResetPlayer_currentRoom();
        });
    }

    /// <summary>
    /// 所有浮动房间同时归位
    /// </summary>
    void HoverDownAll()
    {
        var toDown = new List<HexRoomTag>(_floatingSet);
        foreach (var room in toDown)
            FloatDown(room);
    }

    void FadeFirstChild(HexRoomTag room, float targetAlpha)
    {
        if (room == null || room.transform.childCount == 0) return;
        var sprite = room.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.DOKill();
            sprite.DOFade(targetAlpha, childFadeDuration).SetEase(Ease.OutQuad);
        }
    }
    #endregion
}