using Core;
using UnityEngine;
using UnityEngine.Events;

public class NPCHexRoom : IHexRoom
{
    public void DoHexRoomInit()
    {
    }

    public void DoHexRoomLogic(UnityAction roomJob)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        UnityEngine.Debug.Log("玩家进入NPC房间");
        GameRoot.GetManager<UIManager>().OpenPanel<NPCPanel>(E_UIPanelType.NPCPanel,
            (p) => p.SetNPC_Action(() => { }));
    }

    public void DoHexRoomModel(Vector3 pos)
    {
    }
}
