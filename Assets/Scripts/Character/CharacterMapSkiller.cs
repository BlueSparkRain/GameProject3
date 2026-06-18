using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图场景下管理角色的技能配置和技能数据
/// 每实例独立存档（玩家用"Player"key，非玩家用GUID）
/// </summary>
public class CharacterMapSkiller : MonoBehaviour{
    CharacterHandler characterDataTag;
    public int restSkillSlotNum => Mathf.Max(restWholeSkillDatas.Count, 3);
    public int normalSkillSlotNum => characterDataTag?.CharacterData?.AutoSkillSlotCount ?? 6;
    public int atbSkillSlotNum => characterDataTag?.CharacterData?.AtbSkillSlotCount ?? 4;
    private List<SkillData> restWholeSkillDatas = new List<SkillData>();
    private List<SkillData> normalSkillDatas = new List<SkillData>();
    private List<SkillData> atbSkillDatas = new List<SkillData>();
    public List<SkillData> RestWholeSkillDatas => restWholeSkillDatas;
    public List<SkillData> NormalSkillDatas => normalSkillDatas;
    public List<SkillData> ATBSkillDatas => atbSkillDatas;
    [Header("是否可以调整技能分配")]
    public bool canActSettle = false;

    [SerializeField, HideInInspector]
    string _instanceSaveId;

    string GetSaveId(){
        if (!string.IsNullOrEmpty(_instanceSaveId))
            return _instanceSaveId;
        _instanceSaveId = characterDataTag != null && characterDataTag.isPlayer
            ? "Player": System.Guid.NewGuid().ToString("N").Substring(0, 8);
        return _instanceSaveId;
    }

    private void Awake(){
        characterDataTag = GetComponent<CharacterHandler>();
    }

    private void Start(){
        EventCenter.EventTrigger(E_EventType.Character_Skiller_Regist, this, characterDataTag.isPlayer);
        LoadSkillAssignments();
    }

    public void UpdateSkilerSettle(List<SkillData> restWholeDatas,
                                     List<SkillData> normalDatas,
                                     List<SkillData> atbDatas)
    {
        restWholeSkillDatas = restWholeDatas;
        normalSkillDatas = normalDatas;
        atbSkillDatas = atbDatas;
        SaveSkillAssignments();
    }

    public void UpdateActableDataList(
        List<SkillData> _RestWholeSkillDatas,
        List<SkillData> _NormalSkillDatas,
        List<SkillData> _ATBSkillDatas){
        UpdateSkilerSettle(_RestWholeSkillDatas, _NormalSkillDatas, _ATBSkillDatas);
    }

    public void GetNewSkill(int skillID){
        var newSkillData = new SkillData(ResourcesLoader.FindSkillSOByID(skillID));
        restWholeSkillDatas.Add(newSkillData);
        EventCenter.EventTrigger(E_EventType.Character_GetNewSkill);
    }

    void SaveSkillAssignments(){
        if (characterDataTag == null) return;
        var saveData = new Save_CharacterSkillData(normalSkillDatas, atbSkillDatas, restWholeSkillDatas);
        string saveId = GetSaveId();
        _instanceSaveId = saveId;
        JsonSaver.Save(saveData, saveId);
        DebugManager.Log(EDebugCategory.MapRoom, $"[CharacterMapSkiller] 已保存技能: Normal={normalSkillDatas.Count}, ATB={atbSkillDatas.Count}, Rest={restWholeSkillDatas.Count}, key={saveId}");
    }

    void LoadSkillAssignments(){
        if (characterDataTag == null) return;
        string newKey = GetSaveId();
        _instanceSaveId = newKey;
        var saveData = JsonSaver.Load<Save_CharacterSkillData>(newKey);
        if (saveData != null && saveData.IsValid() && HasAnySkillIDs(saveData)){
            ApplyLoadedData(saveData);
            return;
        }
        string legacyKey = characterDataTag.CharacterData.characterType.ToString();
        if (legacyKey != newKey){
            var legacyData = JsonSaver.Load<Save_CharacterSkillData>(legacyKey);
            if (legacyData != null && legacyData.IsValid() && HasAnySkillIDs(legacyData)){
                DebugManager.Log(EDebugCategory.MapRoom, $"[CharacterMapSkiller] 从旧存档迁移: {legacyKey} -> {newKey}");
                ApplyLoadedData(legacyData);
                SaveSkillAssignments(); 
                return;
            }
        }
        DebugManager.Log(EDebugCategory.MapRoom, $"[CharacterMapSkiller] 未找到角色存档 (key={newKey})，从空白开始");
    }
    void ApplyLoadedData(Save_CharacterSkillData saveData) { 
        normalSkillDatas = RebuildSkillListFromIDs(saveData.normalSkillIDs);
        atbSkillDatas = RebuildSkillListFromIDs(saveData.atbSkillIDs);
        if (saveData.restWholeSkillIDs != null && saveData.restWholeSkillIDs.Count > 0
            && restWholeSkillDatas.Count == 0)
            restWholeSkillDatas = RebuildSkillListFromIDs(saveData.restWholeSkillIDs);

        DebugManager.Log(EDebugCategory.MapRoom, $"[CharacterMapSkiller] 已加载存档: Normal={normalSkillDatas.Count}, ATB={atbSkillDatas.Count}, key={_instanceSaveId}");
    }
    bool HasAnySkillIDs(Save_CharacterSkillData data){
        return (data.normalSkillIDs != null && data.normalSkillIDs.Count > 0) ||
               (data.atbSkillIDs != null && data.atbSkillIDs.Count > 0) ||
               (data.restWholeSkillIDs != null && data.restWholeSkillIDs.Count > 0);
    }

    List<SkillData> RebuildSkillListFromIDs(List<int> ids)
    {
        var list = new List<SkillData>();
        if (ids == null) return list;
        foreach (int id in ids)
        {
            var so = ResourcesLoader.FindSkillSOByID(id);
            if (so != null)
                list.Add(new SkillData(so));
            else
                DebugManager.LogWarning(EDebugCategory.MapRoom, $"[CharacterMapSkiller] 无法根据ID={id}重建技能数据");
        }
        return list;
    }
}

[System.Serializable]
public class Save_CharacterSkillData : IValidatable{
    public Save_CharacterSkillData() { }
    public Save_CharacterSkillData(List<SkillData> normalDatas, List<SkillData> atbDatas, List<SkillData> restDatas){
        normalSkillIDs = new List<int>();
        atbSkillIDs = new List<int>();
        restWholeSkillIDs = new List<int>();
        if (normalDatas != null)
            foreach (var d in normalDatas) normalSkillIDs.Add(d.skill_ID);
        if (atbDatas != null)
            foreach (var d in atbDatas) atbSkillIDs.Add(d.skill_ID);
        if (restDatas != null)
            foreach (var d in restDatas) restWholeSkillIDs.Add(d.skill_ID);
    }
    public List<int> normalSkillIDs = new List<int>();
    public List<int> atbSkillIDs = new List<int>();
    public List<int> restWholeSkillIDs = new List<int>();
    public bool IsValid() =>
        normalSkillIDs != null && atbSkillIDs != null && restWholeSkillIDs != null;
}
