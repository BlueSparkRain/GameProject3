using UnityEngine;

[RequireComponent(typeof(BattleMVCHandler),
    typeof(BattleDamageHandler),
    typeof(BattleSkillHandler))]
/// <summary>
/// 负责本战斗对象上所有组件的初始化
/// </summary>
public class BattleHandler : MonoBehaviour
{
    IBattlable self;
    BattleMVCHandler MVCHandler;
    BattleSkillHandler skillHandler;
    BattleBuffHandler buffHandler;
    BattleDamageHandler damageHandler;
    BattlerStateTag battlerStateTag;

    bool start=false;
    public void InitBattler(CharacterData characterData){
        bool isplayer = (characterData.characterType == E_CharacterType.P_1 ||
                    characterData.characterType == E_CharacterType.P_2 ||
                    characterData.characterType == E_CharacterType.P_3);

        self = isplayer ? new Player(GetComponent<BattleDamageHandler>()) : new Enemy(GetComponent<BattleDamageHandler>());
        //注册战斗单位
        BattleTargetSelector.RegisteABattler(self);
        battlerStateTag = new BattlerStateTag();
        //启动BattleMVC
        MVCHandler = GetComponentInChildren<BattleMVCHandler>();
        MVCHandler.InitMVCHandler(characterData,battlerStateTag);
        
        //启动Buff
        buffHandler=GetComponent<BattleBuffHandler>();
        buffHandler.InitBattleBufferHandle(self);

        //启动BattleDamageHandler
        damageHandler= GetComponentInChildren<BattleDamageHandler>();
        damageHandler.InitDataHandler(MVCHandler,buffHandler);

        //启动BattleSkiller
        skillHandler = GetComponentInChildren<BattleSkillHandler>();
        skillHandler.InitBattleSkillHandler(self, MVCHandler,battlerStateTag);

        start=true;
        Debug.Log(characterData.Character_Name + "-----本角色开始战斗");
    }

    void Update(){
        if (start) { 
            skillHandler.OnSkillerUpdate();
            buffHandler.OnBuffUpdate();
        }   
    }
}
