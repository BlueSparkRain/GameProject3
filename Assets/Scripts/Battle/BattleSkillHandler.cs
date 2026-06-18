using Core;
using UnityEngine;

public class BattleSkillHandler : MonoBehaviour
{
    IBattlable self;
    public SkillIconSpawner normalSkillIconSpawner;
    public SkillIconSpawner atbSkillIconSpawner;
    BattleSkiller battleSkiller;
    Battle_Controller battleController;
    BattlerStateTag battlerStateTag;
    bool battleEnd;

    public void InitBattleSkillHandler(IBattlable _self, BattleMVCHandler _battleMVCHandle, BattlerStateTag _battlerStateTag){
        self = _self;
        battlerStateTag = _battlerStateTag;
        battleSkiller = new BattleSkiller(normalSkillIconSpawner, atbSkillIconSpawner,_self, battlerStateTag);
        battleController = _battleMVCHandle.BattleController;
        EventCenter.AddEventListener<IBattlable, float>(E_EventType.SkillExcute, SkillCost);
        EventCenter.AddEventListener<BattlerStateTag>(E_EventType.Battle_CharacterDead, StopCylcle);
        EventCenter.AddEventListener(E_EventType.BattleEnd, OnBattleEnd);
    }

    public BattleSkiller GetSkiller() => battleSkiller;

    public void OnSkillerUpdate(){
        if (battleEnd)
            return;

        var phaseMgr = GameRoot.GetManager<BattlePhaseManager>();
        if (phaseMgr != null && phaseMgr.CurrentPhase != BattlePhase.InProgress)
            return;

        if (!battlerStateTag.State_Dead){
            float sp = battleController.GetCharacterModelValue(E_BattleModelType.SP);
            battleSkiller.OnSkillUpdate(sp);
            battleSkiller.OnATBUpdate(Time.deltaTime);
            if (atbSkillIconSpawner != null)
                battleSkiller.OnATBModeUpdate(sp);
        }
    }
    void StopCylcle(BattlerStateTag battler){
        if (battler != battlerStateTag) return;
        SelfEnd();
    }
    void OnBattleEnd(){
        SelfEnd();
    }
    void SelfEnd(){
        battleEnd = true;
        battleSkiller.StopATB();
    }
    void OnDestroy(){
        EventCenter.RemoveEventListener<IBattlable, float>(E_EventType.SkillExcute, SkillCost);
        EventCenter.RemoveEventListener<BattlerStateTag>(E_EventType.Battle_CharacterDead, StopCylcle);
        EventCenter.RemoveEventListener(E_EventType.BattleEnd, OnBattleEnd);
    }
    void SkillCost(IBattlable skillOwner, float sp_cost){
        if (skillOwner != self) return;
        if (sp_cost <= 0) return;  
        DebugManager.Log(EDebugCategory.BattleState,$"{battleController.CharacterData.Character_Name}释放了自动技能，消耗{sp_cost}");
        battleController.AdjustCharacterModelValue(E_BattleModelType.SP, -sp_cost);
    }
}
