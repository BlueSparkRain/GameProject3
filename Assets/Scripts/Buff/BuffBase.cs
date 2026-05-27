using System.Collections.Generic;
using UnityEngine;

public enum E_Dot{
    冻结,灼烧,感电,
}

public enum E_BuffPositive
{
    正面, 负面
}

public enum E_BuffType
{
    炽焰连锁,
    迅雷连锁_正面,
    暴雪连锁_正面,
    雷电风暴,
    火焰风暴_正面,
    冰雪风暴_正面,
    冰雪场地,
    火焰场地_正面,
    
    物攻加N,
    物攻减N,

    魔攻加N,
    魔攻减N,

    物防加N,
    物防减N,

    魔防加N,
    魔防减N,

    生命偷取加N,
    生命偷取减N,
    
    治疗效果加N,
    治疗效果减N,

    雷电场地,
    战意_正面,
    退化_负面,
    大魔法化_正面,
    超大魔法化_正面,
    易损_负面,
    魔力收束_正面,
    无双_正面,
    晕眩_负面,
    灼伤之剑_正面,
    冻结之弓_正面,
    感电之枪_正面,
}

//状态效果 维度：
//每次结算特定类型伤害（段数影响）  附带特定弱点伤害
//为目标添加Dot标记
//特定/类型 技能重复释放
//Property暂时修正
//【static】造成 最终伤害 修正
//间隔/直接 修正Model
//特定（力竭）状态下 受到最终伤害修正
//修改技能的 释放目标
//【static】获取敌方状态（正负面标记）
//【static】为目标添加弱点
//外部能够获取到一个buff的正负面属性


//角色持有一个Buff状态Tag
//当获得对应的状态就会实例出一种Buff，并开启计时
public class BuffBase{
    protected E_BuffType buffType;
    public E_BuffType Buff_Type => buffType;
    protected E_BuffPositive buff_attr;
    /// <summary>
    /// BUFF的正负面属性
    /// </summary>
    public E_BuffPositive Buff_Attr => buff_attr;
    protected float buff_dura;
    /// <summary>
    /// BUFF的持续时间
    /// </summary>
    public float Buff_Dura => buff_dura;

    public BuffBase(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura){
        buff_attr = _BuffPositive;
        buff_dura = _buff_dura;
        buffType = _buffName;
        Debug.Log($"获得BUFF:{_buffName},BUFF属性：{_BuffPositive},BUFF时长：{_buff_dura}");
    }
    public virtual void OnBuffTrigger() { }
    public virtual void OnBuffUpdate() { }
}

/// <summary>
/// Buff_附带弱点攻击
/// </summary>
public class Buff_AdditiveAttack : BuffBase
{
    SkillBase skillBase;
    E_WeaknessType weaknessType;
    float damageRate;
    //直接获取到当前对象的所有
    public Buff_AdditiveAttack(E_BuffType _buffName, E_BuffPositive e_BuffPositive, float _buff_dura,
                   SkillBase skillBase, E_WeaknessType weaknessType, float damageRate) :
                   base(_buffName, e_BuffPositive, _buff_dura){
        this.skillBase = skillBase;
        this.weaknessType = weaknessType;
        this.damageRate = damageRate;
    }
    public override void OnBuffTrigger(){
        BufferLogicBucket.AdditiveWeaknessAttack(skillBase.self, skillBase.targets, weaknessType, damageRate);
        base.OnBuffTrigger();
    }
}

/// <summary>
/// Buff_对目标间隔触发特定弱点伤害
/// </summary>
public class Buff_AutoDamage : BuffBase
{
    IBattlable self;
    E_Camp myEnemy;
    E_WeaknessType weaknessType;
    float damageRate;
    bool useRandomTarget = true;
    float timer;
    float triggerInterval;
    public Buff_AutoDamage(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        IBattlable _self, E_Camp _myEnemy, E_WeaknessType weaknessType, float damageRate,
        float triggerInterval, bool useRandomTarget = true)
        : base(_buffName, _BuffPositive, _buff_dura)
    {
        this.myEnemy = _myEnemy;
        this.self = _self;
        this.weaknessType = weaknessType;
        this.damageRate = damageRate;
        this.useRandomTarget = useRandomTarget;
        this.triggerInterval = triggerInterval;
        timer = triggerInterval;
    }
    public override void OnBuffTrigger(){
        List<IBattlable> target;
        if (useRandomTarget)
            target = BattleTargetSelector.GetRandomNAliveTargets(myEnemy, 1);
        else
            target = BattleTargetSelector.GetValidTargets(self, E_SkillTargetType.对单体);
        BufferLogicBucket.AdditiveWeaknessAttack(self, target, weaknessType, damageRate);
        base.OnBuffTrigger();
    }
    public override void OnBuffUpdate(){
        base.OnBuffUpdate();
        if (timer >= 0)
            timer -= Time.deltaTime;
        else{
            Debug.Log("Buff_AutoDamage--触发效果");
            OnBuffTrigger();
            timer = triggerInterval ;
        }
    }
}

/// <summary>
/// 为目标（间隔）调整特定Dot层数
/// </summary>
public class Buffer_AssignDot : BuffBase{
    /// <summary>
    /// Dot类型
    /// </summary>
    DotBase dot;
    int addPerTrigger;

    float timer;

    float triggerInterval;
    /// <summary>
    /// 目标身上的Dot控制器
    /// </summary>
    BattleDotHandler targetDot;
    public Buffer_AssignDot(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,DotBase _dot, BattleDotHandler _dotHandler, float _triggerInterval, int _addPerTrigger=1)
        : base(_buffName, _BuffPositive, _buff_dura){
        targetDot = _dotHandler;
        dot = _dot;
        addPerTrigger = _addPerTrigger;
        triggerInterval= _triggerInterval;
        timer= _triggerInterval;
    }

    public override void OnBuffTrigger(){
        base.OnBuffTrigger();
        EventCenter.EventTrigger(E_EventType.Battle_RegisterDot,targetDot,dot,addPerTrigger);
    }

    public override void OnBuffUpdate(){
        base.OnBuffUpdate();
        if (timer >= 0) 
            timer-= Time.deltaTime;
        else {
            Debug.Log("Buffer_AssignDot--触发效果");
            OnBuffTrigger();
            timer= triggerInterval;
        }
    }
}
/// <summary>
/// BUFF_调整属性增益
/// </summary>
public class Buff_AdjustProperty : BuffBase{
    public Buff_AdjustProperty(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        Battle_Controller battleControl,E_CharacterPropertyType PropertyType,float adjustValue
        ) : base(_buffName, _BuffPositive, _buff_dura){
        this.battle_Controller = battleControl;
        this.propertyType = PropertyType;
        timer = _buff_dura;
        battle_Controller.AdjustCharacterPropertyValue(propertyType, adjustValue);
        Debug.Log($"属性增益BUFF 生效：{propertyType}调整{adjustValue}");
    }
    float adjustValue;
    float timer;
    Battle_Controller battle_Controller;
    E_CharacterPropertyType propertyType;
    public override void OnBuffTrigger(){
        base.OnBuffTrigger();
    }
    void ReSetProperty() {
        battle_Controller.AdjustCharacterPropertyValue(propertyType,-adjustValue);
        Debug.Log($"属性增益BUFF 失效！：{propertyType}调整{-adjustValue}");
    }

    public override void OnBuffUpdate(){
        base.OnBuffUpdate();
        if (timer>=0) 
            timer-=Time.deltaTime;
        else 
            ReSetProperty();
    }
}

/// <summary>
/// Buff_提供造成伤害增幅
/// </summary>
public class Buff_DamageBoomer : BuffBase{
    public Buff_DamageBoomer(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        float boomerRate) : base(_buffName, _BuffPositive, _buff_dura){
        this.boomerRate = boomerRate;
    }
    float boomerRate;
    public float BoomerRate => boomerRate;
}

