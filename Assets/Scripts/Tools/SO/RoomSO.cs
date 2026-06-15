using UnityEngine;

[CreateAssetMenu(menuName = "SOData/RoomSO", fileName = "RoomSOData")]
public class RoomSO : ScriptableObject
{
    /// <summary>
    /// 房间类型
    /// </summary>
    public E_HexRoomType roomType = E_HexRoomType.None;

    /// <summary>
    /// 房间图标
    /// </summary>
    public Sprite roomIcon;

}

public enum E_HexRoomType
{
    None,
    Battle_LowLevel,
    Battle_MidLevel,
    Battle_HighLevel,
    CityShop,
    NPC,
    UnknownEvent,
    Reward,
}

public enum E_BattleType
{
    Low,
    Mid,
    Boss,
}

public enum E_NPCType
{
    任务,
    事件,
    交易,
    比试
}

public static class HexRoomTypeExtensions
{
    public static bool IsBattleRoom(this E_HexRoomType type) =>
        type == E_HexRoomType.Battle_LowLevel ||
        type == E_HexRoomType.Battle_MidLevel ||
        type == E_HexRoomType.Battle_HighLevel;

    public static E_BattleType ToBattleType(this E_HexRoomType type) => type switch
    {
        E_HexRoomType.Battle_LowLevel => E_BattleType.Low,
        E_HexRoomType.Battle_MidLevel => E_BattleType.Mid,
        E_HexRoomType.Battle_HighLevel => E_BattleType.Boss,
        _ => E_BattleType.Low,
    };
}
