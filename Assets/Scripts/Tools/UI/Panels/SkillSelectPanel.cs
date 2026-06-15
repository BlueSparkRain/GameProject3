using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectPanel : UIPanelBase
{
    [Header("选择容器")]
    public Transform skillContent;

    [Header("技能选择项预制件")]
    public GameObject skillItemPrefab;

    [Header("确认按钮")]
    public Button confirmButton;

    [Header("跳过按钮")]
    public Button skipButton;

    [Header("剩余选择次数文本")]
    public TMP_Text remainingCountText;

    [Header("关闭按钮")]
    public Button closeButton;

    [Header("黑色幕布(选择满时遮挡)")]
    public Image blackCurtain;

    [Header("最大选择数量")]
    public int maxSelectCount = 1;

    [Header("技能ID范围")]
    public int skillIDMin = 0;
    public int skillIDMax = 10;

    public List<SkillSelector_UIItem> skillSelectorUIs = new List<SkillSelector_UIItem>();
    public List<int> skillIDList = new List<int>();

    List<GameObject> _spawnedItems = new List<GameObject>();
    SkillSelector_UIItem _highlightedItem;
    int _confirmedCount;
    int _skipCount;
    Action _onPanelClose;

    int EffectiveSelectedCount => _confirmedCount + _skipCount;

    protected override void OnInit()
    {
        base.OnInit();
        confirmButton?.onClick.AddListener(OnConfirmClicked);
        skipButton?.onClick.AddListener(OnSkipClicked);
        closeButton?.onClick.AddListener(Hide);
        if (confirmButton != null)
            confirmButton.interactable = false;
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);
        if (blackCurtain != null)
            blackCurtain.gameObject.SetActive(false);
    }

    public override void Show()
    {
        base.Show();
        ClearSpawnedItems();
        _highlightedItem = null;
        _confirmedCount = 0;
        _skipCount = 0;
        if (confirmButton != null)
            confirmButton.interactable = false;
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);
        if (blackCurtain != null)
            blackCurtain.gameObject.SetActive(false);

        int spawnCount = 3;
        List<int> skillIDs = GetBatchSkillIDS(spawnCount, skillIDMin, skillIDMax);

        for (int i = 0; i < skillIDs.Count; i++)
        {
            SkillPropertySO so = ResourcesLoader.FindSkillSOByID(skillIDs[i]);
            if (so == null) continue;

            var obj = Instantiate(skillItemPrefab, skillContent);
            obj.SetActive(true);
            var item = obj.GetComponent<SkillSelector_UIItem>();
            if (item != null)
            {
                item.InitSelf(so);
                item.SetClickCallback(OnSkillItemClicked);
                item.SetHighlighted(false);
                skillSelectorUIs.Add(item);
            }
            _spawnedItems.Add(obj);
        }
        RefreshSelectionUI();
    }

    /// <summary>设置面板关闭时的回调（如等级奖励流程：关闭后继续处理队列）</summary>
    public void SetCloseCallback(Action callback)
    {
        _onPanelClose = callback;
    }

    public override void Hide()
    {
        base.Hide();
        ClearSpawnedItems();
        _highlightedItem = null;
        _confirmedCount = 0;
        _skipCount = 0;
        if (confirmButton != null)
            confirmButton.interactable = false;
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);
        if (blackCurtain != null)
            blackCurtain.gameObject.SetActive(false);
        _onPanelClose?.Invoke();
        _onPanelClose = null;
    }

    void ClearSpawnedItems()
    {
        foreach (var obj in _spawnedItems)
            Destroy(obj);
        _spawnedItems.Clear();
        skillSelectorUIs.Clear();
        skillIDList.Clear();
    }

    void OnSkillItemClicked(SkillSelector_UIItem clickedItem)
    {
        if (clickedItem == null) return;
        if (EffectiveSelectedCount >= maxSelectCount) return;

        // 点击取消高亮：取消选择
        if (_highlightedItem == clickedItem)
        {
            _highlightedItem.SetHighlighted(false);
            _highlightedItem = null;
        }
        else
        {
            // 取消旧高亮，设置新高亮
            if (_highlightedItem != null)
                _highlightedItem.SetHighlighted(false);
            _highlightedItem = clickedItem;
            _highlightedItem.SetHighlighted(true);
        }

        RefreshSelectionUI();
    }

    void OnSkipClicked()
    {
        _skipCount++;
        RefreshSelectionUI();
    }

    void RefreshSelectionUI()
    {
        bool isFull = EffectiveSelectedCount >= maxSelectCount;

        // 确认按钮高亮和交互状态
        if (confirmButton != null)
            confirmButton.interactable = _highlightedItem != null && !isFull;

        if (blackCurtain != null)
            blackCurtain.gameObject.SetActive(isFull);

        if (skipButton != null)
            skipButton.gameObject.SetActive(!isFull);

        if (closeButton != null)
            closeButton.gameObject.SetActive(isFull);

        if (remainingCountText != null)
        {
            int remaining = maxSelectCount - EffectiveSelectedCount;
            remainingCountText.text = $"剩余{remaining}次机会可供选择";
        }
    }

    void OnConfirmClicked()
    {
        if (_highlightedItem == null) return;

        var playerSkiller = CharacterHandler.PlayerInstance?.GetComponent<CharacterMapSkiller>();
        if (playerSkiller != null)
        {
            playerSkiller.GetNewSkill(_highlightedItem.SkillID);
            playerSkiller.UpdateSkilerSettle(
                playerSkiller.RestWholeSkillDatas,
                playerSkiller.NormalSkillDatas,
                playerSkiller.ATBSkillDatas);
        }

        _confirmedCount++;
        _highlightedItem.SetHighlighted(false);
        _highlightedItem = null;
        RefreshSelectionUI();
    }

    void AssignSkillData(SkillSelector_UIItem skillSelectorUI, SkillPropertySO skillPropertySO)
    {
        skillSelectorUI.InitSelf(skillPropertySO);
    }

    SkillPropertySO GetNewSkill()
    {
        int newSkillID = GetAvailableId(skillIDMin, skillIDMax);
        DebugManager.Log(EDebugCategory.UIPanel, $"当前List内{skillIDList[0]},{skillIDList[1]},{skillIDList[2]},给skillID为{newSkillID}");
        return ResourcesLoader.FindSkillSOByID(newSkillID);
    }

    int GetAvailableId(int min, int max)
    {
        var existing = new HashSet<int>(skillIDList);
        for (int id = min; id <= max; id++)
        {
            if (!existing.Contains(id))
                return id;
        }
        return -1;
    }

    public void AssignNewSkillData(SkillSelector_UIItem skillSelectorUI)
    {
        SkillPropertySO newSkillSo = GetNewSkill();
        AssignSkillData(skillSelectorUI, newSkillSo);
    }

    List<int> GetBatchSkillIDS(int skillCount, int minID, int maxID)
    {
        skillIDList = RandomUtility.GetUniqueRandomList(skillCount, minID, maxID);
        return skillIDList;
    }

    /// <summary>
    /// 结算当前面板中的技能选择 —— 用新随机技能替换现有技能项。
    /// 仅当 skillSelectorUIs 已由 Show() 填充后才有效。
    /// </summary>
    public void SetttleSelect()
    {
        if (skillSelectorUIs.Count == 0)
        {
            DebugManager.LogWarning(EDebugCategory.UIPanel, "[SkillSelectPanel] SetttleSelect: skillSelectorUIs 为空，跳过。请确保 Show() 已先调用。");
            return;
        }
        List<int> skillIDs = GetBatchSkillIDS(skillSelectorUIs.Count, skillIDMin, skillIDMax);
        for (int i = 0; i < skillSelectorUIs.Count; i++)
        {
            SkillPropertySO skillData = ResourcesLoader.FindSkillSOByID(skillIDs[i]);
            AssignSkillData(skillSelectorUIs[i], skillData);
        }
    }
}
