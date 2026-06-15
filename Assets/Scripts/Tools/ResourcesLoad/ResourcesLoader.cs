using Core;
using Core.Interfaces;
using UnityEngine;

/// <summary>
/// 统一管理各类加载路径，并提供相应的加载接口
/// </summary>
public static class ResourcesLoader
{
    [Header("SkillSO加载路径")]
    static string skillDataPath = "SOData/SkillSOData/";

    /// <summary>
    /// 加载MapIcon
    /// </summary>
    static string mapIcon_RectPrefabPath = "Prefab/MapUI/PlayerMapIcon";
    static string mapIcon_CirclePrefabPath = "Prefab/MapUI/PlayerMapIcon_Circle";

    static string hexRoomPath = "Prefab/HexRoom/HexRoom";
    static string roomCloudePath = "Prefab/HexRoom/RoomCloude";
    static string hexFacePath = "Prefab/HexRoom/HexFace";
    static string hexRoomIconPath = "Prefab/HexRoom/HexRoomIcon";

    static string skillSlotPath = "Prefab/SkillUI/SkillSlot";
    static string skillIconPath = "Prefab/SkillUI/SkillIcon";
    static string floatingTextPath = "Prefab/FloatingText";
    static string atbDotPath = "Prefab/ATB_Point";

    static string mapCharacterPath = "Prefab/Character/Character_";
    static string battlerCharacterPath = "Prefab/BattleArea/CharacterBattleArea";


    static string characterSOPath = "SOData/CharacterSOData/";
    static string characterGrowthSOPath = "SOData/CharacterGrowthSOData/";

    static string mapSaveDataPath = "SOData/HexMapSOData/HexMapSOData";

    static string terrainSODataPath = "SOData/TerrainSOData/";

    static string roomModelSOPath = "SOData/RoomModelSOData/";

    static string weaknessIconPath = "Prefab/BattleArea/CharacterBattle/WeaknessIcon";
    static string weaknessIconConfigPath = "SOData/WeaknessIconConfig/WeaknessIconConfig";

    static string weaknessConfigPath = "SOData/CharacterWeaknessConfig/Weakness_";
    static string atbIntentionConfigPath = "SOData/ATBIntentionConfig/ATBIntention_";
    static string autoSkillConfigPath = "SOData/AutoSkillConfig/AutoSkill_";

    public static GameObject FindHexRoomObj() {
        return Resources.Load<GameObject>(hexRoomPath);}
    public static GameObject FindRoomCloudeObj() {
        return Resources.Load<GameObject>(roomCloudePath);}
    public static GameObject FindHexFaceObj() {
        return Resources.Load<GameObject>(hexFacePath);}
    public static GameObject FindHexRoomIconObj() {
        return Resources.Load<GameObject>(hexRoomIconPath);}
    public static GameObject FindSkillIconObj() {
        return Resources.Load<GameObject>(skillIconPath);}
    public static GameObject FindFloatingTextObj() {
        return Resources.Load<GameObject>(floatingTextPath);}
    public static GameObject FindATBDotObj() {
        return Resources.Load<GameObject>(atbDotPath);}
    public static GameObject FindCharacterObj(string backStr) {
        return Resources.Load<GameObject>(mapCharacterPath+backStr);}
    public static GameObject FindMapIcon_RectObj() {
        return Resources.Load<GameObject>(mapIcon_RectPrefabPath);}
    public static GameObject FindMapIcon_CircleObj(){
        return Resources.Load<GameObject>(mapIcon_CirclePrefabPath);}
    public static RoomModelSOData FindRoomModelSO(E_RoomModelType  roomModelType) {
        return Resources.Load<RoomModelSOData>(roomModelSOPath+roomModelType);
    }
    public static TerrainSOData FindTerrainData(E_HexTerrainType terrainType) {
        return Resources.Load<TerrainSOData>(terrainSODataPath+terrainType);
    }
    public static CharacterDataSO FindCharaterSO(E_CharacterType characterType) {
        return Resources.Load<CharacterDataSO>(characterSOPath + characterType);
    }

    public static CharcterPropertyGrowthSO FindCharaterGrowthSO(E_CharacterType characterType)
    {
        return Resources.Load<CharcterPropertyGrowthSO>(characterGrowthSOPath + characterType);
    }
    /// <summary>
    /// 根据技能ID加载对应的SkillSOData
    /// </summary>
    public static SkillPropertySO FindSkillSOByID(int id)
    {
        return Resources.Load<SkillPropertySO>($"{skillDataPath}S_{id}");
    }

    /// <summary>
    /// 根据E_SkillName枚举查找对应的SkillPropertySO（枚举值即技能ID）
    /// 返回的SO中包含 skill_ID、skill_Name 等完整数据。
    /// </summary>
    public static SkillPropertySO FindSkillSOBySkillName(E_SkillName name)
    {
        return FindSkillSOByID((int)name);
    }
    public static GameObject FindSkillSlotObj() {
        return Resources.Load<GameObject>(skillSlotPath);
    }
    public static MapSaveSOData FindMapSaveData(string mapdataBack){
        return Resources.Load<MapSaveSOData>(mapSaveDataPath+mapdataBack);
    }
    public static GameObject FindBattleCharacterObj() {
        return Resources.Load<GameObject>(battlerCharacterPath);
    }
    public static GameObject FindWeaknessIconObj() {
        return Resources.Load<GameObject>(weaknessIconPath);
    }
    public static WeaknessIconConfigSO FindWeaknessIconConfig() {
        return Resources.Load<WeaknessIconConfigSO>(weaknessIconConfigPath);
    }
    public static CharacterWeaknessConfigSO FindWeaknessConfig(E_CharacterType characterType){
        return Resources.Load<CharacterWeaknessConfigSO>(weaknessConfigPath + characterType);
    }
    public static ATBIntentionConfigSO FindATBIntentionConfig(E_CharacterType characterType){
        return Resources.Load<ATBIntentionConfigSO>(atbIntentionConfigPath + characterType);
    }
    public static AutoSkillConfigSO FindAutoSkillConfig(E_CharacterType characterType){
        return Resources.Load<AutoSkillConfigSO>(autoSkillConfigPath + characterType);
    }

}
