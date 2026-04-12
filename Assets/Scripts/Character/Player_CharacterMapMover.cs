using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 更新角色当前坐标
/// </summary>
public class Player_CharacterMapMover : IMapMoveable
{
    HexRoomData IMapMoveable.currentRoom { get; set; }
    public E_CharacterType CharacterType => characterType;
    E_CharacterType characterType;

    public int max_Actionpoints = 7;
    [Header("剩余行动点")]
    public int remain_Acionpoints;

    CoroutineManager coroutineManager;
    //MapMoverChecker  moverChecker;

    //玩家操控角色-需要与Icon交互
    PlayerMapIcon mapIcon;
    //记录角色的位置
    Transform charcaterTrans;

    public bool IsMoving { get { return isMoving; } }
    /// <summary>
    /// Mover正在移动？
    /// </summary>
    bool isMoving = false;

    /// <summary>
    /// 移动被终止
    /// </summary>
    bool moveStop;

    public Player_CharacterMapMover(E_CharacterType characterType, Transform charcaterTrans)
    {
        EventCenter.AddEventListener(E_EventType.Mover_OneTimeMove, OneTimeMove_MinusActionPoint);
        EventCenter.AddEventListener(E_EventType.Mover_MoveStop, MoveStop);
        EventCenter.AddEventListener(E_EventType.Player_RoundEnd, PlayerGetMovePoints);

        this.characterType = characterType;
        this.charcaterTrans = charcaterTrans;
        coroutineManager = GameRoot.GetManager<CoroutineManager>();


        coroutineManager.StartCoroutine(WaitMapIcon());
    }


    void PlayerGetMovePoints()
    {
        remain_Acionpoints = max_Actionpoints;
        //EventCenter.EventTrigger(E_EventType.OneMoverEndRound,this);
        moveStop = remain_Acionpoints <= 0;
        mapIcon.SetMoveDot(remain_Acionpoints);
    }

    IEnumerator WaitMapIcon()
    {
        yield return new WaitForSeconds(1f);
        mapIcon = GameRoot.GetManager<MapMoverChecker>().CreateNewMapIcon(this, charcaterTrans);
        mapIcon.SetMoveDot(remain_Acionpoints);
        PlayerGetMovePoints();
    }

    void OneTimeMove_MinusActionPoint(){ remain_Acionpoints--;}

    void MoveStop()
    {
        moveStop = true;
        //将剩余行动点传递给寻路管理器
        mapIcon.SetMoveDot(remain_Acionpoints);
    }

    /// <summary>
    /// 角色出生时更新数据
    /// </summary>
    public void CharacterZeroMove()
    {
        //更新当前房间
        EventCenter.EventTrigger(E_EventType.Mover_CheckCurrrentRoom, this as IMapMoveable, charcaterTrans.position);
        //根据MoverChecker顺序来决定当前的Mover
        //GameRoot.GetManager<MapMoverChecker>().SetCurrentMover(this);

        //玩家角色登场先走0步
        //设置当前Mover
        GameRoot.GetManager<MapMoverChecker>().SetCurrentMover(this);

        //只有玩家会相机聚焦
        GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(charcaterTrans.gameObject);
        //只有玩家会清除积云
        EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);
    }


    public void DoMoveFunc(List<HexRoomData> path)
    {
        //仅仅包含对于MapIcon的禁用操作
        EventCenter.EventTrigger(E_EventType.Mover_PlayerStartMove);
        coroutineManager.StartCoroutine(MoveAnim(path));

    }


    IEnumerator MoveAnim(List<HexRoomData> roomPath)
    {
        moveStop = remain_Acionpoints <= 0;
        Debug.Log("Mover开始移动：路径长度" + roomPath.Count);
        isMoving = true;

        for (int i = 0; i < roomPath.Count; i++)
        {
            if (moveStop)
            {
                //只有敌人AI才能直接应用房间的结果，玩家需经理房间后才能获得奖励
                //EventCenter.EventTrigger(E_EventType.Mover_IntoSpecialRoom, GetComponent<CharacterMapSkiller>(),  currentRoom.roomType);
                //EventCenter.EventTrigger(E_EventType.Mover_IntoSpecialRoom, GetComponent<CharacterMapSkiller>(), currentRoom.roomType);

                Debug.Log("Mover被打断，剩余移动点" + remain_Acionpoints);
                break;
            }
            var targetPos = roomPath[i].transform.position + Vector3.up * 0.6f;
            MagicAnimExtens.PerfectJump_WorldAnim(charcaterTrans,targetPos);

            //更新当前房间
            //更新行动点,清除视野内云朵
            yield return new WaitForSeconds(0.4f);
            EventCenter.EventTrigger(E_EventType.Mover_CheckCurrrentRoom, this as IMapMoveable, charcaterTrans.position);
            EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);

        }
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        isMoving = false;
    }
}
