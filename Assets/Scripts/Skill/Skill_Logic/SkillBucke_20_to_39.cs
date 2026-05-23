using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBucke_20_to_39 { }

/***
（20）获得【吸血+1】（生命偷取+15%），持续20S
（21）获得【治疗+1】（治疗加成+30%），持续20S
（22）清除自身所有属性下降效果
（23）延长自身正面状态持续时长10S
（24）复制1名敌人的正面效果，并将自身的负面效果复制给这名敌人
（25）清除自身所有正面效果，然后对1敌人造成剑弱点伤害，每清除1种，伤害倍率+0.1
（26）清除1名敌人所有负面效果，然后并对其造成枪弱点伤害，每清除1种，伤害倍率+0.1
（27）获得随机 1 种持续5S的属性类型增益，重放X（=携带的攻击技能弱点种类数量）
（28）获得【退化】状态（造成伤害减少30%）持续20S，回复25%最大生命值
（29）对随机敌人造成3次随机类型（冰、火、雷）的少量伤害，重放0
（30）对全体敌人造成剑、枪、弓的中量伤害各2次
（31）使对方随机获得1未拥有的物理弱点（中耗）
（32）对1名敌人造成2次大量枪弱点伤害，该敌人每具有1个弱点，该技能伤害增加20%
（33）对全体敌人造成大量弓弱点伤害，重放2，如果这次攻击击杀了敌人或者使敌人进入力竭状态，重放次数+1（可叠加）
（34）造成大量剑弱点伤害，对力竭敌人造成2倍伤害
（35）延长1名敌人击破状态5S，并且使其使其获得【易损】状态（力竭时受到伤害增加35%）持续5S
（36）对敌方全体造成大量剑/枪/弓弱点伤害1次，这个技能只能释放1次（方向键切换弱点类型）
（37）获得【迅雷连锁】状态（物理攻击技能附带雷弱点攻击伤害）持续20S
（38）获得【暴雪连锁】状态（物理攻击技能附带冰弱点攻击伤害）持续20S
（39）获得【超大魔法化】状态（魔法类型攻击技能将会额外释放2次），持续30S
***/



/// <summary>
///（20）获得【吸血+1】（生命偷取+15%），持续20S
/// </summary>
[SkillID(20)]
public class Skill_20 : SkillBase
{
    public Skill_20(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能20--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target)
    {

    }
}

/// <summary>
///（21）获得【治疗+1】（治疗加成+30%），持续20S
/// </summary>
[SkillID(21)]
public class Skill_21 : SkillBase
{
    public Skill_21(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能21--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （22）清除自身所有属性下降效果
/// </summary>
[SkillID(22)]
public class Skill_22 : SkillBase
{
    public Skill_22(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能22--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （23）延长自身正面状态持续时长10S
/// </summary>
[SkillID(23)]
public class Skill_23 : SkillBase
{
    public Skill_23(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能23--$$$$$");
    }

    

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （24）复制1名敌人的正面效果，并将自身的负面效果复制给这名敌人
[SkillID(24)]
public class Skill_24 : SkillBase
{
    public Skill_24(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能24--$$$$$");
    }

 
    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （25）清除自身所有正面效果，然后对1敌人造成剑弱点伤害，每清除1种，伤害倍率+0.1
/// </summary>
[SkillID(25)]
public class Skill_25 : SkillBase
{
    public Skill_25(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能25--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （26）清除1名敌人所有负面效果，然后并对其造成枪弱点伤害，每清除1种，伤害倍率+0.1
/// </summary>
[SkillID(26)]
public class Skill_26 : SkillBase
{
    public Skill_26(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能26--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （27）获得随机 1 种持续5S的属性类型增益，重放X（=携带的攻击技能弱点种类数量）
/// </summary>
[SkillID(27)]
public class Skill_27 : SkillBase
{
    public Skill_27(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能27--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （28）获得【退化】状态（造成伤害减少30%）持续20S，回复25%最大生命值
/// </summary>
[SkillID(28)]
public class Skill_28 : SkillBase
{
    public Skill_28(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能28--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （29）对随机敌人造成3次随机类型（冰、火、雷）的少量伤害，重放0
/// </summary>
[SkillID(29)]
public class Skill_29 : SkillBase
{
    public Skill_29(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能29--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target)
    {

    }
}



/// <summary>
/// (30）对全体敌人造成剑、枪、弓的中量伤害各2次
/// </summary>
[SkillID(30)]
public class Skill_30 : SkillBase
{
    public Skill_30(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能30--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}

/// <summary>
///（31）使对方随机获得1未拥有的物理弱点（中耗）
/// </summary>
[SkillID(31)]
public class Skill_31 : SkillBase
{
    public Skill_31(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能31--$$$$$");
    }



    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
///（32）对1名敌人造成2次大量枪弱点伤害，该敌人每具有1个弱点，该技能伤害增加20%
/// </summary>
[SkillID(32)]
public class Skill_32 : SkillBase
{
    public Skill_32(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能32--$$$$$");
    }



    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （33）对全体敌人造成大量弓弱点伤害，重放2，如果这次攻击击杀了敌人或者使敌人进入力竭状态，重放次数+1（可叠加）
/// </summary>
[SkillID(33)]
public class Skill_33 : SkillBase
{
    public Skill_33(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能33--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// (34）造成大量剑弱点伤害，对力竭敌人造成2倍伤害
/// </summary>
[SkillID(34)]
public class Skill_34 : SkillBase
{
    public Skill_34(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能34--$$$$$");
    }



    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （35）延长1名敌人击破状态5S，并且使其使其获得【易损】状态（力竭时受到伤害增加35%）持续5S
/// </summary>
[SkillID(35)]
public class Skill_35 : SkillBase
{
    public Skill_35(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能35--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （36）对敌方全体造成大量剑/枪/弓弱点伤害1次，这个技能只能释放1次（方向键切换弱点类型）
/// </summary>
[SkillID(36)]
public class Skill_36 : SkillBase
{
    public Skill_36(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能36--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （37）获得【迅雷连锁】状态（物理攻击技能附带雷弱点攻击伤害）持续20S
/// </summary>
[SkillID(37)]
public class Skill_37 : SkillBase{
    public Skill_37(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能37--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （38）获得【暴雪连锁】状态（物理攻击技能附带冰弱点攻击伤害）持续20S
/// </summary>
[SkillID(38)]
public class Skill_38 : SkillBase
{
    public Skill_38(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能38--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （39）获得【超大魔法化】状态（魔法类型攻击技能将会额外释放2次），持续30S
/// </summary>
[SkillID(39)]
public class Skill_39 : SkillBase
{
    public Skill_39(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能39--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
