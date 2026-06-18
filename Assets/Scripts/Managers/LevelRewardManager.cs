using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class LevelRewardManager : MonoSceneManager
{
    static readonly HashSet<int> SkillRewardLevels = new HashSet<int> { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30 };
    static readonly HashSet<int> SlotRewardLevels  = new HashSet<int> { 5, 10, 15, 20, 25 };

    Queue<(E_RewardMode mode, int level)> _pendingRewards = new Queue<(E_RewardMode, int)>();
    bool _processingQueue;
    int _lastRewardedLevel;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();

        // 监听 MapScene 内的升级事件（战斗外升级，如神像奖励）
        EventCenter.AddEventListener<int, int>(E_EventType.CharacterLevelUp, OnCharacterLevelUp);

        // 从存档恢复上次已领取奖励的最高等级
        var saved = JsonSaver.Load<Save_RewardProgress>();
        _lastRewardedLevel = saved.lastRewardedLevel;

        // 检测未领取的跨级奖励（如战斗中升级后回到 MapScene）
        var charData = CharacterHandler.PlayerInstance?.CharacterData;
        int currentLevel = charData?.CurrentLevel ?? 1;
        EnqueueMissedRewards(_lastRewardedLevel, currentLevel);
    }

    public override void MgrDispose()
    {
        EventCenter.RemoveEventListener<int, int>(E_EventType.CharacterLevelUp, OnCharacterLevelUp);
        base.MgrDispose();
    }

    void OnCharacterLevelUp(int oldLevel, int newLevel)
    {
        EnqueueMissedRewards(oldLevel, newLevel);
    }

    void EnqueueMissedRewards(int fromLevel, int toLevel)
    {
        for (int lv = fromLevel + 1; lv <= toLevel; lv++)
        {
            if (SkillRewardLevels.Contains(lv))
                _pendingRewards.Enqueue((E_RewardMode.LevelUpSkill, lv));
            else if (SlotRewardLevels.Contains(lv))
                _pendingRewards.Enqueue((E_RewardMode.LevelUpSlot, lv));
        }

        if (!_processingQueue && _pendingRewards.Count > 0)
            StartCoroutine(ProcessRewardQueue());
    }

    IEnumerator ProcessRewardQueue()
    {
        _processingQueue = true;

        while (_pendingRewards.Count > 0)
        {
            var (mode, level) = _pendingRewards.Dequeue();
            bool confirmed = false;
            RewardPanel currentPanel = null;

            var uiMgr = GameRoot.GetManager<UIManager>();
            if (uiMgr != null)
            {
                currentPanel = uiMgr.OpenPanel<RewardPanel>(E_UIPanelType.RewardPanel, panel =>
                {
                    panel.SetMode(mode, level);
                    panel.onRewardConfirmed = () => confirmed = true;
                });
            }

            // 等待用户确认当前奖励
            yield return new WaitUntil(() => confirmed);

            // 标记该等级已领取，持久化
            if (level > _lastRewardedLevel)
            {
                _lastRewardedLevel = level;
                JsonSaver.Save(new Save_RewardProgress(_lastRewardedLevel));
            }

            // 等待面板退出动画播完 + 面板真正关闭，再弹出下一个
            if (currentPanel != null)
                yield return new WaitUntil(() => !currentPanel.IsAnimating && !currentPanel.gameObject.activeSelf);
            else
                yield return new WaitForSeconds(0.4f);
        }

        _processingQueue = false;
    }

    public override void MgrUpdate(float deltaTime) { }
}

[System.Serializable]
public class Save_RewardProgress : IValidatable
{
    public int lastRewardedLevel;
    public Save_RewardProgress() { }
    public Save_RewardProgress(int lv) { lastRewardedLevel = lv; }
    public bool IsValid() => true;
}
