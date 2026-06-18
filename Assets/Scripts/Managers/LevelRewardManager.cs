using System.Collections.Generic;
using Core;

public class LevelRewardManager : MonoSceneManager
{
    static readonly HashSet<int> SkillRewardLevels = new HashSet<int> { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30 };
    static readonly HashSet<int> SlotRewardLevels  = new HashSet<int> { 5, 10, 15, 20, 25 };

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
        for (int lv = oldLevel + 1; lv <= newLevel; lv++)
        {
            if (SkillRewardLevels.Contains(lv))
                EventCenter.EventTrigger(E_EventType.LevelUp_SkillReward, lv);
            else if (SlotRewardLevels.Contains(lv))
                EventCenter.EventTrigger(E_EventType.LevelUp_SlotReward, lv);
        }
    }

    public override void MgrUpdate(float deltaTime) { }
}
