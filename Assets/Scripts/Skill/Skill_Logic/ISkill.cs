using UnityEngine;
using Core;

public interface ISkill
{
    /// <summary>
    /// 一次技能的基础效果
    /// </summary>
    /// <param name="self">释放者</param>
    /// <param name="target">目标</param>
    public void Excute(IBattlable self, IBattlable target);
}

/// <summary>
/// 一次攻击效果：直接对target造成伤害
/// </summary>
public class Attack_Skill : ISkill
{
    /// <summary>
    /// 造成的伤害的弱点类型
    /// </summary>
    E_WeaknessType weaknessType;
    /// <summary>
    /// 技能的基础伤害值
    /// </summary>
    float baseAttackValue = 0;
    /// <summary>
    /// 技能的基础伤害倍率
    /// </summary>
    float baseAttackRate = 1f;

    public Attack_Skill(){}

    /// <summary>
    /// 在技能执行前使用，设置本次攻击的状态
    /// </summary>
    /// <param name="_WeaknessType"></param>
    /// <param name="_baseAttackValue"></param>
    /// <param name="_baseAttackRate"></param>
    public void SetAttackState(E_WeaknessType _WeaknessType,
                        float _baseAttackValue,
                        float _baseAttackRate)
    {
        weaknessType = _WeaknessType;
        baseAttackValue = _baseAttackValue;
        baseAttackRate = _baseAttackRate;
    }
    public void Excute(IBattlable self, IBattlable target)
    {
        E_Skill_DamageType damageType = DamageTypeChecker.GetDamageType(weaknessType);

        if (damageType == E_Skill_DamageType.物理)
            EventCenter.EventTrigger(E_EventType.Do_PhyAttack, self.battleDamageHandler.BuffHandler);

        EventCenter.EventTrigger(E_EventType.Battle_ElementalAttack, self.battleDamageHandler.BuffHandler, weaknessType, target);
        Skill_41.RecordWeakness(weaknessType);

        float value = self.battleDamageHandler.DoDamage(damageType, baseAttackRate * baseAttackValue);

        //通过WeaknessHandler处理弱点判定和伤害倍率+破盾逻辑
        float weakMulti = target.battleDamageHandler.WeaknessHandler.ProcessWeaknessHit(weaknessType);
        value *= weakMulti;

        // 暴击判定
        float critRate = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.CritRate);
        bool isCrit = critRate > 0f && Random.value < critRate;
        if (isCrit)
        {
            float critDamage = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.CritDamage);
            value *= critDamage;
        }

        if (weakMulti > 1f)
            DebugManager.Log(EDebugCategory.SkillExecution,$"{self.Camp}对{target.battleDamageHandler.name}造成一次[(弱点)]攻击:{baseAttackRate}*{baseAttackValue}*{weakMulti}*当前倍率=[{weaknessType}-税前伤害]{value}{(isCrit ? "(暴击)" : "")}");
        else
            DebugManager.Log(EDebugCategory.SkillExecution,$"{self.Camp}对{target.battleDamageHandler.name}造成一次攻击:{baseAttackRate}*{baseAttackValue}*当前倍率=[{weaknessType}-税前伤害]{value}{(isCrit ? "(暴击)" : "")}");
        target.battleDamageHandler.GetDamage(damageType, value);
    }
}


/// <summary>
/// 一次Model效果：直接对target的Model属性进行调整
/// </summary>
public class ModelAdjust_Skill : ISkill
{
    /// <summary>
    /// 调整的基础值
    /// </summary>
    float baseAdjValue;

    /// <summary>
    /// 要调整的模型属性类型
    /// </summary>
    E_BattleModelType modelType;

    float skillRate;

    public void SetModelState(E_BattleModelType _modelType, float baseValue, float _multi_value){
        skillRate = _multi_value;
        modelType = _modelType;
        baseAdjValue = baseValue;
    }

    public void Excute(IBattlable self, IBattlable target){
        float value = baseAdjValue * skillRate;
        target.battleDamageHandler.DoModelValue(modelType, value);
        DebugManager.Log(EDebugCategory.SkillExecution,$"{self.Camp}对{target.battleDamageHandler.name}造成一次Model调整[{modelType}]：{value}");
    }
}

/// <summary>
/// 一次Property效果：直接对target的Property属性进行调整
/// </summary>
public class PropertyAdjust_Skill : ISkill{
    /// <summary>
    /// 调整的基础值
    /// </summary>
    float baseAdjValue;

    /// <summary>
    /// 要调整的属性类型
    /// </summary>
    E_CharacterPropertyType propertyType;

    float skillRate;

    public void SetPropertyState(E_CharacterPropertyType _propertyType, float _baseValue, float _multi_value)
    {
        skillRate = _multi_value;
        propertyType = _propertyType;
        baseAdjValue = _baseValue;
    }

    public void Excute(IBattlable self, IBattlable target)
    {
        int value =(int)(baseAdjValue * skillRate);
        target.battleDamageHandler.DoPropertyValue(propertyType, value);
        DebugManager.Log(EDebugCategory.SkillExecution,$"{self.Camp}对{target.battleDamageHandler.name}造成一次Property调整[{propertyType}]：{value}");
    }
}
