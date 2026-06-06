using System.Collections.Generic;

public enum E_SkillLevel
{
    /// <summary>
    /// 背包技自动循环-基础版本
    /// </summary>
    基础版本,
    /// <summary>
    /// 使用>=1的 ATB点数
    /// </summary>
    加强版本
}

public interface IHaveWeakable
{
    public void GetWeakness();
}

public abstract class SkillBase
{
    public IBattlable self { get; set; }
    public List<IBattlable> targets { get; set; }

    public E_SkillTargetType skillTargetType { get; set; }

    public int AtbCost { get; set; }
    public float AngGrow { get; set; }
    protected Battle_Controller Controller => self?.battleDamageHandler?.BattleController;
    protected BattleBuffHandler BuffHandler => self.battleDamageHandler.BuffHandler;
    public virtual bool IsMagicType => false;

    public SkillBase(E_SkillTargetType _skillTargetType)
    {
        skillTargetType = _skillTargetType;
    }

    void GetTargets()
    {
        var targetType = BuffHandler.GetModifiedTargetType(skillTargetType, IsMagicType);
        targets = BattleTargetSelector.GetValidTargets(self, targetType);
    }

    public void GetCaster(IBattlable _caster)
    {
        self = _caster;
    }
    /// <summary>
    /// 对所有目标一次释放单体技能
    /// </summary>
    /// <param name="casters"></param>
    public void SkillExcute(E_SkillLevel skillLevel, int henceTime = 0)
    {
        if (skillLevel == E_SkillLevel.加强版本 && AtbCost > 0)
        {
            float currentATB = Controller?.GetCharacterModelValue(E_BattleModelType.ATBPoints) ?? 0;
            if (currentATB < AtbCost)
            {
                UnityEngine.Debug.LogWarning($"[SkillBase] ATB不足，无法释放加强技能(需要{AtbCost}, 当前{currentATB})");
                return;
            }
            Controller.AdjustCharacterModelValue(E_BattleModelType.ATBPoints, -AtbCost);
            UnityEngine.Debug.Log($"[SkillBase] 加强技能消耗ATB:{AtbCost}, 剩余:{currentATB - AtbCost}");
        }

        GetTargets();
        if (targets.Count <= 0)
        {
            UnityEngine.Debug.Log("何意味，无目标技能？");
            return;
        }
        for (int i = 0; i < targets.Count; i++)
        {
            switch (skillLevel)
            {
                case E_SkillLevel.基础版本: SkillEffect_Base(targets[i]); break;
                case E_SkillLevel.加强版本: SkillEffect_Enhence(targets[i], henceTime); break;
            }
        }
    }


    /// <summary>
    /// 技能基础效果
    /// </summary>
    /// <param name="target"></param>
    public abstract void SkillEffect_Base(IBattlable target);

    /// <summary>
    /// 技能增强
    /// </summary>
    /// <param name="target"></param>
    /// <param name="henceTime"></param>
    public virtual void SkillEffect_Enhence(IBattlable target, int henceTime) { }


}
