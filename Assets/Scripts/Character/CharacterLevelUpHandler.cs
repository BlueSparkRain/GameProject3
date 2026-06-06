using UnityEngine;

/// <summary>
/// EXP UI更新数据
/// </summary>
public struct EXPUpdateInfo
{
    public int currentLevel;
    public float currentEXP;
    public float levelGoalEXP;
    public bool skip;
}

/// <summary>
/// 负责角色当前等级的更新和对应的UI更新
/// </summary>
public class CharacterLevelUpHandler : MonoBehaviour
{
    [Header("等级提升到下一级所需经验")]
    public float levelGoalEXP;

    [Header("当前经验值")]
    public float currentEXP;

    CharacterData charData;
    IUpGradable iUpgrade;

    public void InitLevelHandler(CharacterData characterData, IUpGradable upGradable)
    {
        iUpgrade = upGradable;
        charData = characterData;
        currentEXP = charData.CurrentEXP;
        levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(charData.CurrentLevel);
        TriggerEXPUIEvent(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            AdjustEXP(250);
    }

    public void AdjustEXP(float trans)
    {
        bool skip = false;
        if (currentEXP + trans >= levelGoalEXP)
        {
            charData.AdjustProperty(E_CharacterPropertyType.CurrentLevel, 1);
            skip = true;
            iUpgrade.UpGrade();
            levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(charData.CurrentLevel);
            currentEXP = (currentEXP + trans - levelGoalEXP + 100);
        }
        else
            currentEXP += trans;
        levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(charData.CurrentLevel);

        charData.SetCurrentEXP(currentEXP);
        JsonSaver.Save(new Save_CharacterData(charData), charData.characterType.ToString());
        TriggerEXPUIEvent(skip);
    }

    void TriggerEXPUIEvent(bool skip)
    {
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
