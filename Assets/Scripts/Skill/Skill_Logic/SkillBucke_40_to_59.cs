using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBucke_40_to_59{ }
/***
（40）  消耗所有法力值，对全体敌人造成魔法伤害，每消耗100法力值增幅2%伤害
（41）  对全体敌人造成魔法伤害，每造成过1次不同弱点种类的攻击伤害，伤害增幅10%
（42）  获得【魔力收束】状态（所有以全体敌人为目标的魔法攻击技能会变为以敌方单体作为目标且增伤100%），持续20S
（43）  清除1名敌人的所有【灼伤/冻结/感电】层数，对其造成火/冰/雷弱点伤害，每清除1层增幅5%伤害（方向键切换清除类型及对应弱点类型伤害）
（44）  将自身生命值降低至1点，对全体敌人造成大量魔法伤害，每消耗100生命值增幅5%伤害
（45）  获得【火焰风暴】状态（每10S对1名随机敌人造成0.4倍率火弱点伤害），持续30S
（46）  获得【冰雪风暴】状态（每10S对1名随机敌人造成0.4倍率冰弱点伤害），持续30S
（47）  获得【无双】状态（所有物理攻击技能会以敌方全体作为目标），持续20S
（48）  对1名敌人造成超大量剑弱点伤害
（49）  造成中量枪弱点伤害，此次攻击25%概率暴击
（50）  对所有敌人造成50%最大生命值物理伤害（超高耗）
（51）  若之前没有释放过其他物理攻击技能，每次释放后基础倍率提升0.2，对1名敌人造成少量枪弱点伤害
（52）  对1名敌人造成中量弓伤害，其ATB为0时，伤害*3
（53）  对1名敌人造成大量物理伤害，施加【晕眩】状态（无法释放技能）持续 10S，每次战斗只可释放1次
（54）  对1名敌人造成大量物理伤害，如果击杀敌人则获得3ATB点数
（55）  获得【灼伤之剑】状态（剑弱点攻击技能会附带1层灼伤），持续20S
（56）  获得【冻结之弓】状态（弓弱点攻击技能会附带1层冻结），持续20S
（57）  获得【感电之枪】状态（枪弱点攻击技能会附带1层感电），持续20S
（58）  获得【火焰场地】状态（每4秒使敌方全体获得1层灼伤），持续40S（中耗）
（59）  获得【雷电场地】状态（每4秒使敌方全体获得1层感电），持续40S（中耗）
***/


/// <summary>
/// （20) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(40)]
public class Skill_40 : SkillBase
{
    public Skill_40(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能40--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}

/// <summary>
/// （21) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(41)]
public class Skill_41 : SkillBase
{
    public Skill_41(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能41--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （12) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(42)]
public class Skill_42 : SkillBase
{
    public Skill_42(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能42--$$$$$");
    }



    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （13) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(43)]
public class Skill_43 : SkillBase
{
    public Skill_43(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能43--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （24) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(44)]
public class Skill_44 : SkillBase
{
    public Skill_44(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能44--$$$$$");
    }



    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （25) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(45)]
public class Skill_45 : SkillBase
{
    public Skill_45(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能45--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （26) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(46)]
public class Skill_46 : SkillBase
{
    public Skill_46(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能46--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （17) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(47)]
public class Skill_47 : SkillBase
{
    public Skill_47(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能47--$$$$$");
    }



    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （18) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(48)]
public class Skill_48 : SkillBase
{
    public Skill_48(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能48--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （15) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(49)]
public class Skill_49 : SkillBase
{
    public Skill_49(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能49--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}



/// <summary>
/// （10) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(50)]
public class Skill_50 : SkillBase
{
    public Skill_50(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能50--$$$$$");
    }



    public override void SkillEffect_Base(IBattlable target)
    {

    }
}

/// <summary>
/// （11) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(51)]
public class Skill_51 : SkillBase
{
    public Skill_51(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能51--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （12) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(52)]
public class Skill_52 : SkillBase
{
    public Skill_52(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能52--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （13) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(53)]
public class Skill_53 : SkillBase
{
    public Skill_53(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能53--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （14) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(54)]
public class Skill_54 : SkillBase
{
    public Skill_54(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能54--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （15) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(55)]
public class Skill_55 : SkillBase
{
    public Skill_55(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能55--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （16) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(56)]
public class Skill_56 : SkillBase
{
    public Skill_56(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能56--$$$$$");
    }


    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （17) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(57)]
public class Skill_57 : SkillBase
{
    public Skill_57(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能57--$$$$$");
    }

 
    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （18) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(58)]
public class Skill_58 : SkillBase
{
    public Skill_58(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能58--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
/// <summary>
/// （15) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(59)]
public class Skill_59 : SkillBase
{
    public Skill_59(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能59--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {

    }
}
