using Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 更新角色当前坐标
/// </summary>
public class Player_CharacterMapMover : IMapMoveable
{
    HexRoomTag IMapMoveable.currentRoom { get; set; }
    public E_CharacterType CharacterType => characterType;
    E_CharacterType characterType;

    CoroutineManager coroutineManager;
    ActionPointsManager apManager;

    //玩家操控角色-需要与Icon交互
    PlayerMapIcon mapIcon;
    //记录角色的位置
    Transform charcaterTrans;
    public Transform CharacterTransform => charcaterTrans;
    public bool IsMoving { get { return isMoving; } }
    /// <summary>
    /// Mover正在移动
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
        apManager = GameRoot.GetManager<ActionPointsManager>();
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        coroutineManager.StartCoroutine(WaitMapIcon(), charcaterTrans);
    }

    void PlayerGetMovePoints(){
        int charLevel = charcaterTrans.GetComponent<CharacterHandler>()?.CharacterData?.CurrentLevel ?? 1;
        int storedPoints = Mathf.Min(apManager.RemainActionPoints, charLevel);

        var (maxPoints, remainPoints) = apManager.EndRound(charLevel, storedPoints);
        moveStop = remainPoints <= 0;
        mapIcon.SetMoveDot(remainPoints);
        EventCenter.EventTrigger<IMapMoveable>(E_EventType.OneMoverEndRound, this);
    }
    IEnumerator WaitMapIcon(){
        yield return new WaitForSeconds(1f);
        mapIcon = GameRoot.GetManager<MapMoverManager>().CreateNewMapIcon(this, charcaterTrans);
        mapIcon.SetMoveDot(apManager.RemainActionPoints);
    }
    void OneTimeMove_MinusActionPoint() { apManager.SpendActionPoints(1); }

    void MoveStop(){
        moveStop = true;
        //将剩余行动点传递给寻路管理器
        mapIcon?.SetMoveDot(apManager.RemainActionPoints);
    }

    /// <summary>
    /// 角色出生时更新数据
    /// </summary>
    public void CharacterZeroMove(){
        //更新当前房间
        EventCenter.EventTrigger(E_EventType.Mover_CheckCurrrentRoom, this as IMapMoveable, charcaterTrans.position);
        //设置当前Mover
        GameRoot.GetManager<MapMoverManager>().SetCurrentMover(this, charcaterTrans.position);

        //只有玩家会相机聚焦
        GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(charcaterTrans.gameObject);
        //只有玩家会清除积云
        EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);
    }


    public void DoMoveFunc(List<HexRoomTag> path){
        //仅仅包含对于MapIcon的禁用操作
        EventCenter.EventTrigger(E_EventType.Mover_PlayerStartMove);
        coroutineManager.StartCoroutine(MoveAnim(path), charcaterTrans);
    }

    /// <summary>战败踢飞移动 — 使用翻滚动画变种，不影响正常行走</summary>
    public void DoKickMove(List<HexRoomTag> path)
    {
        EventCenter.EventTrigger(E_EventType.Mover_PlayerStartMove);
        coroutineManager.StartCoroutine(KickMoveAnim(path), charcaterTrans);
    }

    IEnumerator MoveAnim(List<HexRoomTag> roomPath){
        if (roomPath.Last() != null){
            //寻找终点，相机缓慢平移到目标点
            GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(roomPath.Last().gameObject);
        }
        moveStop = apManager.RemainActionPoints <= 0;
        //Debug.Log("Mover开始移动：路径长度" + roomPath.Count);
        isMoving = true;
        for (int i = 0; i < roomPath.Count; i++){
            if (moveStop){
                DebugManager.Log(EDebugCategory.MapRoom, "Mover被打断，剩余移动点" + apManager.RemainActionPoints);
                break;
            }
            var targetPos = roomPath[i].transform.position + Vector3.up * GameRoot.GetManager<GameMapManager>().characterYOffset;
            MagicAnimExtens.PerfectJump_WorldAnim(charcaterTrans, targetPos);

            //更新当前房间
            //更新行动点,清除视野内云朵
            yield return new WaitForSeconds(0.4f);
            EventCenter.EventTrigger(E_EventType.Mover_CheckCurrrentRoom, this as IMapMoveable, charcaterTrans.position);
            EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);

        }
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        isMoving = false;
    }

    IEnumerator KickMoveAnim(List<HexRoomTag> roomPath)
    {
        if (roomPath.Last() != null)
            GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(roomPath.Last().gameObject);

        isMoving = true;
        for (int i = 0; i < roomPath.Count; i++)
        {
            var targetPos = roomPath[i].transform.position + Vector3.up * GameRoot.GetManager<GameMapManager>().characterYOffset;
            MagicAnimExtens.RollingKick_WorldAnim(charcaterTrans, targetPos);

            yield return new WaitForSeconds(0.45f);
            EventCenter.EventTrigger(E_EventType.Mover_CheckCurrrentRoom, this as IMapMoveable, charcaterTrans.position);
            EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);
        }
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        isMoving = false;
    }
}
