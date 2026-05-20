using System;
using UnityEngine;


/// <summary>
/// 管理本角色当前等级的更新和对应的UI更新
/// </summary>
public class CharacterLevelUpHandler : MonoBehaviour
{
    /// <summary>
    /// 角色当前等级
    /// </summary>
    private int currentLevel;
    public int CurrentLevel => currentLevel;

    [Header("到达下一级所需总经验")]
    public float levelGoalEXP;

    [Header("当前经验值")]
    public float currentEXP;

    public event Action<int, float, float,bool> EXPUIUpdateEvent;

    public void InitLevelHandler()
    {
        //重置UI数据
        EXPUIUpdateEvent?.Invoke(1, 0, LevelCalculator.GetLevelUP_EXPGoal(1),false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AdjustEXP(250);
        }
    }
    public void AdjustEXP(float trans)
    {
        Debug.Log("EXP+250");
        bool skip = false;
        if (currentEXP + trans >= levelGoalEXP)
        {
            currentLevel++;
            skip = true;
            //角色升级！-Trigger
            levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(currentLevel);
            currentEXP = (currentEXP + trans - levelGoalEXP+100);//取出-溢出的经验值
        }
        else
            currentEXP += trans;
        levelGoalEXP = LevelCalculator.GetLevelUP_EXPGoal(currentLevel);
        EXPUIUpdateEvent?.Invoke(currentLevel, currentEXP, levelGoalEXP,skip);
    }
}

