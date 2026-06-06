using Core;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : UIPanelBase
{
    public Button battleButton;
    public Button guitButton;

    void OnClickBattleButton(){
        GameRoot.GetManager<TimeManager>()?.SetTimeScale(1f, 0.3f);
        GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("BattleScene");
        Hide();
    }
    void OnClickQuitButton()
    {
        Hide();
        GameRoot.GetManager<TimeManager>()?.SetTimeScale(1f, 0.3f);
        //后期可以做成如果逃跑或者失败，玩家会被强制踢出战斗地块
        EventCenter.EventTrigger(E_EventType.PlayerOutBattle);
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
        battleButton.onClick.AddListener(OnClickBattleButton);
        guitButton.onClick.AddListener(OnClickQuitButton);
    }
    public void SetTragetPos(Vector3 bornPos, Vector3 targetPos)
    {

        Anim_BornPos = bornPos;
        Anim_TargetTrans = targetPos;
        Debug.Log("设置新位置");
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
