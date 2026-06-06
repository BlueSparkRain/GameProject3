using System;
using Core;
using UnityEngine;

public class ActionPointsManager : MonoGlobalManager, ICanSave_And_Load
{
    public int RemainActionPoints { get; private set; }
    public int MaxActionPoints { get; private set; }

    protected override void MgrOnInit(){
        base.MgrOnInit();
        JsonSaver.InitData<Save_ActionPoints>(this);
    }

    public override void MgrUpdate(float deltaTime) { }

    /// <summary>
    /// 消费行动点，返回是否成功
    /// </summary>
    public bool SpendActionPoints(int amount = 1){
        if (RemainActionPoints < amount) return false;
        RemainActionPoints -= amount;
        SaveAndNotify();
        return true;
    }

    /// <summary>
    /// 增加行动点
    /// </summary>
    public void AddActionPoints(int amount){
        if (amount <= 0) return;
        RemainActionPoints += amount;
        SaveAndNotify();
    }

    /// <summary>
    /// 设置本回合最大行动点并补满
    /// </summary>
    public void FillActionPoints(int max){
        MaxActionPoints = max;
        RemainActionPoints = max;
        SaveAndNotify();
    }
    /// <summary>
    /// 回合结束时结算：剩余点数存入，计算下回合点数
    /// </summary>
    public (int maxPoints, int remainPoints) EndRound(int chaosLevel, int storedPoints){
        int newMax = UnityEngine.Random.Range(6, 9) + storedPoints;
        MaxActionPoints = newMax;
        RemainActionPoints = newMax;
        SaveAndNotify();
        return (newMax, newMax);
    }
    public void InitBySaveData(){
        var data = JsonSaver.Load<Save_ActionPoints>();
        RemainActionPoints = data.remainActionPoints;
        MaxActionPoints = data.maxActionPoints;
        EventCenter.EventTrigger(E_EventType.UpdateUIActionPoints);
    }

    public void InitBySelf(){
        RemainActionPoints = 0;
        MaxActionPoints = 0;
    }

    void SaveAndNotify(){
        JsonSaver.Save(new Save_ActionPoints(RemainActionPoints, MaxActionPoints));
        EventCenter.EventTrigger(E_EventType.UpdateUIActionPoints);
    }
}

[Serializable]
public class Save_ActionPoints : IValidatable{
    public int remainActionPoints;
    public int maxActionPoints;
    public Save_ActionPoints() { }
    public Save_ActionPoints(int remain, int max)
    {
        remainActionPoints = remain;
        maxActionPoints = max;
    }
    public bool IsValid() => true;
}
