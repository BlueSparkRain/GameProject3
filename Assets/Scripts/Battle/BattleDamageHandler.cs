using UnityEngine;

/// <summary>
/// 负责对局内角色（因【技能】而导致的）属性/模型 调整    
/// </summary>
public class BattleDamageHandler : MonoBehaviour
{
    BattleDamager_Magic magic_damageChecker;
    BattleDamager_Physic physic_damageChecker;
    Battle_Controller battleController;
    public Battle_Controller BattleController=>battleController;
    public void InitDataHandler(BattleMVCHandler mvcHandler)
    {
        battleController = mvcHandler.BattleController;
        magic_damageChecker = new BattleDamager_Magic(battleController);
        physic_damageChecker = new BattleDamager_Physic(battleController);
    }

    /// <summary>
    /// 输出 本角色造成的一次税前伤害
    /// </summary>
    /// <param name="damageType"></param>
    /// <param name="skillBaseDamage">技能的基础伤害</param>
    public float DoDamage(E_Skill_DamageType damageType, float skillBaseDamage)
    {
        switch (damageType)
        {
            case E_Skill_DamageType.物理:
                return physic_damageChecker.DoDamage(skillBaseDamage);
            case E_Skill_DamageType.魔法:
                return magic_damageChecker.DoDamage(skillBaseDamage);
        }
        return 0;
    }

    
    /// <summary>
    /// 检查本次伤害是否时弱点攻击（如果是->削减盾点x1）
    /// </summary>
    /// <param name="weaknessType"></param>
    public void CheckWeakness(E_WeaknessType weaknessType) { 
    
    } 

    /// <summary>
    /// 外部接口，调用字段battleController的属性调整方法
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="value"></param>
    public void DoPropertyValue(E_CharacterPropertyType propertyType, float value)
    {
        magic_damageChecker.DoPropertyValue(propertyType, value);
    }
    /// <summary>
    /// 外部接口，调用字段battleController的模型调整方法
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="value"></param>
    public void DoModelValue(E_BattleModelType modelType, float value)
    {
        magic_damageChecker.DoModelValue(modelType, value);
    }

    /// <summary>
    /// 输入 外部的税前伤害 输出 实际结算伤害 并调整 角色模型
    /// </summary>
    /// <param name="damageType"></param>
    /// <param name="damageValue"></param>
    public void GetDamage(E_Skill_DamageType damageType, float damageValue)
    {
        Debug.Log("擦撒德哈的是粉色粉色粉色"+damageValue);
        switch (damageType)
        {
            case E_Skill_DamageType.物理:
                float da = physic_damageChecker.GetDamage(damageValue);
                Debug.Log(name + "收到-----税后伤害:" + da);

                //检查本角色力竭状态（如是->结算伤害x2）

                //触发事件（结算物理伤害时，检测角色是否存在某些协同攻击BUFF）

                battleController.AdjustCharacterModelValue(E_BattleModelType.HP, da);
                break;
            case E_Skill_DamageType.魔法:
                float db = magic_damageChecker.GetDamage(damageValue);
                Debug.Log(name + "收到-----税后伤害:" + db);
                battleController.AdjustCharacterModelValue(E_BattleModelType.HP, db);
                break;
            default:
                break;
        }

    }




}
