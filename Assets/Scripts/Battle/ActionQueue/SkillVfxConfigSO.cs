using UnityEngine;

/// <summary>
/// 技能 VFX 配置 — ScriptableObject，按 SkillDeliveryType 映射到预制件。
/// 放置于 Assets/Resources/SOData/SkillVfxConfig.asset。
/// </summary>
[CreateAssetMenu(menuName = "SOData/SkillVfxConfig", fileName = "SkillVfxConfig")]
public class SkillVfxConfigSO : ScriptableObject
{
    [Header("Instant — 立即结算（目标冲击闪光）")]
    public GameObject instantVfxPrefab;

    [Header("Projectile — 投射物（飞行弹道）")]
    public GameObject projectileVfxPrefab;

    [Header("SelfBuff — 自身增益（升腾光环）")]
    public GameObject selfBuffVfxPrefab;

    [Header("AOE Burst — 范围爆发（直接爆开）")]
    public GameObject aoeBurstVfxPrefab;

    [Header("Enchant — 附魔驻留（长时间持续特效）")]
    public GameObject enchantVfxPrefab;

    public GameObject GetPrefab(SkillDeliveryType type) => type switch
    {
        SkillDeliveryType.Instant    => instantVfxPrefab,
        SkillDeliveryType.Projectile => projectileVfxPrefab,
        SkillDeliveryType.SelfBuff   => selfBuffVfxPrefab,
        SkillDeliveryType.AOE_Burst  => aoeBurstVfxPrefab,
        SkillDeliveryType.Enchant    => enchantVfxPrefab,
        _ => instantVfxPrefab,
    };
}
