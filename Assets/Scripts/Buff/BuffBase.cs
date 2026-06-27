using System.Collections.Generic;
using UnityEngine;
public enum E_Dot{
    冻结,燃烧,感电,
}
public enum E_BuffPositive{
    正面, 负面
}
public enum E_BuffType
{
    炽焰连锁,
    迅雷之影_正面,
    冰雪风暴_正面,
    雷电风暴,
    烈焰风暴_正面,
    冰霜风暴_正面,
    冰雪场地,
    寒冰场地_正面,
    大魔法化_正面,

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

    治疗强化加N,
    治疗强化减N,

    雷电场地,
    战意_正面,
    圣盾_正面,
    魔法反射_正面,
    元素反射_正面,
    防御_正面,
    魔法防御_正面,
    魔双_正面,
    剑晕_正面,
    冰冻之触_正面,
    燃烧之触_正面,
    中毒之枪_正面,
    脆弱_负面,
    退化_负面,
    超大魔法化_正面,
    魔力收束_正面,
    无双_正面,
    晕眩_负面,
    火焰场地_正面,
    灼伤之剑_正面,
    冻结之弓_正面,
    感电之枪_正面,
}
public class BuffBase{
    protected E_BuffType buffType;
    public E_BuffType Buff_Type => buffType;
    protected E_BuffPositive buff_attr;
    /// <summary>
    /// BUFF的正负属性
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
        DebugManager.Log(EDebugCategory.BattleBuff, $"获得BUFF:{_buffName},BUFF属性：{_BuffPositive},BUFF时间：{_buff_dura}");
    }
    public virtual void OnBuffTrigger() { }
    public virtual void OnBuffUpdate() { }
    public virtual void OnBuffRemove() { }
}

/// <summary>
/// Buff_附加属性攻击
/// </summary>
public class Buff_AdditiveAttack : BuffBase
{
    SkillBase skillBase;
    E_WeaknessType weaknessType;
    float damageRate;
    //直接获取当前攻击的所有目标
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
    /// <summary>
    /// 对指定目标触发附加攻击（用于物理攻击事件携带当前目标）
    /// </summary>
    public void TriggerOnTarget(IBattlable target){
        BufferLogicBucket.AdditiveWeaknessAttack(skillBase.self, new List<IBattlable> { target }, weaknessType, damageRate);
    }
}

/// <summary>
/// Buff_对目标造成特定属性伤害
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
            target = BattleTargetSelector.GetValidTargets(self, E_SkillTargetType_Auto.对单体);
        BufferLogicBucket.AdditiveWeaknessAttack(self, target, weaknessType, damageRate);
        base.OnBuffTrigger();
    }
    public override void OnBuffUpdate(){
        base.OnBuffUpdate();
        if (timer >= 0)
            timer -= Time.deltaTime;
        else{
            DebugManager.Log(EDebugCategory.BattleBuff, "Buff_AutoDamage--持续效果");
            OnBuffTrigger();
            timer = triggerInterval ;
        }
    }
}

/// <summary>
/// 为目标（敌我方）附加特定Dot层数
/// </summary>
public class Buffer_AssignDot : BuffBase{
    /// <summary>
    /// Dot模板
    /// </summary>
    DotBase dot;
    int addPerTrigger;

    float timer;

    float triggerInterval;
    /// <summary>
    /// 目标身上的Dot处理器
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
            DebugManager.Log(EDebugCategory.BattleBuff, "Buffer_AssignDot--持续效果");
            OnBuffTrigger();
            timer= triggerInterval;
        }
    }
}
/// <summary>
/// BUFF_调整角色属性
/// </summary>
public class Buff_AdjustProperty : BuffBase{
    public Buff_AdjustProperty(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        Battle_Controller battleControl,E_CharacterPropertyType PropertyType,float adjustValue
        ) : base(_buffName, _BuffPositive, _buff_dura){
        this.battle_Controller = battleControl;
        this.propertyType = PropertyType;
        timer = _buff_dura;
        battle_Controller.AdjustCharacterPropertyValue(propertyType, adjustValue);
        DebugManager.Log(EDebugCategory.BattleBuff, $"调整BUFF 生效：{propertyType}变化{adjustValue}");
    }
    float adjustValue;
    float timer;
    Battle_Controller battle_Controller;
    E_CharacterPropertyType propertyType;
    public override void OnBuffTrigger(){
        base.OnBuffTrigger();}

    void ReSetProperty() {
        battle_Controller.AdjustCharacterPropertyValue(propertyType,-adjustValue);
        DebugManager.Log(EDebugCategory.BattleBuff, $"调整BUFF 失效：{propertyType}变化{-adjustValue}");}

    public override void OnBuffRemove() { ReSetProperty(); }

    public override void OnBuffUpdate(){
        base.OnBuffUpdate();
        if (timer>=0)
            timer-=Time.deltaTime;
        else
            ReSetProperty();
    }
}

/// <summary>
/// Buff_提供额外伤害倍率
/// </summary>
public class Buff_DamageBoomer : BuffBase{
    public Buff_DamageBoomer(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        float boomerRate) : base(_buffName, _BuffPositive, _buff_dura){
        this.boomerRate = boomerRate;
    }
    float boomerRate;
    public float BoomerRate => boomerRate;
}

/// <summary>
/// Buff_技能重放：当特定伤害类型的攻击技能释放时，额外重放N次
/// 可复用组件，不依赖SkillBase，通过事件驱动
/// </summary>
public class Buff_SkillRecast : BuffBase{
    /// <summary>
    /// 重放次数
    /// </summary>
    int recastCount;

    /// <summary>
    /// 技能版本
    /// </summary>
    E_SkillLevel skillLevel;
    /// <summary>
    /// 强化次数
    /// </summary>
    int henctime;

    bool _isRecasting;
    SkillBase skill;
    public void SetRecastContext(SkillBase _skill, E_SkillLevel _skillLevel, int _henctime){
        skill = _skill; skillLevel = _skillLevel; henctime = _henctime;
    }
    public Buff_SkillRecast(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        int _recastCount = 1) : base(_buffName, _BuffPositive, _buff_dura){
        recastCount = _recastCount;
    
    }

    public override void OnBuffTrigger(){
        base.OnBuffTrigger();
        DoRecast();
    }
    public void DoRecast()
    {
        if (_isRecasting) return;
        _isRecasting = true;
        for (int i = 0; i < recastCount; i++){
            DebugManager.Log(EDebugCategory.BattleBuff, $"[大魔法化]重放技能:{skill.GetType().Name},次数:{i + 1}/{recastCount}");
            skill.SkillExcute(skillLevel,henctime);
        }
        _isRecasting = false;
    }
}

/// <summary>
/// Buff_脆弱：降低双抗，模拟受伤增加
/// </summary>
public class Buff_Vulnerable : BuffBase{
    Battle_Controller controller;
    float phyReduce, magReduce;
    float timer;

    public Buff_Vulnerable(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        Battle_Controller _controller, float _phyReduce, float _magReduce)
        : base(_buffName, _BuffPositive, _buff_dura){
        controller = _controller;
        phyReduce = _phyReduce;
        magReduce = _magReduce;
        timer = _buff_dura;
        controller.AdjustCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance, -phyReduce);
        controller.AdjustCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance, -magReduce);
        DebugManager.Log(EDebugCategory.BattleBuff, $"脆弱BUFF生效：物抗-{phyReduce}，魔抗-{magReduce}");
    }

    void Revert(){
        controller.AdjustCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance, phyReduce);
        controller.AdjustCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance, magReduce);
        DebugManager.Log(EDebugCategory.BattleBuff, $"脆弱BUFF失效：物抗+{phyReduce}，魔抗+{magReduce}");
    }
    public override void OnBuffRemove() { Revert(); }
    public override void OnBuffUpdate(){
        base.OnBuffUpdate();
        if (timer >= 0)
            timer -= Time.deltaTime;
        else Revert();
    }
}
/// <summary>
/// Buff_附魔：特定弱点攻击附加Dot层数
/// </summary>
public class Buff_DotOnAttack : BuffBase{
    E_WeaknessType triggerWeakness;
    E_Dot dotType;
    IBattlable self;
    public Buff_DotOnAttack(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        E_WeaknessType _triggerWeakness, E_Dot _dotType, IBattlable _self)
        : base(_buffName, _BuffPositive, _buff_dura){
        triggerWeakness = _triggerWeakness;
        dotType = _dotType;
        self = _self;
    }
    public override void OnBuffTrigger(){
        base.OnBuffTrigger();
    }
    public void TryApplyDot(E_WeaknessType attackWeakness, IBattlable target){
        if (attackWeakness == triggerWeakness){
            DotBase dot = CreateDot(target);
            if (dot != null)
                EventCenter.EventTrigger(E_EventType.Battle_RegisterDot, target.battleDamageHandler.DotHandler, dot, 1);
        }
    }
    DotBase CreateDot(IBattlable target){
        switch (dotType){
            case E_Dot.燃烧: return new Dot_Burn(E_Dot.燃烧, target, 1);
            case E_Dot.冻结: return new Dot_Freeze(E_Dot.冻结, target, 1);
            case E_Dot.感电: return new Dot_Shock(E_Dot.感电, target, 1);
        }
        return null;
    }
}
public class Buff_FieldDot : BuffBase {
    E_Dot dotType;
    E_Camp enemyCamp;
    IBattlable self;
    float timer;
    float triggerInterval;
    int dotLayers;

    public Buff_FieldDot(E_BuffType _buffName, E_BuffPositive _BuffPositive, float _buff_dura,
        E_Dot _dotType, IBattlable _self, float _triggerInterval, int _dotLayers = 1)
        : base(_buffName, _BuffPositive, _buff_dura) {
        dotType = _dotType;
        self = _self;
        triggerInterval = _triggerInterval;
        dotLayers = _dotLayers;
        enemyCamp = self.Camp == E_Camp.玩家方 ? E_Camp.敌方 : E_Camp.玩家方;
        timer = 0;
    }

    public override void OnBuffUpdate() {
        base.OnBuffUpdate();
        if (timer >= 0)
            timer -= Time.deltaTime;
        else {
            var enemies = BattleTargetSelector.GetAllAliveTargets(enemyCamp);
            foreach (var enemy in enemies) {
                DotBase dot = CreateDot(enemy);
                if (dot != null)
                    EventCenter.EventTrigger(E_EventType.Battle_RegisterDot,
                        enemy.battleDamageHandler.DotHandler, dot, dotLayers);
            }
            timer = triggerInterval;
        }
    }

    DotBase CreateDot(IBattlable target) {
        switch (dotType) {
            case E_Dot.燃烧: return new Dot_Burn(E_Dot.燃烧, target, dotLayers);
            case E_Dot.冻结: return new Dot_Freeze(E_Dot.冻结, target, dotLayers);
            case E_Dot.感电: return new Dot_Shock(E_Dot.感电, target, dotLayers);
        }
        return null;
    }
}
