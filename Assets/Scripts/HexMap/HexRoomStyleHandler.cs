using Core;
using UnityEngine;

/// <summary>
/// 管理房间的外观装载 及 浮云召唤 及 房间模型
/// </summary>
public class HexRoomStyleHandler : MonoBehaviour
{
    [Header("房间类型")]
    E_HexRoomType roomType = E_HexRoomType.None_无交互地形;

    HexJumpAnimHandler hexJumpAnimation;
    HexRoomTag roomTag;
    public E_HexRoomType RoomType => roomType;
    public void InitRoomStyle(HexRoomTag _roomTag)
    {
        hexJumpAnimation = GetComponent<HexJumpAnimHandler>();
        roomTag = _roomTag;
        //只有海洋不会产生云朵

        if (GetComponent<HexTerrainStyleHandler>().hexTerrainType != E_HexTerrainType.Obstacle_Ocean)
        {
            LoadRoomCloude();
        }
    }

    void LoadRoomCloude()
    {
        var cloude = GameRoot.GetManager<ObjectPoolManager>().GetInstance(E_PoolType.RoomCloude_房间遮云);
        cloude.transform.position = transform.position + Vector3.up * 23f;
        hexJumpAnimation.CloudeAppear(cloude.transform);
    }

    public void SetRoomType(E_HexRoomType _roomType)
    {
        roomType = _roomType;
        IHexRoom iHexRoom = null;
        switch (roomType)
        {
            case E_HexRoomType.None_无交互地形: iHexRoom = new NoneHexRoom(); break;
            case E_HexRoomType.Battle_LowLevel_战斗_杂鱼: iHexRoom = new BattleHexRoom(roomTag, E_BattleType.杂鱼敌人); break;
            case E_HexRoomType.Battle_MidLevel_战斗_精英: iHexRoom = new BattleHexRoom(roomTag, E_BattleType.精英敌人); break;
            case E_HexRoomType.Battle_HighLevel_战斗_首领: iHexRoom = new BattleHexRoom(roomTag, E_BattleType.首领敌人); break;
            case E_HexRoomType.NPC_特定交互: iHexRoom = new NPCHexRoom(); break;
            case E_HexRoomType.UnknownEvent_随机事件: iHexRoom = new UnknownEventHexRoom(); break;
            case E_HexRoomType.Reward_神像奖励: iHexRoom = new RewardHexRoom(); break;
            case E_HexRoomType.CityShop_城商镇: iHexRoom = new CityShopHexRoom(); break;
            default: break;
        }
        GetComponent<HexRoomTag>().GetIHexRoom(iHexRoom);
        iHexRoom.DoHexRoomModel(transform.position + Vector3.up);

    }
}
