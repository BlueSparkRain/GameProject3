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
}
