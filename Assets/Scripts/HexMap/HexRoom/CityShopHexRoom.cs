using Core;
using UnityEngine;
using UnityEngine.Events;

public class CityShopHexRoom : IHexRoom
{
    public void DoHexRoomInit() { }

    public void DoHexRoomLogic(UnityAction roomJob = null)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        GameRoot.GetManager<UIManager>().OpenPanel<ShopPanel>(E_UIPanelType.ShopPanel);
    }

    public void DoHexRoomModel(Vector3 pos) { }
}
