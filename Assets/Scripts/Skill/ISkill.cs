public interface ISkill
{
    public void SkillExcute(IBattlable caster);

    /// <summary>
    /// 技能增强
    /// </summary>
    /// <param name="targets"></param>
    public void SkillEnhance(IBattlable caster);

    public E_SkillTargetType skillTargetType { get; set; }

}

