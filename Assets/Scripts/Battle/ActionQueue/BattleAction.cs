using System;

/// <summary>
/// 战斗行动 — 封装一个待结算的技能（谁放的、什么技能、什么等级、投递类型）。
/// 由 SkillCharger / ATBIntentionExecutor 提交到 BattleActionQueue。
/// </summary>
public class BattleAction
{
    public SkillBase Skill { get; }
    public IBattlable Caster => Skill?.self;
    public string CasterName { get; }
    public string SkillName { get; }
    public SkillDeliveryType DeliveryType { get; }
    public E_SkillLevel SkillLevel { get; }
    public int HenceTime { get; }

    /// <summary>已支付 SP 时非 0，结算时不再重复扣</summary>
    public float PrepaidSP { get; }

    /// <summary>是否为 ATB 主动技能（日志显示用）</summary>
    public bool IsATB { get; }

    /// <summary>预解析的技能目标（用于 VFX 定位，可能为 null）</summary>
    public IBattlable Target { get; }

    public BattleAction(SkillBase skill, string casterName, string skillName,
        SkillDeliveryType deliveryType, E_SkillLevel level, int henceTime = 0,
        float prepaidSP = 0f, bool isATB = false, IBattlable target = null)
    {
        Skill = skill;
        CasterName = casterName;
        SkillName = skillName;
        DeliveryType = deliveryType;
        SkillLevel = level;
        HenceTime = henceTime;
        PrepaidSP = prepaidSP;
        IsATB = isATB;
        Target = target;
    }

    /// <summary>结算技能效果（由队列在演出完成后调用）</summary>
    public void Settle()
    {
        if (Caster == null || !Caster.IsAlive)
        {
            BattleDebugManager.LogFormat("  {0} 因角色已阵亡而取消", SkillName);
            return;
        }
        Skill.SkillExcute(SkillLevel, HenceTime);
    }
}
