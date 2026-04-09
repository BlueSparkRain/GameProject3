using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : IBattlable
{
    public E_Camp Camp => E_Camp.玩家方;
    public bool IsAlive => true;

    public CharacterBattle_Controller BattleController { get ; set; }

    public Player() { }

    public Player(CharacterBattle_Controller characterData)
    {
        BattleController = characterData;
    }
}