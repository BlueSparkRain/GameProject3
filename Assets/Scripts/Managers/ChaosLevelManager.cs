using Core;
using UnityEngine;

public class ChaosLevelManager : MonoSceneManager, ICanSave_And_Load
{
    public int currentLevel;

    /// <summary>敌人强度倍率 (1.3^level)</summary>
    public float EnemyStrengthMultiplier => Mathf.Pow(1.3f, currentLevel - 1);
    /// <summary>奖励倍率 (1.2^level)</summary>
    public float RewardMultiplier => Mathf.Pow(1.2f, currentLevel - 1);
    /// <summary>商店售价倍率 (1.2^level)</summary>
    public float ShopPriceMultiplier => Mathf.Pow(1.2f, currentLevel - 1);
    /// <summary>治疗花费倍率 (1.3^level)</summary>
    public float HealCostMultiplier => Mathf.Pow(1.3f, currentLevel - 1);

    public override void MgrUpdate(float deltaTime) { }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        JsonSaver.InitData<Save_ChaosState>(this);
    }

    public void AdjustChaosLevelByRound(int round)
    {
        int level = round / 10 + 1;
        if (currentLevel != level)
        {
            currentLevel = level;
            SpawnRandomEventOnMap();
            EventCenter.EventTrigger(E_EventType.ChaosLevelUP, level);
            JsonSaver.Save(new Save_ChaosState(currentLevel));
        }
    }

    void SpawnRandomEventOnMap()
    {
        var mapMgr = GameRoot.GetManager<GameMapManager>();
        if (mapMgr == null) return;

        var room = mapMgr.GetRnadomRoom();
        if (room == null) return;

        mapMgr.UpdateHexTag(
            new Vector2Int(room.row, room.col),
            E_HexTerrainType.Walkable_UnknownEventRoom);
    }

    public void InitBySaveData()
    {
        var data = JsonSaver.Load<Save_ChaosState>();
        currentLevel = data.currentChaosLevel;
    }

    public void InitBySelf()
    {
        currentLevel = 1;
    }
}

public class Save_ChaosState : IValidatable
{
    public int currentChaosLevel;
    public Save_ChaosState() { }
    public Save_ChaosState(int chaosLevel) { currentChaosLevel = chaosLevel; }
    public bool IsValid() => true;
}
