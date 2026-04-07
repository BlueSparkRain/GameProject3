using UnityEngine;

/// <summary>
/// 六边形地图噪声数据（高度解耦，纯数据存储）
/// </summary>
[CreateAssetMenu(fileName = "HexMapNoiseData", menuName = "Game/HexMapNoiseData")]
public class HexMapNoiseData : ScriptableObject
{
    [Header("基础地图尺寸")]
    public int mapWidth = 200;
    public int mapHeight = 200;

    [Header("噪声参数")]
    public float noiseScale = 20f;
    public int octaves = 3;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("地形占比")]
    [Range(0.5f, 0.95f)]
    public float walkableRate = 0.75f;

    [Header("平滑迭代次数")]
    public int smoothIterations = 3;

    // 🔥 修复：可序列化一维数组（永久保存地形数据，运行时不会为null）
    [HideInInspector] public bool[] serializedTerrainMap;
    [HideInInspector] public Texture2D previewTexture;

    // 运行时用的二维数组（内存转换，不序列化）
    private bool[,] _terrainMap;

    /// <summary>
    /// 🔥 外部统一调用的地形数组（自动转换，永远不为null）
    /// </summary>
    public bool[,] terrainMap
    {
        get
        {
            if (_terrainMap == null && serializedTerrainMap != null)
            {
                _terrainMap = ConvertToOneDimToTwoDim();
            }
            return _terrainMap;
        }
        set { }
    }

    // 一维转二维
    private bool[,] ConvertToOneDimToTwoDim()
    {
        bool[,] map = new bool[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x, y] = serializedTerrainMap[y * mapWidth + x];
            }
        }
        return map;
    }

    // 二维转一维（保存用）
    public void ConvertTwoDimToOneDim(bool[,] map)
    {
        serializedTerrainMap = new bool[mapWidth * mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                serializedTerrainMap[y * mapWidth + x] = map[x, y];
            }
        }
    }
}