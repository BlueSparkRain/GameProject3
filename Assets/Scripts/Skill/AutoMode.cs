using System;

/// <summary>
/// 自动技能模式：包裹 SkillCharger，保持原有计时器充能 → 自动释放行为不变。
/// </summary>
public class AutoMode : ISkillMode
{
    SkillCharger _charger;

    /// <summary>向后兼容：暴露内部 SkillCharger（BattleSkiller 提取用）</summary>
    public SkillCharger Charger => _charger;

    public SkillData SkillData { get; private set; }
    public E_SkillMode ModeType => E_SkillMode.Auto;

    public event Action<float> OnCooldownChanged;
    public event Action<bool> OnSPStatusChanged;
    public event Action OnExecuted;

    public void Init(SkillData data, SkillBase skill)
    {
        SkillData = data;
        if (_charger == null)
        {
            _charger = new SkillCharger();
            _charger.OnCooldownChanged += f => OnCooldownChanged?.Invoke(f);
            _charger.OnSPStatusChanged += b => OnSPStatusChanged?.Invoke(b);
            _charger.OnExecuted += () => OnExecuted?.Invoke();
        }
        _charger.Init(data, skill);
    }

    public void Update(float currentSP, float deltaTime)
    {
        _charger?.Update(currentSP, deltaTime);
    }

    public void Freeze(bool freeze)
    {
        _charger?.Freeze(freeze);
    }

    public void SkillBreak()
    {
        _charger?.SkillBreak();
    }

    public void Dispose()
    {
        _charger = null;
        SkillData = null;
        OnCooldownChanged = null;
        OnSPStatusChanged = null;
        OnExecuted = null;
    }
}
