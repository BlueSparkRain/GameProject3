using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 六边形网格交互管理器（单例，统一处理点击、区域计算、动画触发 + （不同条件）悬浮材质替换）
/// </summary>
public class HexMapInteractManager : MonoGlobalManager
{
    [Header("检索配置")]
    [Tooltip("点击后触发跳动的半径")]
    public int jumpRadius = 3;
    [Tooltip("延迟系数（距离每增加1，延迟增加的时间）")]
    public float delayPerDistance = 0.08f;

    Material hoverMaterial;

    string hoverMatPath = "Material/HexRoomTag/NPC__HexRoom";
    /// <summary>
    /// 玩家视野
    /// </summary>
    public int eyeRadius = 6;

    // 悬浮材质缓存（性能核心：O(1)查找，仅缓存需要恢复的材质）
    Dictionary<HexRoomTag, Material> _originMaterialMap = new Dictionary<HexRoomTag, Material>();
    // 当前悬浮的房间（避免每帧重复检测/替换材质）
    HexRoomTag _currentHoverRoom;

    WaitForSeconds cloudeDelay;
    CoroutineManager coroutineManager;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        mapManager = GameRoot.GetManager<GameMapManager>();
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        EventCenter.AddEventListener(E_EventType.Mover_OneTimeMove, OneMoverCloudeCheck);
        EventCenter.AddEventListener(E_EventType.Editor_Terrain_OneTime, SwitchEditingState);
        cloudeDelay = new WaitForSeconds(0.01f);
    }
    public override void MgrUpdate(float deltaTime)
    {
        // 检测鼠标左键点击（原有逻辑保留）
        if (Input.GetMouseButtonDown(0))
            CheckClickHexRoom();
        // 检测鼠标悬浮（仅在房间变化时处理材质，性能高效）
        CheckHoverHexRoom();
    }

    /// <summary>
    /// 检测点击的六边形房间
    /// </summary>
    void CheckClickHexRoom()
    {
        // 射线检测（正交相机适配）
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            HexRoomTag clickedRoomData = hit.collider.GetComponent<HexRoomTag>();
            if (clickedRoomData != null)
            {
                //触发半径内的所有房间跳动
                TriggerRadiusJump(clickedRoomData.row, clickedRoomData.col);
                EditTerrainLogic(clickedRoomData);
            }

            //如果是在寻路状态下被点击到的房间，将触发RoomMover的移动
            HexPathFindingManager hexPathFindingManager = GameRoot.GetManager<HexPathFindingManager>();
            MapMoverManager mapCharacterMoveChecker = GameRoot.GetManager<MapMoverManager>();

            if (hexPathFindingManager.canTriggerMover)
            {

                GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/mambo");
                hexPathFindingManager.SetPathFindState(false);

                mapCharacterMoveChecker.MoverGo(hexPathFindingManager.TargetMoverPath);
                hexPathFindingManager.EndOneTimeMove();
            }
        }
    }
    GameMapManager mapManager;
    /// <summary>
    /// 触发指定坐标半径内的所有房间跳动（原有逻辑保留）
    /// </summary>
    void TriggerRadiusJump(int centerRow, int centerCol)
    {
        // 1. 生成正六边形范围的行+列坐标（无冗余、不遗漏）
        List<Vector2Int> radiusRowCols = HexCoordinateUtility.GetRowColsInRadius(centerRow, centerCol, jumpRadius);

        // 2. 遍历仅触发存在的房间
        foreach (Vector2Int rowCol in radiusRowCols)
        {
            if (mapManager.HexRoomMap.TryGetValue(rowCol, out HexRoomTag room))
            {
                // 2.1 计算距离（直接用行+列，无需HexRoom提供轴向坐标）
                int distance = HexCoordinateUtility.GetDistanceByRowCol(centerRow, centerCol, room.row, room.col);

                // 2.2 计算幅度和延迟
                float distanceRatio = Mathf.Clamp01((float)distance / jumpRadius);
                float delay = distance * delayPerDistance;

                // 2.3 触发动画（动画组件无修改）
                HexJumpAnimHandler jumpAnim = room.GetComponent<HexJumpAnimHandler>();
                if (jumpAnim != null)
                    jumpAnim.TriggerJump(distanceRatio, delay);
            }
        }
    }

    void OneMoverCloudeCheck()
    {
        HexRoomTag characterRoom = GameRoot.GetManager<MapMoverManager>().currentIMovable.currentRoom;
        Debug.Log(coroutineManager+"??//"+ characterRoom);
        coroutineManager.StartCoroutine(TriggerCloudeDisappear(characterRoom.row, characterRoom.col));
    }

    /// <summary>
    /// 依据玩家当前位置来消除视野内的云朵
    /// </summary>
    /// <param name="centerRow"></param>
    /// <param name="centerCol"></param>
    IEnumerator TriggerCloudeDisappear(int centerRow, int centerCol)
    {
        // 1. 生成正六边形范围的行+列坐标（无冗余、不遗漏）
        List<Vector2Int> radiusRowCols = HexCoordinateUtility.GetRowColsInRadius(centerRow, centerCol, eyeRadius);
        foreach (Vector2Int rowCol in radiusRowCols)
        {
            if (mapManager.HexRoomMap.TryGetValue(rowCol, out HexRoomTag room))
            {
                // 2.3 触发动画（动画组件无修改）
                room.GetComponent<HexJumpAnimHandler>()?.CloudeDisAppear();
                yield return cloudeDelay;
            }
        }
    }

    #region 鼠标悬浮材质替换核心逻辑
    /// <summary>
    /// 检测鼠标悬浮的六边形房间（性能优化：仅房间变化时处理）
    /// </summary>
    void CheckHoverHexRoom()
    {
        //射线检测获取当前悬浮房间
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HexRoomTag newHoverRoom = null;
        if (Physics.Raycast(ray, out RaycastHit hit))
            newHoverRoom = hit.collider.GetComponent<HexRoomTag>();

        //悬浮房间无变化 → 直接返回（避免每帧重复操作）
        if (newHoverRoom == _currentHoverRoom) return;

        //恢复上一个悬浮房间的原始材质
        if (_currentHoverRoom != null)
            RestoreOriginMaterial(_currentHoverRoom);

        //新悬浮房间设置hover材质
        if (newHoverRoom != null)
            SetHoverMaterial(newHoverRoom);

        //更新当前悬浮房间缓存
        _currentHoverRoom = newHoverRoom;
    }

    /// <summary>
    /// 给房间设置(判断路径是否可行)悬浮材质（创建实例，不影响原材质和其他房间）
    /// </summary>
    void SetHoverMaterial(HexRoomTag room)
    {
        hoverMaterial = Resources.Load<Material>(hoverMatPath);
        if (hoverMaterial == null) return;

        Renderer roomRenderer = room.GetComponent<Renderer>();
        if (roomRenderer == null) return;

        // 缓存原始材质（仅第一次悬浮时缓存，避免重复赋值）
        if (!_originMaterialMap.ContainsKey(room))
            _originMaterialMap.Add(room, roomRenderer.material); // 注意用material（实例）而非sharedMaterial

        // 创建hover材质的实例 → 多个房间悬浮时互不影响
        roomRenderer.material = Instantiate(hoverMaterial);
    }

    /// <summary>
    /// 恢复房间的原始材质
    /// </summary>
    void RestoreOriginMaterial(HexRoomTag room)
    {
        Renderer roomRenderer = room.GetComponent<Renderer>();
        if (roomRenderer == null) return;

        // 从缓存获取原始材质并恢复
        if (_originMaterialMap.TryGetValue(room, out Material originMat))
            roomRenderer.material = originMat;
    }

    /// <summary>
    /// 可选：清理房间材质缓存（如场景卸载时调用，避免内存泄漏）
    /// </summary>
    public void ClearMaterialCache()
    {
        _originMaterialMap.Clear();
        _currentHoverRoom = null;
    }
    #endregion


    #region 编辑器模式——地形编辑区域
    HexRoomTag editingRoom;
    bool UseEditMode = false;
    bool EditingTerrain;
    public void USEEditMode()
    {
        UseEditMode = true;
        EditingTerrain = true;
    }

    public void SwitchEditingState()
    {
        EditingTerrain = !EditingTerrain;
    }


    void EditTerrainLogic(HexRoomTag clickedRoomData)
    {
        if (!UseEditMode)
            return;
        GameRoot.GetManager<UIManager>().OpenPanel<MapTerrainEditorPanel>
  (E_UIPanelType.MapTerrainEditorPanel);
        if (EditingTerrain)
        {
            EditingTerrain = false;
            editingRoom = clickedRoomData;
            GameRoot.GetManager<UIManager>().GetPanel<MapTerrainEditorPanel>(E_UIPanelType.MapTerrainEditorPanel).GetHexTag(clickedRoomData);
        }
        else if (!EditingTerrain)
        {
            if (editingRoom != null && editingRoom == clickedRoomData)
            {
                EventCenter.EventTrigger(E_EventType.Editor_Terrain_ExitEdit);
                EditingTerrain = true;
            }

        }
    }
    #endregion
}

