using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;


public enum E_SkillLevel
{
    基础版本,
    加强版本
}

public interface ISkill
{
    public IBattlable self { get; set; }
    public List<IBattlable> targets { get; set; }

    public E_SkillTargetType skillTargetType { get; set; }

    void GetTargets() {
        targets = BattleTargetSelector.GetValidTargets(self, skillTargetType);
    }

    public void GetCaster(IBattlable _caster){
        self = _caster;
    }

    /// <summary>
    /// 对所有目标一次释放单体技能
    /// </summary>
    /// <param name="casters"></param>
    public void SkillExcute(E_SkillLevel skillLevel) {
        GetTargets();
        if (targets.Count<=0){
            UnityEngine.Debug.Log("何意味，无目标技能？");
            return;}
        
        for (int i = 0; i < targets.Count; i++) {
            switch (skillLevel){
                case E_SkillLevel.基础版本: SkillExcuteSingle(targets[i]); break;
                case E_SkillLevel.加强版本: SkillEnhanceSingle(targets[i]);break;
            }
        }
    }

    /// <summary>
    /// 技能基础效果
    /// </summary>
    /// <param name="target"></param>
    public void SkillExcuteSingle(IBattlable target);

    /// <summary>
    /// 技能增强-单体
    /// </summary>
    /// <param name="targets"></param>
    public void SkillEnhanceSingle(IBattlable target);


}

