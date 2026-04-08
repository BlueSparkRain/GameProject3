using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class HexTerrainTag : MonoBehaviour
{
    [Header("是否已经被配置过")]
    public bool isEdited = false;
    public E_HexTerrainType  hexTerrainType=E_HexTerrainType.Obstacle__Ocean;

    public void SetTag(E_HexTerrainType _hexTerrainType) { 
        hexTerrainType= _hexTerrainType;
        isEdited=true;

        GetComponent<HexRoomData>().UpdateRoomType(InitRoomType());
    }

     E_HexRoomType InitRoomType()
    {
        switch (hexTerrainType)
        {
            case E_HexTerrainType.Obstacle__Ocean:      return E_HexRoomType.None_无;
            case E_HexTerrainType.Walkable_EmptyLand:   return E_HexRoomType.None_无;
            case E_HexTerrainType.Obstacle_Tree:        return E_HexRoomType.None_无;
            case E_HexTerrainType.Obstacle_Stone:       return E_HexRoomType.None_无;
            case E_HexTerrainType.Obstacle_Mountain:    return E_HexRoomType.None_无;
            case E_HexTerrainType.Walkable_LowLevel_BattleRoom:     return E_HexRoomType.Battle_LowLevel_战斗_杂鱼;
            case E_HexTerrainType.Walkable_MidLevel_BattleRoom:     return E_HexRoomType.Battle_MidLevel_战斗_精英;
            case E_HexTerrainType.Walkable_HighLevel_BattleRoom:    return E_HexRoomType.Battle_HighLevel_战斗_首领;
            case E_HexTerrainType.Walkable_UnknownEventRoom:        return E_HexRoomType.UnknownEvent_随机事件;
            case E_HexTerrainType.Walkable_RewardRoom:              return E_HexRoomType.Reward_神像奖励;
            case E_HexTerrainType.Walkable_CityShopRoom:            return E_HexRoomType.CityShop_城商镇;
            default: return E_HexRoomType.None_无;
        }
    }
}
