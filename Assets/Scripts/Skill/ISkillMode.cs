using System;

public interface ISkillMode
{
    SkillData SkillData { get; }
    E_SkillMode ModeType { get; }

    void Init(SkillData data, SkillBase skill);
    void Update(float currentSP, float deltaTime);
    void Freeze(bool freeze);
    void SkillBreak();
    void Dispose();

    event Action<float> OnCooldownChanged;
    event Action<bool> OnSPStatusChanged;
    event Action OnExecuted;
}
