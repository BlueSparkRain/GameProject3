using UnityEngine;

public class BattleSkillHandler : MonoBehaviour
{
    IBattlable self;
    public SkillIconSpawner normalSkillIconSpawner;
    public SkillIconSpawner atbSkillIconSpawner;
    BattleSkiller battleSkiller;
    Battle_Controller battleController;
    BattlerStateTag battlerStateTag;
    bool battleEnd=false;

    public void InitBattleSkillHandler(IBattlable _self, BattleMVCHandler _battleMVCHandle, BattlerStateTag _battlerStateTag)
    {
        self = _self;
        battlerStateTag = _battlerStateTag;
        battleSkiller = new BattleSkiller(normalSkillIconSpawner, atbSkillIconSpawner,_self);
        battleController = _battleMVCHandle.BattleController;
        EventCenter.AddEventListener<IBattlable, float>(E_EventType.SkillExcute, SkillCost);
        EventCenter.AddEventListener<BattlerStateTag>(E_EventType.Battle_CharacterDead, StopCylcle);
    }

    public void OnSkillerUpdate(){
        if (battleEnd)
            return;

        //只有背包技能才会自动循环释放
        if (!battlerStateTag.State_Dead){
            battleSkiller.OnSkillUpdate(battleController.GetCharacterModelValue(E_BattleModelType.SP));
        }
    }

    /// <summary>
    /// 本角色死亡，技能停止循环
    /// </summary>
    /// <param name="battler"></param>
    void StopCylcle(BattlerStateTag battler){
        if (battler != battlerStateTag) return;
        SelfEnd();
    }

    void SelfEnd() => battleEnd = true;

    void SkillCost(IBattlable skillOwner, float sp_cost){
        if (skillOwner != self) return;
        Debug.Log($"{battleController.CharacterData.Character_Name}消耗了蓝量:{sp_cost}");
        battleController.AdjustCharacterModelValue(E_BattleModelType.SP, -sp_cost);
    }
}
