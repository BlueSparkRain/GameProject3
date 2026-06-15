using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 战斗房间逻辑(旧架构, 保持向后兼容)
/// </summary>
public class BattleHexRoom : IHexRoom
{
    E_CharacterType enemyCharacterType;
    string enemyCharacterSoDataPath = "SOData/CharacterSOData/";
    E_BattleType battleType;
    CharacterDataSO enemyCharacterDataSO;
    public E_CharacterType EnemyType => enemyCharacterType;
    CharacterHandler _spawnedModel;

    float scaleRate = 1;
    HexRoomTag roomTag;

    #region 敌人类型池（按前缀动态匹配枚举值）
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

    /// <summary>
    /// 从指定前缀的敌人池中随机取一种敌人类型
    /// </summary>
    static E_CharacterType GetRandomEnemyType(string prefix)
    {
        if (!EnemyPools.TryGetValue(prefix, out var pool) || pool.Length == 0)
        {
            Debug.LogError($"[BattleHexRoom] 敌人池为空，前缀: {prefix}");
            return E_CharacterType.LE_剑兵;
        }
        int index = UnityEngine.Random.Range(0, pool.Length);
        DebugManager.Log(EDebugCategory.MapRoom, $"[BattleHexRoom] 从{prefix}池中随机取: {pool[index]} (index:{index}/{pool.Length})");
        return pool[index];
    }
    #endregion

    public BattleHexRoom(HexRoomTag _roomTag, E_BattleType _battleType)
    {
        battleType = _battleType;
        roomTag = _roomTag;
    }

    public void DoHexRoomInit()
    {
        // 根据战斗等级从对应前缀的敌人池中随机取一种敌人
        string poolPrefix;
        switch (battleType)
        {
            case E_BattleType.Low:
                scaleRate = 0.8f;
                poolPrefix = "LE_";
                break;
            case E_BattleType.Mid:
                scaleRate = 1f;
                poolPrefix = "ME_";
                break;
            case E_BattleType.Boss:
                scaleRate = 1.5f;
                poolPrefix = "BOSS_";
                break;
            default:
                Debug.LogError($"[BattleHexRoom] 未定义战斗类型: {battleType}");
                scaleRate = 1f;
                poolPrefix = "LE_";
                break;
        }

        enemyCharacterType = GetRandomEnemyType(poolPrefix);
        enemyCharacterDataSO = Resources.Load<CharacterDataSO>(enemyCharacterSoDataPath + enemyCharacterType);

        if (enemyCharacterDataSO == null)
            Debug.LogError($"[BattleHexRoom] 无法加载敌人SO: {enemyCharacterSoDataPath}{enemyCharacterType}");
    }

    int num = 0;
    public void DoHexRoomLogic(UnityAction roomJob)
    {
        GameBattleManager gameBattleManager = GameRoot.GetManager<GameBattleManager>();
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        EventCenter.EventTrigger(E_EventType.PlayerBeforeIntoBattle);
        DebugManager.Log(EDebugCategory.MapRoom, roomTag + "--进入战斗房间" + num++);
        gameBattleManager.CheckBattleEnemy(roomTag);
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);
    }

    public void DoHexRoomModel(Vector3 modelPos)
    {
        var charac = MapCharacterCaller.CallNewCharacter("DisMoveable");
        charac.InitCharacterDataTag(enemyCharacterType, false, false);
        charac.transform.localScale = Vector3.zero;
        charac.transform.localPosition = modelPos;
        charac.transform.DOScale(scaleRate, 0.5f);
        _spawnedModel = charac;
    }

    public void DestroyModel()
    {
        if (_spawnedModel != null)
        {
            UnityEngine.Object.Destroy(_spawnedModel.gameObject);
            _spawnedModel = null;
        }
    }
}
