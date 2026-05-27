using System.Collections.Generic;

public static class BufferLogicBucket{

    /// <summary>
    /// 像特定目标发动一次特定弱点的攻击
    /// </summary>
    /// <param name="self"></param>
    /// <param name="targets"></param>
    /// <param name="weaknessType"></param>
    /// <param name="damageRate"></param>
    public static void AdditiveWeaknessAttack(IBattlable self,List<IBattlable> targets,
                        E_WeaknessType  weaknessType, float damageRate) {
        Attack_Skill attack_Skill=new Attack_Skill();
        E_Skill_DamageType damageType=DamageTypeChecker.GetDamageType(weaknessType);
        float base_attack = 1;

        //if (damageType == E_Skill_DamageType.物理)
        //    base_attack = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack);
        //else 
        //    base_attack = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack);

        UnityEngine.Debug.Log($"附加伤害BUFF--触发效果！造成附加{base_attack* damageRate}点{weaknessType}[{damageType}]伤害");
        //设置攻击状态（内部结算时会自动考虑角色的攻击力）
        attack_Skill.SetAttackState(weaknessType,-base_attack, damageRate);
        //依次为目标结算
        foreach (IBattlable target in targets)
            attack_Skill.Excute(self, target);
    }

    /// <summary>
    /// 为目标增加一层特定目标类型的Buff
    /// </summary>
    public static void Assign_a_Dot() { 
        
    }
}
