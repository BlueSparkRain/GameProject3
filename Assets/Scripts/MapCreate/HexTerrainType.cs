/// <summary>
/// 六边形地形类型
/// </summary>
public enum E_HexTerrainType
{
    //None,
    Obstacle__Ocean,     //海洋（不可行走）
    Walkable_EmptyLand,  //空白陆地（可行走）
    Obstacle_Tree,       //障碍-树（不可行走）
    Obstacle_Stone,      //障碍-石头（不可行走）
    Obstacle_Mountain,   //障碍-山（不可行走）

    Walkable_LowLevel_BattleRoom,  //小怪战斗房间（可行走）
    Walkable_MidLevel_BattleRoom,  //精英战斗房间（可行走）
    Walkable_HighLevel_BattleRoom, //Boss战斗房间（可行走）
    Walkable_UnknownEventRoom,     //随机事件房间（可行走）
    Walkable_RewardRoom,           //神像房间（可行走）
    Walkable_CityShopRoom,         //城商镇房间（可行走）
}

/// <summary>
/// 区域加载状态
/// </summary>
public enum HexRegionState
{
    Unload,     // 未加载
    Loading,    // 加载中
    Loaded      // 已加载
}