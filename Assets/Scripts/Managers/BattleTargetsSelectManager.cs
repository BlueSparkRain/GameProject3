using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using UnityEditor.Rendering;

/// <summary>
/// 获取到战斗中的所有单位，在任意技能初始化时分配施法目标
/// </summary>
public class BattleTargetsSelectManager : MonoSceneManager
{
    private List<CharacterBattle_Controller> playerControllers = new List<CharacterBattle_Controller>();
    private List<CharacterBattle_Controller> enemyControllers = new List<CharacterBattle_Controller>();

    //记录本局战斗中所有的技能释放者，方便技能初始化时分配目标
    protected override void MgrOnInit(){
        base.MgrOnInit();
        EventCenter.AddEventListener<CharacterBattle_Controller>(E_EventType.CharacterDead, CheckBattleEnd);
    }

    /// <summary>
    /// 注册一名skiller
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="battleController"></param>
    public void RegisterSkiller(bool isPlayer, CharacterBattle_Controller battleController)
    {
        (isPlayer ? playerControllers : enemyControllers).Add(battleController);
    }

    void CheckBattleEnd(CharacterBattle_Controller characterBattle_Controller) {
        if (PlayerWinBattle()){
            EventCenter.EventTrigger(E_EventType.BattleEnd);
            GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,
                p => p.SetMessage("你获胜了", ()=> { GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
                }));
            return;
        }
        if (EnemyWinBattle()) { 
            EventCenter.EventTrigger(E_EventType.BattleEnd);
            GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel, 
                p => p.SetMessage("你失败了", () => { GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene");
                    //GameRoot.GetManager<UIManager>().HidePanel(E_UIPanelType.MessagePanel);
                }));
            return;
        }
    }

    bool EnemyWinBattle() {

        Debug.Log(playerControllers.Count+"??玩家人数");
        foreach (var battler in playerControllers){
            if (!battler.charcaterDead)
                return false;
        }
        return true;
    }
    bool PlayerWinBattle() {
        Debug.Log(enemyControllers.Count + "??敌人人数");
        foreach (var battler in enemyControllers){
            if(!battler.charcaterDead)
            return false;
        }
        return true;
    }
    /// <summary>
    /// 每当战斗中的角色出现变动，会更新目标
    /// </summary>
    /// <param name="skillTargetType"></param>
    /// <returns></returns>
    public List<CharacterBattle_Controller> GetSkillTarget(E_SkillTargetType skillTargetType){
        List<CharacterBattle_Controller> targets = new List<CharacterBattle_Controller>();

        switch (skillTargetType){
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
            //case E_SkillTargetType.对敌方单体:
            //    int randomIndex2 = UnityEngine.Random.Range(0, enemyControllers.Count);
            //    //Debug.Log(randomIndex2 +" "+ enemyControllers.Count);
            //    targets.Add(enemyControllers[randomIndex2]);
            //    break;
            //case E_SkillTargetType.对敌方全体:
            //    for (int i = 0; i < enemyControllers.Count; i++)
            //        targets.Add(enemyControllers[i]);
                //break;
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
