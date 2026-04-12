using System;
using UnityEngine;

public class GameRoundManager : MonoSceneManager, ISaveable
{
    /// <summary>
    /// 当前游戏进行的回合数
    /// </summary>
    int roundNum = 0;
    public int RoundNum => roundNum;

    public void InitBySaveData()
    {
        //读取存档记录
        var roudSaveData = JsonSaver.Load<Save_GameRoundState>();
        roundNum = roudSaveData.currentRound;
    }

    public void InitBySelf()
    {
     
    }

    //在特定波次修改混沌等级
    public override void MgrUpdate(float deltaTime)
    {
    }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener(E_EventType.NewRound, PlusRoundNum);
        JsonSaver.InitData<Save_GameRoundState>(this);
    }

    void PlusRoundNum()
    {
        UnityEngine.Debug.Log("回合数+1");
        roundNum++;
        JsonSaver.Save(new Save_GameRoundState(roundNum));
        //EventCenter.EventTrigger(E_EventType.NewRound);
    }
}

[Serializable]
public class Save_GameRoundState : IValidatable
{
    public Save_GameRoundState() { }
    public Save_GameRoundState(int roundNum)
    {
        currentRound = roundNum;
    }

    public int currentRound;
    public bool IsValid()
    {
        return true;
    }
}