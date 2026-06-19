using System;
using System.Linq;
using Core;
using UnityEngine;

/// <summary>
/// 主动技能模式：点击选中 → 时缓0.2x → Q增幅/W释放/E取消。
/// 同一时间只有一个ATB技能可被选中。
/// </summary>
public class ATBMode : ISkillMode
{
    public static ATBMode CurrentSelected { get; private set; }

    public SkillData SkillData { get; private set; }
    public E_SkillMode ModeType => E_SkillMode.ATB;

    public event Action<float> OnCooldownChanged;
    public event Action<bool> OnSPStatusChanged;
    public event Action OnExecuted;

    /// <summary>选中状态变化</summary>
    public event Action<bool> OnSelectionChanged;
    /// <summary>增幅等级变化 (0~3)</summary>
    public event Action<int> OnEnhanceLevelChanged;
    /// <summary>ATB点数是否不足释放基础版本</summary>
    public event Action<bool> OnATBStatusChanged;

    SkillBase _skill;
    bool _lastNoATB;
    public IBattlable Caster => _skill?.self;
    int _enhanceLevel;
    bool _selected;
    bool _frozen;

    const int MaxEnhance = 3;
    const float SlowTimeScale = 0.2f;
    const float SlowDuration = 0.1f;

    public void Init(SkillData data, SkillBase skill)
    {
        SkillData = data;
        _skill = skill;
        _enhanceLevel = 0;
        _selected = false;
        _frozen = false;
        OnEnhanceLevelChanged?.Invoke(0);
    }

    public void Update(float currentSP, float deltaTime)
    {
        // 始终检查 ATB 是否足够释放基础版本（用于按钮禁用显示）
        CheckATBStatus();

        if (_frozen || !_selected) return;

        if (Input.GetKeyDown(KeyCode.Q))
            Enhance();
        else if (Input.GetKeyDown(KeyCode.W))
            Release(currentSP);
        else if (Input.GetKeyDown(KeyCode.E))
            CancelEnhance();
    }

    /// <summary>ATB按钮点击：切换选中状态</summary>
    public void ToggleSelection()
    {
        if (_selected)
            Deselect();
        else
            Select();
    }

    void Select()
    {
        if (_frozen) return;

        // 取消之前选中的技能
        if (CurrentSelected != null && CurrentSelected != this)
            CurrentSelected.Deselect();

        CurrentSelected = this;
        _selected = true;
        SlowTime(true);
        OnSelectionChanged?.Invoke(true);
        EventCenter.EventTrigger(E_EventType.PrepareATBSkillExcute, true);
    }

    void Deselect()
    {
        bool wasSelected = _selected;

        if (CurrentSelected == this)
            CurrentSelected = null;
        _selected = false;
        _enhanceLevel = 0;
        SlowTime(false);
        OnSelectionChanged?.Invoke(false);
        OnEnhanceLevelChanged?.Invoke(0);

        if (wasSelected)
            EventCenter.EventTrigger(E_EventType.PrepareATBSkillExcute, false);
    }

    void Enhance()
    {
        if (_enhanceLevel >= MaxEnhance) return;
        _enhanceLevel++;
        OnEnhanceLevelChanged?.Invoke(_enhanceLevel);
    }

    void CancelEnhance()
    {
        _enhanceLevel = 0;
        OnEnhanceLevelChanged?.Invoke(0);
    }

    void Release(float currentSP)
    {
        var caster = _skill.self;

        // ATB检测+扣除：基础消耗 + 每层增幅额外+1点
        int totalAtbCost = SkillData.skill_atb_cost + _enhanceLevel;
        if (totalAtbCost > 0)
        {
            var controller = caster.battleDamageHandler?.BattleController;
            float currentATB = controller?.GetCharacterModelValue(E_BattleModelType.ATBPoints) ?? 0;
            if (currentATB < totalAtbCost)
            {
                DebugManager.Log(EDebugCategory.General, $"[ATBMode] ATB不足，无法释放(需要{totalAtbCost}, 当前{currentATB})");
                return;
            }
            controller.AdjustCharacterModelValue(E_BattleModelType.ATBPoints, -totalAtbCost);
        }

        IBattlable target = ResolveATBTarget(caster);

        var queue = GameRoot.GetManager<BattleActionQueue>();
        if (queue == null) return;

        var skill = BattleSkillFactory.CreateBattleSkill(SkillData.skill_ID, caster);
        var charName = caster.battleDamageHandler?.BattleController?.CharacterData?.Character_Name
            ?? caster.Camp.ToString();

        var action = new BattleAction(
            skill, charName, SkillData.skill_Name,
            SkillData.skill_DeliveryType,
            _enhanceLevel > 0 ? E_SkillLevel.加强版本 : E_SkillLevel.基础版本,
            prepaidSP: 0,   // ATB 模式不消耗 SP
            target: target,
            henceTime: _enhanceLevel,
            isATB: true);

        queue.Enqueue(action);

        EventCenter.EventTrigger<IBattlable, float>(E_EventType.SkillExcute, caster, 0);
        OnExecuted?.Invoke();
        Deselect();
    }

    IBattlable ResolveATBTarget(IBattlable caster)
    {
        var atbType = SkillData.skill_ATBTargetType;
        bool isPlayer = caster.Camp == E_Camp.玩家方;
        E_Camp enemyCamp = isPlayer ? E_Camp.敌方 : E_Camp.玩家方;

        switch (atbType)
        {
            case E_SkillTargetType_ATB.敌方单体:
            {
                var selectorMgr = UnityEngine.Object.FindObjectOfType<ActiveSkillTargetSelectorManager>();
                if (selectorMgr?.ConfirmedTarget != null)
                    return selectorMgr.ConfirmedTarget;
                return BattleTargetSelector.GetAllAliveTargets(enemyCamp).FirstOrDefault();
            }
            case E_SkillTargetType_ATB.自身:
                return caster;
            case E_SkillTargetType_ATB.敌方全体:
                return BattleTargetSelector.GetAllAliveTargets(enemyCamp).FirstOrDefault();
            case E_SkillTargetType_ATB.随机敌方单体:
            {
                var list = BattleTargetSelector.GetRandomNAliveTargets(enemyCamp, 1);
                return list != null && list.Count > 0 ? list[0] : null;
            }
            case E_SkillTargetType_ATB.自身加敌方单体:
            {
                var selectorMgr = UnityEngine.Object.FindObjectOfType<ActiveSkillTargetSelectorManager>();
                if (selectorMgr?.ConfirmedTarget != null)
                    return selectorMgr.ConfirmedTarget;
                return BattleTargetSelector.GetAllAliveTargets(enemyCamp).FirstOrDefault();
            }
            default:
                return null;
        }
    }

    public void Freeze(bool freeze)
    {
        _frozen = freeze;
        if (freeze && _selected)
            Deselect();
    }

    public void SkillBreak()
    {
        if (_selected)
            Deselect();
        _enhanceLevel = 0;
        OnEnhanceLevelChanged?.Invoke(0);
    }

    void CheckATBStatus()
    {
        int neededAtb = SkillData.skill_atb_cost + _enhanceLevel;
        bool noATB = neededAtb > 0
            && (Caster?.battleDamageHandler?.BattleController?.GetCharacterModelValue(E_BattleModelType.ATBPoints) ?? 0) < neededAtb;
        if (noATB != _lastNoATB)
        {
            _lastNoATB = noATB;
            OnATBStatusChanged?.Invoke(noATB);
        }
    }

    public void Dispose()
    {
        if (_selected) Deselect();
        _skill = null;
        SkillData = null;
        OnCooldownChanged = null;
        OnSPStatusChanged = null;
        OnExecuted = null;
        OnSelectionChanged = null;
        OnEnhanceLevelChanged = null;
        OnATBStatusChanged = null;
    }

    static void SlowTime(bool enter)
    {
        var tm = GameRoot.GetManager<TimeManager>();
        if (tm == null) return;
        tm.SetTimeScale(enter ? SlowTimeScale : 1f, SlowDuration);
    }
}
