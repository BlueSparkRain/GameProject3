using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class SkillBucket_20_to_39 { }


/// <summary>
/// 第20个：汲取【吸血+1】生命偷取+15%，持续20S
/// </summary>
[SkillID(20)]
public class Skill_20 : SkillBase
{
    public Skill_20(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能20--$$$$$");
    }
    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //基础倍率
    float baseRiseRate = 0.15f;
    /// <summary>
    /// 当前BUFF
    /// </summary>
    BuffBase buff;
    /// <summary>
    /// 当前调整值（+值）
    /// </summary>
    float adjustValue;
    public override void SkillEffect_Base(IBattlable target)
    {
        CreateBuff(level);
    }
    /// <summary>
    /// 生命偷取+1→生命偷取+2（生命偷取+30%）/生命偷取+3（生命偷取+45%）/生命偷取+4（生命偷取+60%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        CreateBuff(level + henceTime);
    }
    void CreateBuff(int level)
    {
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;
        Debug.Log($"[Skill 20]{self.Camp}发动技能20-[生命偷取+N（{level}）-BUFF]");
        if (buff != null)
        {
            Debug.Log("[Skill 20]:替换上一个属性调整效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Life_Steal, -adjustValue);
        }
        adjustValue = baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.生命偷取加N, E_BuffPositive.正面, buff_duration, battleControl, E_CharacterPropertyType.Life_Steal, adjustValue);
        //为目标注册BUFF
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}

/// <summary>
/// 第21个：再生【治疗+1】治疗加成+30%，持续20S
/// </summary>
[SkillID(21)]
public class Skill_21 : SkillBase
{
    public Skill_21(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能21--$$$$$");
    }

    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //基础倍率
    float baseRiseRate = 0.15f;
    /// <summary>
    /// 当前BUFF
    /// </summary>
    BuffBase buff;
    /// <summary>
    /// 当前调整值（+值）
    /// </summary>
    float adjustValue;
    public override void SkillEffect_Base(IBattlable target)
    {
        CreateBuff(level);
    }
    /// <summary>
    /// 治疗加成+1→治疗加成+2（治疗加成+30%）/治疗加成+3（治疗加成+45%）/治疗加成+4（治疗加成+60%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;
        Debug.Log($"[Skill 21]{self.Camp}发动技能21-[治疗强化+N（{level}）-BUFF]");
        if (buff != null){
            Debug.Log("[Skill 21]:替换上一个属性调整效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Heal_Amplification, -adjustValue);
        }
        adjustValue = baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.治疗强化加N, E_BuffPositive.正面, buff_duration, battleControl, E_CharacterPropertyType.Heal_Amplification, adjustValue);
        //为目标注册BUFF
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
/// <summary>
/// 第22个：净化之仪消除携带的全部减益效果
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
        BuffHandler.UnRegistBuffsByAttr(E_BuffPositive.负面);
    }
}
/// <summary>
/// 第23个：希望之歌 延长全部正面状态持续时间10S
/// </summary>
[SkillID(23)]
public class Skill_23 : SkillBase{
    public Skill_23(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能23--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"{self.Camp}延长全部正面BUFF持续时间10S");
        BuffHandler.ExtendBuffTimers(E_BuffPositive.正面, 10f);
    }
}
/// <summary>
/// ？？？ 第24个：镜像反射 复制1个敌人的正面效果，清除自身的全部负面效果（替换正面？）
/// </summary>
[SkillID(24)]
public class Skill_24 : SkillBase
{
    public Skill_24(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能24--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        // 清除自身全部负面效果
        BuffHandler.UnRegistBuffsByAttr(E_BuffPositive.负面);
        // 复制目标1个随机正面效果
        var targetHandler = target.battleDamageHandler.BuffHandler;
        var positiveBuffs = targetHandler.GetBuffsByAttr(E_BuffPositive.正面);
        if (positiveBuffs.Count > 0) {
            var copied = positiveBuffs[Random.Range(0, positiveBuffs.Count)];
            Debug.Log($"[Skill 24]{self.Camp}复制敌人正面BUFF：{copied.Buff_Type}");
            EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, copied);
        }
    }
}
/// <summary>
/// ？？？第25个 无心长刀：清除自身所有正面效果，然后对1敌人造成剑弱点伤害，每清除1种，伤害倍率+0.1
/// </summary>
[SkillID(25)]
public class Skill_25 : SkillBase
{
    public Skill_25(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能25--$$$$$");
    }

    float baseRate = 0.3f;

    public override void SkillEffect_Base(IBattlable target)
    {
        int removedCount = BuffHandler.GetBuffsByAttr(E_BuffPositive.正面).Count;
        BuffHandler.UnRegistBuffsByAttr(E_BuffPositive.正面);
        float rate = baseRate + removedCount * 0.1f;
        Debug.Log($"[Skill 25]{self.Camp}清除{removedCount}种正面效果，剑弱点伤害，倍率：{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.剑, -1, rate);
        atk.Excute(self, target);
    }
}
/// <summary>
/// ？？？第26个 戒心长枪：清除1名敌人所有负面效果，然后并对其造成枪弱点伤害，每清除1种，伤害倍率+0.1
/// </summary>
[SkillID(26)]
public class Skill_26 : SkillBase
{
    public Skill_26(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能26--$$$$$");
    }

    float baseRate = 0.3f;

    public override void SkillEffect_Base(IBattlable target)
    {
        var targetHandler = target.battleDamageHandler.BuffHandler;
        int removedCount = targetHandler.GetBuffsByAttr(E_BuffPositive.负面).Count;
        targetHandler.UnRegistBuffsByAttr(E_BuffPositive.负面);
        float rate = baseRate + removedCount * 0.1f;
        Debug.Log($"[Skill 26]{self.Camp}清除目标{removedCount}种负面效果，枪弱点伤害，倍率：{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.枪, -1, rate);
        atk.Excute(self, target);
    }
}
/// <summary>
/// ？？？（27）折射：获得随机 1 种持续5S的属性上升类型增益，重放X（=携带的攻击技能弱点种类数量）
/// </summary>
[SkillID(27)]
public class Skill_27 : SkillBase
{
    float buffDuration = 5f;
    float baseRiseRate = 0.2f;
    int recastCount = 3; // 默认攻击弱点种类：剑/枪/弓

    PropertyAdjust_Skill propty_Skill;
    PropertyAdjust_Skill propty_Skill_revert;
    ISkill revertDecorator;

    public Skill_27(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能27--$$$$$");
        propty_Skill = new PropertyAdjust_Skill();
        propty_Skill_revert = new PropertyAdjust_Skill();
        revertDecorator = new DelayTrigger_SkillDecorator(propty_Skill_revert, buffDuration);
    }

    E_CharacterPropertyType RandomProperty()
    {
        int index = Random.Range(0, 4);
        switch (index)
        {
            case 0: return E_CharacterPropertyType.Phy_Attack;
            case 1: return E_CharacterPropertyType.Phy_Resistance;
            case 2: return E_CharacterPropertyType.Mag_Attack;
            default: return E_CharacterPropertyType.Mag_Resistance;
        }
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        E_CharacterPropertyType propType = RandomProperty();
        Debug.Log($"[Skill 27]{self.Camp}发动技能27-[折射]，随机属性{propType}，重放{recastCount}次");

        propty_Skill.SetPropertyState(propType, 1, baseRiseRate);
        propty_Skill_revert.SetPropertyState(propType, 1, 1.0f / baseRiseRate);

        var multi = new MultiTime_SkillDecorator(propty_Skill, recastCount, 0.2f);
        multi.Excute(self, target);
        revertDecorator.Excute(self, target);
    }
}
/// <summary>
/// ？？？（28）断尾求生：获得【退化】状态（造成伤害减少25%）持续20S，回复25%最大生命值
/// </summary>
[SkillID(28)]
public class Skill_28 : SkillBase
{
    public Skill_28(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能28--$$$$$");
    }

    float buffDuration = 20f;
    float degradeRate = -0.25f;
    float healRate = 0.25f;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 28]{self.Camp}发动技能28-[退化]，伤害-25%，回复25%最大生命值");
        var buff = new Buff_DamageBoomer(E_BuffType.退化_负面, E_BuffPositive.负面, buffDuration, degradeRate);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);

        var heal = new ModelAdjust_Skill();
        heal.SetModelState(E_BattleModelType.HP, self.battleDamageHandler.GetMaxHealth(), healRate);
        heal.Excute(self, self);
    }
}
/// <summary>
/// ？？？第29个属性混乱：对随机敌人造成3次随机类型（冰、火、雷）的少量伤害，重放0
/// </summary>
[SkillID(29)]
public class Skill_29 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.2f;
    int hitCount = 3;
    static readonly E_WeaknessType[] randomElements = { E_WeaknessType.冰, E_WeaknessType.火, E_WeaknessType.雷 };

    public Skill_29(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能29--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        E_Camp enemyCamp = (self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方);
        var enemy = BattleTargetSelector.GetRandomNAliveTargets(enemyCamp, 1);
        if (enemy.Count == 0) return;

        var randomTarget = enemy[0];
        Debug.Log($"[Skill 29]{self.Camp}发动技能29-[属性混乱]对{randomTarget.battleDamageHandler.name}");

        for (int i = 0; i < hitCount; i++)
        {
            var element = randomElements[Random.Range(0, randomElements.Length)];
            var atk = new Attack_Skill();
            atk.SetAttackState(element, baseAttackValue, baseAttackRate);
            atk.Excute(self, randomTarget);
            Debug.Log($"[Skill 29]第{i + 1}击：{element}弱点伤害");
        }
    }
}



/// <summary>
/// ？？？（30）三器缭乱对全体敌人造成剑、枪、弓的中量伤害各2次
/// </summary>
[SkillID(30)]
public class Skill_30 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    int repeatPerWeapon = 2;
    static readonly E_WeaknessType[] weapons = { E_WeaknessType.剑, E_WeaknessType.枪, E_WeaknessType.弓 };
    WaitForSeconds hitDelay = new WaitForSeconds(0.15f);

    public Skill_30(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能30--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 30]{self.Camp}发动技能30-[三器缭乱]剑/枪/弓各{repeatPerWeapon}次");
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DoMultiWeapon(target));
    }

    IEnumerator DoMultiWeapon(IBattlable target)
    {
        foreach (var weapon in weapons)
        {
            for (int i = 0; i < repeatPerWeapon; i++)
            {
                var atk = new Attack_Skill();
                atk.SetAttackState(weapon, baseAttackValue, baseAttackRate);
                atk.Excute(self, target);
                yield return hitDelay;
            }
        }
    }
}

/// <summary>
/// ？？？（31）识破：使对方随机获得1未拥有的物理弱点（中耗）
/// </summary>
[SkillID(31)]
public class Skill_31 : SkillBase
{
    static readonly E_WeaknessType[] physPool = {
        E_WeaknessType.剑, E_WeaknessType.刀, E_WeaknessType.斧, E_WeaknessType.杖,
        E_WeaknessType.弓, E_WeaknessType.枪, E_WeaknessType.通解
    };

    public Skill_31(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能31--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        var candidates = new List<E_WeaknessType>();
        foreach (var w in physPool) {
            if (!target.weaknesses.Contains(w))
                candidates.Add(w);
        }
        if (candidates.Count > 0) {
            var newWeak = candidates[Random.Range(0, candidates.Count)];
            target.AddWeakness(newWeak);
            Debug.Log($"[Skill 31]{self.Camp}使目标获得新物理弱点：{newWeak}");
        }
    }
}
/// <summary>
/// ？？？（32）狮王狩猎：对1名敌人造成2次大量枪弱点伤害，该敌人每具有1个弱点，该技能伤害增加20%
/// </summary>
[SkillID(32)]
public class Skill_32 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.5f;
    float boostPerWeakness = 0.2f;
    int hitCount = 2;

    public Skill_32(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能32--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        // 计算目标弱点数量（多弱点系统）
        int weaknessCount = target.weaknesses.Count;
        float rate = baseAttackRate + weaknessCount * boostPerWeakness;
        Debug.Log($"[Skill 32]{self.Camp}发动技能32-[狮王狩猎]，目标弱点数{weaknessCount}，枪伤害倍率{rate}");

        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, rate);
        var multi = new MultiTime_SkillDecorator(atk, hitCount, 0.3f);
        multi.Excute(self, target);
    }
}
/// <summary>
/// ？？？（33）倾盆大雨：对全体敌人造成大量弓弱点伤害，重放2，如果这次攻击击杀了敌人或者使敌人进入力竭状态，重放次数+1（可叠加）
/// </summary>
[SkillID(33)]
public class Skill_33 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.5f;
    int baseRecast = 2;
    WaitForSeconds recastDelay = new WaitForSeconds(0.25f);

    public Skill_33(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能33--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 33]{self.Camp}发动技能33-[倾盆大雨]，基础重放{baseRecast}");
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DoRecastLoop(target));
    }

    IEnumerator DoRecastLoop(IBattlable target)
    {
        int remaining = baseRecast;
        while (remaining > 0)
        {
            bool wasAlive = target.IsAlive;
            float prevShield = target.battleDamageHandler.BattleController.GetCharacterModelValue(E_BattleModelType.ShieldPoints);

            var atk = new Attack_Skill();
            atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, baseAttackRate);
            atk.Excute(self, target);

            remaining--;
            // 检查是否击杀或力竭（护盾归零）
            if (!target.IsAlive && wasAlive) { remaining++; Debug.Log($"[Skill 33]击杀目标，重放+1，剩余{remaining}"); }
            else {
                float curShield = target.battleDamageHandler.BattleController.GetCharacterModelValue(E_BattleModelType.ShieldPoints);
                if (prevShield > 0 && curShield <= 0) { remaining++; Debug.Log($"[Skill 33]目标力竭，重放+1，剩余{remaining}"); }
            }

            if (remaining > 0) yield return recastDelay;
        }
    }
}
/// <summary>
/// ？？？（34）无尽终结：造成大量剑弱点伤害，对力竭敌人造成2倍伤害
/// </summary>
[SkillID(34)]
public class Skill_34 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.6f;
    float breakMulti = 2f;

    public Skill_34(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能34--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        float shield = target.battleDamageHandler.BattleController.GetCharacterModelValue(E_BattleModelType.ShieldPoints);
        bool isBroken = shield <= 0;
        float rate = baseAttackRate * (isBroken ? breakMulti : 1f);
        Debug.Log($"[Skill 34]{self.Camp}发动技能34-[无尽终结]，目标力竭:{isBroken}，倍率{rate}");
        var atk = new Attack_Skill();
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, rate);
        atk.Excute(self, target);
    }
}
/// <summary>
/// ？？？（35）乘胜追击：延长1名敌人击破状态5S，并且使其获得【易损】状态（力竭时受到伤害增加35%）持续5S
/// </summary>
[SkillID(35)]
public class Skill_35 : SkillBase
{
    float vulDuration = 5f;
    float vulRate = 0.35f;

    public Skill_35(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能35--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        var targetController = target.battleDamageHandler.BattleController;
        var targetHandler = target.battleDamageHandler.BuffHandler;

        // 延长击破状态：削减护盾来延长力竭（若已无盾则保持）
        float curShield = targetController.GetCharacterModelValue(E_BattleModelType.ShieldPoints);
        if (curShield > 0)
            target.battleDamageHandler.DoModelValue(E_BattleModelType.ShieldPoints, -curShield);

        Debug.Log($"[Skill 35]{self.Camp}延长目标击破状态5S，施加易损");

        // 施加易损：双抗降低35%，持续5S
        float phyRes = targetController.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance);
        float magRes = targetController.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance);
        var vulBuff = new Buff_Vulnerable(E_BuffType.脆弱_负面, E_BuffPositive.负面, vulDuration,
            targetController, phyRes * vulRate, magRes * vulRate);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, targetHandler, vulBuff);
    }
}
/// <summary>
/// ？？？（36）先发制人：对敌方全体造成大量剑/枪/弓弱点伤害1次，这个技能只能释放1次（方向键切换弱点类型）
/// </summary>
[SkillID(36)]
public class Skill_36 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.5f;
    E_WeaknessType weakness = E_WeaknessType.剑;

    public Skill_36(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能36--$$$$$");
    }

    public void SwitchWeakness(E_WeaknessType type)
    {
        if (type == E_WeaknessType.剑 || type == E_WeaknessType.枪 || type == E_WeaknessType.弓)
            weakness = type;
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 36]{self.Camp}发动技能36-[全体{weakness}伤害]");
        var atk = new Attack_Skill();
        atk.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }
}
/// <summary>
/// ？？？（37）迅雷连锁：获得【迅雷连锁】状态（物理攻击技能附带0.1倍率雷弱点攻击伤害）持续15S
/// </summary>
[SkillID(37)]
public class Skill_37 : SkillBase
{
    float buffDuration = 15f;
    float damageRate = 0.1f;

    public Skill_37(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能37--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 37]{self.Camp}发动技能37-[迅雷连锁-BUFF]");
        var buff = new Buff_AdditiveAttack(E_BuffType.迅雷之影_正面, E_BuffPositive.正面, buffDuration,
            this, E_WeaknessType.雷, damageRate);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}
/// <summary>
///？？？ 38（暴雪连锁）：获得【暴雪连锁】状态（物理攻击技能附带0.1倍率冰弱点攻击伤害）持续15S
/// </summary>
[SkillID(38)]
public class Skill_38 : SkillBase
{
    float buffDuration = 15f;
    float damageRate = 0.1f;

    public Skill_38(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能38--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 38]{self.Camp}发动技能38-[暴雪连锁-BUFF]");
        var buff = new Buff_AdditiveAttack(E_BuffType.冰雪风暴_正面, E_BuffPositive.正面, buffDuration,
            this, E_WeaknessType.冰, damageRate);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}
/// <summary>
/// ？？？（39）超大魔法化：获得【超大魔法化】状态（魔法类型攻击技能将会额外释放2次），持续20S
/// </summary>
[SkillID(39)]
public class Skill_39 : SkillBase
{
    float buffDuration = 20f;
    int recastCount = 2;

    public Skill_39(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能39--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 39]{self.Camp}发动技能39-[超大魔法化-BUFF]，额外释放{recastCount}次");
        var buff = new Buff_SkillRecast(E_BuffType.超大魔法化_正面, E_BuffPositive.正面, buffDuration, recastCount);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}
