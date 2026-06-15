using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 测试用：地图加载完成后投放指定数量的机器人高级移动角色。
/// 挂载到 MapScene 任意持久 GameObject（如 MapSceneSetUp 所在的 GameObject）。
/// 机器人与玩家使用同一预制件 Character_Moveable，通过 isPlayer=false 创建 Robot_CharacterMapMover。
/// </summary>
public class RobotSpawnTest : MonoBehaviour
{
    [Header("开关")]
    [SerializeField] bool _enableRobotSpawn = true;

    [Header("投放数量")]
    [SerializeField] int _robotCount = 3;

    [Header("机器人角色类型（按优先级随机）")]
    [SerializeField] E_CharacterType[] _robotTypes = new[]
    {
        E_CharacterType.R_复制体1,
        E_CharacterType.R_复制体2,
        E_CharacterType.R_复制体3,
    };

    [Header("地图加载后延迟（秒）")]
    [SerializeField] float _spawnDelay = 1.5f;

    void Awake()
    {
        EventCenter.AddEventListener(E_EventType.LoadMapEnd, OnMapLoaded);
    }

    void OnDestroy()
    {
        EventCenter.RemoveEventListener(E_EventType.LoadMapEnd, OnMapLoaded);
    }

    void OnMapLoaded()
    {
        StartCoroutine(SpawnRobotsRoutine());
    }

    System.Collections.IEnumerator SpawnRobotsRoutine()
    {
        yield return new WaitForSeconds(_spawnDelay);

        var mapManager = GameRoot.GetManager<GameMapManager>();
        if (mapManager == null)
        {
            Debug.LogError("[RobotSpawnTest] GameMapManager 未就绪");
            yield break;
        }

        if (!_enableRobotSpawn)
        {
            DebugManager.Log(EDebugCategory.MapRoom, "[RobotSpawnTest] 机器人投放已关闭");
            yield break;
        }

        for (int i = 0; i < _robotCount; i++)
        {
            var charTag = MapCharacterCaller.CallNewCharacter("Moveable");
            if (charTag == null)
            {
                Debug.LogError("[RobotSpawnTest] 实例化 Character_Moveable 失败");
                continue;
            }

            charTag.name = $"Robot_{i}";
            var type = _robotTypes[Random.Range(0, _robotTypes.Length)];

            // 设置唯一ID避免与玩家存档冲突
            var moveHandle = charTag.GetComponent<CharacterMapMoveHandle>();
            if (moveHandle != null)
                moveHandle.SetUniqueId($"Robot_{i}");

            // isPlayer=false → 创建 Robot_CharacterMapMover，InitBySelf随机放置到可行走房间
            charTag.InitCharacterDataTag(type, false, true);

            //Debug.Log($"[RobotSpawnTest] 生成 {type} Robot_{i}");
        }

        //Debug.Log($"[RobotSpawnTest] 完成投放 {_robotCount} 个机器人");
    }
}
