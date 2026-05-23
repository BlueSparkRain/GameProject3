using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

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

    public SkillBase(E_SkillTargetType _skillTargetType){
        skillTargetType = _skillTargetType;
    }

    void GetTargets(){
        targets = BattleTargetSelector.GetValidTargets(self, skillTargetType);
    }

    public void GetCaster(IBattlable _caster){
        self = _caster;
    }
    /// <summary>
    /// 对所有目标一次释放单体技能
    /// </summary>
    /// <param name="casters"></param>
    public void SkillExcute(E_SkillLevel skillLevel,int henceTime = 0) { 
        GetTargets();
        if (targets.Count <= 0){
            UnityEngine.Debug.Log("何意味，无目标技能？");
            return;
        }
        for (int i = 0; i < targets.Count; i++){
            switch (skillLevel){
                case E_SkillLevel.基础版本: SkillEffect_Base(targets[i]); break;
                case E_SkillLevel.加强版本: SkillEffect_Enhence(targets[i],henceTime); break;
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

