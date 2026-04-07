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

    // 运行时二维数组
    public E_HexTerrainType[,] cellData;
    private int mapSize => mapRadius * 2 + 1;

    // 初始化（加载已有数据 / 新建默认数据）
    public void InitializeIfEmpty()
    {
        cellData = new E_HexTerrainType[mapSize, mapSize];

        // 有数据就加载，没有就初始化默认
        if (savedCells != null && savedCells.Length > 0)
        {
            foreach (var cell in savedCells)
                cellData[cell.x, cell.y] = cell.type;
            Debug.Log($"加载已保存的地图数据：{savedCells.Length} 个地块");
            return;
        }

        Debug.Log("初始化地图：海洋");
        for (int x = 0; x < mapSize; x++)
            for (int y = 0; y < mapSize; y++)
                cellData[x, y] = E_HexTerrainType.Obstacle__Ocean;
    }

    // ====================== 【仅修改这里：只保存有效六边形地块】 ======================
    // 保存数据到持久化数组（优化后：无浪费，只存正六边形内部）
    public void SaveData()
    {
        if (cellData == null) return;

        var list = new System.Collections.Generic.List<SerializedHexCell>();
        int center = mapRadius;

        for (int row = 0; row < mapSize; row++)
        {
            int offset = Mathf.Abs(row - center);
            //int startCol = offset / 2;
            int startCol = 0;
            int endCol = startCol + (mapSize - offset) - 1;

            for (int col = startCol; col <= endCol; col++)
            {
                list.Add(new SerializedHexCell(col, row, cellData[col, row]));
            }
        }
        savedCells = list.ToArray();
        SaveAssetImmediate();
    }

    // ====================== 【原有功能完全不变】 ======================
    [ContextMenu("🗑️清空所有数据并重置")]
    public void ClearAndResetMap()
    {
        savedCells = new SerializedHexCell[0];
        cellData = null;
        InitializeIfEmpty();
        SaveAssetImmediate();

        Debug.Log("🗑️已彻底清空所有地图数据!");
    }

    // 强制保存SO资产到磁盘（关键）
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
        public int x;
        public int y;
        public E_HexTerrainType type;

        public SerializedHexCell(int x, int y, E_HexTerrainType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }
}