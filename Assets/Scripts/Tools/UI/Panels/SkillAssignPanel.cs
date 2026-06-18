using Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillAssignPanel : UIPanelBase
{
    public Button hideButton;
    [Header("尚未分配的所有技能数据")]
    public SkillIconSpawner restWholeSkillsSpawner;
    [Header("背包分配的技能数据")]
    public SkillIconSpawner normalSkillsSpawner;
    [Header("ATB栏分配的技能数据")]
    public SkillIconSpawner ATBSkillsSpawner;

    [Header("当前操作的skiller对象")]
    public CharacterMapSkiller currentMapSkiller;

    [Header("是否可调整技能分配")]
    public bool canSettled = true;

    const int REST_SLOT_COUNT = 24;
    bool _restSlotsReady;
    List<SkillData> _cachedRest, _cachedNormal, _cachedAtb;
    bool _cachedCanDrag;
    int _cachedNormalSlotNum, _cachedAtbSlotNum;
    bool _panelVisible;

    void OnEnable()
    {
        _panelVisible = true;
        EventCenter.AddEventListener(E_EventType.Character_GetNewSkill, OnPlayerGetNewSkill);
    }

    void OnDisable()
    {
        _panelVisible = false;
        EventCenter.RemoveEventListener(E_EventType.Character_GetNewSkill, OnPlayerGetNewSkill);
    }

    /// <summary>
    /// 根据当前操作的skiller数据更新面板
    /// </summary>
    public void LoadSkillIconBySettle(
        bool canPlayerDrag,
        int restSkillSlotNum,
        List<SkillData> _RestWholeSkillDatas,
        int normalSkillSlotNum,
        List<SkillData> _NormalSkillDatas,
        int atbSkillSlotNum,
        List<SkillData> _ATBSkillDatas)
    {
        _cachedCanDrag = canPlayerDrag;
        _cachedRest = _RestWholeSkillDatas;
        _cachedNormal = _NormalSkillDatas;
        _cachedNormalSlotNum = normalSkillSlotNum;
        _cachedAtb = _ATBSkillDatas;
        _cachedAtbSlotNum = atbSkillSlotNum;

        if (!_restSlotsReady)
        {
            restWholeSkillsSpawner.EnsureSlots(REST_SLOT_COUNT);
            _restSlotsReady = true;
        }
        restWholeSkillsSpawner.RefreshIcons(_RestWholeSkillDatas, canPlayerDrag);

        // normal/ATB 刷新
        normalSkillsSpawner.LoadSlotsAndSkills(normalSkillSlotNum, _NormalSkillDatas, canPlayerDrag);
        ATBSkillsSpawner.LoadSlotsAndSkills(atbSkillSlotNum, _ATBSkillDatas, canPlayerDrag);
    }

    /// <summary>实时响应玩家获得新技能</summary>
    void OnPlayerGetNewSkill()
    {
        if (!_panelVisible || currentMapSkiller == null) return;
        var restList = currentMapSkiller.RestWholeSkillDatas;
        if (restList.Count == 0) return;
        var newData = restList[restList.Count - 1];
        restWholeSkillsSpawner.AddSkill(newData, _cachedCanDrag);
    }

    /// <summary>
    /// 将面板此时的配置写回操作的玩家数据
    /// </summary>
    void UpdateSettleBackSkiller()
    {

        var restskills = restWholeSkillsSpawner.GetSettledSkilldatas();
        var backetskills = normalSkillsSpawner.GetSettledSkilldatas();
        var atbskills = ATBSkillsSpawner.GetSettledSkilldatas();
        var skillchecker = GameRoot.GetManager<MapSkillerCheker>();
        if (skillchecker != null)
            skillchecker.UpdateSkillSettle(restskills, backetskills, atbskills);
        EventCenter.EventTrigger(E_EventType.SkillSettle);
    }
    protected override void BeforeFadeInAnimCallBack()
    {
        base.BeforeFadeInAnimCallBack();
        canOpen = false;
    }

    protected override void EnterAnimCallBack()
    {
        base.EnterAnimCallBack();
        canOpen = true;
    }

    protected override void BeforeFadeOutAnimCallBack()
    {
        canOpen = false;
        UpdateSettleBackSkiller();
        base.BeforeFadeOutAnimCallBack();
        restWholeSkillsSpawner.UnloadIconsOnly();
        normalSkillsSpawner.UnloadSkills();
        ATBSkillsSpawner.UnloadSkills();
    }
    protected override void ExitAnimCallBack()
    {
        base.ExitAnimCallBack();
        canOpen = true;
    }

    protected override void UnitBeforeAnimCallBack()
    {
        base.UnitBeforeAnimCallBack();
    }

    void ReturnAllSkill()
    {



    }



    protected override void OnInit()
    {
        base.OnInit();
        hideButton.onClick.AddListener(OnClickHideButton);
    }

    void OnClickHideButton() { if (canOpen) Hide(); }


    protected override void PlayEnterAnim(Action onComplete)
    {
        base.PlayEnterAnim(onComplete);
    }

    protected override void PlayExitAnim(Action onComplete)
    {
        base.PlayExitAnim(onComplete);

    }
}
