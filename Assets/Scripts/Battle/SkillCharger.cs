using System;
using Core;
using UnityEngine;

/// <summary>
/// 纯数据类 — 管理单个技能的冷却充能、SP检测与自动释放逻辑。
/// 从 SkillIcon 中抽离，使技能充能不再依赖 GameObject/UI 预制件。
/// </summary>
public class SkillCharger
{
    public float skillTimer;
    public bool isFreezzing;
    public bool hasNoSP;

    public SkillData SkillData { get; private set; }
    public SkillBase CurrentSkill { get; private set; }

    public event Action<float> OnCooldownChanged;
    public event Action<bool> OnSPStatusChanged;
    public event Action OnExecuted;

    const float MinCooldown = 0.5f;

    public void Init(SkillData data, SkillBase skill)
    {
        SkillData = data;
        CurrentSkill = skill;
        skillTimer = Mathf.Max(data.skill_CoolDown, MinCooldown);
        isFreezzing = false;
        hasNoSP = false;
    }

    /// <summary>
    /// 每帧调用 — 倒计时充能，冷却完毕后检测SP并自动释放技能
    /// </summary>
    public void Update(float currentSP, float deltaTime)
    {
        if (isFreezzing || CurrentSkill == null)
            return;

        if (skillTimer > -0.01f)
        {
            OnCooldownChanged?.Invoke(skillTimer / Mathf.Max(SkillData.skill_CoolDown, MinCooldown));
            skillTimer -= deltaTime;
        }
        else
        {
            float effectiveCd = Mathf.Max(SkillData.skill_CoolDown, MinCooldown);
            hasNoSP = SkillData.skill_sp_cost > currentSP;
            OnSPStatusChanged?.Invoke(hasNoSP);
            if (hasNoSP)
            {
                skillTimer = effectiveCd;
                return;
            }

            var caster = CurrentSkill.self;
            var charName = caster.battleDamageHandler?.BattleController?.CharacterData?.Character_Name
                ?? caster.Camp.ToString();

            var targets = BattleTargetSelector.GetValidTargets(caster, SkillData.skill_targetType);
            IBattlable target = targets != null && targets.Count > 0 ? targets[0] : null;

            var queue = GameRoot.GetManager<BattleActionQueue>();
            if (queue == null)
            {
                skillTimer = effectiveCd;
                return;
            }

            var skill = BattleSkillFactory.CreateBattleSkill(SkillData.skill_ID, caster);
            var action = new BattleAction(
                skill, charName, SkillData.skill_Name,
                SkillData.skill_DeliveryType, E_SkillLevel.基础版本,
                prepaidSP: SkillData.skill_sp_cost, target: target);
            queue.EnqueueNormal(action);

            skillTimer = effectiveCd;
            EventCenter.EventTrigger<IBattlable, float>(E_EventType.SkillExcute, caster, SkillData.skill_sp_cost);
            OnExecuted?.Invoke();
        }
    }

    public void SkillBreak()
    {
        isFreezzing = true;
        skillTimer = SkillData?.skill_CoolDown ?? 0f;
    }

    public void Freeze(bool freeze)
    {
        isFreezzing = freeze;
    }
}
