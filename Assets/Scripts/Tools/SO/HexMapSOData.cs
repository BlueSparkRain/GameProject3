using UnityEngine;

[CreateAssetMenu(fileName = "MapSaveSOData", menuName = "HexMap/MapSaveSOData")]
public class MapSaveSOData : ScriptableObject
{
    [Header("SO数据最上方右键可清空数据")]
    [Space(5)]
    [Header("地图配置")]
    public int mapCols;
    public int mapRows;
    public E_MapShape mapShape = E_MapShape.Rectangle;

    [Header("持久化地块数据")]
    [SerializeField] private SerializedHexCell[] savedCells;

    // 运行时二维数组 [行, 列]
    public E_HexTerrainType[,] cellData;

    public void InitializeIfEmpty()
    {
        cellData = new E_HexTerrainType[mapRows, mapCols];

        if (savedCells != null && savedCells.Length > 0)
        {
            int loaded = 0;
            foreach (var cell in savedCells)
            {
                if (cell.row >= 0 && cell.row < mapRows && cell.col >= 0 && cell.col < mapCols)
                {
                    cellData[cell.row, cell.col] = cell.type;
                    loaded++;
                }
            }
            //Debug.Log($"加载已保存的地图数据：{loaded}/{savedCells.Length} 个地块");
            return;
        }

        //Debug.Log("初始化地图：海洋");
        for (int r = 0; r < mapRows; r++)
            for (int c = 0; c < mapCols; c++)
                cellData[r, c] = E_HexTerrainType.Obstacle_Ocean;
    }

    public void SaveData()
    {
        if (cellData == null) return;

        var list = new System.Collections.Generic.List<SerializedHexCell>();
        int center = mapRows / 2;

        for (int row = 0; row < mapRows; row++)
        {
            int startCol, endCol;
            if (mapShape == E_MapShape.Rectangle)
            {
                startCol = 0;
                endCol = mapCols - 1;
            }
            else
            {
                int offset = Mathf.Abs(row - center);
                int width = mapRows - offset;
                startCol = offset / 2;
                endCol = startCol + width - 1;
            }

            for (int col = startCol; col <= endCol; col++)
                list.Add(new SerializedHexCell(row, col, cellData[row, col]));
        }
        savedCells = list.ToArray();
        SaveAssetImmediate();
    }

    [ContextMenu("清空所有数据并重置")]
    public void ClearAndResetMap()
    {
        savedCells = new SerializedHexCell[0];
        cellData = null;
        InitializeIfEmpty();
        SaveAssetImmediate();
        DebugManager.Log(EDebugCategory.MapRoom, "已彻底清空所有地图数据!");
    }

    private void SaveAssetImmediate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    [System.Serializable]
    private class SerializedHexCell
    {
        public int row;
        public int col;
        public E_HexTerrainType type;

        public SerializedHexCell(int row, int col, E_HexTerrainType type)
        {
            this.row = row;
            this.col = col;
            this.type = type;
        }
    }
}

public enum E_MapShape
{
    Hex,
    Rectangle
}
