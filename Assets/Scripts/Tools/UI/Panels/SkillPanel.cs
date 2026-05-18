using Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanel : UIPanelBase
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

    /// <summary>
    /// 根据当前操作的skiller数据更新面板
    /// </summary>
    /// <param name="_RestWholeSkillDatas"></param>
    /// <param name="restSkillSlotNum"></param>
    /// <param name="_NormalSkillDatas"></param>
    /// <param name="normalSkillSlotNum"></param>
    /// <param name="_ATBSkillDatas"></param>
    /// <param name="atbSkillSlotNum"></param>
    public void LoadSkillIconBySettle(
        bool canPlayerDrag,
        int restSkillSlotNum,
        List<SkillData> _RestWholeSkillDatas,
        int normalSkillSlotNum,
        List<SkillData> _NormalSkillDatas,
        int atbSkillSlotNum,
        List<SkillData> _ATBSkillDatas)
    {
        //不要在关闭的时候再次加载
        if (!Anim_DoFadeIn)
            return;

        //先立即卸载所有按钮
        Debug.Log(_RestWholeSkillDatas.Count + "???_RestWholeSkillDatas");
        Debug.Log(_NormalSkillDatas.Count + "???_NormalSkillDatas");
        Debug.Log(_ATBSkillDatas.Count + "???_ATBSkillDatas");
        restWholeSkillsSpawner.LoadSlotsAndSkills(restSkillSlotNum, _RestWholeSkillDatas, canPlayerDrag);
        normalSkillsSpawner.LoadSlotsAndSkills(normalSkillSlotNum, _NormalSkillDatas, canPlayerDrag);
        ATBSkillsSpawner.LoadSlotsAndSkills(atbSkillSlotNum, _ATBSkillDatas, canPlayerDrag);
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
        canOpen = !canOpen;
    }

    protected override void EnterAnimCallBack()
    {
        base.EnterAnimCallBack();
        canOpen = !canOpen;
    }

    protected override void BeforeFadeOutAnimCallBack()
    {
        canOpen = !canOpen;
        UpdateSettleBackSkiller();
        Debug.Log("离场前卸载Icons");
        base.BeforeFadeOutAnimCallBack();
        restWholeSkillsSpawner.UnloadSkills();
        normalSkillsSpawner.UnloadSkills();
        ATBSkillsSpawner.UnloadSkills();
    }
    protected override void ExitAnimCallBack()
    {
        base.ExitAnimCallBack();
        canOpen = !canOpen;

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

    void OnClickHideButton() => Hide();


    protected override void PlayEnterAnim(Action onComplete)
    {
        base.PlayEnterAnim(onComplete);
    }

    protected override void PlayExitAnim(Action onComplete)
    {
        base.PlayExitAnim(onComplete);
    }
}
