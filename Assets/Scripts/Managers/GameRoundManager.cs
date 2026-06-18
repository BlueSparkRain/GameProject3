using System;
using UnityEngine;
using Core;
public class GameRoundManager : MonoGlobalManager, ICanSave_And_Load
{
    /// <summary>
    /// ��ǰ��Ϸ���еĻغ���
    /// </summary>
    int roundNum = 0;
    public int RoundNum => roundNum;
    Save_GameRoundState roundSaveData;

    public void InitBySaveData()
    {
        //��ȡ�浵��¼
        roundSaveData = JsonSaver.Load<Save_GameRoundState>();
        roundNum = roundSaveData.currentRound;
    }
    public void InitBySelf(){
        roundSaveData=new Save_GameRoundState(0);
        JsonSaver.Save<Save_GameRoundState>(roundSaveData);
    }

    //���ض������޸Ļ���ȼ�
    public override void MgrUpdate(float deltaTime){
    }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener(E_EventType.NewRound, PlusRoundNum);
        JsonSaver.InitData<Save_GameRoundState>(this);
        JsonSaver.OnNewGame += InitBySelf;
    }
    public override void MgrDispose()
    {
        JsonSaver.OnNewGame -= InitBySelf;
        EventCenter.RemoveEventListener(E_EventType.NewRound, PlusRoundNum);
        base.MgrDispose();
    }
    void CheckChaosLevel() {
        GameRoot.GetManager<ChaosLevelManager>().AdjustChaosLevelByRound(roundNum);
    }

    void PlusRoundNum()
    {
        DebugManager.Log(EDebugCategory.MapRoom, "回合数+1");
        roundNum++;
        EventCenter.EventTrigger(E_EventType.UpdateRoundState);
        roundSaveData.currentRound = roundNum;
        JsonSaver.Save(roundSaveData);
        CheckChaosLevel();
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