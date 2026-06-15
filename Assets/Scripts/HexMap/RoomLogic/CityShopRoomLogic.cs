using Core;
using UnityEngine;

/// <summary>
/// 城池商店房间逻辑——触发时打开ShopPanel
/// 一次性消耗型，触发后变为普通地块
/// </summary>
public class CityShopRoomLogic : RoomLogicComponent
{
    public CityShopRoomLogic()
    {
        _roomType = E_HexRoomType.CityShop;
    }

    public override void OnPlayerEnter(HexRoomTag roomTag)
    {
        if (!_canTrigger) return;

        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        GameRoot.GetManager<UIManager>().OpenPanel<ShopPanel>(E_UIPanelType.ShopPanel);

        Consume();
    }
}
