using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel : UIPanelBase
{
    public Button closeButton;
    public Button hideButton;

    public override void Hide()
    {
        base.Hide();
    }

    protected override void ExitAnimCallBack()
    {
        base.ExitAnimCallBack();
    }

    protected override void OnInit()
    {
        base.OnInit();
        Debug.Log("woojaddadaddwd");
       
        closeButton.onClick.AddListener(OnClickCloseButton);
        hideButton.onClick.AddListener(OnClickHideButton);
    }

    void OnClickHideButton()=>Hide();
    void OnClickCloseButton()=>Close();

    protected override void EnterAnimCallBack()
    {
        base.EnterAnimCallBack();
    }

    protected override void PlayEnterAnimation()
    {
        base.PlayEnterAnimation();

        MagicAnimExtens.DoLocal_UIAnim(
               panelRoot,Anim_Duration,Anim_EaseType,
               Anim_BornPos, Anim_TargetTrans,
               Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }

    protected override void PlayExitAnim(Action onComplete)
    {
        base.PlayExitAnim(onComplete);

        MagicAnimExtens.DoLocal_UIAnim(
              panelRoot, Anim_Duration, Anim_EaseType,
              Anim_BornPos, Anim_TargetTrans,
              Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }

}
