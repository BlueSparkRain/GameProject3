using Core;
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 六边形房间Tag标识类（仅保存坐标信息+可行走状态，无业务逻辑）
/// </summary>
public class HexRoomTag : MonoBehaviour
{
    [Header("六边形轴向坐标")]
    public int row; // 轴向坐标q
    public int col; // 轴向坐标r

    [Header("是否可行走")]
    public bool walkable;

    IHexRoom iHexRoom;
    public IHexRoom IHexRoom => iHexRoom;

    bool hasCloude;
    public void InitRoomTag(int _row, int _col)
    {
        row = _row; col = _col;
    }
    public void GetIHexRoom(IHexRoom hexRoom) {
        iHexRoom = hexRoom;
        iHexRoom.DoHexRoomInit();
    }

    /// <summary>
    /// 设置地块的可行走属性（寻路前置设置）
    /// </summary>
    /// <param name="_walkable"></param>
    public void SetCellState(bool _walkable){
        walkable = _walkable;
        //高低差动画
        //if (walkable)
            //coroutineManager.StartDelayedCoroutine(0.4f, () => hexJumpAnimation.WalkableUpAnim());
    }
    public void CallBattle(){
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

