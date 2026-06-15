using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 房间重生管理器——追踪已消耗的战斗房间，按冷却回合自动重生
/// 同时负责每回合结束时在空白格子上随机生成事件
/// 杂兵6回合 / 精英15回合 / 首领30回合
/// </summary>
public class RoomRespawnManager : MonoGlobalManager
{
    /// <summary>重生条目</summary>
    [System.Serializable]
    class RespawnEntry
    {
        public HexRoomTag roomTag;
        public E_BattleType battleType;
        public E_HexTerrainType originalTerrain;
        public int remainingTurns;
    }

    List<RespawnEntry> _respawnList = new List<RespawnEntry>();

    const float RandomEventSpawnChance = 0.15f;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener(E_EventType.Player_RoundEnd, OnRoundEnd);
    }

    public override void MgrDispose()
    {
        EventCenter.RemoveEventListener(E_EventType.Player_RoundEnd, OnRoundEnd);
        base.MgrDispose();
    }

    /// <summary>注册战斗房间重生</summary>
    public void RegisterRespawn(HexRoomTag roomTag, E_BattleType battleType, E_HexTerrainType originalTerrain)
    {
        int cooldown = BattleRoomLogic.GetRespawnCooldown(battleType);
        _respawnList.Add(new RespawnEntry
        {
            roomTag = roomTag,
            battleType = battleType,
            originalTerrain = originalTerrain,
            remainingTurns = cooldown
        });
        DebugManager.Log(EDebugCategory.MapRoom, $"[RoomRespawnManager] 注册重生: {roomTag.name} ({battleType}), {cooldown}回合后刷新");
    }

    void OnRoundEnd()
    {
        ProcessRespawns();
        SpawnRandomEvents();
    }

    /// <summary>重新绑定事件（MapSceneSetUp 中 ClearAllEvents 后会清掉全局管理器的事件绑定）</summary>
    public void RebindEvents()
    {
        EventCenter.RemoveEventListener(E_EventType.Player_RoundEnd, OnRoundEnd);
        EventCenter.AddEventListener(E_EventType.Player_RoundEnd, OnRoundEnd);
    }

    void ProcessRespawns()
    {
        for (int i = _respawnList.Count - 1; i >= 0; i--)
        {
            var entry = _respawnList[i];
            if (entry.roomTag == null)
            {
                _respawnList.RemoveAt(i);
                continue;
            }

            entry.remainingTurns--;
            if (entry.remainingTurns <= 0)
            {
                RespawnRoom(entry);
                _respawnList.RemoveAt(i);
            }
        }
    }

    void RespawnRoom(RespawnEntry entry)
    {
        var terrainStyle = entry.roomTag.GetComponent<HexTerrainStyleHandler>();
        if (terrainStyle != null)
        {
            terrainStyle.InitTerrainStyle(entry.originalTerrain, entry.roomTag);
            DebugManager.Log(EDebugCategory.MapRoom, $"[RoomRespawnManager] 战斗房间重生: {entry.roomTag.name} ({entry.battleType})");
        }
    }

    /// <summary>
    /// 每回合结束15%概率在随机空白格子上生成随机事件("?")
    /// </summary>
    void SpawnRandomEvents()
    {
        if (Random.value > RandomEventSpawnChance) return;

        var mapMgr = GameRoot.GetManager<GameMapManager>();
        if (mapMgr == null) return;

        // 收集所有空白可走地块
        var emptyRooms = new List<HexRoomTag>();
        foreach (var roomTag in mapMgr.HexRoomMap.Values)
        {
            if (roomTag == null) continue;
            var terrain = roomTag.GetComponent<HexTerrainStyleHandler>();
            if (terrain != null && terrain.HexTerrainType == E_HexTerrainType.Walkable_EmptyLand)
                emptyRooms.Add(roomTag);
        }

        DebugManager.Log(EDebugCategory.MapRoom, $"[RoomRespawnManager] 随机事件抽中! 空白地块总数={emptyRooms.Count}");

        if (emptyRooms.Count == 0) return;

        var target = emptyRooms[Random.Range(0, emptyRooms.Count)];
        var targetTerrain = target.GetComponent<HexTerrainStyleHandler>();
        if (targetTerrain != null)
        {
            targetTerrain.InitTerrainStyle(E_HexTerrainType.Walkable_UnknownEventRoom, target);
            DebugManager.Log(EDebugCategory.MapRoom, $"[RoomRespawnManager] 随机事件房间生成于 ({target.row},{target.col})，原为 Walkable_EmptyLand → Walkable_UnknownEventRoom");
        }
    }

    public override void MgrUpdate(float deltaTime) { }
}
