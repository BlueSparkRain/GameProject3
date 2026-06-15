using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 等级奖励管理器——监听角色升级事件，在特定等级节点弹出奖励面板
/// 技能选择: 3, 6, 9, 12, 15, 18, 21, 24, 27, 30
/// 槽位解锁: 5, 10, 15, 20, 25
/// </summary>
public class LevelRewardManager : MonoSceneManager
{
    static readonly HashSet<int> SkillRewardLevels = new HashSet<int> { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30 };
    static readonly HashSet<int> SlotRewardLevels = new HashSet<int> { 5, 10, 15, 20, 25 };

    /// <summary>待处理的奖励等级队列（处理连续升级）</summary>
    Queue<int> _pendingRewards = new Queue<int>();
    bool _isProcessing;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener<int, int>(E_EventType.CharacterLevelUp, OnCharacterLevelUp);
    }

    public override void MgrDispose()
    {
        EventCenter.RemoveEventListener<int, int>(E_EventType.CharacterLevelUp, OnCharacterLevelUp);
        base.MgrDispose();
    }

    void OnCharacterLevelUp(int oldLevel, int newLevel)
    {
        // 收集 oldLevel+1 到 newLevel 之间的所有奖励等级
        for (int lv = oldLevel + 1; lv <= newLevel; lv++)
        {
            if (SkillRewardLevels.Contains(lv) || SlotRewardLevels.Contains(lv))
                _pendingRewards.Enqueue(lv);
        }

        if (_pendingRewards.Count > 0 && !_isProcessing)
            ProcessNextReward();
    }

    void ProcessNextReward()
    {
        if (_pendingRewards.Count == 0)
        {
            _isProcessing = false;
            return;
        }

        _isProcessing = true;
        int level = _pendingRewards.Dequeue();

        if (SkillRewardLevels.Contains(level))
            OpenSkillReward(level);
        else if (SlotRewardLevels.Contains(level))
            OpenSlotReward(level);
        else
            ProcessNextReward(); // 跳过（理论上不会到这里）
    }

    void OpenSkillReward(int level)
    {
        var uiMgr = GameRoot.GetManager<UIManager>();
        uiMgr?.OpenPanel<RewardPanel>(E_UIPanelType.RewardPanel, panel =>
        {
            panel.SetMode(E_RewardMode.LevelUpSkill, level, onClose: () => ProcessNextReward());
        });
    }

    void OpenSlotReward(int level)
    {
        var uiMgr = GameRoot.GetManager<UIManager>();
        uiMgr?.OpenPanel<RewardPanel>(E_UIPanelType.RewardPanel, panel =>
        {
            panel.SetMode(E_RewardMode.LevelUpSlot, level, onClose: () => ProcessNextReward());
        });
    }

    public override void MgrUpdate(float deltaTime) { }
}
