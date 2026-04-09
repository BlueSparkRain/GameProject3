using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSceneSetUp : MonoBehaviour
{

    public Button gameButton;
    public Button continueButton;
    public Button settingButton;
    public Button creditsButton;

    private void Start()
    {
        gameButton.onClick.AddListener(OnClickGameButton);
        continueButton.onClick.AddListener(OnClickNoButton);
        settingButton.onClick.AddListener(OnClickNoButton);
        creditsButton.onClick.AddListener(OnClickNoButton);
        ObjectPoolManager obj = GameRoot.GetManager<ObjectPoolManager>();

        StartCoroutine(LoadAllPool());
    }

    IEnumerator LoadAllPool() {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.MapRoom_地图房间);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.RoomCloude_房间遮云);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.SkillSlot_技能槽位);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.SkillIcon_技能图标);
    }

    void OnClickGameButton() {
        GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
    
    }

    void OnClickNoButton() {
        GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,(p)=>p.SetMessage("相关内容尚未完成"));
        //StartCoroutine(Wait());
    }

    IEnumerator Wait() {
        GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,(p)=>p.SetMessage("相关内容尚未完成1"));
        yield return new WaitForSeconds(0.3f);
        GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,(p)=>p.SetMessage("相关内容尚未完成2"));

    }
}
