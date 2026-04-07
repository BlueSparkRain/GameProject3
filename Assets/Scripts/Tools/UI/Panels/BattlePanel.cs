using Core;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : UIPanelBase
{
    public Button ambushButton;
    public Button battleButton;
    public Button guitButton;

    void OnClickAmbushButton() {
        Debug.Log("伏击");
        Hide();
    }
    void OnClickBattleButton() { 
        Debug.Log("战斗");
        GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("BattleScene");
        Hide();
    }
    void OnClickQuitButton() {
        Hide();
        Debug.Log("撤退");
    }
    public override void Hide()
    {
        base.Hide();
    }

    protected override void ExitAnimCallBack()
    {
        base.ExitAnimCallBack();
    }

    protected override void OnInit(){
        base.OnInit();
        ambushButton.onClick.AddListener(OnClickAmbushButton);
        battleButton.onClick.AddListener(OnClickBattleButton);
        guitButton.onClick.AddListener(OnClickQuitButton);
    }

    public void SetTragetPos(Vector3 bornPos,Vector3 targetPos) {

        Anim_BornPos = bornPos;
        Anim_TargetTrans = targetPos;
        Debug.Log("设置新位置");
    }

    protected override void PlayEnterAnim(System.Action onComplete)
    {
        base.PlayEnterAnim(onComplete);

        //MagicAnimExtens.DoLocal_UIAnim(
        //   panelRoot, Anim_Duration, Anim_EaseType,
        //   Anim_BornPos, Anim_TargetTrans,
        //   Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }

    protected override void PlayExitAnim(Action onComplete){
        base.PlayExitAnim(onComplete);
        //MagicAnimExtens.DoLocal_UIAnim(
        //   panelRoot, Anim_Duration, Anim_EaseType,
        //   Anim_BornPos, Anim_TargetTrans,
        //   Anim_DoFadeIn, Anim_NeedAlphaFadeIn);
    }
}
