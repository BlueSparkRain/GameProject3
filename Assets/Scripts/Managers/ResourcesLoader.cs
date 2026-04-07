using Core;
using Core.Interfaces;
using UnityEngine;

/// <summary>
/// 定义所有加载对象的路径，并提供对应的加载接口
/// </summary>
public static class ResourcesLoader 
{

    [Header("SkillSO加载路径")]
    static string skillDataPath = "SOData/SkillSOData/";


    static string skillSlotPath = "Prefab/BattleArea/CharacterBattle/SkillSlots/SkillSlot";


    static string hexRoomPath = "Prefab/HexRoom/MapRoom";
    static string roomCloudePath = "Prefab/HexRoom/RoomCloude";
    static string skillIconPath = "Prefab/Skill/SkillIcon";


    static string mapSaveDataPath = "SOData/HexMapSOData/HexMapSOData";

    public static GameObject FindHexRoomObj() {
        return Resources.Load<GameObject>(hexRoomPath);
    }
    public static GameObject FindRoomCloudeObj() {
        return Resources.Load<GameObject>(roomCloudePath);
    }
     public static GameObject FindSkillIconObj() {
        return Resources.Load<GameObject>(skillIconPath);
    }
    /// <summary>
    /// 根据技能ID返回对应的SkillSOData
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static SkillPropertySO FindSkillSOByID(int id)
    {
        return Resources.Load<SkillPropertySO>($"{skillDataPath}S_{id}");
    }


    public static GameObject FindSkillSlotObj() {
        return Resources.Load<GameObject>(skillSlotPath);
    }

    public static MapSaveSOData FindMapSaveData()
    {
        return Resources.Load<MapSaveSOData>(mapSaveDataPath);

    }
}     

