using Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : UIPanelBase
{
    public Button battleButton;
    public Button guitButton;
    public TMP_Text participantsText;

    public override void Show()
    {
        base.Show();
        if (participantsText != null)
            participantsText.text = GameRoot.GetManager<GameBattleManager>()?.GetBattleParticipantsInfo() ?? "";
    }

    void OnClickBattleButton(){
        GameRoot.GetManager<TimeManager>()?.SetTimeScale(1f, 0.3f);
        // 回收六边形地块到对象池，卸载 MapScene，加载 BattleScene
        GameRoot.GetManager<GameMapManager>()?.ReclaimAllRooms();
        GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("BattleScene", SceneSwitchManager.LoadMode.Single);
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
        DebugManager.Log(EDebugCategory.UIPanel,"设置新位置");
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
