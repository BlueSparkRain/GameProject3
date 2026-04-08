using Core;
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

    /// <summary>
    /// 随机产生低等级怪物
    /// </summary>
    /// <param name="_enemyCharacterType"></param>
    public BattleHexRoom(E_BattleType _battleType)
    {
        battleType= _battleType;
        enemyCharacterDataSO = Resources.Load<CharacterDataSO>(enemyCharacterSoDataPath + enemyCharacterType);
    }

    public void LoadCharacterType() {
        //如果存档中有记录，加载对应的类型
        //如果没有，表示第一次获取，
        //对应等级的怪物池子中抽取一种随机的怪物
        E_CharacterType _characterType = E_CharacterType.LE_1;
        enemyCharacterType= _characterType;

        //之后根据具体的类型加载对应的模型
        //和
        //存档/初始化的怪物数据
    }

    public void DoHexRoomLogic(UnityAction roomJob)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        //读取敌人信息，并进入战斗场景
        Debug.Log("玩家进入战斗房间");
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);
        //GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("BattleScene", SceneSwitchManager.LoadMode.Single);
        
    }

    public void DoHexRoomModel()
    {
        //根据战斗类型，从对应的池子里取出随机的怪物数据
        //根据具体怪物数据产生对应的模型



    }
}
