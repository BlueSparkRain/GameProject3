using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;

/// <summary>
/// 更新角色当前坐标
/// </summary>
public class Player_CharacterMapMover : IMapMoveable
{
    public E_CharacterType CharacterType => characterType;

    public bool IsMoving { get { return isMoving; } }
    bool isMoving = false;
    E_CharacterType characterType;

    IHexRoom currentIHexRoom;

    HexRoomData IMapMoveable.currentRoom { get; set; }

    public int max_Actionpoints = 7;
    public int remain_Acionpoints;

    CoroutineManager coroutineManager;
    CharacterMapIcon mapIcon;
    Transform charcaterTrans;
    public Player_CharacterMapMover(E_CharacterType characterType, Transform charcaterTrans)
    {
        EventCenter.AddEventListener(E_EventType.Mover_OneTimeMove, OneTimeMove);
        EventCenter.AddEventListener(E_EventType.Mover_MoveStop, MoveStop);
        EventCenter.AddEventListener(E_EventType.Player_RoundEnd, PlayerGetMovePoints);

        this.characterType = characterType;
        this.charcaterTrans = charcaterTrans;
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        CreateMapIcon();
    }

    void CreateMapIcon()
    {
        coroutineManager.StartCoroutine(WaitMapIcon());
    }

    void PlayerGetMovePoints()
    {
        Debug.Log("玩家主动结束回合，获得行动点");
        remain_Acionpoints = max_Actionpoints;
        moveStop = remain_Acionpoints <= 0;
        mapIcon.SetMoveDot(remain_Acionpoints);
    }

    IEnumerator WaitMapIcon()
    {
        yield return new WaitForSeconds(1.5f);
        mapIcon = GameRoot.GetManager<MapMoverChecker>().CreateNewMapIcon(this,charcaterTrans);
        mapIcon.SetMoveDot(remain_Acionpoints);
        PlayerGetMovePoints();
    }

    void OneTimeMove()
    {
        remain_Acionpoints--;
    }

    void MoveStop()
    {
        moveStop = true;
        //将剩余行动点纯递给寻路管理器
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

    //移动被终止了
    public bool moveStop;

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
            var targetPos= roomPath[i].transform.position + Vector3.up * 0.6f;
            //charcaterTrans.DOMove(targetPos, 0.2f);
            PerfectJumpAnim(targetPos);

            //更新当前房间
            //更新行动点,清除视野内云朵
            yield return new WaitForSeconds(0.4f);
            EventCenter.EventTrigger(E_EventType.Mover_CheckCurrrentRoom,this as IMapMoveable,charcaterTrans.position);
            EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);
            currentIHexRoom?.DoHexRoomLogic();

        }
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        isMoving = false;
    }

    void PerfectJumpAnim(Vector3 targetPos) {
        // 清空旧动画，防止卡顿重叠
        charcaterTrans.DOKill();
        float totalDuration = 0.2f;   // 总时长（0.4~0.6最丝滑）
        float jumpPower = 0.8f;      // 弹跳高度
                                     // 挤压/拉伸幅度（数值越小越柔和，越大越Q弹）
        float squeezeXZ = 1.15f;     // 起跳/落地 XZ挤压
        float squeezeY = 0.8f;      // 起跳/落地 Y压缩
        float stretchXZ = 0.9f;      // 空中 XZ拉伸
        float stretchY = 1.2f;      // 空中 Y拉长

        // 创建序列动画
        Sequence seq = DOTween.Sequence();

        // 1. 抛物线跳跃（基础位移，丝滑曲线）
        seq.Join(charcaterTrans.DOJump(targetPos, jumpPower, 1, totalDuration)
            .SetEase(Ease.InOutSine)); // 【关键】最丝滑的正弦曲线，抛弃Flash

        // ====================== 无缝缩放动画（完美同步跳跃）======================
        // 阶段1：起跳快速挤压 (0 ~ 20% 总时长)
        seq.Insert(0, charcaterTrans.DOScale(
            new Vector3(squeezeXZ, squeezeY, squeezeXZ),
            totalDuration * 0.3f
        ).SetEase(Ease.OutSine));

        // 阶段2：腾空缓慢拉伸 (20% ~ 50% 总时长)
        seq.Insert(totalDuration * 0.2f, charcaterTrans.DOScale(
            new Vector3(stretchXZ, stretchY, stretchXZ),
            totalDuration * 0.3f
        ).SetEase(Ease.InOutSine));

        // 阶段3：落地前挤压 (50% ~ 85% 总时长)
        seq.Insert(totalDuration * 0.5f, charcaterTrans.DOScale(
            new Vector3(squeezeXZ, squeezeY, squeezeXZ),
            totalDuration * 0.4f
        ).SetEase(Ease.InOutSine));

        // 阶段4：落地回弹复原 (85% ~ 100% 总时长)
        seq.Insert(totalDuration * 0.85f, charcaterTrans.DOScale(
            Vector3.one,
            totalDuration * 0.3f
        ).SetEase(Ease.OutSine));

        // 播放
        seq.Play();
    }

}
