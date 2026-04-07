using Core;
using UnityEngine.Events;

public class UnknownHexRoom : IHexRoom
{
    public void DoRoomLogic(UnityAction roomJob)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
    }
}
