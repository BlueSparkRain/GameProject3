public class Player : IBattlable
{
    public E_Camp Camp => E_Camp.玩家方;
    public bool IsAlive => true;

    public Player() { }

    public BattleDamageHandler battlerDataHandler { get; set; }
    public E_WeaknessType selfWeakness { get ; set; }

    public Player(BattleDamageHandler _damageHandle)
    {
        battlerDataHandler = _damageHandle;
    }

    public bool GetWeakAttack(E_WeaknessType attackWeakType)
    { 
        return selfWeakness == attackWeakType;
    }
}


