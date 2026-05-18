using UnityEngine;

[CreateAssetMenu(fileName = "MapSaveSOData", menuName = "HexMap/MapSaveSOData")]
public class MapSaveSOData : ScriptableObject
{
    [Header("总地块数 = 3 × MapRadius × (MapRadius + 1) + 1")]
    [Header("SO数据最上方右键可清空数据")]
    [Space(5)]
    [Header("地图配置")]
    public int mapRadius;

    [Header("持久化地块数据")]
    [SerializeField] private SerializedHexCell[] savedCells;

    // 运行时二维数组 [行, 列]  关键：这里是 row,col
    public E_HexTerrainType[,] cellData;
    private int mapSize => mapRadius * 2 + 1;

    // 初始化（加载已有数据 / 新建默认数据）
    public void InitializeIfEmpty()
    {
        cellData = new E_HexTerrainType[mapSize, mapSize];

        // 有数据就加载
        if (savedCells != null && savedCells.Length > 0)
        {
            foreach (var cell in savedCells)
                // 【修复1】坐标对应正确：cellData[行, 列]
                cellData[cell.row, cell.col] = cell.type;
            Debug.Log($"加载已保存的地图数据：{savedCells.Length} 个地块");
            return;
        }

        Debug.Log("初始化地图：海洋");
        for (int x = 0; x < mapSize; x++)
            for (int y = 0; y < mapSize; y++)
                cellData[x, y] = E_HexTerrainType.Obstacle_Ocean;
    }

    // ====================== 【完全还原你原生正确公式】 ======================
    public void SaveData()
    {
        if (cellData == null) return;

        var list = new System.Collections.Generic.List<SerializedHexCell>();
        int center = mapRadius;

        for (int row = 0; row < mapSize; row++)
        {
            // 【完全用你自己的正确计算】一字不差！
            int offset = Mathf.Abs(row - center);
            int width = mapSize - offset;
            int startCol = offset / 2;
            int endCol = startCol + width - 1;

            for (int col = startCol; col <= endCol; col++)
            {
                // 【修复2】保存正确的 row 和 col，不写反！
                list.Add(new SerializedHexCell(row, col, cellData[row, col]));
            }
        }
        savedCells = list.ToArray();
        SaveAssetImmediate();
    }

    [ContextMenu("🗑️清空所有数据并重置")]
    public void ClearAndResetMap()
    {
        savedCells = new SerializedHexCell[0];
        cellData = null;
        InitializeIfEmpty();
        SaveAssetImmediate();
        Debug.Log("🗑️已彻底清空所有地图数据!");
    }

    private void SaveAssetImmediate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // 可序列化的地块数据
    [System.Serializable]
    private class SerializedHexCell
    {
        public int row;
        public int col;
        public E_HexTerrainType type;

        // 构造函数传入 row,col
        public SerializedHexCell(int row, int col, E_HexTerrainType type)
        {
            this.row = row;
            this.col = col;
            this.type = type;
        }
    }
}