using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName ="SOData/RoomSO",fileName ="RoomSOData")]
public class RoomSO :ScriptableObject
{
    /// <summary>
    /// 房间类型
    /// </summary>
    public E_HexRoomType roomType=E_HexRoomType.None_无;

    /// <summary>
    /// 房间图标
    /// </summary>
    public Sprite roomIcon;



}



public enum E_HexRoomType {
    None_无,
    Battle_战斗,
    NPC_特定交互,
    Unknown_随机事件,
    NewArea_锚点,
}


public enum E_BattleType { 
    普通敌人,
    精英敌人,
    首领敌人,
}

public enum E_NPCType { 
    任务,
    事件,
    交易,
    比试
}
