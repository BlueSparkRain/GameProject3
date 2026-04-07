using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : IBattleUnit
{
    public E_Camp Camp => E_Camp.玩家方;
    public bool IsAlive => true;

}