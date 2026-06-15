using Core;
using System.Collections;
using UnityEngine;


public class SkillBucket_0_to_19 { }

#region 技能描述
/***
 (0)   造成剑 / 枪 / 弓弱点的较低伤害（初始技能）（方向键切换弱点类型）
（1）   回复{200*（等级//10+1）}点法力值（初始技能）
（2）   赋予自身一个随机属性上升类型的强化效果，持续20S（初始技能）
（3）   无视弱点，削减对方1点护盾点数，并造成中量物理伤害（中低耗）（初始技能）
（4）   获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
（5）   获得【雷电风暴】状态（每10S对1名随机敌人造成0.4倍率雷弱点伤害），持续30S（初始技能）
（6）   获得【冰雪场地】状态（每4秒使敌方全体获得1层冻结），持续40S（中耗）（初始技能）
（7）   对随机敌人造成枪弱点伤害，重放（3~5）次（区间内随机）（初始技能）
（8）   获得【物攻+1】（物攻+20%），持续20S（初始技能）
（9）   回复 30% 最大生命值，每次击破后 ATB-2/充能时间减少40S，释放后重置（初始技能）
（10）  获得【战意】状态（造成伤害增加30%）持续20S，减少20%当前生命值（初始技能）
（11）  获得【大魔法化】状态（魔法类型攻击技能将会额外释放1次）持续20S（初始技能）
（12）  对全体敌人造成冰/雷/火弱点的伤害（中耗）（方向键切换）（初始技能）
（13）  获得【魔攻+1】（魔攻+20%），持续20S（初始技能）
（14）  获得【物防+1】（物防+20%），持续20S
（15）  获得【魔防+1】（魔防+20%），持续20S
（16）  对1名敌人施加【物攻-1】（物攻-15%），持续20S
（17）  对1名敌人施加【魔攻-1】（魔攻-15%），持续20S
（18）  对1名敌人施加【物防-1】（物防-15%），持续20S
（19）  对1名敌人施加【魔防-1】（魔防-15%），持续20S
***/
#endregion

/// <summary>
///（0）【斩击/刺击/射击】造成剑/枪/弓弱点的较低伤害（初始技能）（方向键切换弱点类型）
/// </summary>
[SkillID(0)]
public class Skill_BaseAttack : SkillBase{
    #region 技能基础Info
    float baseAttackValue = -1;
    float baseAttackRate = 0.1f;
    E_WeaknessType weakness = E_WeaknessType.剑;
    Attack_Skill atk_iSkill;
    #endregion

    #region 技能加强Info
    float multiInterval = 0.3f;
    #endregion

    //魔法类型技能
    public override bool IsMagicType => true;
    public Skill_BaseAttack(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType){
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能0--$$$$$");
        atk_iSkill = new Attack_Skill();

    }
    public override void SkillEffect_Base(IBattlable target){
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 0]{self.Camp}发动技能0-[{weakness}属性攻击]");
        atk_iSkill.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        atk_iSkill.Excute(self, target);
    }

    public void SwitchWeakness(E_WeaknessType type){
        weakness = type;
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 0]{self.Camp}发动强化技能0——等级{henceTime}");
        Attack_Skill innerSkill = new Attack_Skill();
        innerSkill.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        ISkill henceISkill = new MultiTime_SkillDecorator(innerSkill, henceTime + 1, multiInterval);
        henceISkill.Excute(self, target);
    }
}

/// <summary>
/// （1）【魔力复原】回复{200*（等级//10+1）}点法力值（初始技能）
/// </summary>
[SkillID(1)]
public class Skill_1 : SkillBase
{
    float baseHealValue = 200;
    float divRate = 10;
    ModelAdjust_Skill mdl_iSkill;
    public override bool IsMagicType => true;
    public Skill_1(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能1--$$$$$");
        mdl_iSkill = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target)
    {
        float healRate = (Controller.GetCharacterPropertyValue(E_CharacterPropertyType.CurrentLevel) / divRate) + 1;
        mdl_iSkill.SetModelState(E_BattleModelType.SP, baseHealValue, healRate);
        mdl_iSkill.Excute(self, target);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 1]{self.Camp}发动技能1-[回蓝]");
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 1]{self.Camp}发动强化技能1——等级{henceTime}");
        float healRate = (Controller.GetCharacterPropertyValue(E_CharacterPropertyType.CurrentLevel) / divRate) + 1;
        ModelAdjust_Skill innerSkill = new ModelAdjust_Skill();
        innerSkill.SetModelState(E_BattleModelType.SP, baseHealValue, healRate);
        ISkill henceISkill = new MultiTime_SkillDecorator(innerSkill, henceTime + 1, 0);
        henceISkill.Excute(self, target);
    }
}

/// <summary>
///（2）【灵光一闪】赋予自身一个随机属性上升类型的强化效果，持续20S（初始技能）
/// </summary>
[SkillID(2)]
public class Skill_2 : SkillBase
{
    float buff_duration = 20;
    float baseRiseRate = 1.5f;

    PropertyAdjust_Skill propty_Skill;
    PropertyAdjust_Skill propty_Skill_deco;
    ISkill buffLike_decorator;

    public Skill_2(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能2--$$$$$");
        propty_Skill = new PropertyAdjust_Skill();
        propty_Skill_deco = new PropertyAdjust_Skill();
        buffLike_decorator = new DelayTrigger_SkillDecorator(propty_Skill_deco, buff_duration);
    }
    E_CharacterPropertyType RandomProperty()
    {
        int index = Random.Range(0, 4);
        E_CharacterPropertyType propertyType = E_CharacterPropertyType.Phy_Attack;
        switch (index)
        {
            case 0: DebugManager.Log(EDebugCategory.SkillExecution,"[Skill2]-0"); propertyType = E_CharacterPropertyType.Phy_Attack; break;
            case 1: DebugManager.Log(EDebugCategory.SkillExecution,"[Skill2]-1"); propertyType = E_CharacterPropertyType.Phy_Resistance; break;
            case 2: DebugManager.Log(EDebugCategory.SkillExecution,"[Skill2]-2"); propertyType = E_CharacterPropertyType.Mag_Attack; break;
            case 3: DebugManager.Log(EDebugCategory.SkillExecution,"[Skill2]-3"); propertyType = E_CharacterPropertyType.Mag_Resistance; break;
        }
        return propertyType;
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 2]{self.Camp}发动技能2-[属性加强Buff]");
        E_CharacterPropertyType propertyType = RandomProperty();
        propty_Skill.SetPropertyState(propertyType, 1, baseRiseRate);
        propty_Skill.Excute(self, target);
        propty_Skill_deco.SetPropertyState(propertyType, 1, 1.0f / baseRiseRate);
        buffLike_decorator.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 2]{self.Camp}发动强化技能2——等级{henceTime}");
        int count = henceTime + 1; // 增幅1→2个, 增幅2→3个, 增幅3→4个
        for (int i = 0; i < count; i++)
            SkillEffect_Base(target);
    }
}

/// <summary>
///（3）【猛击要害】无视弱点，削减对方1点护盾点数，并造成中量物理伤害（中低耗）（初始技能）
/// </summary>
[SkillID(3)]
public class Skill_3 : SkillBase
{
    float baseDamageValue = -1;
    float damageRate = 0.6f;
    Attack_Skill atk_iSkill;
    ModelAdjust_Skill mdl_iSkill;
    public Skill_3(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能3--$$$$$");
        atk_iSkill = new Attack_Skill();
        mdl_iSkill = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target)
    {
        mdl_iSkill.SetModelState(E_BattleModelType.ShieldPoints, -1, 1);
        mdl_iSkill.Excute(self, target);
        atk_iSkill.SetAttackState(E_WeaknessType.无_, baseDamageValue, damageRate);
        atk_iSkill.Excute(self, target);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 3]{self.Camp}发动技能3-[盾点-1+伤害]");
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 3]{self.Camp}发动强化技能3——等级{henceTime}");
        int shieldReduce = 1 + henceTime; // 增幅1→减2, 增幅2→减3, 增幅3→减4
        mdl_iSkill.SetModelState(E_BattleModelType.ShieldPoints, -shieldReduce, 1);
        mdl_iSkill.Excute(self, target);
        atk_iSkill.SetAttackState(E_WeaknessType.无_, baseDamageValue, damageRate);
        atk_iSkill.Excute(self, target);
    }
}

/// <summary>
///（4) 【炽焰连锁】获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续15S（初始技能）
/// </summary>
[SkillID(4)]
public class Skill_4 : SkillBase
{
    float buffDuration = 15;
    public Skill_4(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能4--$$$$$");
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        CreateBuff(buffDuration + henceTime * 10); // 15→25→35→45
    }
    public override void SkillEffect_Base(IBattlable target)
    {
        CreateBuff(buffDuration);
    }
    void CreateBuff(float buffDuration)
    {
        BuffBase buff = new Buff_AdditiveAttack(E_BuffType.炽焰连锁, E_BuffPositive.正面, buffDuration, this, E_WeaknessType.火, 0.1f);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 4]{self.Camp}发动技能4-[炽焰连锁-BUFF]");
    }
}

/// <summary>
///（5）【雷电风暴】获得【雷电风暴】状态（每10S对1名随机敌人造成0.4倍率雷弱点伤害），持续30S（初始技能）
/// </summary>
[SkillID(5)]
public class Skill_5 : SkillBase
{
    float buffDuration = 15;
    float damageRate = 0.4f;
    float attackInterval = 5;
    public Skill_5(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能5--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target)
    {
        CreateBuff(buffDuration, damageRate);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float _damageRate = damageRate + 0.2f * henceTime;
        CreateBuff(buffDuration, _damageRate);
    }

    void CreateBuff(float buffDuration, float _damageRate)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 5]{self.Camp}发动技能5-[雷电风暴-BUFF]");
        E_Camp enemy_Camp = (self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方);
        BuffBase buff = new Buff_AutoDamage(E_BuffType.雷电风暴, E_BuffPositive.正面, buffDuration, self, enemy_Camp,
            E_WeaknessType.雷, _damageRate, attackInterval);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（6）【冰霜领域】获得【冰雪场地】状态（每4秒使敌方全体获得1层冻结），持续40S（中耗）（初始技能）
/// </summary>
[SkillID(6)]
public class Skill_6 : SkillBase
{
    float buff_duration = 10;
    float triggerInterval = 1;
    public Skill_6(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能6--$$$$$");
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        CreateBuff(buff_duration, 1 + henceTime);
    }
    public override void SkillEffect_Base(IBattlable target)
    {
        CreateBuff(buff_duration);
    }
    void CreateBuff(float buffDuration, int baseDotCount = 1)
    {
        var targets = BattleTargetSelector.GetValidTargets(self, E_SkillTargetType_Auto.对全体);
        foreach (var target in targets)
        {
            DotBase dot = new Dot_Freeze(E_Dot.冻结, target, baseDotCount);
            BuffBase buff = new Buffer_AssignDot(E_BuffType.冰雪场地, E_BuffPositive.正面, buffDuration, dot, target.battleDamageHandler.DotHandler, triggerInterval, baseDotCount);
            EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
        }
    }
}

/// <summary>
///（7）【海纳百川】对随机敌人造成枪弱点伤害，重放（3~5）次（区间内随机）（初始技能）
/// </summary>
[SkillID(7)]
public class Skill_7 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.2f;
    E_WeaknessType weakness = E_WeaknessType.枪;
    Attack_Skill atk_iSkill;
    WaitForSeconds excuteDelay;
    public Skill_7(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能7--$$$$$");
        atk_iSkill = new Attack_Skill();
        excuteDelay = new WaitForSeconds(0.2f);
    }

    public override void SkillEffect_Base(IBattlable target){
        int excuteTime = Random.Range(3, 6);
        SelectRandomTargetExcute(excuteTime);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        int excuteTime = Random.Range(3 + henceTime * 2, 6 + henceTime * 2);
        SelectRandomTargetExcute(excuteTime);
    }

    IEnumerator DoHurt(int excuteTime, IBattlable target)
    {
        for (int i = 0; i < excuteTime; i++)
        {
            atk_iSkill.Excute(self, target);
            yield return excuteDelay;
        }
    }
    void SelectRandomTargetExcute(int excuteTime)
    {
        E_Camp ememyCamp = (self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方);
        var _target = BattleTargetSelector.GetRandomNAliveTargets(ememyCamp, 1)[0];
        atk_iSkill.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 7]{self.Camp}发动技能7-[重放-枪弱点攻击],重放次数{excuteTime}");
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DoHurt(excuteTime, _target));
    }
}

/// <summary>
///（8）【力量增效】获得【物攻+1】（物攻+20%），持续20S（初始技能）
/// </summary>
[SkillID(8)]
public class Skill_8 : SkillBase{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.2f;
    public Skill_8(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType){
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能8--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level){
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 8]{self.Camp}发动技能8-[物攻+N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.物攻加N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack) * baseRiseRate * level;
        BuffBase buff = new Buff_AdjustProperty(E_BuffType.物攻加N, E_BuffPositive.正面, buff_duration, Controller, E_CharacterPropertyType.Phy_Attack, adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（9）【气沉丹田】回复 30% 最大生命值，每次击破后 ATB-2/充能时间减少10S，释放后重置（初始技能）
/// </summary>
[SkillID(9)]
public class Skill_9 : SkillBase
{
    ModelAdjust_Skill mdl_iSkill;
    float baseHealRate = 0.3f; // 30% → 50% → 70% → 90%
    public Skill_9(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能9--$$$$$");
        mdl_iSkill = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target)
    {
        Heal(target, baseHealRate);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float rate = baseHealRate + henceTime * 0.2f; // 30→50→70→90%
        Heal(target, rate);
    }
    void Heal(IBattlable target, float rate)
    {
        float healValue = self.battleDamageHandler.GetMaxHealth() * rate;
        mdl_iSkill.SetModelState(E_BattleModelType.HP, healValue, 1f);
        mdl_iSkill.Excute(self, target);
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 9]{self.Camp}发动技能9-[回血:{rate*100}%={healValue}]");
    }
}

/// <summary>
///（10）【背水一战】获得【战意】状态（造成伤害增加30%）持续15S，减少20%当前生命值（初始技能）
/// </summary>
[SkillID(10)]
public class Skill_10 : SkillBase
{
    public Skill_10(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能10--$$$$$");
    }
    float buffDuration = 15;
    float boomerRate = 0.3f;
    float hpCostRate = 0.2f;
    ModelAdjust_Skill hpCost;

    public override void SkillEffect_Base(IBattlable target) { Apply(target, buffDuration, boomerRate, hpCostRate); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        Apply(target, buffDuration + henceTime * 10, boomerRate, hpCostRate); // 15→25→35→45
    }

    void Apply(IBattlable target, float dur, float rate, float costPct)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 10]{self.Camp}发动技能10-[战意-BUFF {dur}S]");
        BuffBase buff = new Buff_DamageBoomer(E_BuffType.战意_正面, E_BuffPositive.正面, dur, rate);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);

        // 减少20%当前生命值
        float cost = self.battleDamageHandler.GetCurrentHealth() * costPct;
        if (hpCost == null) hpCost = new ModelAdjust_Skill();
        hpCost.SetModelState(E_BattleModelType.HP, -cost, 1f);
        hpCost.Excute(self, target);
    }
}

/// <summary>
///（11）【大魔法化】获得【大魔法化】状态（魔法类型攻击技能将会额外释放1次）持续20S（初始技能）
/// </summary>
[SkillID(11)]
public class Skill_11 : SkillBase{
    float buffDuration = 20;
    public Skill_11(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType){
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能11--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(buffDuration,E_SkillLevel.基础版本,0,1);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(buffDuration,E_SkillLevel.加强版本,henceTime,1);
    }

    void CreateBuff(float buffDuration,E_SkillLevel skillLevel,int henctime,int reactTime=1){
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 11]{self.Camp}发动技能11-[大魔法化-BUFF]");
        BuffBase buff = new Buff_SkillRecast(E_BuffType.大魔法化_正面, E_BuffPositive.正面, buffDuration, reactTime);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（12）【寒冰魔法/雷电魔法/火焰魔法】对全体敌人造成冰/雷/火弱点的伤害（中耗）（方向键切换）（初始技能）
/// </summary>
[SkillID(12)]
public class Skill_12 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.5f;
    E_WeaknessType weakness = E_WeaknessType.冰;
    Attack_Skill atk_iSkill;

    public override bool IsMagicType => true;

    public Skill_12(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能12--$$$$$");
        atk_iSkill = new Attack_Skill();
    }

    public void SwitchWeakness(E_WeaknessType type)
    {
        // 只接受冰/雷/火
        if (type == E_WeaknessType.冰 || type == E_WeaknessType.雷 || type == E_WeaknessType.火)
            weakness = type;
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 12]{self.Camp}发动技能12-[全体{weakness}弱点攻击]");
        atk_iSkill.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        atk_iSkill.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 12]{self.Camp}发动强化技能12——等级{henceTime}");
        Attack_Skill innerSkill = new Attack_Skill();
        innerSkill.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        ISkill henceISkill = new MultiTime_SkillDecorator(innerSkill, henceTime + 1, 0.3f);
        henceISkill.Excute(self, target);
    }
}

/// <summary>
///（13）【魔法增效】获得【魔攻+1】（魔攻+20%），持续20S（初始技能）
/// </summary>
[SkillID(13)]
public class Skill_13 : SkillBase
{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.2f;

    public Skill_13(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能13--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 13]{self.Camp}发动技能13-[魔攻+N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.魔攻加N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack) * baseRiseRate * level;
        var buff = new Buff_AdjustProperty(E_BuffType.魔攻加N, E_BuffPositive.正面, buff_duration, Controller, E_CharacterPropertyType.Mag_Attack, adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（14）【坚铁防壁】获得【物防+1】（物防+20%），持续20S
/// </summary>
[SkillID(14)]
public class Skill_14 : SkillBase
{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.2f;

    public Skill_14(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能14--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 14]{self.Camp}发动技能14-[物防+N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.物防加N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance) * baseRiseRate * level;
        var buff = new Buff_AdjustProperty(E_BuffType.物防加N, E_BuffPositive.正面, buff_duration, Controller, E_CharacterPropertyType.Phy_Resistance, adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（15）【秘银结界】获得【魔防+1】（魔防+20%），持续20S
/// </summary>
[SkillID(15)]
public class Skill_15 : SkillBase
{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.2f;

    public Skill_15(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能15--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 15]{self.Camp}发动技能15-[魔防+N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.魔防加N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance) * baseRiseRate * level;
        var buff = new Buff_AdjustProperty(E_BuffType.魔防加N, E_BuffPositive.正面, buff_duration, Controller, E_CharacterPropertyType.Mag_Resistance, adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（16）【力量弱化】对1名敌人施加【物攻-1】（物攻-15%），持续20S
/// </summary>
[SkillID(16)]
public class Skill_16 : SkillBase
{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.15f;

    public Skill_16(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能16--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 16]{self.Camp}发动技能16-[物攻-N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.物攻减N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack) * baseRiseRate * level;
        var buff = new Buff_AdjustProperty(E_BuffType.物攻减N, E_BuffPositive.负面, buff_duration, Controller, E_CharacterPropertyType.Phy_Attack, -adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（17）【魔法弱化】对1名敌人施加【魔攻-1】（魔攻-15%），持续20S
/// </summary>
[SkillID(17)]
public class Skill_17 : SkillBase
{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.15f;

    public Skill_17(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能17--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 17]{self.Camp}发动技能17-[魔攻-N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.魔攻减N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack) * baseRiseRate * level;
        var buff = new Buff_AdjustProperty(E_BuffType.魔攻减N, E_BuffPositive.负面, buff_duration, Controller, E_CharacterPropertyType.Mag_Attack, -adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（18）【防壁破坏】对1名敌人施加【物防-1】（物防-15%），持续20S
/// </summary>
[SkillID(18)]
public class Skill_18 : SkillBase
{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.15f;

    public Skill_18(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能18--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 18]{self.Camp}发动技能18-[物防-N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.物防减N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance) * baseRiseRate * level;
        var buff = new Buff_AdjustProperty(E_BuffType.物防减N, E_BuffPositive.负面, buff_duration, Controller, E_CharacterPropertyType.Phy_Resistance, -adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}

/// <summary>
///（19）【结界破坏】对1名敌人施加【魔防-1】（魔防-15%），持续20S
/// </summary>
[SkillID(19)]
public class Skill_19 : SkillBase
{
    int level = 1;
    float buff_duration = 20;
    float baseRiseRate = 0.15f;

    public Skill_19(E_SkillTargetType_Auto _skillTargetType) : base(_skillTargetType)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"$$$$$--技能19--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target) { CreateBuff(level); }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime) { CreateBuff(level + henceTime); }

    void CreateBuff(int level)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,$"[Skill 19]{self.Camp}发动技能19-[魔防-N（{level}）-BUFF]");
        BuffHandler.UnRegistBuff(E_BuffType.魔防减N);
        float adjustValue = Controller.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance) * baseRiseRate * level;
        var buff = new Buff_AdjustProperty(E_BuffType.魔防减N, E_BuffPositive.负面, buff_duration, Controller, E_CharacterPropertyType.Mag_Resistance, -adjustValue);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, BuffHandler, buff);
    }
}
