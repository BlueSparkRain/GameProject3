using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSceneSetUp : MonoBehaviour{


    private void Start()
    {
        ObjectPoolManager obj = GameRoot.GetManager<ObjectPoolManager>();

        GameRoot.GetManager<UIManager>().OpenPanel<MenuPanel>(E_UIPanelType.MenuPanel,null);
        StartCoroutine(LoadAllPool());
    }

    IEnumerator LoadAllPool() {
        WaitForSeconds delay = new WaitForSeconds(0.4f);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.MapRoom_地图房间);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.HexRoomIcon_房间图标);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.HexFace_投影面片);
        //yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.RoomCloude_房间遮云);

        //yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillSlot_技能槽位);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillIcon_技能图标);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.ATBDot_ATB点数);
    }

    IEnumerator Wait() {
        GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,(p)=>p.SetMessage("相关内容尚未完成1"));
        yield return new WaitForSeconds(0.3f);
        GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,(p)=>p.SetMessage("相关内容尚未完成2"));

    }
}
