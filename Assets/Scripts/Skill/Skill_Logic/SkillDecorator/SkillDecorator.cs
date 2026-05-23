/// <summary>
/// 有些技能的逻辑是通用的（如多段数伤害技能，延迟回收技能，延迟释放技能）
/// </summary>
public abstract class SkillDecorator : ISkill
{
    ISkill iSkill;
    public SkillDecorator(ISkill skill) { iSkill = skill; }

    public virtual void Excute(IBattlable self, IBattlable target)
    {
        iSkill.Excute(self, target);
        UnityEngine.Debug.Log("Excute!");
    }
}






