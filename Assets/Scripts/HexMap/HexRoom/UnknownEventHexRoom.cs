using Core;
using UnityEngine.Events;

public class UnknownEventHexRoom : IHexRoom
{
    public void DoHexRoomLogic(UnityAction roomJob)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
    }

    public void DoHexRoomModel()
    {
    }
}
