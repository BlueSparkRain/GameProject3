using Core;
using UnityEngine;
using UnityEngine.Events;

public class RewardHexRoom : IHexRoom
{
    public RewardHexRoom()
    {

    }

    public void DoHexRoomInit()
    {
    }

    public void DoHexRoomLogic(UnityAction roomJob = null)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        GameRoot.GetManager<UIManager>().OpenPanel<RewardPanel>(E_UIPanelType.RewardPanel);
    }

    public void DoHexRoomModel(Vector3 pos)
    {
    }

    public void DestroyModel() { }
}
