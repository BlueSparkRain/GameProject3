using System.Collections.Generic;

/// <summary>
/// 被动效果子接口——实现此接口的效果会自动被 EquipmentController 聚合被动值
/// 无需在 RebuildPassiveValues 中硬编码效果类型
/// </summary>
public interface IPassiveEffect : IEquipmentEffect
{
    /// <summary>返回本效果提供的所有被动值（可多条，如一个效果同时影响冷却+伤害）</summary>
    IEnumerable<(E_EquipmentPassiveEffect type, float value)> GetPassiveValues();
}
