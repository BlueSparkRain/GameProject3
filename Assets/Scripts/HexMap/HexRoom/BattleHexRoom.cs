using Core;
using UnityEngine;
using UnityEngine.Events;

public class BattleHexRoom : IHexRoom
{
    public E_CharacterType enemyCharacterType;

    CharacterDataSO enemyCharacterDataSO;
    string enemyCharacterSoDataPath = "SOData/CharacterSOData/";

    /// <summary>
    /// 随机产生低等级怪物
    /// </summary>
    /// <param name="_enemyCharacterType"></param>
    public BattleHexRoom(E_CharacterType _enemyCharacterType)
    {
        enemyCharacterType = _enemyCharacterType;
        enemyCharacterDataSO = Resources.Load<CharacterDataSO>(enemyCharacterSoDataPath + enemyCharacterType);
    }
    public void DoRoomLogic(UnityAction roomJob)
    {
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        //读取敌人信息，并进入战斗场景
        Debug.Log("玩家进入战斗房间");
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);
        //GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("BattleScene", SceneSwitchManager.LoadMode.Single);
        
    }
}
