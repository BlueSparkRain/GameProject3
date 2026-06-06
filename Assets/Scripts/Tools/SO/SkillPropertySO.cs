using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[CreateAssetMenu(menuName = "SOData/SkillPropertyData", fileName = "SkillPropertyData")]
public class SkillPropertySO : ScriptableObject
{
    [Header("技能ID")]
    public int skill_ID;

    [Header("技能名称")]
    public E_SkillName skill_name;

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

    [Header("技能ATB消耗")]
    public int skill_atb_cost;

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
     E_Camp Camp { get; }
    bool IsAlive { get; }
    public BattleDamageHandler battleDamageHandler { get; set; }

    /// <summary>
    /// 当前弱点列表（支持多弱点，战斗中可增删）
    /// </summary>
    List<E_WeaknessType> weaknesses { get; }

    bool GetWeakAttack(E_WeaknessType attackWeakType);

    void AddWeakness(E_WeaknessType w);
    void RemoveWeakness(E_WeaknessType w);
}

//public enum E_SkillName {
//斩_刺_射击,
//魔力复原                          ,
//灵光一闪                          ,
//猛击要害                          ,
//炽焰连锁                          ,
//雷电风暴                          ,
//冰霜领域                          ,
//海纳百川                          ,
//力量增效                          ,
//气沉丹田                          ,
//背水一战                          ,
//大魔法化                          ,
//寒冰_雷电_火焰魔法         ,
//魔法增效                          ,
//坚铁防壁                          ,
//秘银结界                          ,
//力量弱化                          ,
//魔法弱化                          ,
//防壁破坏                          ,
//结界破坏                          ,
//汲取                              ,
//再生                              ,
//净化之仪                          ,
//希望之歌                          ,
//镜像反射                          ,
//无心长刀                          ,
//戒心长枪                          ,
//折射                              ,
//断尾求生                          ,
//属性混乱                          ,
//三器缭乱                          ,
//识破                              ,
//狮王狩猎                          ,
//倾盆大雨                          ,
//无尽终结                          ,
//乘胜追击                          ,
//先发制人                          ,
//迅雷连锁                          ,
//暴雪连锁                          ,
//超大魔法化                        ,
//神圣魔法                          ,
//过曝                              ,
//魔力逆转                          ,
//灼热_霜冻_电感爆发         ,
//彗星                              ,
//火焰风暴                          ,
//冰雪风暴                          ,
//天下无双架势                      ,
//武神霸斩                          ,
//会心之枪                          ,
//陨石                              ,
//绵里藏针                          ,
//落井下石                          ,
//猛击                              ,
//吞噬                              ,
//炎剑附魔                          ,
//霜弓附魔                          ,
//雷枪附魔                          ,
//火焰领域                          ,
//雷电领域                          ,

//}