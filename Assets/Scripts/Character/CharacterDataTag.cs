using Core;
using UnityEngine;

/// <summary>
/// 负责角色数据的读取和更新(升级)/阵营/角色死亡时通知战斗结束
/// </summary>
public class CharacterDataTag : MonoBehaviour
{
    public bool isPlayer { get; private set; }
    IUpGradable iUpgrade;
    IBattlable ibattle;

    CharacterLevelUpHandler levelUpHandler;
    CharacterData characterData;
    public CharacterData CharacterData => characterData;
    void TryGetSaveData() {
    }
    void OnDisable(){
        if (isPlayer)
            EventCenter.RemoveEventListener(E_EventType.PlayerBeforeIntoBattle, OnPlayerBeforeIntoBattle);
    }
    void OnPlayerBeforeIntoBattle(){
        GameRoot.GetManager<GameBattleManager>().RegisterPlayerToBattle(characterData);
    }
    /// <summary>
    /// 初始化角色数据标签
    /// </summary>
    /// <param name="characterType"></param>
    /// <param name="isPlayer"></param>
    /// <param name="canLevelUP"></param>
    public void InitCharacterDataTag(E_CharacterType characterType, bool isPlayer, bool canLevelUP){
        characterData = new CharacterData(characterType);
        this.isPlayer = isPlayer;
        if (isPlayer){
            ibattle = new Player();
            //只有玩家角色才会注册自身到玩家方
            EventCenter.AddEventListener(E_EventType.PlayerBeforeIntoBattle, OnPlayerBeforeIntoBattle);
        }
        else ibattle = new Enemy();
        if (canLevelUP)
            iUpgrade = new LevelUpGradeMode(characterType, characterData);
        else iUpgrade = new StageUpGradeMode(characterType, characterData);

        if (canLevelUP){
            levelUpHandler = GetComponent<CharacterLevelUpHandler>();
            levelUpHandler.InitLevelHandler(characterData, iUpgrade);

            GetComponent<CharacterMapMoveHandle>().InitMover(isPlayer, characterType);
        }
    }

    void Update(){

        if (isPlayer && Input.GetKeyDown(KeyCode.B)){
            iUpgrade.UpGrade();
        }
    }
}
