using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 机器人地图移动器 —— 每回合自动寻路到最优房间并交互。
/// 与 Player_CharacterMapMover 同级实现 IMapMoveable，但不依赖鼠标/UI面板。
/// 使用优先级AI决策(RobotAIDecisionMaker)选择目标，RobotRoomAutoResolver 自动结算房间。
/// </summary>
public class Robot_CharacterMapMover : IMapMoveable
{
    public HexRoomTag currentRoom { get; set; }

    Transform _transform;
    CharacterHandler _dataTag;
    CharacterLevelUpHandler _levelHandler;
    CharacterMapSkiller _skiller;
    CoroutineManager _coroutineMgr;
    HexPathFindingManager _pathfinder;
    RobotAIDecisionMaker _ai;
    RobotRoomAutoResolver _resolver;

    int _remainAP;
    int _maxAP;
    bool _moveStop;
    int _characterLevel;

    public Robot_CharacterMapMover(Transform characterTransform, CharacterHandler dataTag,
        CharacterLevelUpHandler levelHandler, CharacterMapSkiller skiller)
    {
        _transform = characterTransform;
        _dataTag = dataTag;
        _levelHandler = levelHandler;
        _skiller = skiller;
        _characterLevel = dataTag.CharacterData?.CurrentLevel ?? 1;

        _coroutineMgr = GameRoot.GetManager<CoroutineManager>();
        _pathfinder = GameRoot.GetManager<HexPathFindingManager>();

        _ai = new RobotAIDecisionMaker(dataTag.CharacterData, currentRoom);
        _resolver = new RobotRoomAutoResolver(levelHandler, skiller);

        EventCenter.AddEventListener(E_EventType.Mover_MoveStop, OnMoveStop);
    }

    public void Dispose()
    {
        EventCenter.RemoveEventListener(E_EventType.Mover_MoveStop, OnMoveStop);
    }

    /// <summary>回合开始：由MapMoverManager在玩家行动结束后调用</summary>
    public void StartTurn()
    {
        _characterLevel = _dataTag.CharacterData?.CurrentLevel ?? 1;

        int stored = Mathf.Min(_remainAP, _characterLevel);
        _maxAP = Random.Range(1, 7) + _characterLevel / 10 + stored;
        _remainAP = _maxAP;
        _moveStop = false;

        if (currentRoom == null)
            DetectCurrentRoom();
        if (currentRoom != null)
            _ai.UpdateContext(currentRoom);

        var path = _ai.DecideTarget(_remainAP);
        if (path.Count > 0)
        {
            _coroutineMgr.StartCoroutine(MoveRoutine(path), _transform);
        }
        else
        {
            FinishRound();
        }
    }

    void OnMoveStop() { _moveStop = true; }

    /// <summary>执行移动(由MapMoverManager或其他系统调用)</summary>
    public void DoMoveFunc(List<HexRoomTag> path)
    {
        if (path == null || path.Count == 0) return;
        _coroutineMgr.StartCoroutine(MoveRoutine(path), _transform);
    }

    IEnumerator MoveRoutine(List<HexRoomTag> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (_moveStop) break;
            if (_remainAP <= 0) break;

            var targetRoom = path[i];
            var targetPos = targetRoom.transform.position + Vector3.up * GameRoot.GetManager<GameMapManager>().characterYOffset;

            MagicAnimExtens.PerfectJump_WorldAnim(_transform, targetPos);
            yield return new WaitForSeconds(0.35f);

            _remainAP--;

            EventCenter.EventTrigger(E_EventType.Mover_CheckCurrrentRoom, this as IMapMoveable, _transform.position);
            EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);

            if (_moveStop) break;
        }

        _moveStop = true;
        FinishRound();
    }

    void FinishRound()
    {
        EventCenter.EventTrigger(E_EventType.OneMoverEndRound, this as IMapMoveable);
    }

    void DetectCurrentRoom()
    {
        if (Physics.Raycast(_transform.position, Vector3.down, out RaycastHit hit, 5f, LayerMask.GetMask("HexRoom")))
            currentRoom = hit.collider.GetComponent<HexRoomTag>();
    }

    /// <summary>机器人进入特殊房间时被 MapMoverManager 调用，自动结算</summary>
    public void ResolveRoom(HexRoomTag roomTag)
    {
        if (_resolver == null) return;

        var handler = roomTag.GetComponent<HexRoomStyleHandler>();
        if (handler != null && handler.RoomType != E_HexRoomType.None)
        {
            _resolver.Resolve(roomTag);

            if (roomTag.RoomLogic != null)
                roomTag.RoomLogic.Consume();
        }

        _moveStop = true;
    }
}
