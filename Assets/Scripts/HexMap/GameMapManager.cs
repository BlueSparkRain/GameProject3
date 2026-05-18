using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理初始地图的加载+历史状态存档读取
/// 随机地块获取
/// 游戏内地块状态更新+存档保存
/// </summary>
public class GameMapManager : MonoGlobalManager
{
    [Header("正六边形地图设置")]
    public int MapRadius = 20;

    float x_Offset;//每行内的偏移
    float y_Offset;//相邻行的偏移

    private int mapRow;
    private int mapCol; 

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
    float rowBatchInterval = 0.01f;
    //相邻房间延迟
    float bornRoomInterval = 0.002f;
    //地图锚点
    Vector3 MapPivotPos;
    // 缓存所有生成的地块
    private HexRoomData[,] allCells;
    #endregion

    CoroutineManager coroutineManager;

    #region 运行时地块管理
    // 坐标到房间的映射表（高效查找）
    Dictionary<Vector2Int, HexRoomData> _hexRoomMap = new Dictionary<Vector2Int, HexRoomData>();
    Dictionary<Vector2Int, bool> _walkableDic = new Dictionary<Vector2Int, bool>();
    public Dictionary<Vector2Int, bool> WalkableDic => _walkableDic;
    public Dictionary<Vector2Int, HexRoomData> HexRoomMap => _hexRoomMap;

    public HexRoomData GetRandomRoomInBounds(int minCol, int maxCol, int minRow, int maxRow)
    {
        List<HexRoomData> validRooms = new List<HexRoomData>();

        foreach (var kvp in _hexRoomMap)
        {
            Vector2Int pos = kvp.Key;
            HexRoomData room = kvp.Value;

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
            Debug.LogWarning("区域内无可用房间");
            return null;
        }

        return validRooms[UnityEngine.Random.Range(0, validRooms.Count)];
    }

    //寻找一个完全随机的地块
    public HexRoomData GetRnadomRoom()
    {
        if (_hexRoomMap.Count == 0)
        {
            Debug.LogError("地图尚未生成，无法获取随机房间");
            return null;
        }

        Debug.Log(_hexRoomMap.Count+"dasdhkd");
        int maxAttempts = _hexRoomMap.Count * 10;
        HexRoomData hexRoomData = null;
        do
        {
            hexRoomData = _hexRoomMap.GetRandomElement().Value;
        }
        while (!hexRoomData.walkable && --maxAttempts > 0);
        //while (!hexRoomData.walkable) ;

        if (!hexRoomData.walkable)
        {
            Debug.LogError("未找到可行走房间，请检查地图配置");
            return null;
        }

        //Debug.Log(hexRoomData.roomType+":"+hexRoomData.walkable);
        return hexRoomData;
    }

    /// <summary>
    /// 寻找一个确定的房间
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public HexRoomData GetTargetRoom(Vector2Int pos) {
        Debug.Log(_hexRoomMap.Count + "小小");
        Debug.Log(_hexRoomMap[pos]+"大大");
        return  _hexRoomMap[pos];
    }
    #endregion


    /// <summary>
    /// 注册一个六边形房间到映射表，登记数据+可通行性
    /// </summary>
    public void RegisterHexRoom(HexRoomData room, bool walkable)
    {

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
    /// <param name="_x_offset"></param>
    /// <param name="_y_offset"></param>
    /// <param name="_MapRadius"></param>
    /// <param name="_MapPivotPos"></param>
    public void GameMapManagerInit(float _x_offset, float _y_offset, int _MapRadius, Vector3 _MapPivotPos)
    {
        x_Offset = _x_offset;
        y_Offset = _y_offset;
        MapRadius = _MapRadius;
        mapRow = MapRadius * 2 + 1;
        mapCol = mapRow; // 最大列数=总行数
        MapPivotPos = _MapPivotPos;
        mapSaveData = ResourcesLoader.FindMapSaveData(mapdataBack);
        mapSaveData.mapRadius = MapRadius;
        mapSaveData.InitializeIfEmpty();
        // 初始化地块缓存数组（新增）
        allCells = new HexRoomData[mapRow, mapCol];
        EventCenter.AddEventListener<Vector2Int, E_HexTerrainType>(E_EventType.Editor_Terrain, UpdateHexTag);
    }
    public void CreateWholeMap()
    {
        // 加载材质
        obstacle_oceanMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Ocean");
        walkable_landMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Land");
        obstacle_TreeMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Tree");
        obstacle_StoneMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Stone");
        obstacle_MountainMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Mountain");
        
        walkable_BattleLow_RoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_BattleLow");
        walkable_BattleMid_RoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_BattleMid");
        walkable_BattleHigh_RoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_BattleHigh");

        walkable_EventRoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Event");
        walkable_RewardRoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Reward");
        walkable_CityRoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_City");
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
      
        // 优先读取SO数据生成地图
        if (mapSaveData != null && mapSaveData.cellData != null)
        {
            LoadMapFromSaveData();
        }
    }

    #region 正六边形地图生成
    IEnumerator MapCreateCoro()
    {
        WaitForSeconds rowBatchDealy;
     
        rowBatchDealy = new WaitForSeconds(rowBatchInterval);
      
        
            bool fromleft = true;
        for (int row = 0; row < mapRow; row++)
        {
            coroutineManager.StartCoroutine(CreatRowRooms(row, fromleft));
            fromleft = !fromleft;
            yield return rowBatchDealy;
        }
        EventCenter.EventTrigger(E_EventType.LoadMapEnd);
    }
    IEnumerator CreatRowRooms(int row, bool fromleft)
    {
        WaitForSeconds roomDealy;
      
        roomDealy = new WaitForSeconds(bornRoomInterval);

        GetHexRowColRange(row, out int startCol, out int endCol);

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

    private void GetHexRowColRange(int row, out int startCol, out int endCol)
    {
        int center = MapRadius;
        int offset = Mathf.Abs(row - center);
        int width = MapRadius * 2 + 1 - offset;
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
        E_HexTerrainType cellType = E_HexTerrainType.Obstacle_Ocean;
        // 如果有存档，读取存档类型（新增）
        if (mapSaveData != null && mapSaveData.cellData != null)
        {
            cellType = mapSaveData.cellData[_row, _col];
        }

        // 更新行走状态
        bool isWalkable = IsTerrainWalkabke(cellType);
        var newHexRoom = CreateHexRoom(_row, _col, isWalkable,cellType);
        allCells[_row, _col] = newHexRoom; // 缓存地块（新增）

        newHexRoom.SetCellState(isWalkable);

        newHexRoom.transform.DOScale(new Vector3(1,1,0.5f), 0.4f).From(new Vector3(0.7f,0.7f,0));
        newHexRoom.GetComponent<HexJumpAnimation>().TriggerJump(0.3f);

        // 设置对应材质（新增）
        SetCellMaterial(newHexRoom, cellType);
    }

    /// <summary>
    /// 根据地块类型设置材质（新增）
    /// </summary>
    void SetCellMaterial(HexRoomData room, E_HexTerrainType type)
    {
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



    HexRoomData CreateHexRoom(int row, int col, bool walkable, E_HexTerrainType cellType)
    {
        HexRoomData newHexRoom = GameRoot.GetManager<ObjectPoolManager>()
            .GetInstance(E_PoolType.MapRoom_地图房间).GetComponent<HexRoomData>();

        if (row % 2 == 0)
            newHexRoom.GetComponent<HexJumpAnimation>().InitPos(MapPivotPos + new Vector3(y_Offset * col, 0, x_Offset * row));
        else
            newHexRoom.GetComponent<HexJumpAnimation>().InitPos(MapPivotPos + new Vector3(y_Offset * (col + 0.5f), 0, x_Offset * row));

        if (newHexRoom)
        {
            newHexRoom.GetComponent<HexTerrainStyleHandler>().SetTag(cellType);
            newHexRoom.InitRoomID(row, col);
            RegisterHexRoom(newHexRoom, walkable);
        }

        return newHexRoom;
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
        allCells = new HexRoomData[mapRow, mapCol];

        // 按存档生成
        StartCoroutine(MapCreateCoro());
    }
    #endregion

    // 你原有方法（无修改）
    void ReplaceOuterMat(HexRoomData room)
    {
        MeshRenderer renderer = room.GetComponent<MeshRenderer>();
        renderer.material = walkable_landMat;
        renderer.enabled = true;
    }
}