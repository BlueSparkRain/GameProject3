using Core;
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

    //角色尚未分配的所有技能数据
    public int restSkillSlotNum=20;
    private List<SkillData> restWholeSkillDatas = new List<SkillData>();
    
    //底部基础技能数据
    public int normalSkillSlotNum=5;
    private List<SkillData> normalSkillDatas = new List<SkillData>();

    //右侧ATB技能数据
    public int atbSkillSlotNum=5;
    private List<SkillData> atbSkillDatas = new List<SkillData>();

    int current_restSkillNum=0;
    int current_normalSkillNum=0;
    int current_atbSkillNum=0;

    public List<SkillData> RestWholeSkillDatas=>restWholeSkillDatas;//属性，不产生内存
    public List<SkillData> NormalSkillDatas => normalSkillDatas;
    public List<SkillData> ATBSkillDatas => atbSkillDatas;

    [Header("是否可以调整技能分配")]
    public bool canActSettle=false;

    private void Start()
    {
        EventCenter.EventTrigger(E_EventType.Character_Skiller_Regist,this,GetComponent<CharacterDataTag>().isPlayer);
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
    }
}
