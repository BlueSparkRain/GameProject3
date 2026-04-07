using Core;
using System.Collections;
using UnityEngine;

public class GameMapManager : MonoGlobalManager
{
    float x_Offset;//每行内的偏移
    float y_Offset;//相邻行的偏移

    [Header("=== 正六边形地图设置 ===")]
    public int MapRadius = 20;
    private int MapRow;
    private int maxCol; // 正六边形最大列数

    [Header("=== 地块材质配置（新增） ===")]
    public Material oceanMat;        // 海洋材质
    public Material landMat;         // 陆地材质
    public Material obstacle_TreeMat;   
    public Material obstacle_StoneMat;    
    public Material obstacle_MountainMat;    

    public Material walkable_BattleRoomMat;  
    public Material walkable_EventRoomMat;  
    public Material walkable_RewardRoomMat;  
    public Material walkable_CityRoomMat;


    MapSaveSOData mapSaveData;
    float rowBatchInterval = 0.05f;
    float bornRoomInterval = 0.03f;
    Vector3 MapPivotPos;

    // 缓存所有生成的地块（新增）
    private HexRoomData[,] allCells;

    HexMapInteractManager hexGridClickManager;
    CoroutineManager coroutineManager;

    public string mapdataBack;
    public override void MgrUpdate(float deltaTime) { }
    public void GameMapManagerInit(float _x_offset, float _y_offset, int _MapRadius, Vector3 _MapPivotPos)
    {
        x_Offset = _x_offset;
        y_Offset = _y_offset;
        MapRadius = _MapRadius;
        MapRow = MapRadius * 2 + 1;
        maxCol = MapRow; // 最大列数=总行数
        MapPivotPos = _MapPivotPos;
        mapSaveData = ResourcesLoader.FindMapSaveData(mapdataBack);
        mapSaveData.mapRadius = MapRadius;
        mapSaveData.InitializeIfEmpty();
        Debug.Log(mapSaveData + " --" + mapSaveData.cellData+ "??");
        // 初始化地块缓存数组（新增）
        allCells = new HexRoomData[MapRow, maxCol];
        EventCenter.AddEventListener<Vector2Int, E_HexTerrainType>(E_EventType.Editor_Terrain, UpdateHexTag);
    }

    public void CreateWholeMap()
    {
        // 加载材质
        oceanMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Ocean");
        landMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Land");
        obstacle_TreeMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Tree");
        obstacle_StoneMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Stone");
        obstacle_MountainMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Mountain");
        
        walkable_BattleRoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Battle");
        walkable_EventRoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Event");
        walkable_RewardRoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_Reward");
        walkable_CityRoomMat = Resources.Load<Material>("Material/HexRoom/2.0/Base_HexRoom_City");
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        hexGridClickManager = GameRoot.GetManager<HexMapInteractManager>();

        // 优先读取存档数据生成地图
        if (mapSaveData != null && mapSaveData.cellData != null)
        {
            LoadMapFromSaveData();
        }

    }

    #region 正六边形地图生成（你原有逻辑，无修改）
    IEnumerator MapCreateCoro()
    {
        WaitForSeconds rowBatchDealy = new WaitForSeconds(rowBatchInterval);
        bool fromleft = true;
        for (int row = 0; row < MapRow; row++)
        {
            coroutineManager.StartCoroutine(CreatRowRooms(row, fromleft));
            fromleft = !fromleft;
            yield return rowBatchDealy;
        }
        EventCenter.EventTrigger(E_EventType.LoadMapEnd);
    }
    IEnumerator CreatRowRooms(int row, bool fromleft)
    {
        WaitForSeconds roomDealy = new WaitForSeconds(bornRoomInterval);
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

    #region 核心功能：地块创建+类型控制+存档（新增+修改）
    void CreateOneRoom(int _row, int _col)
    {
        E_HexTerrainType cellType = E_HexTerrainType.Obstacle__Ocean;
        // 如果有存档，读取存档类型（新增）
        if (mapSaveData != null && mapSaveData.cellData != null)
        {
            cellType = mapSaveData.cellData[_row, _col];
        }

        // 判断是否可行走（新增）
        bool isWalkable = cellType == E_HexTerrainType.Walkable_EmptyLand;

        var newHexRoom = CreateHexRoom(_row, _col, isWalkable);
        allCells[_row, _col] = newHexRoom; // 缓存地块（新增）

        newHexRoom.SetCellState(isWalkable);
        newHexRoom.GetComponent<HexJumpAnimation>().TriggerJump(0.4f);

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
            case E_HexTerrainType.Obstacle__Ocean : renderer.material = oceanMat; break;
            case E_HexTerrainType.Walkable_EmptyLand: renderer.material = landMat; break;
            case E_HexTerrainType.Obstacle_Tree: renderer.material = obstacle_TreeMat; break;
            case E_HexTerrainType.Obstacle_Stone: renderer.material = obstacle_StoneMat; break;
            case E_HexTerrainType.Obstacle_Mountain: renderer.material = obstacle_MountainMat; break;
            
            case E_HexTerrainType.Walkable_BattleRoom: renderer.material = walkable_BattleRoomMat; break;
            case E_HexTerrainType.Walkable_EventRoom: renderer.material = walkable_EventRoomMat; break;
            case E_HexTerrainType.Walkable_RewardRoom: renderer.material = walkable_RewardRoomMat; break;
            case E_HexTerrainType.Walkable_CityRoom: renderer.material = walkable_CityRoomMat; break;
        }
    }

    HexRoomData CreateHexRoom(int row, int col, bool walkable, E_HexRoomType e_HexRoomType = E_HexRoomType.None_无)
    {
        HexRoomData newHexRoom = GameRoot.GetManager<ObjectPoolManager>()
            .GetInstance(EPoolType.MapRoom_地图房间).GetComponent<HexRoomData>();

        if (row % 2 == 0)
            newHexRoom.GetComponent<HexJumpAnimation>().InitPos(MapPivotPos + new Vector3(y_Offset * col, 0, x_Offset * row));
        else
            newHexRoom.GetComponent<HexJumpAnimation>().InitPos(MapPivotPos + new Vector3(y_Offset * (col + 0.5f), 0, x_Offset * row));
        
        if (newHexRoom){
            newHexRoom.InitRoomID(row, col, e_HexRoomType);
            hexGridClickManager.RegisterHexRoom(newHexRoom, walkable);
        }
        return newHexRoom;
    }
    #endregion

    #region 编辑功能：点击切换地块类型+保存数据（新增）


    /// <summary>
    /// 设置地块类型（新增）
    /// </summary>
    public void UpdateHexTag(Vector2Int pos, E_HexTerrainType type)
    {
        if (allCells[pos.x, pos.y] == null) return;

        // 更新行走状态
        bool isWalkable = type == E_HexTerrainType.Walkable_EmptyLand;
        allCells[pos.x, pos.y].SetCellState(isWalkable);
        hexGridClickManager.RegisterHexRoom(allCells[pos.x, pos.y], isWalkable);
        Debug.Log("更新材质！");
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
        allCells = new HexRoomData[MapRow, maxCol];

        // 按存档生成
        StartCoroutine(MapCreateCoro());
    }
    #endregion

    // 你原有方法（无修改）
    void ReplaceOuterMat(HexRoomData room)
    {
        MeshRenderer renderer = room.GetComponent<MeshRenderer>();
        renderer.material = landMat;
        renderer.enabled = true;
    }
}