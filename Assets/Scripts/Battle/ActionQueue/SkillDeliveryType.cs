/// <summary>
/// 技能投递类型 — 决定技能的演出时序与结算时机。
/// 在 SkillPropertySO 上配置，SkillVfxDirectorManager 据此选择对应的 ISkillDeliveryExecutor。
/// </summary>
public enum SkillDeliveryType
{
    /// <summary>立即结算：0.12s 停顿后在目标位置播放冲击特效</summary>
    Instant = 0,

    /// <summary>投射物：从施法者飞向目标 → 命中特效 → 结算</summary>
    Projectile = 1,

    /// <summary>自身增益：施法者位置浮现升腾光环 → 结算</summary>
    SelfBuff = 2,

    /// <summary>范围爆发：目标位置直接爆发（无预警圈）→ 结算</summary>
    AOE_Burst = 3,

    /// <summary>附魔驻留：施法者位置持续特效，长时间后自动销毁。不阻塞队列。</summary>
    Enchant = 4,
}
