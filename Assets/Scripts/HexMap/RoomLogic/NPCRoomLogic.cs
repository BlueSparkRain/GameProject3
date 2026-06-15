using Core;
using UnityEngine;

/// <summary>
/// NPC交互房间逻辑——触发时打开NPCPanel
/// 一次性消耗型，触发后变为普通地块
/// </summary>
public class NPCRoomLogic : RoomLogicComponent
{
    public NPCRoomLogic()
    {
        _roomType = E_HexRoomType.NPC;
    }

    public override void OnPlayerEnter(HexRoomTag roomTag)
    {
        if (!_canTrigger) return;

        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        DebugManager.Log(EDebugCategory.MapRoom, "玩家进入NPC房间");
        GameRoot.GetManager<UIManager>().OpenPanel<NPCPanel>(E_UIPanelType.NPCPanel,
            (p) => p.SetNPC_Action(() => { }));

        Consume();
    }
}
