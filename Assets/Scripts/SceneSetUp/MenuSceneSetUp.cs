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
        continueButton.onClick.AddListener(OnClickContinueButton);
        settingButton.onClick.AddListener(OnClickNoButton);
        creditsButton.onClick.AddListener(OnClickNoButton);
        ObjectPoolManager obj = GameRoot.GetManager<ObjectPoolManager>();

        StartCoroutine(LoadAllPool());
    }

    IEnumerator LoadAllPool() {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.MapRoom_地图房间);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.RoomCloude_房间遮云);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillSlot_技能槽位);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillIcon_技能图标);
    }

    void OnClickGameButton() {
        //清空历史存档
        
        GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
        JsonSaver.StartNewGame();
    }

    void OnClickContinueButton() { 
        //读取历史存档数据加载游戏
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
