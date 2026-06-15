using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// 管理初始地图的加载+历史状态存档读取
/// 随机地块获取
/// 游戏内地块状态更新+存档保存
/// </summary>
public class GameMapManager : MonoGlobalManager
{
    [Header("地图设置")]
    public int MapCols = 55;
    public int MapRows = 35;
    public E_MapShape mapShape = E_MapShape.Rectangle;

    float x_Offset;//每行内的偏移
    float y_Offset;//相邻行的偏移

    private int mapRow;
    private int mapCol;

    [Header("角色高度")]
    [Tooltip("角色站在地图上的Y轴偏移")]
    public float characterYOffset = 3f;

    [Header("HexRoom材质覆写")]
    [Tooltip("开启后跳过地块材质设置，保留预制件原始材质")]
    public bool useCustomHexRoomMaterial = false;

    #region 地块材质配置
    private Material obstacle_oceanMat;       
    private Material walkable_landMat;         
    private Material obstacle_TreeMat;
    private Material obstacle_StoneMat;
    private Material obstacle_MountainMat;

    private Material walkable_BattleLow_RoomMat;
    private Material walkable_BattleMid_RoomMat;
    private Material walkable_BattleHigh_RoomMat;
    private Material walkable_EventRoomMat;
    private Material walkable_RewardRoomMat;
    private Material walkable_CityRoomMat;
    #endregion

    #region 地图生成
    //初始化地图数据
    MapSaveSOData mapSaveData;
    //地图数据后缀-动态资源加载
    public string mapdataBack;
    //行批次延迟
    float rowBatchInterval = 0.02f;
    //相邻房间延迟
    float bornRoomInterval = 0.005f;
    //地图锚点
    Vector3 MapPivotPos;
    // 缓存所有生成的地块
    private HexRoomTag[,] allCells;
    #endregion

    CoroutineManager coroutineManager;

    #region 运行时地块管理
    // 坐标到房间的映射表（高效查找）
    Dictionary<Vector2Int, HexRoomTag> _hexRoomMap = new Dictionary<Vector2Int, HexRoomTag>();
    Dictionary<Vector2Int, bool> _walkableDic = new Dictionary<Vector2Int, bool>();
    public Dictionary<Vector2Int, bool> WalkableDic => _walkableDic;
    public Dictionary<Vector2Int, HexRoomTag> HexRoomMap => _hexRoomMap;

    /// <summary>
    /// 根据行列计算世界坐标（与HexJumpAnimHandler.InitPos公式一致）
    /// </summary>
    public Vector3 CalculateRoomWorldPos(int row, int col)
    {
        if (row % 2 == 0)
            return MapPivotPos + new Vector3(y_Offset * col, 0, x_Offset * row);
        else
            return MapPivotPos + new Vector3(y_Offset * (col + 0.5f), 0, x_Offset * row);
    }

    /// <summary>
    /// 仅从mapSaveData预注册可行走位置数据（不创建GameObject），供GetRnadomRoom在地图生成期间查询
    /// </summary>
    List<Vector2Int> _preRegWalkablePositions = new List<Vector2Int>();

    void PreRegisterWalkableFromSaveData()
    {
        _preRegWalkablePositions.Clear();
        for (int row = 0; row < mapRow; row++)
        {
            GetRowColRange(row, out int startCol, out int endCol);
            for (int col = startCol; col <= endCol; col++)
            {
                if (IsTerrainWalkabke(mapSaveData.cellData[row, col]))
                    _preRegWalkablePositions.Add(new Vector2Int(row, col));
            }
        }
    }

    public HexRoomTag GetRandomRoomInBounds(int minCol, int maxCol, int minRow, int maxRow)
    {
        List<HexRoomTag> validRooms = new List<HexRoomTag>();

        foreach (var kvp in _hexRoomMap)
        {
            Vector2Int pos = kvp.Key;
            HexRoomTag room = kvp.Value;

            // 判断是否在范围内 + 是否可走
            if (pos.x >= minCol && pos.x <= maxCol &&
                pos.y >= minRow && pos.y <= maxRow &&
                room.walkable)
            {
                validRooms.Add(room);
            }
        }

        if (validRooms.Count == 0)
        {
            DebugManager.LogWarning(EDebugCategory.MapRoom, "区域内无可用房间");
            return null;
        }

        return validRooms[UnityEngine.Random.Range(0, validRooms.Count)];
    }

    public HexRoomTag GetRnadomRoom()
    {
        if (_hexRoomMap.Count == 0)
        {
            if (_preRegWalkablePositions.Count == 0)
            {
                Debug.LogError("地图尚未生成，无法获取随机房间");
                return null;
            }
            var prePos = _preRegWalkablePositions[UnityEngine.Random.Range(0, _preRegWalkablePositions.Count)];
            var cellType = mapSaveData.cellData[prePos.x, prePos.y];
            bool walkable = IsTerrainWalkabke(cellType);
            var room = CreateHexRoom(prePos.x, prePos.y, walkable, cellType);
            room.SetCellState(walkable);
            room.transform.DOScale(new Vector3(1, 1, 0.5f), 0.4f).From(new Vector3(0.7f, 0.7f, 0));
            room.GetComponent<HexJumpAnimHandler>().TriggerJump(0.3f);
            SetCellMaterial(room, cellType);
            allCells[prePos.x, prePos.y] = room;
            return room;
        }

        int maxAttempts = _hexRoomMap.Count * 10;
        HexRoomTag hexRoomData = null;
        do
        {
            hexRoomData = _hexRoomMap.GetRandomElement().Value;
        }
        while (!hexRoomData.walkable && --maxAttempts > 0);

        if (!hexRoomData.walkable)
        {
            Debug.LogError("未找到可行走房间");
            return null;
        }

        return hexRoomData;
    }

    /// <summary>获取地图中心附近的一个可行走房间（玩家出生点）</summary>
    public HexRoomTag GetCenterWalkableRoom()
    {
        int centerRow = mapRow / 2;
        int centerCol = mapCol / 2;

        for (int radius = 0; radius < Mathf.Max(mapRow, mapCol); radius++)
        {
            for (int dr = -radius; dr <= radius; dr++)
            {
                for (int dc = -radius; dc <= radius; dc++)
                {
                    if (Mathf.Abs(dr) != radius && Mathf.Abs(dc) != radius) continue;
                    int r = centerRow + dr;
                    int c = centerCol + dc;
                    if (r < 0 || r >= mapRow || c < 0 || c >= mapCol) continue;
                    var key = new Vector2Int(r, c);
                    if (_hexRoomMap.TryGetValue(key, out var room) && room.walkable)
                        return room;
                }
            }
        }
        return GetRnadomRoom();
    }

    public HexRoomTag GetTargetRoom(Vector2Int pos)
    {
        if (_hexRoomMap.TryGetValue(pos, out var room))
            return room;
        var cellType = mapSaveData.cellData[pos.x, pos.y];
        bool walkable = IsTerrainWalkabke(cellType);
        var newRoom = CreateHexRoom(pos.x, pos.y, walkable, cellType);
        newRoom.SetCellState(walkable);
        newRoom.transform.DOScale(new Vector3(1, 1, 0.5f), 0.4f).From(new Vector3(0.7f, 0.7f, 0));
        newRoom.GetComponent<HexJumpAnimHandler>().TriggerJump(0.3f);
        SetCellMaterial(newRoom, cellType);
        allCells[pos.x, pos.y] = newRoom;
        return newRoom;
    }
    #endregion


    /// <summary>
    /// 注册一个六边形房间到映射表，登记数据+可通行性
    /// </summary>
    public void RegisterHexRoom(HexRoomTag room, bool walkable){

        Vector2Int key = new Vector2Int(room.row, room.col);
        if (!_hexRoomMap.ContainsKey(key))
        {
            _hexRoomMap.Add(key, room);
            _walkableDic.Add(key, walkable);
        }
    }

    public override void MgrUpdate(float deltaTime) { }

    /// <summary>
    /// 地图初始化:
    /// (1)读取SO数据加载自定义的初始地图
    /// (2)依据存档信息来替换某些发生变化的地块的信息
    /// </summary>
    /// <param resName="_x_offset"></param>
    /// <param resName="_y_offset"></param>
    /// <param resName="_MapRadius"></param>
    /// <param resName="_MapPivotPos"></param>
    public void GameMapManagerInit(float _x_offset, float _y_offset, int _mapCols, int _mapRows, Vector3 _MapPivotPos){
        x_Offset = _x_offset;
        y_Offset = _y_offset;
        mapRow = _mapRows;
        mapCol = _mapCols;
        MapPivotPos = _MapPivotPos;
        mapSaveData = ResourcesLoader.FindMapSaveData(mapdataBack);
        mapSaveData.mapCols = _mapCols;
        mapSaveData.mapRows = _mapRows;
        mapSaveData.mapShape = mapShape;
        mapSaveData.InitializeIfEmpty();
        allCells = new HexRoomTag[mapRow, mapCol];
        EventCenter.AddEventListener<Vector2Int, E_HexTerrainType>(E_EventType.Editor_Terrain, UpdateHexTag);
    }
    public void CreateWholeMap(){
        // 加载材质
        obstacle_oceanMat = GetVisableMat("2.0", "Base_HexRoom_Ocean"); 
        walkable_landMat = GetVisableMat("2.0", "Base_HexRoom_Land");                  
        obstacle_TreeMat = GetVisableMat("2.0", "Base_HexRoom_Tree");                  
        obstacle_StoneMat = GetVisableMat("2.0", "Base_HexRoom_Stone");                
        obstacle_MountainMat =       GetVisableMat("2.0", "Base_HexRoom_Mountain");    
        walkable_BattleLow_RoomMat = GetVisableMat("2.0", "Base_HexRoom_BattleLow");   
        walkable_BattleMid_RoomMat = GetVisableMat("2.0", "Base_HexRoom_BattleMid");   
        walkable_BattleHigh_RoomMat =GetVisableMat("2.0", "Base_HexRoom_BattleHigh");  
        walkable_EventRoomMat =      GetVisableMat("2.0", "Base_HexRoom_Event");       
        walkable_RewardRoomMat =     GetVisableMat("2.0", "Base_HexRoom_Reward");      
        walkable_CityRoomMat =       GetVisableMat("2.0", "Base_HexRoom_City");        
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
      
        // 优先读取SO数据生成地图
        if (mapSaveData != null && mapSaveData.cellData != null){
            LoadMapFromSaveData();
        }
    }
    Material GetVisableMat(string version,string resName){ return Resources.Load<Material>($"Material/HexRoom/{version}/{resName}"); }

    #region 正六边形地图生成
    IEnumerator MapCreateCoro()
    {
        WaitForSeconds rowBatchDealy = new WaitForSeconds(rowBatchInterval);

        int i = 0;
        bool fromleft = true;
        for (int row = 0; row < mapRow; row++)
        {
            i++;
            StartCoroutine(CreatRowRooms(row, fromleft));
            fromleft = !fromleft;
            //yield return i % 2 == 0 ? rowBatchDealy : null;
            yield return null;
        }
        EventCenter.EventTrigger(E_EventType.LoadMapEnd);
    }
    IEnumerator CreatRowRooms(int row, bool fromleft)
    {
        WaitForSeconds roomDealy;
      
        roomDealy = new WaitForSeconds(bornRoomInterval);

        GetRowColRange(row, out int startCol, out int endCol);

        if (fromleft)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                CreateOneRoom(row, col);
                yield return roomDealy;
            }
        }
        else
        {
            for (int col = endCol; col >= startCol; col--)
            {
                CreateOneRoom(row, col);
                yield return roomDealy;
            }
        }
    }

    private void GetRowColRange(int row, out int startCol, out int endCol)
    {
        if (mapShape == E_MapShape.Rectangle)
        {
            startCol = 0;
            endCol = mapCol - 1;
            return;
        }

        int center = mapRow / 2;
        int offset = Mathf.Abs(row - center);
        int width = mapRow - offset;
        startCol = offset / 2;
        endCol = startCol + width - 1;
    }
    #endregion


    bool IsTerrainWalkabke(E_HexTerrainType cellType) {

        return cellType == E_HexTerrainType.Walkable_EmptyLand ||
               cellType == E_HexTerrainType.Walkable_CityShopRoom ||
               cellType == E_HexTerrainType.Walkable_RewardRoom ||
               cellType == E_HexTerrainType.Walkable_UnknownEventRoom ||
               cellType == E_HexTerrainType.Walkable_LowLevel_BattleRoom ||
               cellType == E_HexTerrainType.Walkable_MidLevel_BattleRoom ||
               cellType == E_HexTerrainType.Walkable_HighLevel_BattleRoom;
    }

    #region 地块创建+类型控制+存档
    void CreateOneRoom(int _row, int _col)
    {
        // 已被GetRnadomRoom/GetTargetRoom按需提前创建，跳过创建只补动画
        if (allCells[_row, _col] != null)
        {
            var existRoom = allCells[_row, _col];
            existRoom.transform.DOScale(new Vector3(1, 1, 0.5f), 0.2f).From(new Vector3(0.7f, 0.7f, 0));
            existRoom.GetComponent<HexJumpAnimHandler>().TriggerJump(0.3f);
            return;
        }

        E_HexTerrainType cellType = E_HexTerrainType.Obstacle_Ocean;
        if (mapSaveData != null && mapSaveData.cellData != null)
        {
            cellType = mapSaveData.cellData[_row, _col];
        }

        bool isWalkable = IsTerrainWalkabke(cellType);
        var newHexRoom = CreateHexRoom(_row, _col, isWalkable,cellType);
        allCells[_row, _col] = newHexRoom;

        newHexRoom.SetCellState(isWalkable);

        newHexRoom.transform.DOScale(new Vector3(1,1,0.5f), 0.2f).From(new Vector3(0.7f,0.7f,0));
        newHexRoom.GetComponent<HexJumpAnimHandler>().TriggerJump(0.15f);

        SetCellMaterial(newHexRoom, cellType);
    }

    /// <summary>
    /// 根据地块类型设置材质（新增）
    /// </summary>
    void SetCellMaterial(HexRoomTag room, E_HexTerrainType type)
    {
        if (useCustomHexRoomMaterial) return;
        MeshRenderer renderer = room.GetComponent<MeshRenderer>();
        renderer.enabled = true;

        switch (type)
        {
            case E_HexTerrainType.Obstacle_Ocean : renderer.material = obstacle_oceanMat; break;
            case E_HexTerrainType.Walkable_EmptyLand: renderer.material = walkable_landMat; break;
            case E_HexTerrainType.Obstacle_Tree: renderer.material = obstacle_TreeMat; break;
            case E_HexTerrainType.Obstacle_Stone: renderer.material = obstacle_StoneMat; break;
            case E_HexTerrainType.Obstacle_Mountain: renderer.material = obstacle_MountainMat; break;
            
            case E_HexTerrainType.Walkable_LowLevel_BattleRoom: renderer.material = walkable_BattleLow_RoomMat; break;
            case E_HexTerrainType.Walkable_MidLevel_BattleRoom: renderer.material = walkable_BattleMid_RoomMat; break;
            case E_HexTerrainType.Walkable_HighLevel_BattleRoom: renderer.material = walkable_BattleHigh_RoomMat; break;

            case E_HexTerrainType.Walkable_UnknownEventRoom: renderer.material = walkable_EventRoomMat; break;
            case E_HexTerrainType.Walkable_RewardRoom: renderer.material = walkable_RewardRoomMat; break;
            case E_HexTerrainType.Walkable_CityShopRoom: renderer.material = walkable_CityRoomMat; break;
        }
    }



    HexRoomTag CreateHexRoom(int row, int col, bool walkable, E_HexTerrainType cellType, bool playAnim = true)
    {
        HexRoomTag newHexRoomTag = GameRoot.GetManager<ObjectPoolManager>()
            .GetInstance(E_PoolType.MapRoom_地图房间).GetComponent<HexRoomTag>();

        if (row % 2 == 0)
            newHexRoomTag.GetComponent<HexJumpAnimHandler>().InitPos(MapPivotPos + new Vector3(y_Offset * col, 0, x_Offset * row));
        else
            newHexRoomTag.GetComponent<HexJumpAnimHandler>().InitPos(MapPivotPos + new Vector3(y_Offset * (col + 0.5f), 0, x_Offset * row));

        if (newHexRoomTag)
        {
            newHexRoomTag.InitRoomTag(row, col);
            newHexRoomTag.GetComponent<HexRoomHandler>().InitHexRoomHandler(newHexRoomTag, cellType, playAnim);
            //newHexRoomTag.GetComponent<HexTerrainStyleHandler>().InitTerrainStyle(cellType);
            RegisterHexRoom(newHexRoomTag, walkable);
        }

        CreateHexFace(row, col);

        return newHexRoomTag;
    }

    void CreateHexFace(int row, int col)
    {
        var faceObj = GameRoot.GetManager<ObjectPoolManager>().GetInstance(E_PoolType.HexFace_投影面片);
        if (faceObj == null) return;

        var faceTag = faceObj.GetComponent<HexFaceTag>();
        if (faceTag == null) return;

        faceTag.Init(row, col);
        Vector3 worldPos = CalculateRoomWorldPos(row, col);
        faceObj.transform.position = worldPos + Vector3.up * 0.05f;
        //faceObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        faceObj.transform.rotation = Quaternion.identity;
    }
    #endregion

    #region 地图编辑功能：点击切换地块类型+保存数据
    /// <summary>
    /// 设置地块类型（新增）
    /// </summary>
    public void UpdateHexTag(Vector2Int pos, E_HexTerrainType type)
    {
        if (allCells[pos.x, pos.y] == null) return;
        // 更新行走状态
        bool isWalkable = IsTerrainWalkabke(type);
        allCells[pos.x, pos.y].SetCellState(isWalkable);
        //更新地块类型后需要重新注册
        RegisterHexRoom(allCells[pos.x, pos.y], isWalkable);
        // 更新材质
        SetCellMaterial(allCells[pos.x, pos.y], type);
    }

    /// <summary>
    /// 从存档加载地图（新增）
    /// </summary>
    [ContextMenu("从存档加载地图")]
    public void LoadMapFromSaveData()
    {
        if (mapSaveData == null || mapSaveData.cellData == null)
        {
            Debug.LogError("存档数据为空！");
            return;
        }

        // 清空旧地图
        StopAllCoroutines();
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        _hexRoomMap.Clear();
        _walkableDic.Clear();
        allCells = new HexRoomTag[mapRow, mapCol];

        // 预注册可行走位置数据（仅数据，不创建GameObject），GetRnadomRoom/GetTargetRoom立即可用
        PreRegisterWalkableFromSaveData();

        // 分批创建房间 + 动画展示
        StartCoroutine(MapCreateCoro());
    }
    #endregion

    // 你原有方法（无修改）
    void ReplaceOuterMat(HexRoomTag room)
    {
        if (useCustomHexRoomMaterial) return;
        MeshRenderer renderer = room.GetComponent<MeshRenderer>();
        renderer.material = walkable_landMat;
        renderer.enabled = true;
    }
}