using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class BattleHexRoom : IHexRoom
{
    E_CharacterType enemyCharacterType;
    string enemyCharacterSoDataPath = "SOData/CharacterSOData/";
    E_BattleType  battleType;
    //根据随机的结果去加载对应的怪物
    //需存档：记录战斗房间历史的具体怪物类型
    CharacterDataSO enemyCharacterDataSO;
    public E_CharacterType EnemyType=>enemyCharacterType;

    float scaleRate=1;
    HexRoomTag  roomTag;
    /// <summary>
    /// 随机产生低等级怪物
    /// </summary>
    /// <param name="_enemyCharacterType"></param>
    public BattleHexRoom(HexRoomTag _roomTag ,E_BattleType _battleType){
        battleType= _battleType;
        roomTag= _roomTag;
    }
    public void DoHexRoomInit(){
        //如果存档中有记录，加载对应的类型
        //如果没有，表示第一次获取，
        //对应等级的怪物池子中抽取一种随机的怪物

        E_CharacterType _characterType=E_CharacterType.LE_1;
        switch (battleType){
            case E_BattleType.杂鱼敌人:
                scaleRate = 0.8f;
                //后期从类型池中随机取出一个
                _characterType = E_CharacterType.LE_1;
                break;
            case E_BattleType.精英敌人:
                scaleRate = 1f;
                _characterType = E_CharacterType.ME_1;
                break;
            case E_BattleType.首领敌人:
                scaleRate = 1.5f;
                _characterType = E_CharacterType.Boss_1;
                break;
            default:
                break;
        }

        enemyCharacterType = _characterType;
        enemyCharacterDataSO = Resources.Load<CharacterDataSO>(enemyCharacterSoDataPath + enemyCharacterType);

        //之后根据具体的类型加载对应的模型
        //和
        //存档/初始化的怪物数据
    }
    int num = 0;
    public void DoHexRoomLogic(UnityAction roomJob){
        GameBattleManager gameBattleManager=GameRoot.GetManager<GameBattleManager>();
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        //读取敌人信息，并进入战斗场景
        //将玩家信息先进行注册
        EventCenter.EventTrigger(E_EventType.PlayerBeforeIntoBattle);
        Debug.Log("进入战斗房间"+num++);
        gameBattleManager.CheckBattleEnemy(roomTag);
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);  
    }

    public void DoHexRoomModel(Vector3 modelPos){
        //根据战斗类型，从对应的池子里取出随机的怪物数据
        //根据具体怪物数据产生对应的模型
        var charac=MapCharacterCaller.CallNewCharacter("DisMoveable");
        charac.InitCharacterDataTag(enemyCharacterType, false, false);
        charac.transform.localScale = Vector3.zero;
        charac.transform.localPosition= modelPos;

        charac.transform.DOScale(scaleRate, 0.5f);
        //charac.transform.localScale*=scaleRate;
    }
}
