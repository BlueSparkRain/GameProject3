using Core;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MapMoverManager : MonoGlobalManager
{
    public Transform mapIconParent;
    public IMapMoveable currentIMovable;
    HexPathFindingManager hexPathFindingManager;
    public override void MgrUpdate(float deltaTime) { }
    //玩家角色的映射
    private Dictionary<PlayerMapIcon, IMapMoveable> playerIconMoverDic = new Dictionary<PlayerMapIcon, IMapMoveable>();
    private Dictionary<IMapMoveable, MapMoverPosition> posDic = new Dictionary<IMapMoveable, MapMoverPosition>();
    MapMoverPosition posSaveData;
    int iconNum = 0;

    //所有的移动对象每回合的行动情况
    //只有所有都是ready，才能进入下个回合
    public Dictionary<IMapMoveable, bool> roundMoveDic = new Dictionary<IMapMoveable, bool>();

    // 机器人回合队列：玩家先动，玩家结束后依次执行机器人
    Queue<IMapMoveable> _robotQueue = new Queue<IMapMoveable>();

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener<IMapMoveable, Vector3>(E_EventType.Mover_CheckCurrrentRoom, CheckCurrentRoom);
        hexPathFindingManager = GameRoot.GetManager<HexPathFindingManager>();
        EventCenter.AddEventListener<IMapMoveable>(E_EventType.Character_Mover_Regist, RegisterBornMover);
        EventCenter.AddEventListener<IMapMoveable>(E_EventType.OneMoverEndRound, OneMoverEndRound);
        EventCenter.AddEventListener(E_EventType.NewRound, OnNewRound_Reset);
    }

    /// <summary>重新绑定事件（MapSceneSetUp 中 ClearAllEvents 后会清掉全局管理器的事件绑定）</summary>
    public void RebindEvents()
    {
        // 清理上一次 Session 残留的 Icon 映射（旧 PlayerMapIcon 已随场景销毁）
        playerIconMoverDic.Clear();
        _robotQueue.Clear();
        iconNum = 0;

        EventCenter.RemoveEventListener<IMapMoveable, Vector3>(E_EventType.Mover_CheckCurrrentRoom, CheckCurrentRoom);
        EventCenter.RemoveEventListener<IMapMoveable>(E_EventType.Character_Mover_Regist, RegisterBornMover);
        EventCenter.RemoveEventListener<IMapMoveable>(E_EventType.OneMoverEndRound, OneMoverEndRound);
        EventCenter.RemoveEventListener(E_EventType.NewRound, OnNewRound_Reset);
        EventCenter.AddEventListener<IMapMoveable, Vector3>(E_EventType.Mover_CheckCurrrentRoom, CheckCurrentRoom);
        EventCenter.AddEventListener<IMapMoveable>(E_EventType.Character_Mover_Regist, RegisterBornMover);
        EventCenter.AddEventListener<IMapMoveable>(E_EventType.OneMoverEndRound, OneMoverEndRound);
        EventCenter.AddEventListener(E_EventType.NewRound, OnNewRound_Reset);
    }
    //每回合，所有可以移动的角色会依次行动（先按照固定顺序）
    //玩家回合，无限/有限时间，可以根据玩家鼠标来寻路
    //敌人回合，时间，根据策略来自动调用寻路。

    #region 玩家Mover相关逻辑：（1）MapIcon创建注册（2）获取MapIcon关联Mover

    /// <summary>
    /// 创建一个玩家操作角色的UIIcon
    /// </summary>
    /// <param name="characterRoomMover"></param>
    /// <param name="charcaterTrans"></param>
    /// <returns></returns>
    public PlayerMapIcon CreateNewMapIcon(Player_CharacterMapMover characterRoomMover, Transform charcaterTrans)
    {
        if (mapIconParent == null){
            iconNum = 0;
            mapIconParent = GameObject.FindWithTag("MapIconContent").transform;
        }

        var newIcon = Instantiate( ResourcesLoader.FindMapIcon_CircleObj(), mapIconParent).GetComponent<PlayerMapIcon>();
        newIcon.transform.localScale = Vector3.zero;
        newIcon.GetComponent<RectTransform>().localPosition += new Vector3(200, 0, 0) * iconNum;
        newIcon.transform.DOLocalMoveY(120, 0.3f).From(-400);
        newIcon.transform.DOScale(new Vector3(1, 1, 0), 0.5f).SetEase(Ease.OutQuad).From(new Vector3(1, 0, 0));
        playerIconMoverDic.Add(newIcon, characterRoomMover);
        newIcon.InitIcon(characterRoomMover.CharacterType, charcaterTrans);
        iconNum++;
        return newIcon;
    }


    /// <summary>
    /// 注册移动角色的位置数据，便于更新
    /// </summary>
    /// <param name="moveHandle"></param>
    public void RegisterMoverPostion(IMapMoveable iMapMover, MapMoverPosition moverPosData)
    {
        posDic.Add(iMapMover, moverPosData);
        DebugManager.Log(EDebugCategory.MapRoom, "Mover位置数据保存-注册:" + posDic.Count);
    }

    /// <summary>
    /// 只有玩家自身是通过MapIcon来交互移动的
    /// </summary>
    /// <param name="mapIcon"></param>
    /// <returns></returns>
    public IMapMoveable GetTargetPlayerMover(PlayerMapIcon mapIcon)
    {
        if (playerIconMoverDic.ContainsKey(mapIcon))
        {
            currentIMovable = playerIconMoverDic[mapIcon];
            if ((currentIMovable as Player_CharacterMapMover).IsMoving)
            {
                DebugManager.Log(EDebugCategory.MapRoom, "[MapMoverManager]---请求失败！目标玩家Mover正在移动中");
                mapIcon.FlashWarnning();
                return null;
            }
            else
                return currentIMovable;
        }
        else
            return null;
    }

    #endregion

    /// <summary>切换寻路状态——MapIcon点击和空格键共用入口</summary>
    /// <summary>玩家加载完毕后才允许空格开启寻路（由 MapSceneSetUp 在 LoadCharacter 完成后设为 true）</summary>
    public static bool PlayerReadyForPathfinding { get; set; }

    public void TogglePathFinding()
    {
        if (!PlayerReadyForPathfinding) return;
        if (hexPathFindingManager.canPathFind)
        {
            hexPathFindingManager.SetPathFindState(false);
            UpdateIconHighlight(false);
        }
        else
        {
            IMapMoveable target = null;
            if (currentIMovable is Player_CharacterMapMover pm && !pm.IsMoving && ((IMapMoveable)pm).currentRoom != null)
                target = currentIMovable;

            if (target == null)
            {
                foreach (var kvp in playerIconMoverDic)
                {
                    if (kvp.Value is Player_CharacterMapMover playerMover && !playerMover.IsMoving)
                    {
                        if (kvp.Value.currentRoom == null) continue;
                        target = kvp.Value;
                        break;
                    }
                }
            }

            if (target == null) return;

            int points = GameRoot.GetManager<ActionPointsManager>().RemainActionPoints;
            if (points <= 0) return;

            currentIMovable = target;
            hexPathFindingManager.SetPlayerStartRoom(target.currentRoom);
            hexPathFindingManager.SetPathFindState(true, points);
            UpdateIconHighlight(true);
            GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/mambo", default, 0.3f, 1.2f);
        }
    }

    void UpdateIconHighlight(bool active)
    {
        foreach (var kvp in playerIconMoverDic)
            if (kvp.Value == currentIMovable)
                kvp.Key.SetHighlighted(active);
    }

    public void SetCurrentMover(IMapMoveable iMover, Vector3 rayStartPos)
    {
        currentIMovable = iMover;
        CheckCurrentRoom(currentIMovable, rayStartPos);
    }

    public void MoverGo(List<HexRoomTag> path)
    {
        currentIMovable.DoMoveFunc(path);
    }

    ///// <summary>
    ///// 所有Mover在每次移动后都会更新当前所处的Room
    ///// </summary>
    void CheckCurrentRoom(IMapMoveable imover, Vector3 rayStart){
        HexRoomTag downRoom = null;

        Ray ray = new Ray(rayStart, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5, LayerMask.GetMask("HexRoom")))
            downRoom = hit.collider.GetComponent<HexRoomTag>();

        // 同帧激活的 pooled 对象物理未就绪 → raycast 落空 → 用存档坐标补查
        if (downRoom == null && posDic.TryGetValue(imover, out var savedPos))
        {
            var mapMgr = GameRoot.GetManager<GameMapManager>();
            mapMgr?.HexRoomMap.TryGetValue(new Vector2Int(savedPos.pos.x, savedPos.pos.y), out downRoom);
        }

        if (downRoom == null) return;

        if (downRoom != imover.currentRoom)
        {
            hexPathFindingManager.SetPlayerStartRoom(downRoom);
            if (posDic.TryGetValue(imover, out var posData))
                posData.SetPos(downRoom.row, downRoom.col);
            if (imover is Player_CharacterMapMover pm && pm.CharacterTransform != null)
            {
                var handle = pm.CharacterTransform.GetComponent<CharacterMapMoveHandle>();
                if (handle != null)
                    handle.UpdatePosition(downRoom.row, downRoom.col);
            }
        }

        // 脱战检测
        if (imover.currentRoom)
        {
            E_HexRoomType roomType = imover.currentRoom.GetComponent<HexRoomStyleHandler>().RoomType;
            if (roomType.IsBattleRoom())
                EventCenter.EventTrigger(E_EventType.PlayerOutBattle);
        }

        // 触发对应的房间逻辑
        imover.currentRoom = downRoom;
        if (imover is Robot_CharacterMapMover robot)
            robot.ResolveRoom(downRoom);
        else if (downRoom.RoomLogic != null)
            downRoom.RoomLogic.OnPlayerEnter(downRoom);
        else if (downRoom.IHexRoom != null)
            downRoom.IHexRoom.DoHexRoomLogic();
    }

    //加载数据对应
    public override void MgrDispose()
    {
        //卸载场景前保存
        base.MgrDispose();
    }

    //关闭程序前保存
    private void OnApplicationQuit()
    {
        foreach (var item in posDic)
        {

            JsonSaver.Save(item.Value, item.Value.uniqueId);
        }
    }

    #region 回合检测
    /// <summary>
    /// 一个Mover宣布结束自身回合
    /// </summary>
    /// <param name="mover"></param>
    void OneMoverEndRound(IMapMoveable mover)
    {
        if (roundMoveDic.ContainsKey(mover))
            roundMoveDic[mover] = true;

        ProcessNextRobot();
        CheckNewRound();
    }

    /// <summary>
    /// 登记场上新的Mover
    /// </summary>
    /// <param name="mover"></param>
    void RegisterBornMover(IMapMoveable mover)
    {
        if (!roundMoveDic.ContainsKey(mover))
        {
            roundMoveDic.Add(mover, false);
            if (mover is Robot_CharacterMapMover)
                _robotQueue.Enqueue(mover);
        }
    }

    /// <summary>清除所有回合追踪（场景加载时调用，防止残留的旧Mover引用导致无法结束回合）</summary>
    public void ClearRoundTracking()
    {
        roundMoveDic.Clear();
        _robotQueue.Clear();
    }

    /// <summary>新回合开始时重置所有Mover状态，重建机器人队列</summary>
    void OnNewRound_Reset()
    {
        _robotQueue.Clear();
        var keys = new List<IMapMoveable>(roundMoveDic.Keys);
        foreach (var key in keys)
        {
            roundMoveDic[key] = false;
            if (key is Robot_CharacterMapMover)
                _robotQueue.Enqueue(key);
        }
    }

    /// <summary>推进机器人回合队列：玩家结束行动后依次激活机器人</summary>
    void ProcessNextRobot()
    {
        // 玩家未行动完之前不处理机器人
        foreach (var kvp in roundMoveDic)
            if (kvp.Key is Player_CharacterMapMover && !kvp.Value)
                return;

        while (_robotQueue.Count > 0)
        {
            var robot = _robotQueue.Dequeue() as Robot_CharacterMapMover;
            if (robot != null && roundMoveDic.TryGetValue(robot, out bool done) && !done)
            {
                robot.StartTurn();
                return;
            }
        }
    }

    /// <summary>
    /// 每当一个Mover宣布结束回合，就检测一下
    /// </summary>
    void CheckNewRound()
    {
        DebugManager.Log(EDebugCategory.MapRoom, "MoverDic！Count"+roundMoveDic.Count);
        foreach (var item in roundMoveDic)
        {
            if (!item.Value)
            {
                DebugManager.Log(EDebugCategory.MapRoom, "[MapMoverManager]---存在Mover未行动，本回合尚未结束");
                return;
            }
        }
        DebugManager.Log(EDebugCategory.MapRoom, "[MapMoverManager]---本回合结束!");
        EventCenter.EventTrigger(E_EventType.NewRound);
    }
    #endregion
}

