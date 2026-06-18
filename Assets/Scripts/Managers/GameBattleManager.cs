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

    /// <summary>当前触发的战斗房间（玩家踩上的那个，战败时用于踢出玩家）</summary>
    HexRoomTag _currentBattleRoom;
    /// <summary>本次战斗中所有被拉入的 BattleRoom（胜利后全部消耗）</summary>
    List<HexRoomTag> _battleRoomsInCombat = new List<HexRoomTag>();
    /// <summary>战败后下次加载MapScene时需要踢出玩家</summary>
    bool _pendingKickOnLoad;
    /// <summary>战败回场时抑制战斗房间触发（让玩家先站在房间上再被踢开）</summary>
    public bool SuppressBattleTrigger { get; set; }

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
        _currentBattleRoom = roomTag;
        _battleRoomsInCombat.Clear();
        _battleRoomsInCombat.Add(roomTag);  // 玩家踩上的房间

        if(!gameMapManager) gameMapManager=GameRoot.GetManager<GameMapManager>();
        List<Vector2Int> radiusRowCols = HexCoordinateUtility.GetRowColsInRadius(roomTag.row,roomTag.col, battleRadius);
        for (int i = 0; i < radiusRowCols.Count ; i++){
            HexRoomTag cur_room = gameMapManager.GetTargetRoom(radiusRowCols[i]);
            if (cur_room && enemysData.Count< max_enemyNum) {
                var roomType = cur_room.GetComponent<HexRoomStyleHandler>().RoomType;
                if (roomType == E_HexRoomType.Battle_LowLevel ||
                    roomType == E_HexRoomType.Battle_MidLevel) {
                    var battleLogic = cur_room.RoomLogic as BattleRoomLogic;
                    if (battleLogic == null) continue;
                    CharacterData enemyData = new CharacterData(battleLogic.EnemyType);
                    var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
                    if (chaosMgr != null)
                        ApplyChaosScaling(enemyData, chaosMgr.EnemyStrengthMultiplier);
                    RegisterEnemyToBattle(enemyData);
                    // 记录此敌人对应的 BattleRoom
                    if (!_battleRoomsInCombat.Contains(cur_room))
                        _battleRoomsInCombat.Add(cur_room);
                }
            }
        }
        DebugManager.Log(EDebugCategory.MapRoom, $"[GameBattleManager] 战斗注册: {enemysData.Count}名敌人, {_battleRoomsInCombat.Count}个BattleRoom");
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
        DebugManager.Log(EDebugCategory.MapRoom, "清除战斗场景内战斗注册信息");
        playersData.Clear();
        enemysData.Clear();
        _battleRoomsInCombat.Clear();
    }

    /// <summary>战斗结果处理：胜利消耗所有参战房间，战败标记踢出</summary>
    public void OnBattleResult(bool playerWin)
    {
        if (playerWin)
        {
            foreach (var room in _battleRoomsInCombat)
            {
                if (room == null) continue;
                var battleLogic = room.RoomLogic as BattleRoomLogic;
                battleLogic?.Consume();
            }
        }
        else if (!playerWin)
        {
            _pendingKickOnLoad = true;
        }
        _currentBattleRoom = null;
        _battleRoomsInCombat.Clear();

        GameRoot.GetManager<GameMapManager>()?.SaveTerrainDiffToJson();
    }

    /// <summary>战败后获取踢出目标房间（相邻随机可行走地块）</summary>
    public bool TryGetKickTarget(Vector3 playerPos, out HexRoomTag targetRoom)
    {
        targetRoom = null;
        if (!_pendingKickOnLoad) return false;

        if (!gameMapManager) gameMapManager = GameRoot.GetManager<GameMapManager>();
        if (gameMapManager == null) return false;

        // 找到玩家所在的房间（忽略Y轴高度差）
        HexRoomTag currentRoom = null;
        Vector3 playerPosXZ = new Vector3(playerPos.x, 0, playerPos.z);
        foreach (var room in gameMapManager.HexRoomMap.Values)
        {
            if (room == null) continue;
            Vector3 roomPosXZ = new Vector3(room.transform.position.x, 0, room.transform.position.z);
            if (Vector3.Distance(roomPosXZ, playerPosXZ) < 0.5f)
            {
                currentRoom = room;
                break;
            }
        }
        if (currentRoom == null) return false;

        // 扫描相邻1格内的可行走地块
        var neighbors = HexCoordinateUtility.GetRowColsInRadius(currentRoom.row, currentRoom.col, 1);
        var candidates = new System.Collections.Generic.List<HexRoomTag>();
        foreach (var rc in neighbors)
        {
            var room = gameMapManager.GetTargetRoom(rc);
            if (room == null || room == currentRoom) continue;
            var terrain = room.GetComponent<HexTerrainStyleHandler>();
            if (terrain != null && !terrain.HexTerrainType.ToString().StartsWith("Obstacle"))
                candidates.Add(room);
        }

        if (candidates.Count > 0)
        {
            targetRoom = candidates[Random.Range(0, candidates.Count)];
            DebugManager.Log(EDebugCategory.MapRoom, $"[GameBattleManager] 战败踢出: ({currentRoom.row},{currentRoom.col}) → ({targetRoom.row},{targetRoom.col})");
            return true;
        }

        DebugManager.LogWarning(EDebugCategory.MapRoom, "[GameBattleManager] 战败踢出失败：无相邻可行走地块");
        return false;
    }

    /// <summary>战败踢出完成后清除标记</summary>
    public void ClearPendingKick() { _pendingKickOnLoad = false; }

    /// <summary>
    /// 根据之前注册的战斗信息，在战斗场景中生成战斗角色
    /// </summary>
    public void SpawnBattleCharacter() {
        CoroutineManager corManager= GameRoot.GetManager<CoroutineManager>();
        corManager.StartCoroutine(SpawnAll());
    }

    IEnumerator SpawnAll() {
        // 玩家：若场景中存在 PlayerBattleBoard，直接传数据给它；否则走旧版预制件生成
        var playerBoard = GameObject.FindObjectOfType<PlayerBattleBoard>();
        if (playerBoard != null && playersData.Count > 0)
        {
            foreach (var data in playersData)
                playerBoard.InitPlayerBoard(data);
        }
        else
        {
            yield return SpawnBattleCardByData(playersData, true);
        }

        // 敌人：始终使用 CharacterBattleArea 预制件生成
        yield return SpawnBattleCardByData(enemysData, false);
        GameRoot.GetManager<BattlePhaseManager>()?.OnAllCharactersLoaded();
    }

    /// <summary>
    /// [BattleScene]根据战斗数据列表，生成相应的初始战斗角色
    /// </summary>
    IEnumerator SpawnBattleCardByData(List<CharacterData> datas,bool isPlayer){
        BattleLoadManager battleLoadManager=GameRoot.GetManager<BattleLoadManager>();
        if (isPlayer){
            // 玩家不再通过 LoadAPlayer 生成，场景中已预置 PlayerBattleBoard
            // 此分支仅作为无预置玩家时的兜底（理论上不会走到这里）
            DebugManager.LogWarning(EDebugCategory.MapRoom, "SpawnBattleCardByData(isPlayer=true) 被意外调用，玩家应通过PlayerBattleBoard初始化");
            yield break;
        }
        else {
            DebugManager.Log(EDebugCategory.MapRoom, "生成Battle的EnemyCamp"+datas.Count);
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
