using System.Collections.Generic;

public static class BufferLogicBucket{

    /// <summary>
    /// ���ض�Ŀ�귢��һ���ض�����Ĺ���
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

        //if (damageType == E_Skill_DamageType.����)
        //    base_attack = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack);
        //else 
        //    base_attack = self.battleDamageHandler.BattleController.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack);

        DebugManager.Log(EDebugCategory.BattleBuff, $"�����˺�BUFF--����Ч������ɸ���{base_attack* damageRate}��{weaknessType}[{damageType}]�˺�");
        //���ù���״̬���ڲ�����ʱ���Զ����ǽ�ɫ�Ĺ�������
        attack_Skill.SetAttackState(weaknessType,-base_attack, damageRate);
        //����ΪĿ�����
        foreach (IBattlable target in targets)
            attack_Skill.Excute(self, target);
    }

    /// <summary>
    /// ΪĿ������һ���ض�Ŀ�����͵�Buff
    /// </summary>
    public static void Assign_a_Dot() { 
        
    }
}
