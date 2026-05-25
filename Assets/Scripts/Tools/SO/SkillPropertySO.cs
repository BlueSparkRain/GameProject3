using UnityEngine;
using UnityEngine.UIElements.Experimental;

[CreateAssetMenu(menuName = "SOData/SkillPropertyData", fileName = "SkillPropertyData")]
public class SkillPropertySO : ScriptableObject
{
    [Header("技能ID")]
    public int skill_ID;

    [Header("技能图标")]
    public Sprite skill_Sprite;
    [Header("技能名称")]
    public string skill_Name;

    [Header("技能描述")]
    [Multiline]
    public string skill_Description;

    [Header("技能初始冷却")]
    public float skill_CoolDown_origin;

    [Header("技能法力消耗")]
    public float skill_sp_cost;

    [Header("技能怒气增长")]
    public float skill_ang_grow;

    [Header("技能的目标类型")]
    public E_SkillTargetType skill_targetType;
}
public enum E_SkillTargetType
{
    对单体,
    对N目标,
    对全体,
}

//阵营枚举（自动区分敌我）
public enum E_Camp
{
    玩家方,  // 玩家、友军
    敌方     // 敌人、怪物
}

//战斗单位接口(所有角色/敌人通用)
// 技能不依赖具体角色，只依赖这个接口 → 极致解耦
public interface IBattlable
{
     E_Camp Camp { get; }             // 阵营
    bool IsAlive { get; }            // 是否存活
    public BattleDamageHandler battleDamageHandler { get; set; }

    E_WeaknessType selfWeakness {  get; set; }

    bool GetWeakAttack(E_WeaknessType attackWeakType);
}