public class Player : IBattlable
{
    public E_Camp Camp => E_Camp.玩家方;
    public bool IsAlive => true;

    public Player() { }

    public BattleDamageHandler battlerDataHandler { get; set; }

    public Player(BattleDamageHandler _damageHandle)
    {
        battlerDataHandler = _damageHandle;
    }
}


