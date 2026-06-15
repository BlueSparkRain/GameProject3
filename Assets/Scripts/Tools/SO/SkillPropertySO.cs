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
    [Header("技能名字")]
    public string skill_Name;

    [Header("技能描述")]
    [Multiline]
    public string skill_Description;

    [Header("技能初始冷却-自动模式")]
    public float skill_CoolDown_Auto;

    [Header("技能法力消耗")]
    public float skill_sp_cost;

    [Header("技能ATB消耗-主动模式")]
    public int skill_AtbCost_ATB;

    [Header("技能怒气成长")]
    public float skill_ang_grow;

    [Header("施法目标类型-自动模态")]
    public E_SkillTargetType_Auto skill_targetType_Auto;
    [Header("施法目标类型-主动模态")]
    public E_SkillTargetType_ATB skill_targetType_ATB;
    [Header("Skill Delivery Type")]
    public SkillDeliveryType skill_DeliveryType;
}

public enum E_SkillTargetType_Auto
{
    对单体,
    对N目标,
    对全体,
}

/// <summary>
/// ATB主动技能专用目标类型 — 仅"敌方单体"需要玩家手动箭头选择目标。
/// </summary>
public enum E_SkillTargetType_ATB
{
    敌方单体,
    自身,
    敌方全体,
    随机敌方单体,
    自身加敌方单体,
}

//阵营枚举（自动敌对/友善）
public enum E_Camp
{
    玩家方,  // 玩家、盟友
    敌方     // 敌人、怪物
}

//战斗单位接口(所有角色/怪物通用)
// 技能不关心具体角色，只依赖于该接口 + 被动属性
public interface IBattlable
{
     E_Camp Camp { get; }
    bool IsAlive { get; }
    public BattleDamageHandler battleDamageHandler { get; set; }

    /// <summary>
    /// 当前弱点列表（支持多个弱点，战斗中可增删）
    /// </summary>
    List<E_WeaknessType> weaknesses { get; }

    bool GetWeakAttack(E_WeaknessType attackWeakType);

    void AddWeakness(E_WeaknessType w);
    void RemoveWeakness(E_WeaknessType w);

    /// <summary>
    /// 弱点列表发生变更时触发（增/删），用于驱动UI同步
    /// </summary>
    System.Action OnWeaknessChanged { get; set; }
}

//public enum E_SkillName {
//斩_单_物理,
//魔法复原                          ,
//会心一击                          ,
//暗火要害                          ,
//精神污染                          ,
//雷暴电暴                          ,
//冰霜新星                          ,
//百炼百打                          ,
//魔力神效                          ,
//精神统一                          ,
//背水一战                          ,
//万魔射击                          ,
//火炎_雷暴_风刃魔法         ,
//魔法神效                          ,
//精神统一                          ,
//力量融合                          ,
//生命之力                          ,
//魔法融合                          ,
//精神破坏                          ,
//防御破坏                          ,
//夺取                              ,
//麻痹                              ,
//希望之光                          ,
//希望之光                          ,
//力量封印                          ,
//病毒飞沫                          ,
//病毒飞沫                          ,
//灾厄                              ,
//冰晶碎片                          ,
//元素回响                          ,
//魔法反射                          ,
//识破                              ,
//狮子的咆哮                          ,
//狂怒之拳                          ,
//无尽战意                          ,
//必胜壮志                          ,
//炽热拳击                          ,
//迅捷连打                          ,
//冰雪加护                          ,
//暗影魔法弹                        ,
//神圣魔法                          ,
//辉石                              ,
//魔法逆转                          ,
//冰霜_霜刃_横斩斜扫         ,
//舍身                              ,
//冰龙风暴                          ,
//冰雪风暴                          ,
//吉欧达因双手剑                      ,
//吉欧达因双手剑                          ,
//力量之枪                          ,
//辉石                              ,
//大爆炸                            ,
//暗影之石                          ,
//暗火                              ,
//再生                              ,
//冰结魔法                          ,
//霜冻魔法                          ,
//穿枪魔法                          ,
//精神污染                          ,
//雷电风暴                          ,

//}
