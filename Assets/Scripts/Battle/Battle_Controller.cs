using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle_Controller
{
    public Battle_Viewer viewer;
    Battle_Model model;
    Dictionary<E_BattleModelType, Action<float>> modelDic;

    CharacterData characterData;
    public CharacterData CharacterData => characterData;

    BattlerStateTag battlerStateTag;
    IBattlable _battler;

    WaitForSeconds breakRefreshDelay;
    float breakRefreshDuration = 8;

    float modelUpdateTimer;
    float modelUpdateInterval = 1;

    public Battle_Controller(CharacterData charData, Battle_Viewer viewer, BattlerStateTag stateTag, IBattlable battler, int initialShieldPoints = 5)
    {
        characterData = charData;
        this.viewer = viewer;
        battlerStateTag = stateTag;
        _battler = battler;

        model = new Battle_Model(
            charData.Maximum_Health + charData.EquipHandler.GetGreenBonus(E_CharacterPropertyType.Maximum_Health),
            charData.Maximum_Mana   + charData.EquipHandler.GetGreenBonus(E_CharacterPropertyType.Maximum_Mana),
            (int)(charData.Maximum_ATB + charData.EquipHandler.GetGreenBonus(E_CharacterPropertyType.Maximum_ATB)),
            maxShield: initialShieldPoints + charData.GetShieldBonus());

        BuildModelDictionary();

        viewer.UpdataUI(model);
        model.OnDataChanged += () => viewer.UpdataUI(model);
        model.OnHPZero += OnCharacterDead;
        model.OnShieldBreak += OnCharacterBreak;

        breakRefreshDelay = new WaitForSeconds(breakRefreshDuration);
        modelUpdateTimer = modelUpdateInterval;
    }

    void BuildModelDictionary()
    {
        modelDic = new Dictionary<E_BattleModelType, Action<float>>
        {
            { E_BattleModelType.HP,              v => model.HP += v },
            { E_BattleModelType.MAX_HP,          v => model.MaxHP += v },
            { E_BattleModelType.SP,              v => model.SP += v },
            { E_BattleModelType.MAX_SP,          v => model.MaxSP += v },
            { E_BattleModelType.AG,              v => model.AG += v },
            { E_BattleModelType.MAX_AG,          v => model.MaxAG += v },
            { E_BattleModelType.ATBPoints,       v => model.ATBPoints += (int)v },
            { E_BattleModelType.MAX_ATBPoints,   v => model.MaxATBPoints += (int)v },
            { E_BattleModelType.ShieldPoints,    v => model.ShieldPoints += (int)v },
            { E_BattleModelType.Max_ShieldPoints,v => model.MaxShieldPoints += (int)v },
        };
    }

    // ── 死亡 / 力竭 ──
    void OnCharacterDead()
    {
        DebugManager.Log(EDebugCategory.BattleState,characterData.Character_Name + "角色已死亡");
        BattleDebugManager.LogFormat("{0} 已阵亡！", characterData.Character_Name);
        if (battlerStateTag.State_Dead) return;
        battlerStateTag.SetDeadState(true);
        EventCenter.EventTrigger(E_EventType.Battle_CharacterDead, battlerStateTag);
    }

    void OnCharacterBreak()
    {
        DebugManager.Log(EDebugCategory.BattleState,characterData.Character_Name + "角色已力竭");
        BattleDebugManager.LogFormat("{0} 力竭！", characterData.Character_Name);
        if (battlerStateTag.State_Break) return;
        battlerStateTag.SetBreakState(true);
        viewer.OnBreakStarted(breakRefreshDuration);
        EventCenter.EventTrigger(E_EventType.Battle_CharacterBreak, battlerStateTag);
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(BreakRecovery(), viewer);
    }

    IEnumerator BreakRecovery()
    {
        DebugManager.Log(EDebugCategory.BattleState,"角色力竭中");
        yield return breakRefreshDelay;
        battlerStateTag.SetBreakState(false);
        model.ShieldPoints = model.MaxShieldPoints;
        viewer.OnBreakEnded();
        EventCenter.EventTrigger(E_EventType.Battle_CharacterBreakRefresh);
        DebugManager.Log(EDebugCategory.BattleState,"角色力竭结束");
        BattleDebugManager.LogFormat("{0} 力竭恢复", characterData.Character_Name);
    }

    public bool IsBreak => battlerStateTag is { State_Break: true };

    public float GetCharacterPropertyValue(E_CharacterPropertyType propertyType)
        => characterData.GetEffectiveProperty(propertyType);
    public float GetCharacterBasePropertyValue(E_CharacterPropertyType propertyType)
        => characterData.GetProperty(propertyType);
    public void AdjustCharacterPropertyValue(E_CharacterPropertyType propertyType, float targetValue, bool useMulti = false)
        => CharacterData.AdjustProperty(propertyType, targetValue, useMulti);

    public float GetHPPercentage()
    {
        float hp = model.HP, max = model.MaxHP;
        return max > 0 ? hp / max : 0f;
    }

    public Battle_Model Model => model;

    public float GetCharacterModelValue(E_BattleModelType modelType) => modelType switch
    {
        E_BattleModelType.HP                => model.HP,
        E_BattleModelType.MAX_HP           => model.MaxHP,
        E_BattleModelType.SP               => model.SP,
        E_BattleModelType.MAX_SP           => model.MaxSP,
        E_BattleModelType.AG               => model.AG,
        E_BattleModelType.MAX_AG           => model.MaxAG,
        E_BattleModelType.ATBPoints        => model.ATBPoints,
        E_BattleModelType.MAX_ATBPoints    => model.MaxATBPoints,
        E_BattleModelType.ShieldPoints     => model.ShieldPoints,
        E_BattleModelType.Max_ShieldPoints => model.MaxShieldPoints,
        _ => throw new ArgumentOutOfRangeException(nameof(modelType), modelType, null)
    };

    /// <summary>修改模型值（统一入口），同时通知浮字系统</summary>
    public void AdjustCharacterModelValue(E_BattleModelType modelType, float delta)
    {
        modelDic[modelType].Invoke(delta);
        EventCenter.EventTrigger(E_EventType.Battle_ModelValueChanged,
            _battler, viewer.transform.position, modelType, delta);
    }

    /// <summary>
    /// 每帧驱动：被动回复（1s间隔）+ 合并UI刷新（每帧）
    /// </summary>
    public void OnBattleControlUpdate()
    {
        // 被动回复
        modelUpdateTimer -= Time.deltaTime;
        if (modelUpdateTimer <= 0)
        {
            modelUpdateTimer = modelUpdateInterval;
            AdjustCharacterModelValue(E_BattleModelType.SP, 20);
            AdjustCharacterModelValue(E_BattleModelType.AG, 10);
        }

        // 脏标记合并：一帧内多次模型变更只触发一次 UI 全量刷新
        model.FlushUI();
    }
}

public enum E_BattleModelType
{
    HP, MAX_HP,
    SP, MAX_SP,
    AG, MAX_AG,
    ATBPoints, MAX_ATBPoints,
    ShieldPoints, Max_ShieldPoints,
}
