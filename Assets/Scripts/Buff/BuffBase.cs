using System.Collections.Generic;
using UnityEngine;

public enum E_Dot{
    冻结,灼烧,感电,
}

public enum E_BattleBuff
{
    炽焰连锁_s4,
    雷电风暴_s5,
    冰天雪地_s6,
    战意_s10,
    大魔法化_s11,
    属性增强_通用,
    属性减益_通用,
}
public enum E_BuffPositive
{
    正面, 负面
}

public enum E_BuffName
{
    炽焰连锁,
    迅雷连锁_正面,
    暴雪连锁_正面,
    雷电风暴_正面,
    火焰风暴_正面,
    冰雪风暴_正面,
    冰雪场地_正面,
    火焰场地_正面,
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
public class BuffBase
{

    protected E_BuffName buffName;
    public E_BuffName Buff_Name => buffName;

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

    public BuffBase(E_BuffName _buffName, E_BuffPositive _BuffPositive, float _buff_dura)
    {
        buff_attr = _BuffPositive;
        buff_dura = _buff_dura;
        buffName = _buffName;
        //triggerCondition= _triggerCondition;
    }
    public virtual void BuffTrigger() { }
    public virtual void BuffUpdate() { }
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
    public Buff_AdditiveAttack(E_BuffName _buffName, E_BuffPositive e_BuffPositive, float _buff_dura,
                   SkillBase skillBase, E_WeaknessType weaknessType, float damageRate) :
                   base(_buffName, e_BuffPositive, _buff_dura){
        this.skillBase = skillBase;
        this.weaknessType = weaknessType;
        this.damageRate = damageRate;
    }
    public override void BuffTrigger(){
        BufferLogicBucket.AdditiveWeaknessAttack(skillBase.self, skillBase.targets, weaknessType, damageRate);
        base.BuffTrigger();
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
    public Buff_AutoDamage(E_BuffName _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        IBattlable _self, E_Camp _myEnemy, E_WeaknessType weaknessType, float damageRate,
        float triggerInterval, bool useRandomTarget = true)
        : base(_buffName, _BuffPositive, _buff_dura)
    {
        this.myEnemy = _myEnemy;
        this.self = _self;
        this.weaknessType = weaknessType;
        this.damageRate = damageRate;
        this.useRandomTarget = useRandomTarget;
        timer = _buff_dura;
    }
    public override void BuffTrigger()
    {
        List<IBattlable> target;
        if (useRandomTarget)
            target = BattleTargetSelector.GetRandomNAliveTargets(myEnemy, 1);
        else
            target = BattleTargetSelector.GetValidTargets(self, E_SkillTargetType.对单体);
        BufferLogicBucket.AdditiveWeaknessAttack(self, target, weaknessType, damageRate);
        base.BuffTrigger();
    }
    public override void BuffUpdate()
    {
        base.BuffUpdate();
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            BuffTrigger();
            UnityEngine.Debug.Log("间隔结算BUFF--触发一次");
            timer = buff_dura;
        }
    }
}

public class Buffer_AssignDot : BuffBase
{
    /// <summary>
    /// Dot层数
    /// </summary>
    int dot_count;

    /// <summary>
    /// Dot类型
    /// </summary>
    E_Dot dot_type;

    /// <summary>
    /// 目标身上的Dot印记
    /// </summary>
    List<DotBase> targetDots;
    public Buffer_AssignDot(E_BuffName _buffName, E_BuffPositive _BuffPositive, float _buff_dura, E_Dot dot_type, List<DotBase> dots)
        : base(_buffName, _BuffPositive, _buff_dura)
    {
        dot_count = 0;
        this.dot_type = dot_type;
    }

    public override void BuffTrigger()
    {
        base.BuffTrigger();
    }

    public override void BuffUpdate()
    {
        base.BuffUpdate();
    }
}



