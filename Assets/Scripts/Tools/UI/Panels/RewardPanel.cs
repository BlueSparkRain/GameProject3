using System.Collections.Generic;
using Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励类型
/// </summary>
public enum E_RewardType{
    ExpAndGold,
    SkillSelect,
    Restore
}

/// <summary>
/// 奖励面板模式
/// </summary>
public enum E_RewardMode
{
    RewardRoom,     // 默认：奖励房间（ExpGold / SkillSelect / Restore 三选一）
    LevelUpSkill,   // 等级奖励：选择一项技能
    LevelUpSlot     // 等级奖励：选择解锁槽位类型（自动化+1 / ATB+1）
}

/// <summary>
/// 奖励面板——预制体上挂载三个EventSelectItem + 确认/关闭按钮
/// 支持 RewardRoom 默认模式 和 等级奖励模式
/// </summary>
public class RewardPanel : UIPanelBase{
    [Header("奖励选项(预制体直接引用)")]
    public EventSelectItem expGoldItem;
    public EventSelectItem skillSelectItem;
    public EventSelectItem restoreItem;
    [Header("确认按钮")]
    public Button confirmButton;
    [Header("关闭按钮")]
    public Button closeButton;

    [Header("标题文本(可选)")]
    public TMP_Text titleText;

    EventSelectItem _selectedItem;
    Dictionary<EventSelectItem, E_RewardType> _itemRewardMap = new Dictionary<EventSelectItem, E_RewardType>();
    E_RewardMode _currentMode = E_RewardMode.RewardRoom;
    int _rewardLevel;
    Action _onCloseCallback;

    protected override void OnInit(){
        base.OnInit();
        closeButton?.onClick.AddListener(Hide);
        if (confirmButton != null){
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }
    }

    /// <summary>设置奖励面板的模式（OpenPanel 回调中使用，Show 由 UIManager 自动调用）</summary>
    public void SetMode(E_RewardMode mode, int level = 0, Action onClose = null)
    {
        _currentMode = mode;
        _rewardLevel = level;
        _onCloseCallback = onClose;
    }

    public override void Show(){
        base.Show();
        _selectedItem = null;
        if (confirmButton != null)
            confirmButton.interactable = false;

        _itemRewardMap.Clear();

        switch (_currentMode)
        {
            case E_RewardMode.RewardRoom:
                SetupRewardRoom();
                break;
            case E_RewardMode.LevelUpSkill:
                SetupLevelUpSkill();
                break;
            case E_RewardMode.LevelUpSlot:
                SetupLevelUpSlot();
                break;
        }

        // 取消所有高亮
        expGoldItem?.SetHighlighted(false);
        skillSelectItem?.SetHighlighted(false);
        restoreItem?.SetHighlighted(false);
    }

    void SetupRewardRoom()
    {
        if (titleText != null) titleText.text = "选择奖励";
        expGoldItem?.gameObject.SetActive(true);
        skillSelectItem?.gameObject.SetActive(true);
        restoreItem?.gameObject.SetActive(true);

        _itemRewardMap[expGoldItem] = E_RewardType.ExpAndGold;
        _itemRewardMap[skillSelectItem] = E_RewardType.SkillSelect;
        _itemRewardMap[restoreItem] = E_RewardType.Restore;

        expGoldItem?.SetOption("5000经验 + 1000金币", OnItemClicked);
        skillSelectItem?.SetOption("技能三选一", OnItemClicked);
        restoreItem?.SetOption("恢复50%活力 & 行动力", OnItemClicked);
    }

    void SetupLevelUpSkill()
    {
        if (titleText != null) titleText.text = $"Lv.{_rewardLevel} 技能奖励";
        // 只显示技能选择项
        expGoldItem?.gameObject.SetActive(false);
        restoreItem?.gameObject.SetActive(false);
        skillSelectItem?.gameObject.SetActive(true);
        skillSelectItem?.SetOption("选择一项新技能", OnItemClicked);
        _itemRewardMap[skillSelectItem] = E_RewardType.SkillSelect;
    }

    void SetupLevelUpSlot()
    {
        if (titleText != null) titleText.text = $"Lv.{_rewardLevel} 槽位解锁";
        // 用 expGoldItem 做"自动化槽位+1"，skillSelectItem 做"ATB槽位+1"，restoreItem 隐藏
        expGoldItem?.gameObject.SetActive(true);
        skillSelectItem?.gameObject.SetActive(true);
        restoreItem?.gameObject.SetActive(false);

        _itemRewardMap[expGoldItem] = E_RewardType.ExpAndGold;     // 复用为 AutoSlot
        _itemRewardMap[skillSelectItem] = E_RewardType.SkillSelect; // 复用为 ATBSlot

        expGoldItem?.SetOption("自动化槽位 +1", OnItemClicked);
        skillSelectItem?.SetOption("ATB槽位 +1", OnItemClicked);
    }

    void OnItemClicked(EventSelectItem clickedItem){
        if (clickedItem == null) return;
        if (_selectedItem != null)
            _selectedItem.SetHighlighted(false);
        _selectedItem = clickedItem;
        _selectedItem.SetHighlighted(true);
        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    void OnConfirmClicked(){
        if (_selectedItem == null) return;
        if (!_itemRewardMap.TryGetValue(_selectedItem, out E_RewardType rewardType)) return;

        switch (_currentMode)
        {
            case E_RewardMode.RewardRoom:
                ApplyRewardRoom(rewardType);
                break;
            case E_RewardMode.LevelUpSkill:
                ApplyLevelUpSkill();
                break;
            case E_RewardMode.LevelUpSlot:
                ApplyLevelUpSlot(rewardType);
                break;
        }
    }

    void ApplyRewardRoom(E_RewardType rewardType)
    {
        Hide();
        FinalizeReward();  // 先关面板，避免升级触发的奖励面板覆盖当前面板
        switch (rewardType){
            case E_RewardType.ExpAndGold:
                ApplyExpAndGold();
                break;
            case E_RewardType.SkillSelect:
                ApplySkillSelect();
                break;
            case E_RewardType.Restore:
                ApplyRestore();
                break;
        }
    }

    void ApplyLevelUpSkill()
    {
        Hide();
        var uiMgr = GameRoot.GetManager<UIManager>();
        uiMgr?.OpenPanel<SkillSelectPanel>(E_UIPanelType.SkillSelectPanel, p => {
            p.SetttleSelect();
            p.SetCloseCallback(() => FinalizeReward());
        });
    }

    void ApplyLevelUpSlot(E_RewardType rewardType)
    {
        var charData = CharacterHandler.PlayerInstance?.CharacterData;
        if (charData == null) return;

        if (rewardType == E_RewardType.ExpAndGold)
            charData.UnlockAutoSlot(1);
        else
            charData.UnlockAtbSlot(1);

        Hide();
        FinalizeReward();
    }

    void FinalizeReward()
    {
        _onCloseCallback?.Invoke();
        _onCloseCallback = null;
    }

    void ApplyExpAndGold()
    {
        var playerTag = CharacterHandler.PlayerInstance;
        if (playerTag != null){
            var levelHandler = playerTag.GetComponent<CharacterLevelUpHandler>();
            levelHandler?.AdjustEXP(5000);
        }
        GameRoot.GetManager<GoldManager>()?.AddGold(1000);
    }
    void ApplySkillSelect(){
        var uiMgr = GameRoot.GetManager<UIManager>();
        Hide();
        uiMgr.OpenPanel<SkillSelectPanel>(E_UIPanelType.SkillSelectPanel, p =>{
            p.SetttleSelect();
        });
    }
    void ApplyRestore(){
        var vpMgr = GameRoot.GetManager<VitalityPointsManager>();
        if (vpMgr != null){
            int restoreAmount = Mathf.CeilToInt(vpMgr.max_VitalityPoints * 0.5f);
            vpMgr.AdjustVolityPoints(restoreAmount);
        }

        var apMgr = GameRoot.GetManager<ActionPointsManager>();
        if (apMgr != null){
            int restoreAP = Mathf.CeilToInt(apMgr.MaxActionPoints * 0.5f);
            int newAP = Mathf.Min(apMgr.RemainActionPoints + restoreAP, apMgr.MaxActionPoints);
            int actualAdd = newAP - apMgr.RemainActionPoints;
            if (actualAdd > 0) apMgr.AddActionPoints(actualAdd);
        }
    }

    public override void Hide(){
        base.Hide();
        _selectedItem = null;
        if (confirmButton != null)
            confirmButton.interactable = false;
        // 恢复默认可见性（下次 Show 会重新设置）
        expGoldItem?.gameObject.SetActive(true);
        skillSelectItem?.gameObject.SetActive(true);
        restoreItem?.gameObject.SetActive(true);
    }
}
