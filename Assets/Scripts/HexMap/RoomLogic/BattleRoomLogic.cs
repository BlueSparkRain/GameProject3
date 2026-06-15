using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 战斗房间逻辑——触发时打开BattlePanel，消耗后按冷却回合重生
/// 杂兵6回合 / 精英15回合 / 首领30回合
/// </summary>
public class BattleRoomLogic : RoomLogicComponent
{
    public E_BattleType BattleType { get; private set; }
    public E_CharacterType EnemyType => _enemyCharacterType;
    public float ScaleRate => _scaleRate;

    E_CharacterType _enemyCharacterType;
    float _scaleRate = 1f;
    CharacterDataSO _enemyDataSO;
    CharacterHandler _spawnedModel;

    /// <summary>消耗前的原始地形类型(用于重生恢复)</summary>
    E_HexTerrainType _originalTerrainType;

    #region 敌人类型池(按前缀动态匹配)
    static Dictionary<string, E_CharacterType[]> _enemyPools;
    static Dictionary<string, E_CharacterType[]> EnemyPools
    {
        get
        {
            if (_enemyPools == null)
                BuildEnemyPools();
            return _enemyPools;
        }
    }

    static void BuildEnemyPools()
    {
        _enemyPools = new Dictionary<string, E_CharacterType[]>();
        var allTypes = (E_CharacterType[])Enum.GetValues(typeof(E_CharacterType));
        _enemyPools["LE_"]   = allTypes.Where(t => t.ToString().StartsWith("LE_")).ToArray();
        _enemyPools["ME_"]   = allTypes.Where(t => t.ToString().StartsWith("ME_")).ToArray();
        _enemyPools["BOSS_"] = allTypes.Where(t => t.ToString().StartsWith("BOSS_")).ToArray();
    }

    static E_CharacterType GetRandomEnemyType(string prefix)
    {
        if (!EnemyPools.TryGetValue(prefix, out var pool) || pool.Length == 0)
        {
            Debug.LogError($"[BattleRoomLogic] 敌人池为空，前缀: {prefix}");
            return E_CharacterType.LE_剑兵;
        }
        return pool[UnityEngine.Random.Range(0, pool.Length)];
    }
    #endregion

    /// <summary>按战斗等级获取重生冷却回合</summary>
    public static int GetRespawnCooldown(E_BattleType battleType) => battleType switch
    {
        E_BattleType.Low => 6,
        E_BattleType.Mid => 15,
        E_BattleType.Boss => 30,
        _ => 6
    };

    public BattleRoomLogic() { }

    public void SetBattleType(E_BattleType battleType)
    {
        BattleType = battleType;
        switch (battleType)
        {
            case E_BattleType.Low:
                _roomType = E_HexRoomType.Battle_LowLevel;
                _scaleRate = 0.8f;
                _enemyCharacterType = GetRandomEnemyType("LE_");
                break;
            case E_BattleType.Mid:
                _roomType = E_HexRoomType.Battle_MidLevel;
                _scaleRate = 1f;
                _enemyCharacterType = GetRandomEnemyType("ME_");
                break;
            case E_BattleType.Boss:
                _roomType = E_HexRoomType.Battle_HighLevel;
                _scaleRate = 1.5f;
                _enemyCharacterType = GetRandomEnemyType("BOSS_");
                break;
        }

        string soPath = "SOData/CharacterSOData/";
        _enemyDataSO = Resources.Load<CharacterDataSO>(soPath + _enemyCharacterType);
        if (_enemyDataSO == null)
            Debug.LogError($"[BattleRoomLogic] 无法加载敌人SO: {soPath}{_enemyCharacterType}");
    }

    public override void InitLogic(HexRoomTag roomTag)
    {
        base.InitLogic(roomTag);
        _originalTerrainType = _terrainStyle != null
            ? _terrainStyle.HexTerrainType
            : E_HexTerrainType.Walkable_EmptyLand;
    }

    public override void OnPlayerEnter(HexRoomTag roomTag)
    {
        if (!_canTrigger) return;

        // 战败回场：先站在房间上再被踢开，不触发战斗面板
        if (GameRoot.GetManager<GameBattleManager>()?.SuppressBattleTrigger == true)
            return;

        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        EventCenter.EventTrigger(E_EventType.PlayerBeforeIntoBattle);

        var battleMgr = GameRoot.GetManager<GameBattleManager>();
        battleMgr.CheckBattleEnemy(roomTag);
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);

        // Consume延后：战斗胜利时由GameBattleManager.OnBattleResult调用；战败时不消耗
    }

    public override void Consume()
    {
        _canTrigger = false;

        // 清理自身生成的敌人模型
        if (_spawnedModel != null)
        {
            Destroy(_spawnedModel.gameObject);
            _spawnedModel = null;
        }

        // 清理旧 IHexRoom 的模型
        _roomTag?.IHexRoom?.DestroyModel();

        // 注册到重生管理器
        var respawnMgr = GameRoot.GetManager<RoomRespawnManager>();
        respawnMgr?.RegisterRespawn(_roomTag, BattleType, _originalTerrainType);

        // 地块变为空白
        if (_terrainStyle != null)
            _terrainStyle.InitTerrainStyle(E_HexTerrainType.Walkable_EmptyLand, _roomTag);
    }

    void OnDestroy()
    {
        if (_spawnedModel != null)
        {
            Destroy(_spawnedModel.gameObject);
            _spawnedModel = null;
        }
    }

    public override void SpawnModel(Vector3 modelPos)
    {
        var charac = MapCharacterCaller.CallNewCharacter("DisMoveable");
        charac.InitCharacterDataTag(_enemyCharacterType, false, false);
        charac.transform.localScale = Vector3.zero;
        charac.transform.localPosition = modelPos;
        charac.transform.DOScale(_scaleRate, 0.5f);
        _spawnedModel = charac;
    }
}
