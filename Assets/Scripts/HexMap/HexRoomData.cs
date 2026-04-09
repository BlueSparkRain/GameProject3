using Core;
using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 六边形房间基础类（仅保存坐标信息，无业务逻辑）
/// </summary>
public class HexRoomData : MonoBehaviour
{
    [Header("六边形轴向坐标")]
    public int row; // 轴向坐标q
    public int col; // 轴向坐标r

    [Header("是否可行走")]
    public bool walkable;

    [Header("房间类型")]
    public E_HexRoomType roomType = E_HexRoomType.None_无;

    HexJumpAnimation hexJumpAnimation;
    CoroutineManager coroutineManager;


    public IHexRoom IHexRoom=>iHexRoom;
    IHexRoom iHexRoom;

    bool hasCloude;
    public void InitRoomID(int _row, int _col)
    {
        hexJumpAnimation = GetComponent<HexJumpAnimation>();

        //只有海洋不会产生云朵
        if (GetComponent<HexTerrainTag>().hexTerrainType != E_HexTerrainType.Obstacle__Ocean)
        {
            LoadRoomCloude();
        }

        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        row = _row; col = _col;
        hexJumpAnimation.TriggerJump(0.4f);
    }

    public void UpdateRoomType(E_HexRoomType _roomType) {
        InitRoomStyle(_roomType);
    }

    void LoadRoomCloude() {
        var cloude = GameRoot.GetManager<ObjectPoolManager>().GetInstance(EPoolType.RoomCloude_房间遮云);
        cloude.transform.position = transform.position + Vector3.up * 23f;
        hexJumpAnimation.CloudeAppear(cloude.transform);
    }

    void InitRoomStyle(E_HexRoomType _roomType)
    {
        roomType = _roomType;
        switch (roomType)
        {
            case E_HexRoomType.None_无:iHexRoom = new NoneHexRoom();break;
            case E_HexRoomType.Battle_LowLevel_战斗_杂鱼: iHexRoom = new BattleHexRoom(E_BattleType.杂鱼敌人); break;
            case E_HexRoomType.Battle_MidLevel_战斗_精英: iHexRoom = new BattleHexRoom(E_BattleType.精英敌人); break;
            case E_HexRoomType.Battle_HighLevel_战斗_首领: iHexRoom = new BattleHexRoom(E_BattleType.首领敌人); break;
            case E_HexRoomType.NPC_特定交互: iHexRoom = new NPCHexRoom(); break;
            case E_HexRoomType.UnknownEvent_随机事件:iHexRoom = new UnknownEventHexRoom();break;
            case E_HexRoomType.Reward_神像奖励: iHexRoom=new RewardHexRoom();break;
            case E_HexRoomType.CityShop_城商镇:iHexRoom=new CityShopHexRoom();break;
            default: break;
        }
        //初始化房间类型
        IHexRoom.DoHexRoomInit();
        //得到新的类型，加载对应的模型
        iHexRoom.DoHexRoomModel(transform.position+Vector3.up);
    }
    public virtual void ResetSelf()
    {

    }

    public void SetCellState(bool _walkable)
    {
        walkable = _walkable;
        if (walkable)
            coroutineManager.StartDelayedCoroutine(0.4f, () => hexJumpAnimation.WalkableUpAnim());
    }


    public void CallBattle()
    {
        Debug.Log("Go");
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);
    }
}


[Serializable]
public class MapRoomData {

    /// <summary>
    /// 保存时是否有云朵
    /// </summary>
    public bool hasCloude;
}

public interface IHexRoom
{
    public void DoHexRoomInit();
    public void DoHexRoomLogic(UnityAction roomJob=null);

    public void DoHexRoomModel(Vector3 pos);
}

