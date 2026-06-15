using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// 六边形网格交互管理器（单例，统一处理点击、区域计算、动画触发 + （不同条件）悬浮材质替换）
/// </summary>
public class HexMapInteractManager : MonoGlobalManager
{
    [Header("检索配置")]
    [Tooltip("点击后触发跳动的半径")]
    public int jumpRadius = 3;
    [Tooltip("每层之间的过渡延迟")]
    public float layerDelay = 0.12f;
    [Tooltip("每层淡入时长")]
    public float fadeInDuration = 0.2f;
    [Tooltip("统一淡出时长")]
    public float fadeOutDuration = 0.4f;
    [Tooltip("子物体上浮高度")]
    public float riseHeight = 0.6f;

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

    [Header("使用鼠标点击来聚焦相机视角")]
    public bool UseMouseClickFacus = false;

    protected override void MgrOnInit(){
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
    void CheckClickHexRoom(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector2 mousePosition= Input.mousePosition;
        if (Physics.Raycast(ray, out RaycastHit hit)){
            HexRoomTag clickedRoomData = hit.collider.GetComponentInParent<HexRoomTag>();
            if (clickedRoomData != null){
                if (UseMouseClickFacus){
                    if ((mousePosition.x < 185 || mousePosition.x > 500) && mousePosition.y > 400){
                        GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(clickedRoomData.gameObject);
                    }
                }
                //触发半径内的所有房间跳动
                //TriggerRadiusJump(clickedRoomData.row, clickedRoomData.col);
                EditTerrainLogic(clickedRoomData);
            }

            //如果是在寻路状态下被点击到的房间，将触发RoomMover的移动
            HexPathFindingManager hexPathFindingManager = GameRoot.GetManager<HexPathFindingManager>();
            MapMoverManager mapCharacterMoveChecker = GameRoot.GetManager<MapMoverManager>();

            if (hexPathFindingManager.canTriggerMover){
                //GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/mambo");
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
        List<Vector2Int> radiusRowCols = HexCoordinateUtility.GetRowColsInRadius(centerRow, centerCol, jumpRadius);

        // 按距离分层，收集每层的 (SpriteRenderer, Transform, 原始Z)
        Dictionary<int, List<(SpriteRenderer sr, Transform child, float originZ)>> layers = new();

        foreach (Vector2Int rowCol in radiusRowCols)
        {
            if (!mapManager.HexRoomMap.TryGetValue(rowCol, out HexRoomTag room)) continue;
            if (room.transform.childCount == 0) continue;

            Transform firstChild = room.transform.GetChild(0);
            SpriteRenderer sr = firstChild.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            int dist = HexCoordinateUtility.GetDistanceByRowCol(centerRow, centerCol, room.row, room.col);
            if (!layers.ContainsKey(dist))
                layers[dist] = new List<(SpriteRenderer, Transform, float)>();

            float originZ = firstChild.localPosition.z;
            layers[dist].Add((sr, firstChild, originZ));
        }

        if (layers.Count == 0) return;

        // 所有子物体：alpha=0，瞬移到上浮高度
        foreach (var kv in layers)
            foreach (var item in kv.Value)
            {
                item.sr.DOKill();
                var c = item.sr.color;
                c.a = 0f;
                item.sr.color = c;
                item.child.localPosition = new Vector3(item.child.localPosition.x, item.child.localPosition.y, item.originZ + riseHeight);
            }

        int maxDist = 0;
        foreach (var k in layers.Keys) if (k > maxDist) maxDist = k;

        coroutineManager.StartCoroutine(RadiusFadeSequence(layers, maxDist));
    }

    System.Collections.IEnumerator RadiusFadeSequence(Dictionary<int, List<(SpriteRenderer sr, Transform child, float originZ)>> layers, int maxDist)
    {
        // 内层→外层 逐层淡入
        for (int d = 0; d <= maxDist; d++)
        {
            if (!layers.TryGetValue(d, out var list)) continue;

            foreach (var item in list)
                item.sr.DOFade(1f, fadeInDuration).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(layerDelay);
        }

        // 等最外层淡入完成
        yield return new WaitForSeconds(fadeInDuration);

        // 所有层统一归位，alpha保持1不消失
        foreach (var kv in layers)
            foreach (var item in kv.Value)
            {
                item.sr.DOFade(1f, 0f);
                item.child.DOLocalMoveZ(item.originZ, fadeOutDuration).SetEase(Ease.OutCubic);
            }
    }
    void OneMoverCloudeCheck() {
        HexRoomTag characterRoom = GameRoot.GetManager<MapMoverManager>().currentIMovable.currentRoom;
        if (characterRoom){
            coroutineManager.StartCoroutine(TriggerCloudeDisappear(characterRoom.row, characterRoom.col));     
        }
        //coroutineManager = GameRoot.GetManager<CoroutineManager>();
    }
    /// <summary>
    /// 依据玩家当前位置来消除视野内的云朵
    /// </summary>
    /// <param name="centerRow"></param>
    /// <param name="centerCol"></param>
    IEnumerator TriggerCloudeDisappear(int centerRow, int centerCol){
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
        // 寻路状态中，悬浮动画由 HexPathFindingManager 统一管理，此处不干涉
        if (GameRoot.GetManager<HexPathFindingManager>()?.canPathFind == true)
            return;

        //射线检测获取当前悬浮房间
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HexRoomTag newHoverRoom = null;
        if (Physics.Raycast(ray, out RaycastHit hit))
            newHoverRoom = hit.collider.GetComponentInParent<HexRoomTag>();

        //悬浮房间无变化 → 直接返回（避免每帧重复操作）
        if (newHoverRoom == _currentHoverRoom) return;

        //恢复上一个悬浮房间
        if (_currentHoverRoom != null)
        {
            RestoreOriginMaterial(_currentHoverRoom);
            var prevAnim = _currentHoverRoom.GetComponent<HexJumpAnimHandler>();
            if (prevAnim != null) prevAnim.HoverDown();
        }

        //新悬浮房间
        if (newHoverRoom != null)
        {
            SetHoverMaterial(newHoverRoom);
            var nextAnim = newHoverRoom.GetComponent<HexJumpAnimHandler>();
            if (nextAnim == null)
                DebugManager.LogWarning(EDebugCategory.MapRoom, $"[HexMapInteractManager] {newHoverRoom.name} 缺少 HexJumpAnimHandler 组件，请挂载到预制件上");
            else
                nextAnim.HoverUp();
        }

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
    void EditTerrainLogic(HexRoomTag clickedRoomData){
        if (!UseEditMode)
            return;
        GameRoot.GetManager<UIManager>().OpenPanel<MapTerrainEditorPanel>(E_UIPanelType.MapTerrainEditorPanel);
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

