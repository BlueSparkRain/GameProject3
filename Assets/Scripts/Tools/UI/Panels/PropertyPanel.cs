using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyPanel : UIPanelBase
{
    public Button hideButton;
    [Header("角色昵称")]
    public TMP_Text character_Name;

    
    /// <summary>
    /// 根据角色信息来加载面板信息
    /// </summary>
    void LoadData() { 
    
    
    }

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

        hideButton.onClick.AddListener(OnClickHideButton);
    }
    void OnClickHideButton() => Hide();
    protected override void EnterAnimCallBack()
    {
        base.EnterAnimCallBack();
    }

    protected override void PlayEnterAnim(System.Action onComplete)
    {
        base.PlayEnterAnim(onComplete);

        //MagicAnimExtens.DoLocal_UIAnim(
        //       panelRoot, Anim_Duration, Anim_EaseType,
        //       Anim_BornPos, Anim_TargetTrans,
        //       Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }

    protected override void PlayExitAnim(Action onComplete)
    {
        base.PlayExitAnim(onComplete);

        //MagicAnimExtens.DoLocal_UIAnim(
        //      panelRoot, Anim_Duration, Anim_EaseType,
        //      Anim_BornPos, Anim_TargetTrans,
        //      Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }
}
