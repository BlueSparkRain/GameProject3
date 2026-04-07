/// <summary>
/// 六边形地形类型
/// </summary>
public enum E_HexTerrainType
{
    //None,
    Obstacle__Ocean,     //海洋（不可行走）
    Walkable_EmptyLand,  //大陆（可行走）
    Obstacle_Tree,       //障碍-树（不可行走）
    Obstacle_Stone,      //障碍-石头（不可行走）
    Obstacle_Mountain,   //障碍-山（不可行走）

    Walkable_BattleRoom, //战斗房间（可行走）
    Walkable_EventRoom,  //事件房间（可行走）
    Walkable_RewardRoom, //神像房间（可行走）
    Walkable_CityRoom,   //城镇房间（可行走）
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