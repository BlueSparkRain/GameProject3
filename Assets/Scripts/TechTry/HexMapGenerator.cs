using System.Collections.Generic;
using UnityEngine;

// 移除了编辑模式执行，仅在游戏运行时生效！
public class HexMapGenerator : MonoBehaviour
{
    [Header("基础设置")]
    public GameObject hexPrefab;
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float hexRadius = 1f;

    [Header("纹理映射 (运行时可调)")]
    public Texture2D mapTexture;
    public Vector2 Tiling = new Vector2(1, 1);
    public Vector2 Offset = new Vector2(0, 0);

    [Header("运行时数据")]
    public Vector2 mapSize;

    // 缓存运行时生成的面片
    private List<GameObject> hexList = new List<GameObject>();
    private MaterialPropertyBlock _propBlock;

    // 缓存上一帧参数，用于检测变化（运行时实时刷新）
    private int lastWidth, lastHeight;
    private float lastRadius;
    private Texture2D lastMapTex;
    private Vector2 lastTiling, lastOffset;

    // 只有运行时启动才生成
    void Start()
    {
        RefreshHexGrid();
        SaveLastParams();
    }

    // 运行时实时检测参数变化，自动刷新
    void Update()
    {
        // 检测参数是否修改，变化则刷新
        if (gridWidth != lastWidth || gridHeight != lastHeight || hexRadius != lastRadius ||
            mapTexture != lastMapTex || Tiling != lastTiling || Offset != lastOffset)
        {
            RefreshHexGrid();
            SaveLastParams();
        }
    }

    /// <summary>
    /// 刷新网格（运行时专用）
    /// </summary>
    public void RefreshHexGrid()
    {
        if (hexPrefab == null) return;
        _propBlock = new MaterialPropertyBlock();
        mapSize = CalculateMapSize();

        int targetCount = gridWidth * gridHeight;
        int currentCount = hexList.Count;

        // 数量不足 → 新建
        for (int i = currentCount; i < targetCount; i++)
        {
            GameObject hex = Instantiate(hexPrefab, transform);
            hex.name = $"Hex_{i}";
            hexList.Add(hex);
        }

        // 数量多余 → 立即删除（无残留）
        for (int i = currentCount - 1; i >= targetCount; i--)
        {
            DestroyImmediate(hexList[i]);
            hexList.RemoveAt(i);
        }

        // 更新所有面片位置 + 纹理
        int index = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (index >= hexList.Count) break;

                GameObject hex = hexList[index];
                hex.transform.position = CalculateHexPosition(x, y);

                Renderer rend = hex.GetComponent<Renderer>();
                _propBlock.SetTexture("_MainTex", mapTexture);
                _propBlock.SetVector("_MapSize", mapSize);
                _propBlock.SetVector("_MapCenter", transform.position);
                _propBlock.SetFloat("_HexRadius", hexRadius);
                _propBlock.SetVector("_Tiling", Tiling);
                _propBlock.SetVector("_Offset", Offset);
                rend.SetPropertyBlock(_propBlock);

                index++;
            }
        }
    }

    /// <summary>
    /// 【核心】退出游戏运行时，自动删除所有面片
    /// </summary>
    private void OnDestroy()
    {
        ClearGrid();
    }

    /// <summary>
    /// 清空所有面片
    /// </summary>
    public void ClearGrid()
    {
        foreach (var hex in hexList)
        {
            DestroyImmediate(hex);
        }
        hexList.Clear();
    }

    /// <summary>
    /// 保存当前参数，用于检测变化
    /// </summary>
    private void SaveLastParams()
    {
        lastWidth = gridWidth;
        lastHeight = gridHeight;
        lastRadius = hexRadius;
        lastMapTex = mapTexture;
        lastTiling = Tiling;
        lastOffset = Offset;
    }

    /// <summary>
    /// 标准六边形排列 + 居中
    /// </summary>
    private Vector3 CalculateHexPosition(int x, int y)
    {
        float xPos = x * hexRadius * 1.5f;
        float zPos = y * hexRadius * Mathf.Sqrt(3);

        // 尖顶六边形奇数行偏移
        if (y % 2 != 0)
        {
            xPos += hexRadius * 0.75f;
        }

        // 全局居中
        float totalWidth = (gridWidth - 1) * hexRadius * 1.5f;
        float totalHeight = (gridHeight - 1) * hexRadius * Mathf.Sqrt(3);
        xPos -= totalWidth / 2;
        zPos -= totalHeight / 2;

        return new Vector3(xPos, 0, zPos);
    }

    private Vector2 CalculateMapSize()
    {
        float width = gridWidth * hexRadius * 1.5f;
        float height = gridHeight * hexRadius * Mathf.Sqrt(3);
        return new Vector2(width, height);
    }
}