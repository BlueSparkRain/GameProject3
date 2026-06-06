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

    public void InitBattleSkillHandler(IBattlable _self, BattleMVCHandler _battleMVCHandle, BattlerStateTag _battlerStateTag){
        self = _self;
        battlerStateTag = _battlerStateTag;
        battleSkiller = new BattleSkiller(normalSkillIconSpawner, atbSkillIconSpawner,_self, battlerStateTag);
        battleController = _battleMVCHandle.BattleController;
        EventCenter.AddEventListener<IBattlable, float>(E_EventType.SkillExcute, SkillCost);
        EventCenter.AddEventListener<BattlerStateTag>(E_EventType.Battle_CharacterDead, StopCylcle);
    }

    public BattleSkiller GetSkiller() => battleSkiller;

    public void OnSkillerUpdate(){
        if (battleEnd)
            return;
        if (!battlerStateTag.State_Dead){
            battleSkiller.OnSkillUpdate(battleController.GetCharacterModelValue(E_BattleModelType.SP));
            battleSkiller.OnATBUpdate(Time.deltaTime);
        }
    }
    void StopCylcle(BattlerStateTag battler){
        if (battler != battlerStateTag) return;
        SelfEnd();
    }
    void SelfEnd(){
        battleEnd = true;
        battleSkiller.StopATB();
    }
    void SkillCost(IBattlable skillOwner, float sp_cost){
        if (skillOwner != self) return;
        Debug.Log($"{battleController.CharacterData.Character_Name}释放了自动技能，消耗{sp_cost}");
        battleController.AdjustCharacterModelValue(E_BattleModelType.SP, -sp_cost);
    }
}
