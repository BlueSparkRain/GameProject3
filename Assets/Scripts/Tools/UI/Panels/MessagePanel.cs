using Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessagePanel : UIPanelBase
{
    [Header("讯息文本")]
    public TMP_Text messageText;

    [Header("关闭按钮")]
    public Button closeButton;
    [Header("确认按钮")]
    public Button confrimButton;

    public override void Hide()
    {
        base.Hide();
    }

    public override void Show()
    {
        base.Show();
    }
    public void SetMessage(string message, UnityAction confrimAction = null)
    {
        messageText.text = message;
        if (confrimAction != null)
            SetConfrimButtonAction(confrimAction);
    }
 


    void SetConfrimButtonAction(UnityAction action) {
        confrimButton.onClick.RemoveAllListeners();
        confrimButton.onClick.AddListener(action);
        confrimButton.onClick.AddListener(Close);
    }


    protected override void OnInit()
    {
        base.OnInit();
        closeButton.onClick.AddListener(Close);
        confrimButton.onClick.AddListener(Close);

    }

    protected override void PlayEnterAnim(System.Action onComplete)
    {
        base.PlayEnterAnim(onComplete);
    }

    protected override void PlayExitAnim(Action onComplete)
    {
        base.PlayExitAnim(onComplete);
        
    }
}
