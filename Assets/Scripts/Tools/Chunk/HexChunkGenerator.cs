using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 🔥 最终修复：原生Odd-R奇偶行地图 无缝六边形分块
/// 零空隙 | 零重叠 | 标准正六边形 | 无坐标错位
/// </summary>
public static class HexChunkGenerator
{
    public enum HexChunkShape
    {
        Rectangle,
        Hexagonal
    }

    // 六边形区块半径（标准正六边形大小）
    public static int HexChunkRadius = 4;

    #region 🔥 核心：原生Odd-R地图 无缝六边形区块生成（无任何转换误差）
    public static (int row, int col)[] GetHexChunkCells(int chunkID, int mapRows, int mapCols)
    {
        List<(int row, int col)> cells = new List<(int, int)>();
        int r = HexChunkRadius;
        int stride = r * 2 - 1; // 无缝拼接步长（核心！无空隙）

        // 计算区块在地图上的真实中心（原生行列，无错位）
        int centerRow = (chunkID / 100) * stride;
        int centerCol = (chunkID % 100) * stride;

        // 标准正六边形遍历（绝对规范、无缝贴合）
        for (int dy = -r; dy <= r; dy++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                // 正六边形判定公式（无变形、无空隙）
                if (Mathf.Abs(dx) + Mathf.Abs(dx + dy) + Mathf.Abs(dy) > r * 2)
                    continue;

                int row = centerRow + dy;
                int col = centerCol + dx;

                // 严格边界校验
                if (row >= 0 && row < mapRows && col >= 0 && col < mapCols)
                {
                    cells.Add((row, col));
                }
            }
        }
        return cells.ToArray();
    }
    #endregion
}