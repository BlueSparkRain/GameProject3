using Core;
using UnityEngine;
using UnityEngine.Events;

public class UnknownEventHexRoom : IHexRoom
{
    public void DoHexRoomInit()
    {
    }

    public void DoHexRoomLogic(UnityAction roomJob)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
    }

    public void DoHexRoomModel(Vector3 pos)
    {
    }
}
