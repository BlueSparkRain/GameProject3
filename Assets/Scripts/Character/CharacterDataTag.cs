using Core;
using UnityEngine;

/// <summary>
/// ������ɫ���ݵĶ�ȡ�͸���(����)����Ӫ
/// ������ɫ����ʱ��������ֵ����
/// </summary>
public class CharacterDataTag : MonoBehaviour
{
    public bool isPlayer { get; private set; }
    IUpGradable iUpgrade;
    IBattlable ibattle;
    //Save_CharacterData save_characterData;

    CharacterLevelUpHandler levelUpHandler;
    CharacterData characterData;
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
    /// 
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


        if (canLevelUP)
        {
            levelUpHandler = GetComponent<CharacterLevelUpHandler>();
            levelUpHandler.InitLevelHandler(characterData, iUpgrade);

            GetComponent<CharacterMapMoveHandle>().InitMover(isPlayer, characterType);
        }

    }


    void Update(){

        if (isPlayer && Input.GetKeyDown(KeyCode.B)){
            iUpgrade.UpGrade();
            //JsonSaver.Save(new Save_CharacterData(characterData),(characterData.characterType).ToString());
        }
    }

}
