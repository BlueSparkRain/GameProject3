using Core;
using UnityEngine;
/// <summary>
/// EXP UI更新数据
/// </summary>
public struct EXPUpdateInfo{
    public int currentLevel;
    public float currentEXP;
    public float levelGoalEXP;
    public bool skip;
}
/// <summary>
/// 负责角色当前等级的更新和对应的UI更新
/// </summary>
public class CharacterLevelUpHandler : MonoBehaviour{
    [Header("等级提升到下一级所需经验")]
    public float levelGoalEXP;
    [Header("当前经验值")]
    public float currentEXP;
    CharacterData charData;
    IUpGradable iUpgrade;
    public void InitLevelHandler(CharacterData characterData, IUpGradable upGradable){
        iUpgrade = upGradable;
        charData = characterData;
        currentEXP = charData.CurrentEXP;
        levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(charData.CurrentLevel);
        TriggerEXPUIEvent(false);
    }
    //private void Update()
    //{
        //if (Input.GetKeyDown(KeyCode.L))
            //AdjustEXP(250);
    //}
    public void AdjustEXP(float trans){
        bool skip = false;
        float remaining = trans;
        int oldLevel = charData.CurrentLevel;

        // 连续升级处理
        while (currentEXP + remaining >= levelGoalEXP)
        {
            remaining = (currentEXP + remaining) - levelGoalEXP;
            charData.AdjustProperty(E_CharacterPropertyType.CurrentLevel, 1);
            GameRoot.GetManager<VitalityPointsManager>().AdjustMaxVitalityPoints(charData.CurrentLevel);
            skip = true;
            iUpgrade.UpGrade();
            levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(charData.CurrentLevel);
            currentEXP = 0;
        }

        if (skip)
        {
            currentEXP = remaining;
            int newLevel = charData.CurrentLevel;
            EventCenter.EventTrigger(E_EventType.CharacterLevelUp, oldLevel, newLevel);
        }
        else
            currentEXP += remaining;

        levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(charData.CurrentLevel);
        charData.SetCurrentEXP(currentEXP);
        JsonSaver.Save(new Save_CharacterData(charData), charData.characterType.ToString());
        TriggerEXPUIEvent(skip);
    }

    void TriggerEXPUIEvent(bool skip){
        var info = new EXPUpdateInfo
        {
            currentLevel = charData.CurrentLevel,
            currentEXP = currentEXP,
            levelGoalEXP = levelGoalEXP,
            skip = skip
        };
        EventCenter.EventTrigger(E_EventType.AdjustEXP, info);
    }
}
