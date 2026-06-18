using System.Collections.Generic;
using Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum E_RewardMode
{
    RewardRoom,
    LevelUpSkill,
    LevelUpSlot
}

public class RewardPanel : UIPanelBase
{
    [Header("奖励选项")]
    public EventSelectItem itemA;
    public EventSelectItem itemB;
    public EventSelectItem itemC;

    [Header("确认按钮")]
    public Button confirmButton;
    [Header("关闭按钮")]
    public Button closeButton;
    [Header("标题")]
    public TMP_Text titleText;

    EventSelectItem _selectedItem;
    Dictionary<EventSelectItem, Action> _actionMap = new Dictionary<EventSelectItem, Action>();
    E_RewardMode _currentMode;
    int _rewardLevel;
    bool _setupDone;

    /// <summary>用户确认奖励后的回调（供 LevelRewardManager 驱动队列）</summary>
    public System.Action onRewardConfirmed;

    protected override void OnInit()
    {
        base.OnInit();
        closeButton?.onClick.AddListener(Hide);
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }
    }

    public void SetMode(E_RewardMode mode, int level = 0)
    {
        _currentMode = mode;
        _rewardLevel = level;
        _setupDone = true;
        if (gameObject.activeSelf) RebuildUI();
    }

    public override void Show()
    {
        base.Show();
        _selectedItem = null;
        if (confirmButton != null) confirmButton.interactable = false;
        _actionMap.Clear();
        ClearHighlights();
        if (_setupDone) { RebuildUI(); _setupDone = false; }
    }

    void RebuildUI()
    {
        _actionMap.Clear();
        ClearHighlights();
        _selectedItem = null;
        if (confirmButton != null) confirmButton.interactable = false;
        switch (_currentMode)
        {
            case E_RewardMode.RewardRoom:   SetupRewardRoom(); break;
            case E_RewardMode.LevelUpSkill: SetupLevelUpSkill(); break;
            case E_RewardMode.LevelUpSlot:  SetupLevelUpSlot(); break;
        }
    }

    #region Mode Setups
    void SetupRewardRoom()
    {
        if (titleText != null) titleText.text = "神像奖励";
        if (closeButton != null) closeButton.gameObject.SetActive(true); // 神像房间允许关闭
        itemA.gameObject.SetActive(true); itemB.gameObject.SetActive(true); itemC.gameObject.SetActive(true);
        itemA.SetOption("5000经验 + 1000金币", OnItemClicked);
        itemB.SetOption("技能三选一", OnItemClicked);
        itemC.SetOption("恢复50%活力 & 行动力", OnItemClicked);
        _actionMap[itemA] = ApplyExpAndGold;
        _actionMap[itemB] = ApplySkillSelect;
        _actionMap[itemC] = ApplyRestore;
    }

    void SetupLevelUpSkill()
    {
        if (titleText != null) titleText.text = $"Lv.{_rewardLevel} 技能奖励";
        if (closeButton != null) closeButton.gameObject.SetActive(false); // 等级奖励不许跳过
        itemA.gameObject.SetActive(true);
        itemB.gameObject.SetActive(false);
        itemC.gameObject.SetActive(false);
        itemA.SetOption("技能三选一", OnItemClicked);
        _actionMap[itemA] = ApplySkillSelect;
    }

    void SetupLevelUpSlot()
    {
        if (titleText != null) titleText.text = $"Lv.{_rewardLevel} 槽位解锁";
        if (closeButton != null) closeButton.gameObject.SetActive(false); // 等级奖励不许跳过
        var charData = CharacterHandler.PlayerInstance?.CharacterData;
        int autoNow = charData?.AutoSkillSlotCount ?? 0;
        int atbNow  = charData?.AtbSkillSlotCount ?? 0;
        itemA.gameObject.SetActive(true); itemB.gameObject.SetActive(true); itemC.gameObject.SetActive(false);
        itemA.SetOption($"自动化槽位 +1 (当前{autoNow}/上限9)", OnItemClicked);
        itemB.SetOption($"ATB槽位 +1 (当前{atbNow}/上限9)", OnItemClicked);
        _actionMap[itemA] = ApplyAutoSlot;
        _actionMap[itemB] = ApplyATBSlot;
    }

    void ClearHighlights()
    {
        itemA?.SetHighlighted(false);
        itemB?.SetHighlighted(false);
        itemC?.SetHighlighted(false);
    }
    #endregion

    #region Interaction
    void OnItemClicked(EventSelectItem clickedItem)
    {
        if (clickedItem == null) return;
        if (_selectedItem != null) _selectedItem.SetHighlighted(false);
        _selectedItem = clickedItem;
        _selectedItem.SetHighlighted(true);
        if (confirmButton != null) confirmButton.interactable = true;
    }

    void OnConfirmClicked()
    {
        if (_selectedItem == null) return;
        if (!_actionMap.TryGetValue(_selectedItem, out var action)) return;
        // 禁用确认按钮防止重复点击
        if (confirmButton != null) confirmButton.interactable = false;
        action?.Invoke();
        // onRewardConfirmed 由各 Apply 方法在完成后自行触发
    }
    #endregion

    #region Apply — 完成后必须调用 FireConfirmed() 通知 LevelRewardManager
    void FireConfirmed()
    {
        var cb = onRewardConfirmed;
        onRewardConfirmed = null;
        cb?.Invoke();
    }

    void ApplyExpAndGold()
    {
        Hide();
        CharacterHandler.PlayerInstance?.GetComponent<CharacterLevelUpHandler>()?.AdjustEXP(5000);
        GameRoot.GetManager<GoldManager>()?.AddGold(1000);
        FireConfirmed();
    }

    void ApplySkillSelect()
    {
        Hide();
        GameRoot.GetManager<UIManager>()?.OpenPanel<SkillSelectPanel>(E_UIPanelType.SkillSelectPanel, p =>
        {
            p.SetttleSelect();
            // 等 SkillSelectPanel 关闭后再通知队列出下一个奖励
            p.SetCloseCallback(FireConfirmed);
        });
    }

    void ApplyRestore()
    {
        Hide();
        var vpMgr = GameRoot.GetManager<VitalityPointsManager>();
        if (vpMgr != null) vpMgr.AdjustVolityPoints(Mathf.CeilToInt(vpMgr.max_VitalityPoints * 0.5f));
        var apMgr = GameRoot.GetManager<ActionPointsManager>();
        if (apMgr != null)
        {
            int restoreAP = Mathf.CeilToInt(apMgr.MaxActionPoints * 0.5f);
            int newAP = Mathf.Min(apMgr.RemainActionPoints + restoreAP, apMgr.MaxActionPoints);
            if (newAP > apMgr.RemainActionPoints) apMgr.AddActionPoints(newAP - apMgr.RemainActionPoints);
        }
        FireConfirmed();
    }

    void ApplyAutoSlot()
    {
        Hide();
        CharacterHandler.PlayerInstance?.CharacterData?.UnlockAutoSlot(1);
        FireConfirmed();
    }

    void ApplyATBSlot()
    {
        Hide();
        CharacterHandler.PlayerInstance?.CharacterData?.UnlockAtbSlot(1);
        FireConfirmed();
    }
    #endregion

    public override void Hide()
    {
        base.Hide();
        _selectedItem = null;
        if (confirmButton != null) confirmButton.interactable = false;
    }
}
