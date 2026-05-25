using System.Collections.Generic;

public class Enemy : IBattlable
{
    public E_Camp Camp => E_Camp.敌方;
    public bool IsAlive => true;
    public BattleDamageHandler battleDamageHandler { get; set; }
    public E_WeaknessType selfWeakness { get ; set ; }

    public Enemy() { }
    public Enemy(BattleDamageHandler _damageHandle) {
        battleDamageHandler = _damageHandle;
    }

    public bool GetWeakAttack(E_WeaknessType attackWeakType)
    {
       return selfWeakness==attackWeakType;
    }
}