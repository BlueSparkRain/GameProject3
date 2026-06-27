using System.Collections.Generic;

public static class BufferLogicBucket{

    /// <summary>
    /// 对目标发起一次特定弱点的附加攻击
    /// </summary>
    /// <param name="self">攻击者</param>
    /// <param name="targets">目标列表</param>
    /// <param name="weaknessType">弱点类型</param>
    /// <param name="damageRate">伤害倍率</param>
    public static void AdditiveWeaknessAttack(IBattlable self, List<IBattlable> targets,
                        E_WeaknessType  weaknessType, float damageRate) {
        Attack_Skill attack_Skill = new Attack_Skill();
        E_Skill_DamageType damageType = DamageTypeChecker.GetDamageType(weaknessType);
        float base_attack = 1;

        //if (damageType == E_Skill_DamageType.物理)
        //    base_attack = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack);
        //else
        //    base_attack = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack);

        DebugManager.Log(EDebugCategory.BattleBuff, $"附加伤害BUFF--Buff效果生效，造成{base_attack * damageRate}点{weaknessType}[{damageType}]伤害");
        //设置攻击状态，内部执行时会自动考虑角色的攻击力加成
        attack_Skill.SetAttackState(weaknessType, -base_attack, damageRate);
        //对每个目标执行攻击
        foreach (IBattlable target in targets)
            attack_Skill.Excute(self, target);
    }

    /// <summary>
    /// 为目标附加一个特定类型的Dot
    /// </summary>
    public static void Assign_a_Dot() {

    }
}
