using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 六边形地图区块（单个分块）
/// </summary>
public class HexMapChunk
{
    public int ChunkRow { get; private set; }
    public int ChunkCol { get; private set; }
    public int ChunkSize { get; private set; }
    public (int row, int col)[] Cells { get; set; }
    public List<HexRoomData> RoomList { get; private set; } = new List<HexRoomData>();
    public bool IsGenerated { get; set; } = false;

    private float x_Offset;
    private float y_Offset;
    private Vector3 mapPivotPos;

    public HexMapChunk(int chunkRow, int chunkCol, int chunkSize, float xOffset, float yOffset, Vector3 pivotPos)
    {
        ChunkRow = chunkRow;
        ChunkCol = chunkCol;
        ChunkSize = chunkSize;
        x_Offset = xOffset;
        y_Offset = yOffset;
        mapPivotPos = pivotPos;
    }

    public void AddRoom(HexRoomData room)
    {
        if (room != null && !RoomList.Contains(room))
            RoomList.Add(room);
    }

    public (int startRow, int startCol) GetStartRoomCoord()
    {
        return (ChunkRow * ChunkSize, ChunkCol * ChunkSize);
    }
}

/// <summary>
/// 六边形地图 分块管理管理器（最终修复版）
/// </summary>
public class HexRoomChunkManager : MonoGlobalManager
{
    [Header("分块设置")]
    public int _chunkSize = 8;
    public float ChunkGenerateInterval = 0.1f;

    private Dictionary<(int row, int col), HexMapChunk> _chunkDict = new Dictionary<(int row, int col), HexMapChunk>();
    private HashSet<string> _generatedCells = new HashSet<string>();

    private GameMapManager _gameMapManager;
    private CoroutineManager _coroutineManager;
    private HexMapInteractManager _hexGridClickManager;
    private ObjectPoolManager _roomPool;

    private float _xOffset, _yOffset;
    private int _mapRow, _mapCol;
    private Vector3 _mapPivotPos;

    private Material _walkableMat;
    private HexMapNoiseData _terrainData;
    private WaitForSeconds _roomDelay;
    private WaitForSeconds _chunkDelay;

    [Header("分块形状")]
    public HexChunkGenerator.HexChunkShape chunkShape = HexChunkGenerator.HexChunkShape.Rectangle;
    [Range(2, 6)] public int hexChunkRadius = 4;

    public void InitChunkManager(GameMapManager mapManager, float xOffset, float yOffset,
        int mapRow, int mapCol, Vector3 pivotPos, float roomInterval,int chunkSize)
    {
        _gameMapManager = mapManager;
        _xOffset = xOffset;
        _yOffset = yOffset;
        _mapRow = mapRow;
        _mapCol = mapCol;
        _mapPivotPos = pivotPos;
        _chunkSize=chunkSize;
        _coroutineManager = GameRoot.GetManager<CoroutineManager>();
        _hexGridClickManager = GameRoot.GetManager<HexMapInteractManager>();
        _roomPool = GameRoot.GetManager<ObjectPoolManager>();

        _roomDelay = new WaitForSeconds(roomInterval);
        _chunkDelay = new WaitForSeconds(ChunkGenerateInterval);

        HexChunkGenerator.HexChunkRadius = hexChunkRadius;
    }

    public void SetMapResources(Material walkableMat, HexMapNoiseData terrainData)
    {
        _walkableMat = walkableMat;
        _terrainData = terrainData;
    }

    public void PrepareMapChunks()
    {
        ClearAllChunks();
        _generatedCells.Clear(); 
        _chunkDict.Clear();

        if (chunkShape == HexChunkGenerator.HexChunkShape.Rectangle)
            PrepareRectChunks();
        else
            PrepareHexChunks();

        Debug.Log($"地图分块准备完成 → 总区块数：{_chunkDict.Count}");
    }

    // 矩形区块（原有逻辑，无改动）
    private void PrepareRectChunks()
    {
        int totalChunkRows = Mathf.CeilToInt((float)_mapRow / _chunkSize);
        int totalChunkCols = Mathf.CeilToInt((float)_mapCol / _chunkSize);

        for (int r = 0; r < totalChunkRows; r++)
        {
            for (int c = 0; c < totalChunkCols; c++)
            {
                var chunk = new HexMapChunk(r, c, _chunkSize, _xOffset, _yOffset, _mapPivotPos);
                if (!_chunkDict.ContainsKey((r, c)))
                    _chunkDict.Add((r, c), chunk);
            }
        }
        _chunkEnumerator = _chunkDict.Values.GetEnumerator();

        foreach (var item in _chunkDict)
        {
            Debug.Log(item.Key+"--");
        }
    }

    private void PrepareHexChunks()
    {
        HexChunkGenerator.HexChunkRadius = hexChunkRadius;
        int stride = hexChunkRadius * 2 - 1;
        int maxY = Mathf.CeilToInt((float)_mapRow / stride);
        int maxX = Mathf.CeilToInt((float)_mapCol / stride);

        for (int y = 0; y < maxY; y++){
            for (int x = 0; x < maxX; x++){
                int chunkID = y * 100 + x;
                var cells = HexChunkGenerator.GetHexChunkCells(chunkID, _mapRow, _mapCol);
                if (cells.Length == 0) continue;

                var chunk = new HexMapChunk(y, x, hexChunkRadius, _xOffset, _yOffset, _mapPivotPos);
                chunk.Cells = cells;
                if (!_chunkDict.ContainsKey((y, x)))
                    _chunkDict.Add((y, x), chunk);
            }
        }
        _chunkEnumerator = _chunkDict.Values.GetEnumerator();
    }

    private IEnumerator<HexMapChunk> _chunkEnumerator;

    public Coroutine GenerateNextChunk()
    {
        if (_chunkEnumerator == null)
        {
            Debug.LogWarning("请先调用 PrepareMapChunks() 初始化区块!");
            return null;
        }

        HexMapChunk chunk = null;
        while (_chunkEnumerator.MoveNext())
        {
            chunk = _chunkEnumerator.Current;
            if (!chunk.IsGenerated) break;
        }

       
        EventCenter.EventTrigger(E_EventType.LoadMapEnd);
        if (chunk == null || chunk.IsGenerated)
        {
            Debug.Log("<color=green>所有区块已生成完毕！</color>");
            _chunkEnumerator.Dispose();
            return null;
        }

        chunk.IsGenerated = true;

        if (chunkShape == HexChunkGenerator.HexChunkShape.Rectangle)
            return StartCoroutine(GenerateSingleRectChunkCoro(chunk));
        else
            return StartCoroutine(GenerateSingleHexChunkCellsCoro(chunk, chunk.Cells));
        
    }
    /// <summary>
    /// 生成 指定区块（你也可以手动指定区块）
    /// </summary>
    public Coroutine GenerateSpecificChunk(HexMapChunk chunk)
    {
        return StartCoroutine(GenerateSingleRectChunkCoro(chunk));
    }

    /// <summary>
    /// 生成所有地块
    /// </summary>
    public void GenerateAllMapByChunk()
    {
        PrepareMapChunks();
        if (chunkShape == HexChunkGenerator.HexChunkShape.Rectangle)
            _coroutineManager.StartCoroutine(GenerateAllChunksCoro_Rect());
        else
            _coroutineManager.StartCoroutine(GenerateAllChunksCoro_Hex());
    }

    #region 矩形区块生成（原有逻辑，无改动）
    IEnumerator GenerateAllChunksCoro_Rect()
    {
        foreach (var chunk in _chunkDict.Values)
        {
            yield return _coroutineManager.StartCoroutine(GenerateSingleRectChunkCoro(chunk));
            yield return _chunkDelay;
        }
        EventCenter.EventTrigger(E_EventType.LoadMapStart);
        Debug.Log($"<color=green>矩形区块地图生成完成！</color>");
    }

    IEnumerator GenerateSingleRectChunkCoro(HexMapChunk chunk)
    {
        var (startRow, startCol) = chunk.GetStartRoomCoord();
        for (int row = startRow; row < startRow + _chunkSize && row < _mapRow; row++)
        {
            bool fromLeft = row % 2 == 0;
            if (fromLeft)
            {
                for (int col = startCol; col < startCol + _chunkSize && col < _mapCol; col++)
                {
                    CreateRoomInChunk(row, col, chunk);
                    yield return _roomDelay;
                }
            }
            else
            {
                for (int col = Mathf.Min(startCol + _chunkSize - 1, _mapCol - 1); col >= startCol; col--)
                {
                    CreateRoomInChunk(row, col, chunk);
                    yield return _roomDelay;
                }
            }
        }
    }
    #endregion

    #region 🔥 修复：六边形区块生成（无缝、无重复）
    IEnumerator GenerateAllChunksCoro_Hex()
    {
        foreach (var chunk in _chunkDict.Values)
        {
            yield return _coroutineManager.StartCoroutine(GenerateSingleHexChunkCellsCoro(chunk, chunk.Cells));
            yield return _chunkDelay;
        }
        EventCenter.EventTrigger(E_EventType.LoadMapEnd);
        Debug.Log("<color=#FFCE60>无缝六边形区块地图生成完成！</color>");
    }

    IEnumerator GenerateSingleHexChunkCellsCoro(HexMapChunk chunk, (int row, int col)[] cells)
    {
        foreach (var (row, col) in cells)
        {
            CreateRoomInChunk(row, col, chunk);
            yield return _roomDelay;
        }
    }
    #endregion

    #region 🔥 核心修复：房间创建（零重复、无越界）
    void CreateRoomInChunk(int row, int col, HexMapChunk chunk)
    {
        // 1. 边界校验
        if (row < 0 || row >= _mapRow || col < 0 || col >= _mapCol)
            return;

        // 2. 🔥 全局重复校验（无论哪个区块，生成过直接跳过）
        string cellKey = $"{row}_{col}";
        if (_generatedCells.Contains(cellKey))
            return;

        // 3. 标记已生成
        _generatedCells.Add(cellKey);

        // 4. 原有创建逻辑（无改动）
        bool isWalkable = _terrainData.terrainMap[row, col];
        HexRoomData newRoom = CreateHexRoom(row, col, isWalkable);
        if (newRoom == null) return;

        newRoom.SetCellState(isWalkable);
        newRoom.GetComponent<HexJumpAnimation>().TriggerJump(0.4f);
        if (isWalkable) ReplaceRoomMat(newRoom);
        chunk.AddRoom(newRoom);
    }

    // 原有房间创建逻辑（完全保留，无改动）
    HexRoomData CreateHexRoom(int row, int col, bool walkable)
    {
        HexRoomData newHexRoom = _roomPool.GetInstance(EPoolType.MapRoom_地图房间).GetComponent<HexRoomData>();

        if (row % 2 == 0)
        {
            newHexRoom.GetComponent<HexJumpAnimation>().InitPos(_mapPivotPos + new Vector3(_yOffset * col, 0, _xOffset * row));
        }
        else
        {
            newHexRoom.GetComponent<HexJumpAnimation>().InitPos(_mapPivotPos + new Vector3(_yOffset * (col + 0.5f), 0, _xOffset * row));
        }

        E_HexRoomType roomType = E_HexRoomType.None_无;
        if (row == 2 && col == 2) roomType = E_HexRoomType.Battle_战斗;
        else if (row == 4 && col == 5) roomType = E_HexRoomType.NPC_特定交互;

        newHexRoom.InitRoomID(row, col, roomType);
        _hexGridClickManager.RegisterHexRoom(newHexRoom, walkable);

        return newHexRoom;
    }

    void ReplaceRoomMat(HexRoomData room)
    {
        MeshRenderer renderer = room.GetComponent<MeshRenderer>();
        renderer.material = _walkableMat;
        renderer.enabled = true;
    }
    #endregion

    #region 扩展功能（保留原有逻辑）
    public HexMapChunk GetChunkByRoomCoord(int row, int col)
    {
        int chunkRow = row / _chunkSize;
        int chunkCol = col / _chunkSize;
        _chunkDict.TryGetValue((chunkRow, chunkCol), out var chunk);
        return chunk;
    }

    public void ClearAllChunks()
    {
        foreach (var chunk in _chunkDict.Values)
        {
            foreach (var room in chunk.RoomList)
            {
                _roomPool.ReturnPool(EPoolType.MapRoom_地图房间,room.gameObject);
            }
        }
        _chunkDict.Clear();
        _generatedCells.Clear(); // 清空全局记录
    }
    #endregion

    public override void MgrUpdate(float deltaTime) { }
}