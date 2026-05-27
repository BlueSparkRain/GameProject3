using Core;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


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
///（0）造成剑/枪/弓弱点的较低伤害（初始技能）（方向键切换弱点类型）
/// </summary>
[SkillID(0)]
public class Skill_BaseAttack : SkillBase
{
    #region 技能基础Info
    //float baseAttackValue = -2;
    float baseAttackValue = -5;
    float baseAttackRate = 0.1f;
    //决定伤害类型
    E_WeaknessType weakness = E_WeaknessType.剑;
    Attack_Skill atk_iSkill;
    #endregion

    #region 技能加强Info
    //多段攻击间隔=0.3f
    float  multiInterval=0.3f;
    #endregion

    public Skill_BaseAttack(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能0--$$$$$");
        atk_iSkill = new Attack_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 0]{self.Camp}发动技能0-[特定弱点攻击]");
        atk_iSkill.SetAttackState(weakness,baseAttackValue,baseAttackRate);
        atk_iSkill.Excute(self,target);
    }

    public void SwitchWeakness(E_WeaknessType type){
        weakness = type;
    }

    //伤害段数变为2/3/4
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        Debug.Log($"[Skill 0]{self.Camp}发动强化技能0——等级{henceTime}");
        Attack_Skill innerSkill = new Attack_Skill();
        innerSkill.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        ISkill henceISkill = new MultiTime_SkillDecorator(innerSkill, henceTime+1, multiInterval);
        henceISkill.Excute(self,target);
    }
}

/// <summary>
/// （1）回复{200*（等级//10+1）}点法力值（初始技能）
/// </summary>
[SkillID(1)]
public class Skill_1 : SkillBase
{
    float baseHealValue = 200;
    float divRate = 10;
    ModelAdjust_Skill mdl_iSkill;
    public Skill_1(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能1--$$$$$");
        mdl_iSkill = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target)
    {
        float healRate = (self.battleDamageHandler.BattleController
            .GetCharacterPropertyValue(E_CharacterPropertyType.CurrentLevel) / divRate) + 1;
        mdl_iSkill.SetModelState(E_BattleModelType.SP, baseHealValue, healRate);
        mdl_iSkill.Excute(self, target);
        Debug.Log($"[Skill 1]{self.Camp}发动技能1-[回蓝]");
    }
    //回复法力值变为2倍/3倍/4倍
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        Debug.Log($"[Skill 0]{self.Camp}发动强化技能1——等级{henceTime}");
        float healRate = (self.battleDamageHandler.BattleController
          .GetCharacterPropertyValue(E_CharacterPropertyType.CurrentLevel) / divRate) + 1;
        ModelAdjust_Skill innerSkill = new ModelAdjust_Skill();
        innerSkill.SetModelState(E_BattleModelType.SP, baseHealValue, healRate);
        ISkill henceISkill = new MultiTime_SkillDecorator(innerSkill, henceTime + 1, 0);
        henceISkill.Excute(self, target);
    }
}

/// <summary> 
///（2）赋予自身一个随机属性上升类型的强化效果，持续20S（初始技能）
/// </summary>
[SkillID(2)]
public class Skill_2 : SkillBase
{
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 1.5f;

    PropertyAdjust_Skill propty_Skill;
    PropertyAdjust_Skill propty_Skill_deco;
    ISkill buffLike_decorator;

    public Skill_2(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能2--$$$$$");
        propty_Skill=new PropertyAdjust_Skill();
        propty_Skill_deco = new PropertyAdjust_Skill();

        buffLike_decorator = new DelayTrigger_SkillDecorator(propty_Skill_deco, buff_duration);
    }
    E_CharacterPropertyType RandomProperty(){
        int index = Random.Range(0, 4);
        E_CharacterPropertyType propertyType = E_CharacterPropertyType.Phy_Attack;
        switch (index){
            case 0:Debug.Log("[Skill2]-0");propertyType = E_CharacterPropertyType.Phy_Attack; break;
            case 1:Debug.Log("[Skill2]-1");propertyType = E_CharacterPropertyType.Phy_Resistance; break;
            case 2:Debug.Log("[Skill2]-2");propertyType = E_CharacterPropertyType.Mag_Attack; break;
            case 3:Debug.Log("[Skill2]-3");propertyType = E_CharacterPropertyType.Mag_Resistance; break;}
        return propertyType;
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 2]{self.Camp}发动技能2-[属性加强Buff]");
        E_CharacterPropertyType propertyType = RandomProperty();
        propty_Skill.SetPropertyState(propertyType, 1, baseRiseRate);
        propty_Skill.Excute(self,target);
        propty_Skill_deco.SetPropertyState(propertyType, 1, 1.0f/baseRiseRate);
        buffLike_decorator.Excute(self,target);
    }
    //强化数量变为2/3/4 （重复几次）
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){

    }
}

/// <summary>
///（3）无视弱点，削减对方1点护盾点数，并造成中量物理伤害（中低耗）（初始技能）
/// </summary>
[SkillID(3)]
public class Skill_3 : SkillBase{
    float baseDamageValue = -1;
    float damageRate = 0.6f;
    Attack_Skill atk_iSkill;
    ModelAdjust_Skill mdl_iSkill;
    public Skill_3(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能3--$$$$$");
        atk_iSkill = new Attack_Skill();
        mdl_iSkill=new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        //无视弱点进行削韧1点
        mdl_iSkill.SetModelState(E_BattleModelType.ShieldPoints,-1,1);
        mdl_iSkill.Excute(self,target);
        //造成中量物理伤害
        atk_iSkill.SetAttackState(E_WeaknessType.无,baseDamageValue,damageRate);
        atk_iSkill.Excute(self, target);
        Debug.Log($"[Skill 3]{self.Camp}发动技能3-[盾点-1+伤害]");
    }
    //削减点数变为2/3/4
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        
    }
}

/// <summary>
///（4) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(4)]
public class Skill_4 : SkillBase{
    float buffDuration = 20;
    public Skill_4(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能4--$$$$$");
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(buffDuration+henceTime*20);
    }
    public override void SkillEffect_Base(IBattlable target){
        //为角色增加一个BUFF，在BUff生效期间内检测造成的伤害是不是物理伤害
        CreateBuff(buffDuration);
    }
    void CreateBuff(float buffDuration) {
        var buffHandle = self.battleDamageHandler.BuffHandler;
        BuffBase buff = new Buff_AdditiveAttack(E_BuffType.炽焰连锁, E_BuffPositive.正面, buffDuration, this, E_WeaknessType.火, 0.1f);
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle,buff);
        Debug.Log($"[Skill 4]{self.Camp}发动技能4-[炽焰连锁-BUFF]");
    }
}
/// <summary>
///（5）获得【雷电风暴】状态（每10S对1名随机敌人造成0.4倍率雷弱点伤害），持续30S（初始技能）
/// </summary>
[SkillID(5)]
public class Skill_5 : SkillBase{
    float buffDuration = 30;
    float damageRate=0.4f;
    /// <summary>
    /// 打击间隔
    /// </summary>
    float attackInterval=3;
    public Skill_5(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能5--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target){
        //为角色增加一个BUFF，在BUff生效期间内检测造成的伤害是不是物理伤害
        CreateBuff(buffDuration,damageRate);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        float _damageRate = damageRate + 0.2f * henceTime;
        CreateBuff(buffDuration,_damageRate);
    }

    void CreateBuff(float buffDuration,float _damageRate){
        Debug.Log($"[Skill 5]{self.Camp}发动技能5-[雷电风暴-BUFF]");
        var buffHandle = self.battleDamageHandler.BuffHandler;
        E_Camp enemy_Camp = (self.Camp == E_Camp.玩家方 ? E_Camp.敌方 :E_Camp.玩家方);
        BuffBase buff = new Buff_AutoDamage(E_BuffType.雷电风暴, E_BuffPositive.正面, buffDuration,self,enemy_Camp,
            E_WeaknessType.雷, _damageRate,attackInterval);
        //为目标注册BUFF
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle,buff);
    }
}

/// <summary>
///（6）获得【冰雪场地】状态（每4秒使敌方全体获得1层冻结），持续40S（中耗）（初始技能）
/// </summary>
[SkillID(6)]
public class Skill_6 : SkillBase{
    float buff_duration = 40;
    float triggerInterval=4;
    public Skill_6(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能6--$$$$$");
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(buff_duration,1+henceTime);
    }
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(buff_duration);
    }
    /// <summary>
    /// 默认为目标增添n层（默认1层）BUFF
    /// </summary>
    /// <param name="buffDuration"></param>
    /// <param name="baseDotCount"></param>
    void CreateBuff(float buffDuration,int baseDotCount=1){
        var buffHandle = self.battleDamageHandler.BuffHandler;
        var targets = BattleTargetSelector.GetValidTargets(self,E_SkillTargetType.对全体);
        foreach (var target in targets) {
        DotBase dot = new Dot_Freeze(E_Dot.冻结,target,baseDotCount);
        BuffBase buff = new  Buffer_AssignDot(E_BuffType.冰雪场地,E_BuffPositive.正面, buffDuration, dot, target.battleDamageHandler.DotHandler, triggerInterval, baseDotCount);
        //为目标注册BUFF
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle,buff);
        }
    }
}

/// <summary>
///（7）对随机敌人造成枪弱点伤害，重放（3~5）次（区间内随机）（初始技能）
/// </summary>
[SkillID(7)]
public class Skill_7 : SkillBase{
    #region 技能基础Info
    float baseAttackValue = -1;
    float baseAttackRate = 0.2f;
    //决定伤害类型
    E_WeaknessType weakness = E_WeaknessType.枪;
    Attack_Skill atk_iSkill;
    #endregion
    WaitForSeconds excuteDelay;
    public Skill_7(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能7--$$$$$");
        atk_iSkill = new Attack_Skill();
        excuteDelay = new WaitForSeconds(0.2f);
    }

    public override void SkillEffect_Base(IBattlable target){
        int excuteTime = Random.Range(3, 6);
        SelectRandomTargetExcute(excuteTime);
    }

    /// <summary>
    /// 重放随机区间变为（5~7）/（7~9）/（9~12）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        int excuteTime = Random.Range(3+henceTime*2, 6+henceTime*2);
        SelectRandomTargetExcute(excuteTime);
    }

    IEnumerator DoHurt(int excuteTime, IBattlable target){
        for (int i = 0; i < excuteTime; i++){
            atk_iSkill.Excute(self, target);
            yield return excuteDelay;
        }
    }
    void SelectRandomTargetExcute(int excuteTime){
        E_Camp ememyCamp = (self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方);
        var _target = BattleTargetSelector.GetRandomNAliveTargets(ememyCamp, 1)[0];
        atk_iSkill.SetAttackState(weakness, baseAttackValue, baseAttackRate);
        Debug.Log($"[Skill 7]{self.Camp}发动技能7-[重放-枪弱点攻击],重放次数{excuteTime}");
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DoHurt(excuteTime, _target));
    }
}

/// <summary>
///（8）获得【物攻+1】（物攻+20%），持续20S（初始技能）
/// </summary>
[SkillID(8)]
public class Skill_8 : SkillBase{
    int level = 1;
    public Skill_8(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能8--$$$$$");
    }
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.2f;

    BuffBase buff;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    /// 【物防+1】变为【物防+2】（物防+40%）/【物防+3】（物防+60%）/【物防+4】（物防+80%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
 
        CreateBuff(level+henceTime);
    }
    float adjustValue;
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;
        
        Debug.Log($"[Skill 5]{self.Camp}发动技能5-[物攻+N（{level}）-BUFF]");
        if (buff != null) {
          Debug.Log("[Skill 5]:替换掉上一个的增益效果，刷新计时");
          battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack, -adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.物攻加N, E_BuffPositive.正面, buff_duration, battleControl, E_CharacterPropertyType.Phy_Attack, adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}

/// <summary>
/// [已冻结]（9）回复 30% 最大生命值，每次击破后 ATB-2/充能时间减少40S，释放后重置（初始技能）
/// </summary>
[SkillID(9)]
public class Skill_9 : SkillBase{
    ModelAdjust_Skill mdl_iSkill;
    float healRate = 0.3f;
    public Skill_9(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能9--$$$$$");
        mdl_iSkill = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        //回复 30% 最大生命值
        //Heal(healRate);
        Debug.Log($"Skill_9:已冻结");
    }
    //回复生命值变为50%/70%/90%
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        //Heal(healRate+henceTime*0.2f);
        Debug.Log($"Skill_9:已冻结");
    }
    void Heal(float healRate) {
        mdl_iSkill.SetModelState(E_BattleModelType.SP, self.battleDamageHandler.GetMaxHealth(), healRate);
        mdl_iSkill.Excute(self, self);
        Debug.Log($"[Skill 9]{self.Camp}发动技能9-[回血]:{healRate}*最大生命值");
    }
}

/// <summary>
/// （10）获得【战意】状态（造成伤害增加30%）持续20S，减少20%当前生命值（初始技能）
/// </summary>
[SkillID(10)]
public class Skill_10 : SkillBase{
    public Skill_10(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能10--$$$$$");
    }
    float duff_dur = 3; 
    float boomerRate = 0.3f;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(duff_dur,boomerRate);
    }
    //持续时间变为25S/35S/45S
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(duff_dur+10*henceTime,boomerRate);
    }
    void CreateBuff(float buffDuration, float boomerRate){
        Debug.Log($"[Skill 10]{self.Camp}发动技能10-[战意-BUFF]");
        var buffHandle = self.battleDamageHandler.BuffHandler;
        BuffBase buff = new Buff_DamageBoomer(E_BuffType.战意_正面, E_BuffPositive.正面, buffDuration,boomerRate);
        //为目标注册BUFF
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
        
    }
}

/// <summary>
/// （11）获得【大魔法化】状态（魔法类型攻击技能将会额外释放1次）持续20S（初始技能）
/// </summary>
[SkillID(11)]
public class Skill_11 : SkillBase{
    Skill_BaseAttack baseAttackSkill;
    public Skill_11(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能11--$$$$$");
        baseAttackSkill = new Skill_BaseAttack(_skillTargetType);
    }
    public override void SkillEffect_Base(IBattlable target){

    }
}
/// <summary>
///（12）对全体敌人造成冰/雷/火弱点的伤害（中耗）（方向键切换）（初始技能）
/// </summary>
[SkillID(12)]
public class Skill_12 : SkillBase{
    public Skill_12(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能12--$$$$$");
    }

    public override void SkillEffect_Base(IBattlable target){

    }
}
/// <summary>
///（13）获得【魔攻+1】（魔攻+20%），持续20S（初始技能）
/// </summary>
[SkillID(13)]
public class Skill_13 : SkillBase{
    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.2f;
    BuffBase buff;
    public Skill_13(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能13--$$$$$");
    }
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    /// 【魔攻+1】变为【魔攻+2】（魔攻+40%）/【魔攻+3】（魔攻+60%）/【魔攻+4】（魔攻+80%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    float adjustValue;
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;

        Debug.Log($"[Skill 13]{self.Camp}发动技能5-[魔攻+N（{level}）-BUFF]");
        if (buff != null){
            Debug.Log("[Skill 5]:替换掉上一个的增益效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack, -adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.魔攻加N, E_BuffPositive.正面, buff_duration, battleControl, E_CharacterPropertyType.Mag_Attack, adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
/// <summary>
/// （14）  获得【物防+1】（物防+20%），持续20S
/// </summary>
[SkillID(14)]
public class Skill_14 : SkillBase{
    public Skill_14(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能14--$$$$$");
    }
    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.2f;
    BuffBase buff;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    /// 【物防+1】变为【物防+2】（物防+40%）/【物防+3】（物防+60%）/【物防+4】（物防+80%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    float adjustValue;
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;

        Debug.Log($"[Skill 14]{self.Camp}发动技能14-[物防+N（{level}）-BUFF]");
        if (buff != null){
            Debug.Log("[Skill 14]:替换掉上一个的增益效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance, -adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.物防加N, E_BuffPositive.正面, buff_duration, battleControl, E_CharacterPropertyType.Phy_Resistance, adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
/// <summary>
///（15）获得【魔防+1】（魔防+20%），持续20S
/// </summary>
[SkillID(15)]
public class Skill_15 : SkillBase{
    public Skill_15(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能15--$$$$$");
    }
    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.2f;
    /// <summary>
    /// 增益BUFF
    /// </summary>
    BuffBase buff;
    /// <summary>
    /// 调整数值（+）
    /// </summary>
    float adjustValue;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    /// 【魔防+1】变为【魔防+2】（魔防+40%）/【魔防+3】（魔防+60%）/【魔防+4】（魔防+80%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;

        Debug.Log($"[Skill 15]{self.Camp}发动技能15-[魔防+N（{level}）-BUFF]");
        if (buff != null){
            Debug.Log("[Skill 15]:替换掉上一个的增益效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance, -adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.魔防加N, E_BuffPositive.正面, buff_duration, battleControl, E_CharacterPropertyType.Mag_Resistance, adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
/// <summary>
///（16）对1名敌人施加【物攻-1】（物攻-15%），持续20S
/// </summary>
[SkillID(16)]
public class Skill_16 : SkillBase{
    public Skill_16(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能16--$$$$$");
    }
    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.15f;
    /// <summary>
    /// 增益BUFF
    /// </summary>
    BuffBase buff;
    /// <summary>
    /// 调整数值（+）
    /// </summary>
    float adjustValue;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    /// 【物攻-1】变为【物攻-2】（物攻-30%）/【物攻-3】（物攻-45%）/【物攻-4】（物攻-60%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;

        Debug.Log($"[Skill 16]{self.Camp}发动技能16-[魔防-N（{level}）-BUFF]");
        if (buff != null){
            Debug.Log("[Skill 16]:替换掉上一个的增益效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack, adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.物攻减N, E_BuffPositive.负面, buff_duration, battleControl, E_CharacterPropertyType.Phy_Attack, -adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
/// <summary>
///（17）对1名敌人施加【魔攻-1】（魔攻-15%），持续20S
/// </summary>
[SkillID(17)]
public class Skill_17 : SkillBase{
    public Skill_17(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能17--$$$$$");
    }

    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.15f;
    /// <summary>
    /// 增益BUFF
    /// </summary>
    BuffBase buff;
    /// <summary>
    /// 调整数值（+）
    /// </summary>
    float adjustValue;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    ///【魔攻-1】变为【魔攻-2】（魔攻-30%）/【魔攻-3】（魔攻-45%）/【魔攻-4】（魔攻-60%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;

        Debug.Log($"[Skill 17]{self.Camp}发动技能17-[魔攻-N（{level}）-BUFF]");
        if (buff != null)
        {
            Debug.Log("[Skill 17]:替换掉上一个的增益效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack, adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.魔攻减N, E_BuffPositive.负面, buff_duration, battleControl, E_CharacterPropertyType.Mag_Attack, -adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
/// <summary>
///（18）对1名敌人施加【物防-1】（物防-15%），持续20S
/// </summary>
[SkillID(18)]
public class Skill_18 : SkillBase{
    public Skill_18(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能18--$$$$$");
    }

    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.15f;
    /// <summary>
    /// 增益BUFF
    /// </summary>
    BuffBase buff;
    /// <summary>
    /// 调整数值（+）
    /// </summary>
    float adjustValue;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    ///【物防-1】变为【物防-2】（物防-30%）/【物防-3】（物防-45%）/【物防-4】（物防-60%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;

        Debug.Log($"[Skill 18]{self.Camp}发动技能18-[物防-N（{level}）-BUFF]");
        if (buff != null){
            Debug.Log("[Skill 18]:替换掉上一个的增益效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance, adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.物防减N, E_BuffPositive.负面, buff_duration, battleControl, E_CharacterPropertyType.Phy_Resistance, -adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
/// <summary>
///（19）对1名敌人施加【魔防-1】（魔防-15%），持续20S
/// </summary>
[SkillID(19)]
public class Skill_19 : SkillBase{
    public Skill_19(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能19--$$$$$");
    }

    int level = 1;
    //20sBUFF持续时间
    float buff_duration = 20;
    //提升比例
    float baseRiseRate = 0.15f;
    /// <summary>
    /// 增益BUFF
    /// </summary>
    BuffBase buff;
    /// <summary>
    /// 调整数值（+）
    /// </summary>
    float adjustValue;
    public override void SkillEffect_Base(IBattlable target){
        CreateBuff(level);
    }
    /// <summary>
    ///【魔防-1】变为【魔防-2】（魔防-30%）/【魔防-3】（魔防-45%）/【魔防-4】（魔防-60%）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        CreateBuff(level + henceTime);
    }
    void CreateBuff(int level){
        BattleBuffHandler buffHandle = self.battleDamageHandler.BuffHandler;
        Battle_Controller battleControl = self.battleDamageHandler.BattleController;

        Debug.Log($"[Skill 19]{self.Camp}发动技能19-[魔防-N（{level}）-BUFF]");
        if (buff != null)
        {
            Debug.Log("[Skill 19]:替换掉上一个的增益效果，刷新计时");
            battleControl.AdjustCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance, adjustValue);
        }
        adjustValue = battleControl.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance) * baseRiseRate * level;
        buff = new Buff_AdjustProperty(E_BuffType.魔防减N, E_BuffPositive.负面, buff_duration, battleControl, E_CharacterPropertyType.Mag_Resistance, -adjustValue);
        //为目标注册BUFF        
        EventCenter.EventTrigger(E_EventType.Battle_RegisteBUFF, buffHandle, buff);
    }
}
