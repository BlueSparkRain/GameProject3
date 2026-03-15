using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 六边形坐标工具类（仅新增快捷方法，无破坏性修改）
/// </summary>
public static class HexCoordinateUtility
{
    // ========== 原有核心逻辑（完全保留） ==========
    public static Vector2Int RowColToAxial(int row, int col)
    {
        int q = col - (row - (row & 1)) / 2;
        int r = row;
        return new Vector2Int(q, r);
    }

    public static Vector2Int AxialToRowCol(int q, int r)
    {
        int col = q + (r - (r & 1)) / 2;
        int row = r;
        return new Vector2Int(row, col);
    }

    public static int GetDistance(int q1, int r1, int q2, int r2)
    {
        int x1 = q1, z1 = r1, y1 = -x1 - z1;
        int x2 = q2, z2 = r2, y2 = -x2 - z2;
        return (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) + Mathf.Abs(z1 - z2)) / 2;
    }

    public static List<Vector2Int> GetHexesInRadius(int centerQ, int centerR, int radius)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int q = -radius; q <= radius; q++)
        {
            int rStart = Mathf.Max(-radius, -q - radius);
            int rEnd = Mathf.Min(radius, -q + radius);
            for (int r = rStart; r <= rEnd; r++)
            {
                if (GetDistance(q, r, 0, 0) <= radius)
                {
                    result.Add(new Vector2Int(centerQ + q, centerR + r));
                }
            }
        }
        return result;
    }

    // ========== 新增快捷方法（适配无修改的HexRoom） ==========
    /// <summary>
    /// 直接基于行+列生成半径内的所有行+列坐标（无需HexRoom参与转换）
    /// </summary>
    public static List<Vector2Int> GetRowColsInRadius(int centerRow, int centerCol, int radius)
    {
        Vector2Int centerAxial = RowColToAxial(centerRow, centerCol);
        List<Vector2Int> axialRange = GetHexesInRadius(centerAxial.x, centerAxial.y, radius);
        List<Vector2Int> rowColRange = new List<Vector2Int>();
        foreach (var axial in axialRange)
        {
            rowColRange.Add(AxialToRowCol(axial.x, axial.y));
        }
        return rowColRange;
    }

    /// <summary>
    /// 计算两个行+列坐标的距离（直接调用，无需HexRoom处理）
    /// </summary>
    public static int GetDistanceByRowCol(int row1, int col1, int row2, int col2)
    {
        Vector2Int axial1 = RowColToAxial(row1, col1);
        Vector2Int axial2 = RowColToAxial(row2, col2);
        return GetDistance(axial1.x, axial1.y, axial2.x, axial2.y);
    }
}