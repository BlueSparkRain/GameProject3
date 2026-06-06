using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Interfaces;
using UnityEngine;

/// <summary>
/// 地图场景下的战斗触发管理器/数据搬运工
/// </summary>
public class GameBattleManager : IGlobalManager{
    float spawnInterval = 0.2f;
    WaitForSeconds delay;

    List<CharacterData> playersData = new List<CharacterData>();
    List<CharacterData> enemysData = new List<CharacterData>();
    GameMapManager gameMapManager;
    int battleRadius = 2;
    int max_enemyNum = 3;

    public void MgrInit(GameRoot gameRoot){
        delay = new WaitForSeconds(spawnInterval);
        EventCenter.AddEventListener(E_EventType.PlayerOutBattle, UnregisterCharacterToBattle);
    }
    public void MgrDispose(){
        EventCenter.RemoveEventListener(E_EventType.PlayerOutBattle, UnregisterCharacterToBattle);
    }
    public void MgrUpdate(float deltatime) { }

    /// <summary>
    /// [MapScene]注册一个玩家角色数据
    /// </summary>
    public void RegisterPlayerToBattle(CharacterData data){
        playersData.Add(data);
    }

    /// <summary>
    /// 当玩家进入一个战斗房间时触发,扫描一定半径范围内的敌人并自动加入战斗
    /// </summary>
    public void CheckBattleEnemy(HexRoomTag roomTag){
        if(!gameMapManager) gameMapManager=GameRoot.GetManager<GameMapManager>();
        List<Vector2Int> radiusRowCols = HexCoordinateUtility.GetRowColsInRadius(roomTag.row,roomTag.col, battleRadius);
        Debug.Log("[GameBattleManager]-----开始扫描战斗中近邻范围内的敌人:" + radiusRowCols.Count);
        for (int i = 0; i < radiusRowCols.Count ; i++)
        {
            HexRoomTag cur_room = gameMapManager.GetTargetRoom(radiusRowCols[i]);
            if (cur_room && enemysData.Count< max_enemyNum) {

                var roomType = cur_room.GetComponent<HexRoomStyleHandler>().RoomType;
                if (roomType==E_HexRoomType.Battle_LowLevel_战斗_杂鱼||
                    roomType == E_HexRoomType.Battle_MidLevel_战斗_精英) {
                    CharacterData enemyData = new CharacterData((cur_room.IHexRoom as BattleHexRoom).EnemyType);
                    Debug.Log("检测到一只怪物：---"+ (cur_room.IHexRoom as BattleHexRoom).EnemyType);
                    //根据当前的混沌等级，缩放原始数值
                    var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
                    if (chaosMgr != null)
                        ApplyChaosScaling(enemyData, chaosMgr.EnemyStrengthMultiplier);
                    RegisterEnemyToBattle(enemyData);
                }
            }
        }
        Debug.Log("[GameBattleManager]---战斗注册结束:" + enemysData.Count+"名敌人");
    }

    /// <summary>
    /// [MapScene]注册一个敌人角色数据
    /// </summary>
    void RegisterEnemyToBattle(CharacterData data){
        enemysData.Add(data);
    }

    /// <summary>
    /// [MapScene]让敌人的AI扫描所在的区块，触发英雄对决
    /// </summary>
    public void RegisterAIEnemyToBattle() {

    }

    /// <summary>
    /// [MapScene]战斗结束后清空历史数据，让新地图随战斗生成新的当前地块内容
    /// </summary>
    void UnregisterCharacterToBattle(){
        Debug.Log("清除战斗场景内战斗注册信息");
        Debug.Log($"清空前{playersData.Count}---{enemysData.Count}");
        playersData.Clear();
        enemysData.Clear();
        Debug.Log($"清空后{playersData.Count}---{enemysData.Count}");
    }

    /// <summary>
    /// 根据之前注册的战斗信息，在战斗场景中生成战斗角色
    /// </summary>
    public void SpawnBattleCharacter() {
        CoroutineManager corManager= GameRoot.GetManager<CoroutineManager>();
        corManager.StartCoroutine(SpawnBattleCardByData(playersData,true));
        corManager.StartCoroutine(SpawnBattleCardByData(enemysData, false));
    }

    /// <summary>
    /// [BattleScene]根据战斗数据列表，生成相应的初始战斗角色
    /// </summary>
    IEnumerator SpawnBattleCardByData(List<CharacterData> datas,bool isPlayer){
        BattleLoadManager battleLoadManager=GameRoot.GetManager<BattleLoadManager>();
        if (isPlayer){
            Debug.Log("生成Battle的PlayerCamp" + datas.Count);
            for (int i = 0; i < datas.Count; i++){
                battleLoadManager.LoadAPlayer(datas[i]);
                yield return delay;
            }
        }
        else {
            Debug.Log("生成Battle的EnemyCamp"+datas.Count);
            for (int i = 0; i < datas.Count; i++)
            {
                battleLoadManager.LoadAEnemy(datas[i]);
                yield return delay;
            }
        }
    }

    void ApplyChaosScaling(CharacterData data, float multiplier){
        if (multiplier <= 1f) return;
        data.AdjustProperty(E_CharacterPropertyType.Phy_Attack, multiplier, use_multi: true);
        data.AdjustProperty(E_CharacterPropertyType.Mag_Attack, multiplier, use_multi: true);
        data.AdjustProperty(E_CharacterPropertyType.Maximum_Health, multiplier, use_multi: true);
        data.AdjustProperty(E_CharacterPropertyType.Phy_Resistance, multiplier, use_multi: true);
        data.AdjustProperty(E_CharacterPropertyType.Mag_Resistance, multiplier, use_multi: true);
    }
}
