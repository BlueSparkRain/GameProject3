using Core;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 获取到战斗中的所有单位，在任意技能初始化时分配施法目标
/// </summary>
public class BattleTargetsSelectManager : MonoSceneManager
{
    //private List<Battle_Controller> playerControllers = new List<Battle_Controller>();
    //private List<Battle_Controller> enemyControllers = new List<Battle_Controller>();

    private List<BattlerStateTag> playerControllers = new List<BattlerStateTag>();
    private List<BattlerStateTag> enemyControllers = new List<BattlerStateTag>();

    //记录本局战斗中所有的技能释放者，方便技能初始化时分配目标
    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener<BattlerStateTag>(E_EventType.Battle_CharacterDead, CheckBattleEnd);
    }

    /// <summary>
    /// 注册一名skiller
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="battleController"></param>
    //public void RegisterSkiller(bool isPlayer, Battle_Controller battleController)
    //{
    //    (isPlayer ? playerControllers : enemyControllers).Add(battleController);
    //}

    public void RegisterSkiller(bool isPlayer, BattlerStateTag battleController)
    {
        (isPlayer ? playerControllers : enemyControllers).Add(battleController);
    }

    void CheckBattleEnd(BattlerStateTag characterBattle_Controller)
    {
        if (PlayerWinBattle())
        {
            EventCenter.EventTrigger(E_EventType.BattleEnd);
            GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,
                p => p.SetMessage("你获胜了", () =>
                {
                    GameRoot.GetManager<VitalityPointsManager>().AdjustVolityPoints(+1);
                    GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
                }));
            return;
        }
        if (EnemyWinBattle())
        {
            EventCenter.EventTrigger(E_EventType.BattleEnd);
            GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,
                p => p.SetMessage("你失败了", () =>
                {
                    GameRoot.GetManager<VitalityPointsManager>().AdjustVolityPoints(-1);
                    GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
                    //GameRoot.GetManager<UIManager>().HidePanel(E_UIPanelType.MessagePanel);
                }));
            return;
        }
    }

    bool EnemyWinBattle()
    {

        Debug.Log(playerControllers.Count + "??玩家人数");
        foreach (var battler in playerControllers)
        {
            if (!battler.State_Dead)
                return false;
        }
        return true;
    }
    bool PlayerWinBattle()
    {
        Debug.Log(enemyControllers.Count + "??敌人人数");
        foreach (var battler in enemyControllers)
        {
            if (!battler.State_Dead)
                return false;
        }
        return true;
    }
    /// <summary>
    /// 每当战斗中的角色出现变动，会更新目标
    /// </summary>
    /// <param name="skillTargetType"></param>
    /// <returns></returns>
    public List<BattlerStateTag> GetSkillTarget(E_SkillTargetType skillTargetType)
    {
        List<BattlerStateTag> targets = new List<BattlerStateTag>();

        switch (skillTargetType)
        {
            case E_SkillTargetType.对单体:
                int randomIndex1 = UnityEngine.Random.Range(0, playerControllers.Count);
                //targets.Add(playerControllers[randomIndex1]);
                //Debug.Log(playerControllers.Count+"sdjjd");
                targets.Add(playerControllers[0]);
                break;
            case E_SkillTargetType.对全体:
                for (int i = 0; i < playerControllers.Count; i++)
                    targets.Add(playerControllers[i]);
                break;
            case E_SkillTargetType.对N目标:

                break;
            default:
                break;
        }

        return targets;
    }


    public override void MgrUpdate(float deltaTime)
    {

    }

}
