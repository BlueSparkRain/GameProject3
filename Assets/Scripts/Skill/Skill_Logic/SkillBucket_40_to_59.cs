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
    public Skill_40(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, 1); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, 1 + henceTime); } // 1→2→3→4
    void Execute(IBattlable target, int hits)
    {
        float currentSP = Controller.GetCharacterModelValue(E_BattleModelType.SP);
        Controller.AdjustCharacterModelValue(E_BattleModelType.SP, -currentSP);
        float bonusRate = (currentSP / 100f) * ratePer100SP;
        float rate = baseRate + bonusRate;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 40]{self.Camp}消耗全部SP({currentSP})，光伤害倍率{rate}，段数{hits}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.光_, -1, rate);
        var multi = new MultiTime_SkillDecorator(atk, hits, 0.2f);
        multi.Excute(self, target);
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

    public Skill_41(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public static void RecordWeakness(E_WeaknessType w)
    {
        if (w != E_WeaknessType.无_ && recordedWeaknesses.Add(w))
        {
            totalUniqueCount++;
            DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill_41]记录新弱点类型:{w},当前累计{totalUniqueCount}种");
        }
    }

    public static int GetUniqueWeaknessCount() => totalUniqueCount;

    public static void ResetWeaknessRecord()
    {
        recordedWeaknesses.Clear();
        totalUniqueCount = 0;
    }

    public override void SkillEffect_Base(IBattlable target) { Execute(target, ratePerWeakness); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, ratePerWeakness + henceTime * 0.1f); } // 10→20→30→40%
    void Execute(IBattlable target, float perWeakness)
    {
        int uniqueCount = GetUniqueWeaknessCount();
        float rate = baseRate + uniqueCount * perWeakness;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 41]{self.Camp}已使用{uniqueCount}种弱点，光伤害倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.光_, -1, rate);
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

    public Skill_42(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { ApplyBuff(buffDuration); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { ApplyBuff(buffDuration + henceTime * 10f); } // 20→30→40→50
    void ApplyBuff(float dur)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 42]{self.Camp}获得魔力收束BUFF，持续{dur}S");
        BuffBase buff = new BuffBase(E_BuffType.魔力收束_正面, E_BuffPositive.正面, dur);
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

    public Skill_43(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
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

    public override void SkillEffect_Base(IBattlable target) { Execute(target, ratePerLayer); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, ratePerLayer + henceTime * 0.05f); } // 5→10→15→20%
    void Execute(IBattlable target, float perLayer)
    {
        var dotHandler = target.battleDamageHandler.DotHandler;
        int layers = dotHandler.ClearDotAndGetLayers(selectedDot);
        float rate = baseRate + layers * perLayer;
        E_WeaknessType weakness = GetWeaknessForDot(selectedDot);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 43]{self.Camp}清除目标{selectedDot}Dot{layers}层，{weakness}伤害倍率{rate}");
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

    public Skill_44(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, ratePer100HP); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, ratePer100HP + henceTime * 0.05f); } // 5→10→15→20%
    void Execute(IBattlable target, float per100HP)
    {
        float currentHP = Controller.GetCharacterModelValue(E_BattleModelType.HP);
        float hpConsumed = currentHP - 1;
        Controller.AdjustCharacterModelValue(E_BattleModelType.HP, -hpConsumed);
        float bonusRate = (hpConsumed / 100f) * per100HP;
        float rate = baseRate + bonusRate;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 44]{self.Camp}消耗HP({hpConsumed})至1点，光伤害倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.光_, -1, rate);
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

    public Skill_45(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { CreateBuff(damageRate); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(damageRate + henceTime * 0.2f); } // 0.4→0.6→0.8→1.0
    void CreateBuff(float rate)
    {
        E_Camp enemyCamp = self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 45]{self.Camp}获得火焰风暴BUFF，持续{buffDuration}S，倍率{rate}");
        BuffBase buff = new Buff_AutoDamage(E_BuffType.烈焰风暴_正面, E_BuffPositive.正面, buffDuration,
            self, enemyCamp, E_WeaknessType.火, rate, triggerInterval);
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

    public Skill_46(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { CreateBuff(damageRate); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(damageRate + henceTime * 0.2f); } // 0.4→0.6→0.8→1.0
    void CreateBuff(float rate)
    {
        E_Camp enemyCamp = self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 46]{self.Camp}获得冰雪风暴BUFF，持续{buffDuration}S，倍率{rate}");
        BuffBase buff = new Buff_AutoDamage(E_BuffType.冰雪风暴_正面, E_BuffPositive.正面, buffDuration,
            self, enemyCamp, E_WeaknessType.冰, rate, triggerInterval);
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

    public Skill_47(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { ApplyBuff(buffDuration); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { ApplyBuff(buffDuration + henceTime * 10f); } // 20→30→40→50
    void ApplyBuff(float dur)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 47]{self.Camp}获得无双BUFF，持续{dur}S");
        BuffBase buff = new BuffBase(E_BuffType.无双_正面, E_BuffPositive.正面, dur);
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

    public Skill_48(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, 1); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, 1 + henceTime); } // 1→2→3→4
    void Execute(IBattlable target, int hits)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 48]{self.Camp}发动武神霸斩，剑伤害倍率{baseAttackRate}，段数{hits}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.剑, -1, baseAttackRate);
        var multi = new MultiTime_SkillDecorator(atk, hits, 0.3f);
        multi.Excute(self, target);
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

    public Skill_49(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, critChance); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, critChance + henceTime * 0.25f); } // 25→50→75→100%
    void Execute(IBattlable target, float chance)
    {
        bool isCrit = Random.value < chance;
        float rate = baseAttackRate * (isCrit ? critMulti : 1f);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 49]{self.Camp}发动会心之枪，暴击:{isCrit}(概率{chance})，枪伤害倍率{rate}");
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

    public Skill_50(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, hpRate); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, hpRate + henceTime * 0.25f); } // 50→75→100→125%
    void Execute(IBattlable target, float pct)
    {
        float maxHP = self.battleDamageHandler.GetMaxHealth();
        float damageVal = maxHP * pct;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 50]{self.Camp}发动陨石，最大HP{maxHP}，伤害值{damageVal}({pct*100}%)");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.无_, damageVal, 1f);
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

    public Skill_51(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, 1); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, 1 + henceTime); } // 1→2→3→4
    void Execute(IBattlable target, int hits)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 51]{self.Camp}发动绵里藏针，当前倍率{cumulativeRate}，段数{hits}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.枪, -1, cumulativeRate);
        var multi = new MultiTime_SkillDecorator(atk, hits, 0.2f);
        multi.Excute(self, target);
        cumulativeRate += rateIncrement;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 51]倍率提升至{cumulativeRate}");
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

    public Skill_52(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, baseAttackRate); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, baseAttackRate + henceTime * 0.2f); } // 0.4→0.6→0.8
    void Execute(IBattlable target, float atkRate)
    {
        float targetATB = target.battleDamageHandler.BattleController.GetCharacterModelValue(E_BattleModelType.ATBPoints);
        bool atbZero = targetATB <= 0;
        float rate = atkRate * (atbZero ? atbZeroMulti : 1f);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 52]{self.Camp}发动落井下石，目标ATB:{targetATB}，ATB为0:{atbZero}，倍率{rate}");
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

    public Skill_53(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, stunDuration); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, stunDuration + henceTime * 5f); } // 10→15→20→25
    void Execute(IBattlable target, float dur)
    {
        if (usedThisBattle) { DebugManager.Log(EDebugCategory.SkillExecution,"[Skill 53]本场战斗已使用过猛击，跳过"); return; }
        usedThisBattle = true;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 53]{self.Camp}发动猛击，附加晕眩{dur}S");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.无_, -1, baseAttackRate);
        atk.Excute(self, target);
        BuffBase stunBuff = new BuffBase(E_BuffType.晕眩_负面, E_BuffPositive.负面, dur);
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

    public Skill_54(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => false;

    public override void SkillEffect_Base(IBattlable target) { Execute(target, 1); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { Execute(target, 1 + henceTime); } // 1→2→3→4
    void Execute(IBattlable target, int hits)
    {
        bool wasAlive = target.IsAlive;
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 54]{self.Camp}发动吞噬，段数{hits}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.无_, -1, baseAttackRate);
        var multi = new MultiTime_SkillDecorator(atk, hits, 0.2f);
        multi.Excute(self, target);
        if (wasAlive && !target.IsAlive)
        {
            Controller.AdjustCharacterModelValue(E_BattleModelType.ATBPoints, atbGain);
            DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 54]击杀目标，获得{atbGain}ATB");
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

    public Skill_55(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { CreateBuff(buffDuration); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(buffDuration + henceTime * 5f); } // 10→15→20→25
    void CreateBuff(float dur)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 55]{self.Camp}获得灼伤之剑BUFF，持续{dur}S");
        BuffBase buff = new Buff_DotOnAttack(E_BuffType.灼伤之剑_正面, E_BuffPositive.正面, dur,
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

    public Skill_56(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { CreateBuff(buffDuration); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(buffDuration + henceTime * 5f); } // 10→15→20→25
    void CreateBuff(float dur)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 56]{self.Camp}获得冻结之弓BUFF，持续{dur}S");
        BuffBase buff = new Buff_DotOnAttack(E_BuffType.冻结之弓_正面, E_BuffPositive.正面, dur,
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

    public Skill_57(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { CreateBuff(buffDuration); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(buffDuration + henceTime * 5f); } // 10→15→20→25
    void CreateBuff(float dur)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 57]{self.Camp}获得感电之枪BUFF，持续{dur}S");
        BuffBase buff = new Buff_DotOnAttack(E_BuffType.感电之枪_正面, E_BuffPositive.正面, dur,
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

    public Skill_58(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { CreateBuff(dotLayers); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(dotLayers + henceTime); } // 1→2→3→4
    void CreateBuff(int layers)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 58]{self.Camp}获得火焰场地BUFF，持续{buffDuration}S，层数{layers}");
        BuffBase buff = new Buff_FieldDot(E_BuffType.火焰场地_正面, E_BuffPositive.正面, buffDuration,
            E_Dot.燃烧, self, tickInterval, layers);
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

    public Skill_59(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType) { }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target) { CreateBuff(dotLayers); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(dotLayers + henceTime); } // 1→2→3→4
    void CreateBuff(int layers)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 59]{self.Camp}获得雷电场地BUFF，持续{buffDuration}S，层数{layers}");
        BuffBase buff = new Buff_FieldDot(E_BuffType.雷电场地, E_BuffPositive.正面, buffDuration,
            E_Dot.感电, self, tickInterval, layers);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}
