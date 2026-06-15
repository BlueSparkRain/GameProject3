using Core;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class NPCPanel : UIPanelBase
{
    [Header("接受按钮")]
    public Button GetButton;
    [Header("拒绝按钮")]
    public Button RejectButton;
    public override void Hide()
    {
        base.Hide();
    }

    protected override void ExitAnimCallBack()
    {
        base.ExitAnimCallBack();
    }
    UnityAction NPC_Acion;

    public void SetNPC_Action(UnityAction unityAction) => NPC_Acion = unityAction;
  
    
    protected override void OnInit()
    {
        base.OnInit();
        GetButton.onClick.AddListener(OnClickGetButton);
        RejectButton.onClick.AddListener(OnClickRejectButton);
    }

    void OnClickRejectButton()
    {
        DebugManager.Log(EDebugCategory.UIPanel,"你拒绝了他的'好意'");


        Hide();
    }
    void OnClickGetButton() {
        NPC_Acion?.Invoke();
        DebugManager.Log(EDebugCategory.UIPanel,"你接受了他的'好意'");
        GameRoot.GetManager<MapSkillerCheker>().PlayerSkiller.GetNewSkill(0);
        //BattleSkillFactory.Create(0);

    }

    protected override void EnterAnimCallBack()
    {
        base.EnterAnimCallBack();
    }

    protected override void PlayEnterAnim(System.Action onComplete)
    {
        base.PlayEnterAnim( onComplete);

    }

    protected override void PlayExitAnim(Action onComplete)
    {
        base.PlayExitAnim(onComplete);

    }
}
