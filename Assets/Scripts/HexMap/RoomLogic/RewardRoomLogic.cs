using Core;

/// <summary>
/// 神像奖励房间逻辑——触发时打开奖励面板
/// 一次性消耗型，触发后变为普通地块
/// </summary>
public class RewardRoomLogic : RoomLogicComponent
{
    public RewardRoomLogic()
    {
        _roomType = E_HexRoomType.Reward;
    }

    public override void OnPlayerEnter(HexRoomTag roomTag)
    {
        if (!_canTrigger) return;

        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        GameRoot.GetManager<UIManager>().OpenPanel<RewardPanel>(E_UIPanelType.RewardPanel);

        Consume();
    }
}
