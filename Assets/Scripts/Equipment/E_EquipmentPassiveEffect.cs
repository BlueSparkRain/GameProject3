/// <summary>
/// 装备被动效果类型枚举——用于 EquipmentController 聚合查询
/// 外部模块可通过 GetPassiveEffectValue() 获取聚合后的倍率/数值
/// </summary>
public enum E_EquipmentPassiveEffect
{
    /// <summary>技能冷却速度倍率 (0.9 = 快10%, 1.0 = 正常)</summary>
    CooldownRate,

    /// <summary>技能消耗倍率 (0.85 = 省15%)</summary>
    SkillCostRate,

    /// <summary>伤害加成倍率 (1.1 = +10%)</summary>
    DamageRate,

    /// <summary>受到伤害减免倍率 (0.9 = -10%)</summary>
    DamageReduceRate,
}
