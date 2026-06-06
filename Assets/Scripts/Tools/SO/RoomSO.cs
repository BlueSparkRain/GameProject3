using UnityEngine;

[CreateAssetMenu(menuName = "SOData/RoomSO", fileName = "RoomSOData")]
public class RoomSO : ScriptableObject
{
    /// <summary>
    /// 房间类型
    /// </summary>
    public E_HexRoomType roomType = E_HexRoomType.None_无交互地形;

    /// <summary>
    /// 房间图标
    /// </summary>
    public Sprite roomIcon;

}

public enum E_HexRoomType
{
    None_无交互地形,
    Battle_LowLevel_战斗_杂鱼,
    Battle_MidLevel_战斗_精英,
    Battle_HighLevel_战斗_首领,
    CityShop_城商镇,
    NPC_特定交互,
    UnknownEvent_随机事件,
    Reward_神像奖励,
}


public enum E_BattleType
{
    杂鱼敌人,
    精英敌人,
    首领敌人,
}

public enum E_NPCType
{
    任务,
    事件,
    交易,
    比试
}
