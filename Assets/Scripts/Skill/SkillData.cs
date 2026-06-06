using UnityEngine;
/// <summary>
/// 后期可本地保存为json文件
/// </summary>
public class SkillData
{
    //记录一份技能当前的基础属性，MapIcon可以通过读取一份skillData来加载信息
    [Header("技能ID")]
    public int skill_ID;
    [Header("[当前]技能图标")]
    public Sprite skill_Sprite;
    [Header("技能名称")]
    public string skill_Name;
    [Header("技能描述")]
    [Multiline]
    public string skill_Description;
    [Header("[当前]技能冷却")]
    public float skill_CoolDown;
    [Header("[当前]技能法力消耗")]
    public float skill_sp_cost;
    [Header("[当前]技能ATB消耗")]
    public int skill_atb_cost;
    [Header("[当前]技能怒气增长")]
    public float skill_ang_grow;
    [Header("[当前]技能的目标类型")]
    public E_SkillTargetType skill_targetType;

    public SkillData(SkillPropertySO sodata) { 
        skill_ID=sodata.skill_ID;
        skill_Sprite=sodata.skill_Sprite;
        skill_Name=sodata.skill_Name;
        skill_Description=sodata.skill_Description;
        skill_CoolDown=sodata.skill_CoolDown_origin;
        skill_sp_cost=sodata.skill_sp_cost;
        skill_atb_cost=sodata.skill_atb_cost;
        skill_ang_grow=sodata.skill_ang_grow;
        skill_targetType=sodata.skill_targetType;
    }
}
