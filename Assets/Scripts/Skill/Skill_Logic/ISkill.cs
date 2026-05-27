using UnityEngine;
using Core;

public interface ISkill
{
    /// <summary>
    /// 一次技能基础效果
    /// </summary>
    /// <param name="self">释放者</param>
    /// <param name="target">承受者</param>
    public void Excute(IBattlable self, IBattlable target);
}

/// <summary>
/// 一次攻击效果组件（对target的Model属性调整）
/// </summary>
public class Attack_Skill : ISkill
{
    /// <summary>
    /// 造成的伤害属性类型
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

    /// <summary>
    /// 弱点攻击的额外倍率
    /// </summary>
    float weakMulti = 2;
    public Attack_Skill(){}

    /// <summary>
    /// 攻击执行前使用来设置本次攻击的状态
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

        float value = self.battleDamageHandler.DoDamage(damageType, baseAttackRate * baseAttackValue);

        //检查攻击弱点状态（如是->结算伤害x2 + 削盾1点）
        if (target.GetWeakAttack(weaknessType))
        {
            value *= weakMulti;
            target.battleDamageHandler.DoModelValue(E_BattleModelType.ShieldPoints,-1);

            Debug.Log($"{self.Camp}对{target.battleDamageHandler.name}发动一次[(弱点)]攻击:{baseAttackRate}*{baseAttackValue}*{weakMulti}*玩家攻击力=[税前伤害]{value}");
        }
        else
        {
            Debug.Log($"{self.Camp}对{target.battleDamageHandler.name}发动一次攻击:{baseAttackRate}*{baseAttackValue}*玩家攻击力=[税前伤害]{value}");
        }
        target.battleDamageHandler.GetDamage(damageType, value);
    }
}


/// <summary>
/// 一次Model效果组件（直接对target的Model属性调整）
/// </summary>
public class ModelAdjust_Skill : ISkill
{
    /// <summary>
    /// 调整的基础数值
    /// </summary>
    float baseAdjValue;

    /// <summary>
    /// 要调整的模型数据类型
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
        self.battleDamageHandler.DoModelValue(modelType, value);
        Debug.Log($"{self.Camp}对{target.battleDamageHandler.name}发动一次Model调整[{modelType}]：{value}");
    }
}

/// <summary>
/// 一次Property效果组件（直接对target的Property属性调整）
/// </summary>
public class PropertyAdjust_Skill : ISkill{
    /// <summary>
    /// 调整的基础数值
    /// </summary>
    float baseAdjValue;

    /// <summary>
    /// 要调整的模型数据类型
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
        self.battleDamageHandler.DoPropertyValue(propertyType, value);
        Debug.Log($"{self.Camp}对{target.battleDamageHandler.name}发动一次Property调整[{propertyType}]：{value}");
    }
}

