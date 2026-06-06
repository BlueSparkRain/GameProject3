using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBucket_40_to_59 { }

/***
40  神圣魔法    消耗全部法力值，对全体敌人造成魔法伤害，每消耗100法力值，伤害倍率+2%
41  过曝       对全体敌人造成魔法伤害，每在战斗中成功造成过1种不同弱点的攻击伤害，伤害倍率+10%
42  魔力逆转    获得【魔力收束】状态：将全体魔法技能变为单体且伤害+100%，持续20S
43  灼热爆发/霜冻爆发/电感爆发  清除1名敌人身上所有【燃烧/冻结/感电】层数，并根据清除层数造成对应的火/冰/雷伤害，每层+5%（方向键切换dot类型）
44  彗星       消耗全部生命值至1点，对全体敌人造成魔法伤害，每消耗100生命值，伤害倍率+5%
45  火焰风暴    获得【火焰风暴】状态：每5S对1名随机敌人造成0.4倍率火属性伤害，持续15S
46  冰雪风暴    获得【冰雪风暴】状态：每5S对1名随机敌人造成0.4倍率冰属性伤害，持续15S
47  天下无双架势 获得【无双】状态：物理攻击技能将目标变更为敌方全体，持续20S
48  武神霸斩   对1名敌人造成巨量剑弱点伤害
49  会心之枪   对1名敌人造成枪弱点伤害，25%概率造成2倍伤害
50  陨石       对全体敌人造成50%最大生命值物理伤害（高耗）
51  绵里藏针   对1名敌人造成枪弱点伤害，每次释放后此技能倍率+0.2（可叠加）
52  落井下石   对1名敌人造成弓弱点伤害，若目标ATB为0则伤害*3
53  猛击       对1名敌人造成大量物理伤害，附加【晕眩】状态（无法释放技能）持续10S，每场战斗只能使用1次
54  吞噬       对1名敌人造成大量物理伤害，若击杀目标则获得3点ATB
55  炎剑附魔   获得【灼伤之剑】状态：剑弱点攻击附加1层燃烧，持续10S
56  霜弓附魔   获得【冻结之弓】状态：弓弱点攻击附加1层冻结，持续10S
57  雷枪附魔   获得【感电之枪】状态：枪弱点攻击附加1层感电，持续10S
58  火焰领域   获得【火焰场地】状态：每秒为敌方全体附加1层燃烧，持续10S
59  雷电领域   获得【雷电场地】状态：每秒为敌方全体附加1层感电，持续10S
***/


/// <summary>
/// 40) 神圣魔法：消耗全部法力值，对全体敌人造成魔法伤害，每消耗100法力值+2%倍率
/// </summary>
[SkillID(40)]
public class Skill_40 : SkillBase
{
    float baseRate = 0.3f;
    float ratePer100SP = 0.02f;

    public Skill_40(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        float currentSP = Controller.GetCharacterModelValue(E_BattleModelType.SP);
        Controller.AdjustCharacterModelValue(E_BattleModelType.SP, -currentSP);
        float bonusRate = (currentSP / 100f) * ratePer100SP;
        float rate = baseRate + bonusRate;
        Debug.Log($"[Skill 40]{self.Camp}消耗全部SP({currentSP})，光伤害倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.光, -1, rate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 41) 过曝：对全体敌人造成魔法伤害，每1种不同弱点+10%倍率
/// </summary>
[SkillID(41)]
public class Skill_41 : SkillBase
{
    static HashSet<E_WeaknessType> recordedWeaknesses = new HashSet<E_WeaknessType>();
    static int totalUniqueCount = 0;
    float baseRate = 0.3f;
    float ratePerWeakness = 0.1f;

    public Skill_41(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public static void RecordWeakness(E_WeaknessType w)
    {
        if (w != E_WeaknessType.无 && recordedWeaknesses.Add(w))
        {
            totalUniqueCount++;
            Debug.Log($"[Skill_41]记录新弱点类型:{w},当前累计{totalUniqueCount}种");
        }
    }

    public static int GetUniqueWeaknessCount() => totalUniqueCount;

    public static void ResetWeaknessRecord()
    {
        recordedWeaknesses.Clear();
        totalUniqueCount = 0;
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        int uniqueCount = GetUniqueWeaknessCount();
        float rate = baseRate + uniqueCount * ratePerWeakness;
        Debug.Log($"[Skill 41]{self.Camp}已使用{uniqueCount}种弱点，光伤害倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.光, -1, rate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 42) 魔力逆转：获得【魔力收束】状态（全体魔法→单体，伤害+100%），持续20S
/// </summary>
[SkillID(42)]
public class Skill_42 : SkillBase
{
    float buffDuration = 20f;

    public Skill_42(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 42]{self.Camp}获得魔力收束BUFF，持续{buffDuration}S");
        var buff = new BuffBase(E_BuffType.魔力收束_正面, E_BuffPositive.正面, buffDuration);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 43) 灼热/霜冻/电感爆发：清除目标身上所有指定Dot层数，根据层数造成对应元素伤害，每层+5%
/// </summary>
[SkillID(43)]
public class Skill_43 : SkillBase
{
    float baseRate = 0.3f;
    float ratePerLayer = 0.05f;
    E_Dot selectedDot = E_Dot.燃烧;

    public Skill_43(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public void SwitchDotType(E_Dot dot) { selectedDot = dot; }

    E_WeaknessType GetWeaknessForDot(E_Dot dot)
    {
        switch (dot)
        {
            case E_Dot.燃烧: return E_WeaknessType.火;
            case E_Dot.冻结: return E_WeaknessType.冰;
            case E_Dot.感电: return E_WeaknessType.雷;
        }
        return E_WeaknessType.火;
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        var dotHandler = target.battleDamageHandler.DotHandler;
        int layers = dotHandler.ClearDotAndGetLayers(selectedDot);
        float rate = baseRate + layers * ratePerLayer;
        E_WeaknessType weakness = GetWeaknessForDot(selectedDot);
        Debug.Log($"[Skill 43]{self.Camp}清除目标{selectedDot}Dot{layers}层，{weakness}伤害倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(weakness, -1, rate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 44) 彗星：消耗全部生命至1点，对全体敌人造成魔法伤害，每消耗100HP+5%倍率
/// </summary>
[SkillID(44)]
public class Skill_44 : SkillBase
{
    float baseRate = 0.3f;
    float ratePer100HP = 0.05f;

    public Skill_44(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        float currentHP = Controller.GetCharacterModelValue(E_BattleModelType.HP);
        float hpConsumed = currentHP - 1;
        Controller.AdjustCharacterModelValue(E_BattleModelType.HP, -hpConsumed);
        float bonusRate = (hpConsumed / 100f) * ratePer100HP;
        float rate = baseRate + bonusRate;
        Debug.Log($"[Skill 44]{self.Camp}消耗HP({hpConsumed})至1点，光伤害倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.光, -1, rate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 45) 火焰风暴：每5S对1随机敌人造成0.4倍率火属性伤害，持续15S
/// </summary>
[SkillID(45)]
public class Skill_45 : SkillBase
{
    float buffDuration = 15f;
    float damageRate = 0.4f;
    float triggerInterval = 5f;

    public Skill_45(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        E_Camp enemyCamp = self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方;
        Debug.Log($"[Skill 45]{self.Camp}获得火焰风暴BUFF，持续{buffDuration}S");
        var buff = new Buff_AutoDamage(E_BuffType.烈焰风暴_正面, E_BuffPositive.正面, buffDuration,
            self, enemyCamp, E_WeaknessType.火, damageRate, triggerInterval);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 46) 冰雪风暴：每5S对1随机敌人造成0.4倍率冰属性伤害，持续15S
/// </summary>
[SkillID(46)]
public class Skill_46 : SkillBase
{
    float buffDuration = 15f;
    float damageRate = 0.4f;
    float triggerInterval = 5f;

    public Skill_46(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        E_Camp enemyCamp = self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方;
        Debug.Log($"[Skill 46]{self.Camp}获得冰雪风暴BUFF，持续{buffDuration}S");
        var buff = new Buff_AutoDamage(E_BuffType.冰雪风暴_正面, E_BuffPositive.正面, buffDuration,
            self, enemyCamp, E_WeaknessType.冰, damageRate, triggerInterval);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 47) 天下无双架势：获得【无双】状态（物理技能目标变全体），持续20S
/// </summary>
[SkillID(47)]
public class Skill_47 : SkillBase
{
    float buffDuration = 20f;

    public Skill_47(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 47]{self.Camp}获得无双BUFF，持续{buffDuration}S");
        var buff = new BuffBase(E_BuffType.无双_正面, E_BuffPositive.正面, buffDuration);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 48) 武神霸斩：对1名敌人造成巨量剑弱点伤害
/// </summary>
[SkillID(48)]
public class Skill_48 : SkillBase
{
    float baseAttackRate = 0.8f;

    public Skill_48(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 48]{self.Camp}发动武神霸斩，剑伤害倍率{baseAttackRate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.剑, -1, baseAttackRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 49) 会心之枪：对1名敌人造成枪弱点伤害，25%概率造成2倍伤害
/// </summary>
[SkillID(49)]
public class Skill_49 : SkillBase
{
    float baseAttackRate = 0.5f;
    float critChance = 0.25f;
    float critMulti = 2f;

    public Skill_49(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        bool isCrit = Random.value < critChance;
        float rate = baseAttackRate * (isCrit ? critMulti : 1f);
        Debug.Log($"[Skill 49]{self.Camp}发动会心之枪，暴击:{isCrit}，枪伤害倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.枪, -1, rate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 50) 陨石：对全体敌人造成50%最大生命值的物理伤害（高耗）
/// </summary>
[SkillID(50)]
public class Skill_50 : SkillBase
{
    float hpRate = 0.5f;

    public Skill_50(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        float maxHP = self.battleDamageHandler.GetMaxHealth();
        float damageVal = maxHP * hpRate;
        Debug.Log($"[Skill 50]{self.Camp}发动陨石，最大HP{maxHP}，基础伤害值{damageVal}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.无, damageVal, 1f);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 51) 绵里藏针：对1名敌人造成枪弱点伤害，每次释放后此技能倍率+0.2（可叠加）
/// </summary>
[SkillID(51)]
public class Skill_51 : SkillBase
{
    float cumulativeRate = 0.3f;
    float rateIncrement = 0.2f;

    public Skill_51(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 51]{self.Camp}发动绵里藏针，当前倍率{cumulativeRate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.枪, -1, cumulativeRate);
        atk.Excute(self, target);
        cumulativeRate += rateIncrement;
        Debug.Log($"[Skill 51]倍率提升至{cumulativeRate}");
    }
}

/// <summary>
/// 52) 落井下石：对1名敌人造成弓弱点伤害，若目标ATB为0则伤害*3
/// </summary>
[SkillID(52)]
public class Skill_52 : SkillBase
{
    float baseAttackRate = 0.4f;
    float atbZeroMulti = 3f;

    public Skill_52(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        float targetATB = target.battleDamageHandler.BattleController.GetCharacterModelValue(E_BattleModelType.ATBPoints);
        bool atbZero = targetATB <= 0;
        float rate = baseAttackRate * (atbZero ? atbZeroMulti : 1f);
        Debug.Log($"[Skill 52]{self.Camp}发动落井下石，目标ATB:{targetATB}，ATB为0:{atbZero}，倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.弓, -1, rate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 53) 猛击：对1名敌人造成大量物理伤害，附加【晕眩】状态持续10S，每场战斗只能使用1次
/// </summary>
[SkillID(53)]
public class Skill_53 : SkillBase
{
    bool usedThisBattle = false;
    float baseAttackRate = 0.6f;
    float stunDuration = 10f;

    public Skill_53(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        if (usedThisBattle)
        {
            Debug.Log("[Skill 53]本场战斗已使用过猛击，跳过");
            return;
        }
        usedThisBattle = true;
        Debug.Log($"[Skill 53]{self.Camp}发动猛击，附加晕眩{stunDuration}S");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.无, -1, baseAttackRate);
        atk.Excute(self, target);
        var stunBuff = new BuffBase(E_BuffType.晕眩_负面, E_BuffPositive.负面, stunDuration);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, target.battleDamageHandler.BuffHandler, stunBuff);
    }
}

/// <summary>
/// 54) 吞噬：对1名敌人造成大量物理伤害，若击杀目标则获得3点ATB
/// </summary>
[SkillID(54)]
public class Skill_54 : SkillBase
{
    float baseAttackRate = 0.6f;
    int atbGain = 3;

    public Skill_54(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target)
    {
        bool wasAlive = target.IsAlive;
        Debug.Log($"[Skill 54]{self.Camp}发动吞噬");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.无, -1, baseAttackRate);
        atk.Excute(self, target);
        if (wasAlive && !target.IsAlive)
        {
            Controller.AdjustCharacterModelValue(E_BattleModelType.ATBPoints, atbGain);
            Debug.Log($"[Skill 54]击杀目标，获得{atbGain}ATB");
        }
    }
}

/// <summary>
/// 55) 炎剑附魔：获得【灼伤之剑】状态（剑攻击附加燃烧），持续10S
/// </summary>
[SkillID(55)]
public class Skill_55 : SkillBase
{
    float buffDuration = 10f;

    public Skill_55(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 55]{self.Camp}获得灼伤之剑BUFF，持续{buffDuration}S");
        var buff = new Buff_DotOnAttack(E_BuffType.灼伤之剑_正面, E_BuffPositive.正面, buffDuration,
            E_WeaknessType.剑, E_Dot.燃烧, self);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 56) 霜弓附魔：获得【冻结之弓】状态（弓攻击附加冻结），持续10S
/// </summary>
[SkillID(56)]
public class Skill_56 : SkillBase
{
    float buffDuration = 10f;

    public Skill_56(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 56]{self.Camp}获得冻结之弓BUFF，持续{buffDuration}S");
        var buff = new Buff_DotOnAttack(E_BuffType.冻结之弓_正面, E_BuffPositive.正面, buffDuration,
            E_WeaknessType.弓, E_Dot.冻结, self);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 57) 雷枪附魔：获得【感电之枪】状态（枪攻击附加感电），持续10S
/// </summary>
[SkillID(57)]
public class Skill_57 : SkillBase
{
    float buffDuration = 10f;

    public Skill_57(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 57]{self.Camp}获得感电之枪BUFF，持续{buffDuration}S");
        var buff = new Buff_DotOnAttack(E_BuffType.感电之枪_正面, E_BuffPositive.正面, buffDuration,
            E_WeaknessType.枪, E_Dot.感电, self);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 58) 火焰领域：每秒为敌方全体附加1层燃烧，持续10S
/// </summary>
[SkillID(58)]
public class Skill_58 : SkillBase
{
    float buffDuration = 10f;
    float tickInterval = 1f;
    int dotLayers = 1;

    public Skill_58(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 58]{self.Camp}获得火焰场地BUFF，持续{buffDuration}S");
        var buff = new Buff_FieldDot(E_BuffType.火焰场地_正面, E_BuffPositive.正面, buffDuration,
            E_Dot.燃烧, self, tickInterval, dotLayers);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
/// 59) 雷电领域：每秒为敌方全体附加1层感电，持续10S
/// </summary>
[SkillID(59)]
public class Skill_59 : SkillBase
{
    float buffDuration = 10f;
    float tickInterval = 1f;
    int dotLayers = 1;

    public Skill_59(E_SkillTargetType _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 59]{self.Camp}获得雷电场地BUFF，持续{buffDuration}S");
        var buff = new Buff_FieldDot(E_BuffType.雷电场地, E_BuffPositive.正面, buffDuration,
            E_Dot.感电, self, tickInterval, dotLayers);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}
