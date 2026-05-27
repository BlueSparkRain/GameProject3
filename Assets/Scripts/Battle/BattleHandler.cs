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
    BattleDotHandler  dotHandler;
    BattleDamageHandler damageHandler;
    BattlerStateTag battlerStateTag;

    bool start=false;
    public void InitBattler(CharacterData characterData){
        bool isplayer = (characterData.characterType == E_CharacterType.P_1 ||
                    characterData.characterType == E_CharacterType.P_2 ||
                    characterData.characterType == E_CharacterType.P_3);

        self = isplayer ? new Player(GetComponent<BattleDamageHandler>()) : new Enemy(GetComponent<BattleDamageHandler>());
        //注册战斗单位
        BattleTargetSelector.RegisteNewBattler(self);
        battlerStateTag = new BattlerStateTag();
        //启动MVCHandler
        MVCHandler = GetComponentInChildren<BattleMVCHandler>();
        MVCHandler.InitMVCHandler(characterData,battlerStateTag);
        
        //启动BuffHandler
        buffHandler=GetComponent<BattleBuffHandler>();
        buffHandler.InitBattleBuffHandle(self);

        //启动DotHandler
        dotHandler=GetComponentInChildren<BattleDotHandler>();
        dotHandler.InitBattleDotHandle(self);

        //启动BattleDamageHandler
        damageHandler= GetComponentInChildren<BattleDamageHandler>();
        damageHandler.InitDataHandler(MVCHandler,buffHandler,dotHandler);

        //启动BattleSkiller
        skillHandler = GetComponentInChildren<BattleSkillHandler>();
        skillHandler.InitBattleSkillHandler(self, MVCHandler,battlerStateTag);

        start=true;
        Debug.Log(characterData.Character_Name + "---加入战斗");
    }

    void Update(){
        if (start) { 
            skillHandler.OnSkillerUpdate();
            buffHandler.OnBuffUpdate();
            dotHandler.OnDotUpdate();
            MVCHandler.OnMVCHandlerUpdate();
        }   
    }
}
