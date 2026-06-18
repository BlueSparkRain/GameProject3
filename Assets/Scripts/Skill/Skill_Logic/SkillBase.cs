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

    public E_SkillTargetType_Auto skillTargetType { get; set; }

    public int AtbCost { get; set; }
    public float AngGrow { get; set; }
    protected Battle_Controller Controller => self?.battleDamageHandler?.BattleController;
    protected BattleBuffHandler BuffHandler => self.battleDamageHandler.BuffHandler;
    public virtual bool IsMagicType => false;

    public SkillBase(E_SkillTargetType_Auto _skillTargetType)
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
        // ATB 消耗和检查由 ATBMode.Release 在入队前完成，此处不重复处理。
        // 自动技能的 AtbCost/AngGrow 来自 SO 的 skill_AtbCost_ATB/skill_ang_grow 字段，
        // 但自动技能不使用 ATB 系统，因此不在此处扣除。

        GetTargets();
        if (targets.Count <= 0)
        {
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
