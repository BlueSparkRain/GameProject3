using Core;
using UnityEngine;

/// <summary>
/// 随机事件房间逻辑——触发时打开UnknownEventPanel
/// 一次性消耗型，触发后变为普通地块
/// </summary>
public class UnknownEventRoomLogic : RoomLogicComponent{
    public UnknownEventRoomLogic(){
        _roomType = E_HexRoomType.UnknownEvent;
    }
    public override void OnPlayerEnter(HexRoomTag roomTag){
        if (!_canTrigger) return;
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        GameRoot.GetManager<UIManager>().OpenPanel<UnknownEventPanel>(E_UIPanelType.UnknownEventPanel);
        Consume();
    }
}
