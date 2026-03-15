using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 六边形网格点击管理器（单例，统一处理点击、区域计算、动画触发）
/// </summary>
public class HexGridClickManager : MonoGlobalManager
{
    [Header("检索配置")]
    [Tooltip("点击后触发跳动的半径")]
    public int jumpRadius = 3;
    [Tooltip("延迟系数（距离每增加1，延迟增加的时间）")]
    public float delayPerDistance = 0.08f;

    // 坐标到房间的映射表（高效查找）
    private Dictionary<Vector2Int, HexRoom> _hexRoomMap = new Dictionary<Vector2Int, HexRoom>();

    public override void MgrUpdate(float deltaTime)
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            CheckClickHexRoom();
        }
    }

    /// <summary>
    /// 注册一个六边形房间到映射表
    /// </summary>
    public void RegisterHexRoom(HexRoom room)
    {
        Vector2Int key = new Vector2Int(room.row, room.col);
        if (!_hexRoomMap.ContainsKey(key)){
            _hexRoomMap.Add(key, room);
        }
    }

    /// <summary>
    /// 检测点击的六边形房间
    /// </summary>
    private void CheckClickHexRoom()
    {
        // 射线检测（正交相机适配）
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            HexRoom clickedRoom = hit.collider.GetComponent<HexRoom>();
            if (clickedRoom != null){
                // 触发半径内的所有房间跳动
                TriggerRadiusJump(clickedRoom.row, clickedRoom.col);
            }
        }
    }

    /// <summary>
    /// 触发指定坐标半径内的所有房间跳动
    /// </summary>
    private void TriggerRadiusJump(int centerRow, int centerCol)
    {
        Debug.Log("sha1!!");
        // 1. 生成正六边形范围的行+列坐标（无冗余、不遗漏）
        List<Vector2Int> radiusRowCols = HexCoordinateUtility.GetRowColsInRadius(centerRow, centerCol, jumpRadius);

        // 2. 遍历仅触发存在的房间
        foreach (Vector2Int rowCol in radiusRowCols)
        {
            if (_hexRoomMap.TryGetValue(rowCol, out HexRoom room))
            {
                // 2.1 计算距离（直接用行+列，无需HexRoom提供轴向坐标）
                int distance = HexCoordinateUtility.GetDistanceByRowCol(centerRow, centerCol, room.row, room.col);

                // 2.2 计算幅度和延迟
                float distanceRatio = Mathf.Clamp01((float)distance / jumpRadius);
                float delay = distance * delayPerDistance;

                // 2.3 触发动画（动画组件无修改）
                HexJumpAnimation jumpAnim = room.GetComponent<HexJumpAnimation>();
                if (jumpAnim != null)
                {
                    jumpAnim.TriggerJump(distanceRatio, delay);
                }
            }
        }
    }





}