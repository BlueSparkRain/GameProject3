/// <summary>
/// 空白房间逻辑——不做任何交互
/// </summary>
public class NoneRoomLogic : RoomLogicComponent
{
    public NoneRoomLogic()
    {
        _roomType = E_HexRoomType.None;
    }

    public override void OnPlayerEnter(HexRoomTag roomTag) { }
}
