public class ChaosLevelManager : MonoSceneManager, ICanSave_And_Load
{

    public int currentLevel;
    public override void MgrUpdate(float deltaTime)
    {

    }
    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        JsonSaver.InitData<Save_ChaosState>(this);
        //EventCenter.AddEventListener(E_EventType.ChaosLevelUP,);
    }

    public void AdjustChaosLevelByRound(int round)
    {

        int level = round / 30 + 1;
        if (currentLevel != level)
        {
            currentLevel = level;
            EventCenter.EventTrigger(E_EventType.ChaosLevelUP, level);
        }
    }

    public void InitBySaveData()
    {
        var chaosSaveData = JsonSaver.Load<Save_ChaosState>();
        currentLevel = chaosSaveData.currentChaosLevel;
    }

    public void InitBySelf()
    {
        currentLevel = 1;
    }
}


public class Save_ChaosState : IValidatable
{
    /// <summary>
    /// 记录的混沌等级
    /// </summary>
    public int currentChaosLevel;

    public Save_ChaosState() { }
    public Save_ChaosState(int chaosLevel)
    {
        currentChaosLevel = chaosLevel;
    }
    public bool IsValid()
    {
        return true;
    }
}
