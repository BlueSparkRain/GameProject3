using System;
using Core;
using UnityEngine;

public class VitalityPointsManager : MonoGlobalManager, ICanSave_And_Load
{
    public int valityPoint = 0;
    public int max_VitalityPoints;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        JsonSaver.InitData<Save_VitalityPoins>(this);
    }

    public override void MgrUpdate(float deltaTime)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AdjustVolityPoints(-1);
    }

    public void AdjustMaxVitalityPoints(int currentLevel)
    {
        max_VitalityPoints = currentLevel / 10 + 10;
        AdjustVolityPoints(currentLevel / 10 + 1);
    }

    public void AdjustVolityPoints(int transValue)
    {
        int newValue = valityPoint + transValue;
        if (newValue > max_VitalityPoints)
            valityPoint = max_VitalityPoints;
        else if (newValue <= 0)
            valityPoint = 0;
        else
            valityPoint = newValue;

        DebugManager.Log(EDebugCategory.MapRoom, $"[VitalityPointsManager]活力变化{transValue}, 当前:{valityPoint}/{max_VitalityPoints}");

        JsonSaver.Save(new Save_VitalityPoins(valityPoint, max_VitalityPoints));
        EventCenter.EventTrigger(E_EventType.UpdateUIVitalityPoints);

        if (valityPoint <= 0)
        {
            DebugManager.Log(EDebugCategory.MapRoom, "[VitalityPointsManager]活力归零，游戏结束");
            EventCenter.EventTrigger(E_EventType.GameOver);
        }
    }

    public void InitBySaveData()
    {
        var saveData = JsonSaver.Load<Save_VitalityPoins>();
        valityPoint = saveData.currentVitalityPoints;
        max_VitalityPoints = saveData.max_VitalityPoints;
        EventCenter.EventTrigger(E_EventType.UpdateUIVitalityPoints);
    }

    public void InitBySelf()
    {
        AdjustMaxVitalityPoints(1);
        AdjustVolityPoints(10);
    }
}

[Serializable]
public class Save_VitalityPoins : IValidatable
{
    public int currentVitalityPoints;
    public int max_VitalityPoints;
    public Save_VitalityPoins() { }
    public Save_VitalityPoins(int remain_Points, int max_points)
    {
        currentVitalityPoints = remain_Points;
        max_VitalityPoints = max_points;
    }
    public bool IsValid() => true;
}
