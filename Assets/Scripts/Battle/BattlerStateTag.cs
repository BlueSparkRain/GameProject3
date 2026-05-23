using System.Collections;
using UnityEngine;

public class BattlerStateTag 
{
    public BattlerStateTag(){ }
    bool state_Dead;
    /// <summary>
    /// ½ÇÉ«×´Ì¬-ËÀÍö
    /// </summary>
    public bool State_Dead=>state_Dead;

    bool state_Break;
    /// <summary>
    /// ½ÇÉ«×´Ì¬-Á¦½ß
    /// </summary>
    public bool State_Break=>state_Break;
    public void SetDeadState(bool _dead){state_Dead = _dead;}
    public void SetBreakState(bool _break){ state_Break = _break;}

}
