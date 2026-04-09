using System.Collections.Generic;

public class Enemy : IBattlable
{
    public E_Camp Camp => E_Camp.敌方;
    public bool IsAlive => true;

    public CharacterBattle_Controller BattleController { get ; set; }

    public Enemy() { }
    public Enemy(CharacterBattle_Controller _BattleController) {
        BattleController = _BattleController;
    }
}