//using Core;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor.U2D.Animation;
//using UnityEngine;

//public class CharacterHandler : MonoBehaviour
//{
//    public bool isPlayer { get; private set; }
//    IUpGradable iupgradeHandle;
//    IBattlable ibattle;

//    CharacterData characterData;

//    void TryGetSaveData()
//    {
//    }
//    void OnDisable()
//    {
//        if (isPlayer)
//            EventCenter.RemoveEventListener(E_EventType.PlayerBeforeIntoBattle, OnPlayerBeforeIntoBattle);
//    }
//    void OnPlayerBeforeIntoBattle()
//    {
//        GameRoot.GetManager<GameBattleManager>().RegisterPlayerToBattle(characterData);
//    }
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="characterType"></param>
//    /// <param name="isPlayer"></param>
//    /// <param name="canLevelUP"></param>
//    public void InitCharacterHandler(E_CharacterType characterType, bool isPlayer, bool canLevelUP){
//        characterData = new CharacterData(characterType);
//        this.isPlayer = isPlayer;
//        if (isPlayer){
//            ibattle = new Player();
//            //只有玩家角色才会注册自身到玩家方
//            EventCenter.AddEventListener(E_EventType.PlayerBeforeIntoBattle, OnPlayerBeforeIntoBattle);
//        }
//        else ibattle = new Enemy();

//        if (canLevelUP)
//            iupgradeHandle = new LevelUpGradeMode(characterType, characterData);
//        else iupgradeHandle = new StageUpGradeMode(characterType, characterData);

//        //具有移动能力的角色
//        if(canLevelUP)
//        GetComponent<CharacterMapMoveHandle>().InitMover(isPlayer, characterType);
        
      
//    }


//    void Update()
//    {

//        if (isPlayer && Input.GetKeyDown(KeyCode.B))
//        {
//            iupgradeHandle.UpGrade();
//            //JsonSaver.Save(new Save_CharacterData(characterData),(characterData.characterType).ToString());
//        }
//    }
//}
