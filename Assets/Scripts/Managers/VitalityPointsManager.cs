using System;
using UnityEngine;

public class VitalityPointsManager :MonoGlobalManager, ICanSave_And_Load
{
    //记录本局游戏玩家的活力点数
    public int valityPoint = 0;

    public int max_VitalityPoints;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        //读取存档中是否有有效数据
        JsonSaver.InitData<Save_VitalityPoins>(this);

    }
    public override void MgrUpdate(float deltaTime)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AdjustVolityPoints(-1);
        }
    }

    /// <summary>
    /// 调整最大活力点数，每次升级后调整
    /// </summary>
    public void AdjustMaxVitalityPoints(int currentLevel)
    {
        max_VitalityPoints = currentLevel / 10 + 10;
        //升级恢复活力值
        AdjustVolityPoints(currentLevel / 10 + 1);
    }

    /// <summary>
    /// 调整活力点数
    /// </summary>
    /// <param name="transValue">变化值</param>
    public void AdjustVolityPoints(int transValue){
        if (valityPoint + transValue > max_VitalityPoints)
            valityPoint = max_VitalityPoints;
        else if (valityPoint+transValue<=0)
            valityPoint = 0;
        else
            valityPoint += transValue;
        //调整后需要保存数据
        Debug.Log($"[VitalityPointsManager]活力点数变化{transValue}");
        JsonSaver.Save<Save_VitalityPoins>(new Save_VitalityPoins(valityPoint, max_VitalityPoints));
        EventCenter.EventTrigger(E_EventType.UpdateUIVitalityPoints);
    }

    public void InitBySaveData(){
        var saveData = JsonSaver.Load<Save_VitalityPoins>();
        valityPoint = saveData.currentVitalityPoints;
        max_VitalityPoints = saveData.max_VitalityPoints;
        EventCenter.EventTrigger(E_EventType.UpdateUIVitalityPoints);
    }

    public void InitBySelf()
    {
        //初始等级1
        AdjustMaxVitalityPoints(1);

        //valityPoint = 10;
        //最大活力点数
        AdjustVolityPoints(10);
    }
}


[Serializable]
public class Save_VitalityPoins : IValidatable
{
    /// <summary>
    /// 当前剩余活力值
    /// </summary>
    public int currentVitalityPoints;

    /// <summary>
    /// 当前最大活力值
    /// </summary>
    public int max_VitalityPoints;
    public Save_VitalityPoins() { }
    public Save_VitalityPoins(int remain_Points, int max_points)
    {
        currentVitalityPoints = remain_Points;
        max_VitalityPoints = max_points;
    }
    public bool IsValid()
    {
        return true;
    }
}