using Core;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 六边形路径绘制管理器（完整6邻居+鼠标绘制）
/// 核心修复：
/// 1. 完整识别六边形6个方向的邻居（上/下/左/右/左上/右下 或 上/下/左/右/右上/左下）
/// 2. 适配奇偶行错开的六边形地图
/// 3. 保留鼠标绘制/回退无突变的核心逻辑
/// </summary>
public class HexPathFindingManager : MonoGlobalManager
{
    [Header("核心配置")]
    [Tooltip("最大行动点数（路径最大长度）")]
    public int maxActionPoints = 8;
    string walkableMatPath = "Material/HexRoom/Walkable_HexRoom";

    string playerRoomMatPath = "Material/HexRoom/Player_HexRoom";
    [Tooltip("射线检测层（仅检测六边形地块）")]
    public LayerMask hexRoomLayer;

    [Header("六边形地图适配（关键！）")]
    [Tooltip("六边形地图行偏移规则：奇数行右移（true）/偶数行右移（false）")]
    public bool isOddRowStaggered = true;
    [Tooltip("六边形邻居判断调试（开启后打印邻居信息）")]
    public bool debugNeighborCheck = true;

    [Header("调试配置")]
    public bool enableDebugLog = false;

    // 依赖管理
    private HexGridInteractManager _gridManager;
    private Material _walkablePathMat;
    private Material _playerRoomMat;
    private Material _playerRoom_OriginMat;

    // 核心绘制数据
    private HexRoom _playerStartRoom;    // 玩家起始地块（路径起点）
    private HexRoom _currentDrawRoom;    // 鼠标当前指向的地块
    private List<HexRoom> _drawnPath;    // 已绘制的路径
    private Dictionary<HexRoom, Material> _originMatCache; // 原始材质缓存

    #region 管理器生命周期
    public override void MgrInit(GameRoot gameRoot){
        base.MgrInit(gameRoot);
        InitDependencies();
        InitDrawData();

        if (enableDebugLog)
            Debug.Log("[HexPathDrawMgr] 初始化完成（完整6邻居支持）");
        
    }

    void InitDependencies(){
        _gridManager = GameRoot.GetManager<HexGridInteractManager>();
        if (_gridManager == null){
            Debug.LogError("[HexPathDrawMgr] 未找到HexGridInteractManager，功能禁用！");
            enabled = false;
            return;
        }

        _walkablePathMat = Resources.Load<Material>(walkableMatPath);
        _playerRoomMat= Resources.Load<Material>(playerRoomMatPath);

        if (_walkablePathMat == null){
            Debug.LogError($"[HexPathDrawMgr] 材质加载失败：Resources/{walkableMatPath}.mat");
            enabled = false;
            return;
        }

        if (hexRoomLayer.value == 0)
            hexRoomLayer = ~0;
        
    }

    void InitDrawData(){
        _drawnPath = new List<HexRoom>();
        _originMatCache = new Dictionary<HexRoom, Material>();
        _playerStartRoom = null;
        _currentDrawRoom = null;
    }

    public override void MgrUpdate(float deltaTime){
        if (!enabled || _playerStartRoom == null) return;

        UpdateCurrentMouseRoom();
        UpdateDrawPath();
        RefreshPathVisual();
    }

    public override void MgrDispose(){
        base.MgrDispose();
        ClearPathVisual();
        _originMatCache.Clear();
    }
    #endregion

    #region 外部接口
    public void SetPlayerStartRoom(HexRoom room){
        if (room == null){
            Debug.LogWarning("[HexPathDrawMgr] 玩家起始地块为空！");
            return;
        }

        _playerStartRoom = room;
        _drawnPath.Clear();
        _currentDrawRoom = null;
        ClearPathVisual();

        if (debugNeighborCheck){
            List<HexRoom> startNeighbors = GetAllHexNeighbors(room);
        }

        if (enableDebugLog)
            Debug.Log($"[HexPathDrawMgr] 玩家起始地块已设置：({room.row},{room.col})");
        
    }

    public void UpdateMaxActionPoints(int newPoints){
        maxActionPoints = Mathf.Max(newPoints, 1);
        if (_drawnPath.Count > maxActionPoints)

            _drawnPath.RemoveRange(maxActionPoints, _drawnPath.Count - maxActionPoints);
        if (enableDebugLog)
            Debug.Log($"[HexPathDrawMgr] 最大行动点数更新为：{maxActionPoints}");
    }

    public List<HexRoom> GetDrawnPath(){
        return new List<HexRoom>(_drawnPath);
    }
    #endregion

    #region 鼠标地块检测
    private void UpdateCurrentMouseRoom(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexRoomLayer)){
            _currentDrawRoom = null;
            return;
        }

        HexRoom newMouseRoom = hit.collider.GetComponent<HexRoom>();
        if (newMouseRoom != null && newMouseRoom != _currentDrawRoom){
            _currentDrawRoom = newMouseRoom;

            if (enableDebugLog)
                Debug.Log($"[HexPathDrawMgr] 鼠标当前地块：({newMouseRoom.row},{newMouseRoom.col})");
        }
    }
    #endregion

    #region 核心修复：完整6邻居判断+路径绘制
    /// <summary>
    /// 路径绘制/回退逻辑（无变化，保留原有体验）
    /// </summary>
    void UpdateDrawPath() 
    { 
        if (_currentDrawRoom == null) return;

        // 指向起始地块 → 清空路径
        if (_currentDrawRoom == _playerStartRoom){
            _drawnPath.Clear();
            return;
        }

        // 回退操作：鼠标回到已绘制路径中
        int backIndex = _drawnPath.IndexOf(_currentDrawRoom);
        if (backIndex != -1){
            _drawnPath.RemoveRange(backIndex + 1, _drawnPath.Count - (backIndex + 1));
            if (enableDebugLog)
                Debug.Log($"[HexPathDrawMgr] 路径回退到：({_currentDrawRoom.row},{_currentDrawRoom.col})，长度：{_drawnPath.Count}");
            return;
        }

        // 追加操作：判断是否是6邻居之一 + 未超长度
        HexRoom lastPathRoom = _drawnPath.Count > 0 ? _drawnPath[_drawnPath.Count - 1] : _playerStartRoom;
        if (IsHexNeighbor(lastPathRoom, _currentDrawRoom) && _drawnPath.Count < maxActionPoints){
            _drawnPath.Add(_currentDrawRoom);
            GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/mambo");
            if (enableDebugLog)
                Debug.Log($"[HexPathDrawMgr] 路径追加：({_currentDrawRoom.row},{_currentDrawRoom.col})，长度：{_drawnPath.Count}");
        }
    }

    /// <summary>
    /// 核心修复：完整判断六边形6个邻居（适配奇偶行）
    /// 六边形6个方向：上、下、左、右、左上/右上、左下/右下
    /// </summary>
    bool IsHexNeighbor(HexRoom a, HexRoom b){
        if (a == null || b == null) return false;

        int rowA = a.row;
        int colA = a.col;
        int rowB = b.row;
        int colB = b.col;

        // 计算行差和列差
        int dRow = rowB - rowA;
        int dCol = colB - colA;

        // 判断是否是6个邻居方向之一（核心修复）
        bool isNeighbor = false;

        // 1. 上下邻居（行差±1，列差根据行奇偶调整）
        if (dRow == 1 || dRow == -1){
            // 判断当前行是否是奇数行（根据配置）
            bool isOddRow = (rowA % 2 == 1) == isOddRowStaggered;
            if (isOddRow)
                // 奇数行：下/上邻居的列差为 0 或 +1
                isNeighbor = (dCol == 0) || (dCol == 1);
            else
                // 偶数行：下/上邻居的列差为 0 或 -1
                isNeighbor = (dCol == 0) || (dCol == -1);
        }
        // 2. 左右邻居（行差0，列差±1）
        else if (dRow == 0)
        
            isNeighbor = (dCol == 1) || (dCol == -1);
       
        // 调试：打印邻居判断结果
        if (debugNeighborCheck && isNeighbor)
            Debug.Log($"[HexPathDrawMgr] 地块({rowA},{colA}) ↔ ({rowB},{colB}) 是邻居（dRow:{dRow}, dCol:{dCol}）");
        

        return isNeighbor;
    }

    /// <summary>
    /// 辅助方法：获取一个地块的所有6个邻居（用于调试/验证）
    /// </summary>
    public List<HexRoom> GetAllHexNeighbors(HexRoom room)
    {
        List<HexRoom> neighbors = new List<HexRoom>();
        if (room == null || _gridManager == null) return neighbors;

        int row = room.row;
        int col = room.col;
        bool isOddRow = (row % 2 == 1) == isOddRowStaggered;

        // 定义6个邻居的坐标偏移（完整覆盖）
        List<Vector2Int> neighborOffsets = new List<Vector2Int>();
        // 左右邻居
        neighborOffsets.Add(new Vector2Int(row, col + 1)); // 右
        neighborOffsets.Add(new Vector2Int(row, col - 1)); // 左
        // 上下邻居（根据行奇偶调整）
        if (isOddRow)
        {
            neighborOffsets.Add(new Vector2Int(row + 1, col));   // 下1
            neighborOffsets.Add(new Vector2Int(row + 1, col + 1));// 下2
            neighborOffsets.Add(new Vector2Int(row - 1, col));   // 上1
            neighborOffsets.Add(new Vector2Int(row - 1, col + 1));// 上2
        }
        else
        {
            neighborOffsets.Add(new Vector2Int(row + 1, col));   // 下1
            neighborOffsets.Add(new Vector2Int(row + 1, col - 1));// 下2
            neighborOffsets.Add(new Vector2Int(row - 1, col));   // 上1
            neighborOffsets.Add(new Vector2Int(row - 1, col - 1));// 上2
        }

        // 从网格管理器获取有效邻居
        foreach (var offset in neighborOffsets)
        {
            if (_gridManager.GetHexRoomMap().TryGetValue(offset, out HexRoom neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    #endregion

    #region 路径可视化（无变化）
    private void RefreshPathVisual()
    {
        ClearPathVisual();

        if (_drawnPath.Count == 0 || _walkablePathMat == null)
        {
            return;
        }

        if (_playerStartRoom)
        {
            MeshRenderer player_renderer = _playerStartRoom.GetComponent<MeshRenderer>();
            _playerRoom_OriginMat = player_renderer.material;//记录原始材质
            player_renderer.material = _playerRoomMat;
            player_renderer.enabled = true;
        }
        foreach (HexRoom room in _drawnPath)
        {
            MeshRenderer renderer = room.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"[HexPathDrawMgr] 地块({room.row},{room.col}) 无MeshRenderer");
                continue;
            }

            if (!_originMatCache.ContainsKey(room))
            {
                _originMatCache[room] = renderer.material;
            }

            renderer.material = _walkablePathMat;
            renderer.enabled = true;

            if (enableDebugLog)
            {
                Debug.Log($"[HexPathDrawMgr] 可视化：({room.row},{room.col})");
            }
        }
    }

    private void ClearPathVisual()
    {
        foreach (var kvp in _originMatCache)
        {
            HexRoom room = kvp.Key;
            Material originMat = kvp.Value;

            MeshRenderer renderer = room.GetComponent<MeshRenderer>();
            if (renderer != null){
                renderer.material = originMat;
            }
        }
        _originMatCache.Clear();
    }

    void ResetPlayer_currentRoom() {
        if (_playerRoom_OriginMat != null)
        {
            MeshRenderer player_renderer = _playerStartRoom.GetComponent<MeshRenderer>();
            player_renderer.material = _playerRoom_OriginMat;
        }
    }
    #endregion
}