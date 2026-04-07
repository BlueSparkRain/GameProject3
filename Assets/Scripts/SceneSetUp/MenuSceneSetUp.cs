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
