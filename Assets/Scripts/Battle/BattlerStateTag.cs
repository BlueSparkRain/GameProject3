public class BattlerStateTag
{
    public BattlerStateTag() { }

    public E_CharacterType CharacterType { get; set; }

    bool state_Dead;
    public bool State_Dead => state_Dead;

    bool state_Break;
    public bool State_Break => state_Break;
    public void SetDeadState(bool _dead) { state_Dead = _dead; }
    public void SetBreakState(bool _break) { state_Break = _break; }
}
