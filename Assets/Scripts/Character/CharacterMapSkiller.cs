using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图场景下管理玩家当前的技能配置和技能数据组件
/// 技能管理器，增加或减少或禁用特定的技能
/// 同时管理所有技能的更新和角色属性的更新
/// </summary>
public class CharacterMapSkiller : MonoBehaviour
{
    //当打开技能配置面板的时候，读取的就是这里的配置
    CharacterDataTag characterDataTag;
    //槽位数
    public int restSkillSlotNum = 20;
    public int normalSkillSlotNum => characterDataTag?.CharacterData?.AutoSkillSlotCount ?? 9;
    public int atbSkillSlotNum => characterDataTag?.CharacterData?.AtbSkillSlotCount ?? 5;
    //角色尚未分配的所有技能数据
    private List<SkillData> restWholeSkillDatas = new List<SkillData>();

    //底部基础技能数据
    private List<SkillData> normalSkillDatas = new List<SkillData>();

    //右侧ATB技能数据
    private List<SkillData> atbSkillDatas = new List<SkillData>();

    int current_restSkillNum = 0;
    int current_normalSkillNum = 0;
    int current_atbSkillNum = 0;

    public List<SkillData> RestWholeSkillDatas => restWholeSkillDatas;//属性，不产生内存
    public List<SkillData> NormalSkillDatas => normalSkillDatas;
    public List<SkillData> ATBSkillDatas => atbSkillDatas;

    [Header("是否可以调整技能分配")]
    public bool canActSettle = false;

    private void Start()
    {
        characterDataTag = GetComponent<CharacterDataTag>();
        EventCenter.EventTrigger(E_EventType.Character_Skiller_Regist, this, characterDataTag.isPlayer);
        LoadSkillAssignments();
    }

    /// <summary>
    /// 应用配置过后的技能列表
    /// </summary>
    public void UpdateSkilerSettle(List<SkillData> restWholeDatas,
                                     List<SkillData> normalDatas,
                                     List<SkillData> atbDatas)
    {
        restWholeSkillDatas = restWholeDatas;
        normalSkillDatas = normalDatas;
        atbSkillDatas = atbDatas;
        current_restSkillNum = RestWholeSkillDatas.Count;
        current_normalSkillNum = normalSkillDatas.Count;
        current_atbSkillNum = atbSkillDatas.Count;
        SaveSkillAssignments();
    }


    /// <summary>
    /// 通过技能ID获得一个新的技能，并对应的技能数据添加到技能列表中
    /// </summary>
    /// <param name="skillID"></param>
    public void GetNewSkill(int skillID)
    {
        var newSkillData = new SkillData(ResourcesLoader.FindSkillSOByID(skillID));
        restWholeSkillDatas.Add(newSkillData);
        Debug.Log("获取到新技能:" + newSkillData.skill_Name + restWholeSkillDatas.Count);
        current_restSkillNum = restWholeSkillDatas.Count;
    }

    /// <summary>
    /// 更新玩家可配置列表（每当玩家完成自定义配置技能后刷新记录）
    /// </summary>
    /// <param name="_NormalSkillDatas"></param>
    /// <param name="_ATBSkillDatas"></param>
    public void UpdateActableDataList(
        List<SkillData> _RestWholeSkillDatas,
        List<SkillData> _NormalSkillDatas,
        List<SkillData> _ATBSkillDatas)
    {
        restWholeSkillDatas = _RestWholeSkillDatas;
        normalSkillDatas = _NormalSkillDatas;
        atbSkillDatas = _ATBSkillDatas;

        current_restSkillNum = restWholeSkillDatas.Count;
        current_normalSkillNum = normalSkillDatas.Count;
        current_atbSkillNum = atbSkillDatas.Count;
        SaveSkillAssignments();
    }

    /// <summary>
    /// 持久化当前技能分配（仅存ID）
    /// </summary>
    void SaveSkillAssignments()
    {
        if (characterDataTag == null) return;
        var saveData = new Save_CharacterSkillData(normalSkillDatas, atbSkillDatas, restWholeSkillDatas);
        JsonSaver.Save(saveData, characterDataTag.CharacterData.characterType.ToString());
    }

    /// <summary>
    /// 从存档重建技能分配
    /// </summary>
    void LoadSkillAssignments()
    {
        if (characterDataTag == null) return;
        var saveData = JsonSaver.Load<Save_CharacterSkillData>(characterDataTag.CharacterData.characterType.ToString());
        if (saveData != null && saveData.IsValid())
        {
            normalSkillDatas = RebuildSkillListFromIDs(saveData.normalSkillIDs);
            atbSkillDatas = RebuildSkillListFromIDs(saveData.atbSkillIDs);
            if (saveData.restWholeSkillIDs != null && saveData.restWholeSkillIDs.Count > 0 && restWholeSkillDatas.Count == 0)
                restWholeSkillDatas = RebuildSkillListFromIDs(saveData.restWholeSkillIDs);
            Debug.Log($"[CharacterMapSkiller] 已加载技能分配存档: Normal={normalSkillDatas.Count}, ATB={atbSkillDatas.Count}");
        }
    }

    /// <summary>
    /// 从技能ID列表重建SkillData列表
    /// </summary>
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
                Debug.LogWarning($"[CharacterMapSkiller] 无法根据ID={id}重建技能数据");
        }
        return list;
    }
}

/// <summary>
/// 角色技能分配存档DTO —— 只存技能ID
/// </summary>
[System.Serializable]
public class Save_CharacterSkillData : IValidatable
{
    public Save_CharacterSkillData() { }

    public Save_CharacterSkillData(List<SkillData> normalDatas, List<SkillData> atbDatas, List<SkillData> restDatas)
    {
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

    public bool IsValid() => true;
}
