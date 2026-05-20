using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Interfaces;
using UnityEngine;

/// <summary>
/// 负责地图场景中战斗触发的数据搬运工作
/// </summary>
public class GameBattleManager : IGlobalManager
{
    float spawnInterval = 0.2f;
    WaitForSeconds delay;

    List<CharacterData> playersData = new List<CharacterData>();
    List<CharacterData> enemysData = new List<CharacterData>();
    GameMapManager gameMapManager;
    int battleRadius = 4;
    int max_enemyNum = 3;


    /// <summary>
    /// [MapScene]注册一个玩家角色数据
    /// 通过
    /// </summary>
    /// <param name="data"></param>
    public void RegisterPlayerToBattle(CharacterData data){
        Debug.Log("玩家数据加载了一份！！！！！！！！！！");
        playersData.Add(data);
    }
    /// <summary>
    ///当玩家进入一个战斗房间时触发,将一定半径范围内的其他普通敌人拉入战斗
    /// </summary>
    /// <param name="roomTag"></param>
    public void CheckBattleEnemy(HexRoomTag roomTag)
    {
        if(!gameMapManager) gameMapManager=GameRoot.GetManager<GameMapManager>();
        Debug.Log(roomTag+"?????????????????????????????????????");
        List<Vector2Int> radiusRowCols = HexCoordinateUtility.GetRowColsInRadius(roomTag.row,roomTag.col, battleRadius);
        Debug.Log("[GameBattleManager]-----开始检索到本场战斗敌人数量，样本数量:" + radiusRowCols.Count);
        for (int i = 0; i < radiusRowCols.Count ; i++)
        {
            //检测其中的战斗房间
            HexRoomTag cur_room = gameMapManager.GetTargetRoom(radiusRowCols[i]);
            if (cur_room && enemysData.Count< max_enemyNum) {

                var roomType = cur_room.GetComponent<HexRoomStyleHandler>().RoomType;
                if (roomType==E_HexRoomType.Battle_LowLevel_战斗_杂鱼||
                    roomType == E_HexRoomType.Battle_MidLevel_战斗_精英) {
                    //一定要找到
                    CharacterData enemyData = new CharacterData((cur_room.IHexRoom as BattleHexRoom).EnemyType);
                    Debug.Log("发现一只怪物：---"+ (cur_room.IHexRoom as BattleHexRoom).EnemyType);
                    //检查目前的混沌等级，进行原始数值调整
                    //...
                    RegisterEnemyToBattle(enemyData);
                }
            }
        }
        Debug.Log("[GameBattleManager]-----检索到本场战斗" + enemysData.Count+"个敌人");
    }

    /// <summary>
    /// [MapScene]注册一个敌人角色数据
    /// </summary>
    /// <param name="data"></param>
    void RegisterEnemyToBattle(CharacterData data){
        enemysData.Add(data);
    }

    /// <summary>
    /// [MapScene]当玩家走上AI敌人所在的地块，进行英雄对决
    /// </summary>
    public void RegisterAIEnemyToBattle() { 
    
    }

    /// <summary>
    /// [MapScene]战斗结束，放弃历史数据，并更新地图中这个战斗房间的当前地块类型
    /// </summary>
    void UnregisterCharacterToBattle(){
        Debug.Log("玩家脱战，已清空对战注册信息");
        playersData.Clear();
        enemysData.Clear();
    }

    /// <summary>
    /// 根据之前注册到的战斗信息，在战斗场景中产生战斗对象
    /// </summary>
    public void SpawnBattleCharacter() {
        CoroutineManager corManager= GameRoot.GetManager<CoroutineManager>();
        corManager.StartCoroutine(SpawnBattleCardByData(playersData,true));
        corManager.StartCoroutine(SpawnBattleCardByData(enemysData, false));
    }

    /// <summary>
    /// [BattleScene]在战斗场景中根据数据，产生对应的初始战斗对象
    /// </summary>
    IEnumerator SpawnBattleCardByData(List<CharacterData> datas,bool isPlayer)
    {
        //寻找场景中的负责产生战斗对象的管理器
        BattleLoadManager battleLoadManager=GameRoot.GetManager<BattleLoadManager>();
        if (isPlayer){
            Debug.Log("加载Battle：玩家" + datas.Count);
            for (int i = 0; i < datas.Count; i++){
                battleLoadManager.LoadAPlayer(datas[i]);
                yield return delay;
            }
        }
        else {
            Debug.Log("加载Battle：敌人"+datas.Count);
            for (int i = 0; i < datas.Count; i++)
            {
                battleLoadManager.LoadAEnemy(datas[i]);
                yield return delay;
            }
        }
    }
    public void MgrInit(GameRoot gameRoot){
        WaitForSeconds delay = new WaitForSeconds(spawnInterval);
        EventCenter.AddEventListener(E_EventType.PlayerOutBattle,UnregisterCharacterToBattle);
    }
    public void MgrDispose(){
        EventCenter.RemoveEventListener(E_EventType.PlayerOutBattle,UnregisterCharacterToBattle);
    }
    public void MgrUpdate(float deltatime){}
}
