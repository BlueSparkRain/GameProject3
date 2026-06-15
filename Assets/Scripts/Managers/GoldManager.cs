using System;
using Core;
using UnityEngine;

public class GoldManager : MonoGlobalManager, ICanSave_And_Load{
    public int Gold { get; private set; }
    protected override void MgrOnInit(){
        base.MgrOnInit();
        JsonSaver.InitData<Save_Gold>(this);
    }
    public override void MgrUpdate(float deltaTime) { }
    public void AddGold(int amount){
        if (amount <= 0) return;
        Gold += amount;
        JsonSaver.Save(new Save_Gold(Gold));
        EventCenter.EventTrigger(E_EventType.UpdateUIGold);
    }
    public bool SpendGold(int amount){
        if (Gold < amount) return false;
        Gold -= amount;
        JsonSaver.Save(new Save_Gold(Gold));
        EventCenter.EventTrigger(E_EventType.UpdateUIGold);
        return true;
    }
    public void InitBySaveData(){
        var data = JsonSaver.Load<Save_Gold>();
        Gold = data.gold;
    }
    public void InitBySelf(){
        Gold = 0;
    }
}

[Serializable]
public class Save_Gold : IValidatable{
    public int gold;
    public Save_Gold() { }
    public Save_Gold(int g) { gold = g; }
    public bool IsValid() => true;
}
