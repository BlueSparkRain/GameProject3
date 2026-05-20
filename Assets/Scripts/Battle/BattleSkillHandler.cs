using UnityEngine;

public class BattleSkillHandler : MonoBehaviour
{
    public SkillIconSpawner normalSkillIconSpawner;
    public SkillIconSpawner atbSkillIconSpawner;
    BattleSkiller battleSkiller;
    Battle_Controller battleController;
    bool battleEnd=false;

    public void InitBattleSkillHandler(IBattlable self, BattleMVCHandler battleMVCHandle){
        battleSkiller = new BattleSkiller(normalSkillIconSpawner, atbSkillIconSpawner,self);
        this.battleController = battleMVCHandle.BattleController;
        EventCenter.AddEventListener<float>(E_EventType.SkillExcute, SkillCost);
        EventCenter.AddEventListener<Battle_Controller>(E_EventType.CharacterDead, StopCylcle);
    }

    public void OnSkillerUpdate(){
        if (battleEnd)
            return;

        //只有背包技能才会自动循环释放
        if (!battleController.charcaterDead){
            battleSkiller.OnSkillUpdate(battleController.GetCharacterModelValue(E_BattleModelType.SP));
        }
    }

    /// <summary>
    /// 本角色死亡，技能停止循环
    /// </summary>
    /// <param name="battler"></param>
    void StopCylcle(Battle_Controller battler){
        if (battler != battleController) return;
        SelfEnd();
    }

    void SelfEnd() => battleEnd = true;

    void SkillCost(float sp_cost){
        Debug.Log(battleController.CharacterData.Character_Name + "消耗了蓝量：" + sp_cost + "--------------");
        battleController.AdjustCharacterModelValue(E_BattleModelType.SP, -sp_cost);
    }
}
