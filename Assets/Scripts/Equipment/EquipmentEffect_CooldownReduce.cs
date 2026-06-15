using System.Collections.Generic;

/// <summary>
/// 冷却缩减效果——降低所有技能的冷却时间
/// 被动型：实现 IPassiveEffect，由 EquipmentController 自动聚合
/// 查询：controller.GetPassiveEffectValue(E_EquipmentPassiveEffect.CooldownRate)
/// </summary>
public class EquipmentEffect_CooldownReduce : IPassiveEffect{
    public float cooldownRate;
    public EquipmentEffect_CooldownReduce(float cooldownRate){
        this.cooldownRate = cooldownRate;
    }

    public void OnEquip(EquipmentEffectContext ctx) { }

    public void OnUnequip() { }

    IEnumerable<(E_EquipmentPassiveEffect, float)> IPassiveEffect.GetPassiveValues()
    {
        yield return (E_EquipmentPassiveEffect.CooldownRate, cooldownRate);
    }
}
