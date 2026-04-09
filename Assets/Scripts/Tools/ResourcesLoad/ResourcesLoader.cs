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




    static string hexRoomPath = "Prefab/HexRoom/MapRoom";
    static string roomCloudePath = "Prefab/HexRoom/RoomCloude";

    static string skillSlotPath = "Prefab/SkillUI/SkillSlot";
    static string skillIconPath = "Prefab/SkillUI/SkillIcon";

    static string characterPath = "Prefab/Character/Character_";


    static string characterSOPath = "SOData/CharacterSOData/";

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

    public static GameObject FindCharacterObj(string backStr) {
        return Resources.Load<GameObject>(characterPath+backStr);
    }


    public static CharacterDataSO FindCharaterSO(E_CharacterType characterType) {
        return Resources.Load<CharacterDataSO>(characterSOPath + characterType);
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

    public static MapSaveSOData FindMapSaveData(string mapdataBack)
    {
        return Resources.Load<MapSaveSOData>(mapSaveDataPath+mapdataBack);

    }
}     

