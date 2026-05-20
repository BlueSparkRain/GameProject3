using System.Collections.Generic;

public class Enemy : IBattlable
{
    public E_Camp Camp => E_Camp.敌方;
    public bool IsAlive => true;
    public BattleDamageHandler battlerDataHandler { get; set; }

    public Enemy() { }
    public Enemy(BattleDamageHandler _damageHandle) {
        battlerDataHandler = _damageHandle;
    }
}