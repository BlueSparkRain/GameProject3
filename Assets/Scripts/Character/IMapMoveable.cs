using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapMoveable 
{

    public HexRoomData currentRoom { get; set; }
    /// <summary>
    /// 执行回合移动方法
    /// </summary>
    public void DoMoveFunc(List<HexRoomData> path);

    /// <summary>
    /// Mover宣布结束自身回合
    /// </summary>
    public void DoEndSelfRound()
    {
        EventCenter.EventTrigger(E_EventType.OneMoverEndRound,this);
    }
}
