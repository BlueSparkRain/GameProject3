/// <summary>
/// ��Щ���ܵ��߼���ͨ�õģ��������˺����ܣ��ӳٻ��ռ��ܣ��ӳ��ͷż��ܣ�
/// </summary>
public abstract class SkillDecorator : ISkill
{
    ISkill iSkill;
    public SkillDecorator(ISkill skill) { iSkill = skill; }

    public virtual void Excute(IBattlable self, IBattlable target)
    {
        iSkill.Excute(self, target);
        DebugManager.Log(EDebugCategory.SkillExecution,"Excute!");
    }
}






