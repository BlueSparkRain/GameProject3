public interface ISkill
{
    public void SkillExcute(IBattleUnit caster);

    /// <summary>
    /// 技能增强
    /// </summary>
    /// <param name="targets"></param>
    public void SkillEnhance(IBattleUnit caster);

    public E_SkillTargetType skillTargetType { get; set; }

}

