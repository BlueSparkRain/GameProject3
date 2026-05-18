using Core;
using System;
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

    HexJumpAnimation hexJumpAnimation;
    CoroutineManager coroutineManager;
    public IHexRoom IHexRoom => iHexRoom;
    IHexRoom iHexRoom;

    bool hasCloude;
    public void InitRoomID(int _row, int _col)
    {
        hexJumpAnimation = GetComponent<HexJumpAnimation>();

        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        row = _row; col = _col;
        hexJumpAnimation.TriggerJump(0.4f);
    }
    public void GetIHexRoom(IHexRoom hexRoom) {
        iHexRoom = hexRoom;
        iHexRoom.DoHexRoomInit();
    }
 
    public void SetCellState(bool _walkable)
    {
        walkable = _walkable;
        //高低差动画
        //if (walkable)
            //coroutineManager.StartDelayedCoroutine(0.4f, () => hexJumpAnimation.WalkableUpAnim());
    }
    public void CallBattle()
    {
        Debug.Log("Go");
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);
    }
}


[Serializable]
public class MapRoomData
{

    /// <summary>
    /// 保存时是否有云朵
    /// </summary>
    public bool hasCloude;
}

public interface IHexRoom
{
    public void DoHexRoomInit();
    public void DoHexRoomLogic(UnityAction roomJob = null);
    public void DoHexRoomModel(Vector3 pos);
}

