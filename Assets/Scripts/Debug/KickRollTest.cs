using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 测试脚本：MapScene 中按 Y 触发玩家翻滚踢飞动画，随机位移到相邻一格
/// </summary>
public class KickRollTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            var player = CharacterHandler.PlayerInstance;
            if (player == null)
            {
                Debug.LogWarning("[KickRollTest] 未找到玩家");
                return;
            }

            var target = GetRandomAdjacentRoom(player.transform.position);
            if (target == null)
            {
                Debug.LogWarning("[KickRollTest] 未找到相邻可行走房间");
                return;
            }

            Vector3 targetPos = target.transform.position + Vector3.up * GameRoot.GetManager<GameMapManager>().characterYOffset;
            MagicAnimExtens.RollingKick_WorldAnim(player.transform, targetPos);
        }
    }

    HexRoomTag GetRandomAdjacentRoom(Vector3 playerPos)
    {
        var mapMgr = GameRoot.GetManager<GameMapManager>();
        if (mapMgr == null) return null;

        // 从玩家位置找当前房间
        HexRoomTag currentRoom = null;
        foreach (var kv in mapMgr.HexRoomMap)
        {
            float dist = Vector3.Distance(
                new Vector3(kv.Value.transform.position.x, 0, kv.Value.transform.position.z),
                new Vector3(playerPos.x, 0, playerPos.z));
            if (dist < 0.5f) { currentRoom = kv.Value; break; }
        }
        if (currentRoom == null) return null;

        // 收集相邻房间
        var walkable = new List<HexRoomTag>();
        int[] offsets = { -1, 0, 1 };
        foreach (int dr in offsets)
        foreach (int dc in offsets)
        {
            if (dr == 0 && dc == 0) continue;
            var key = new Vector2Int(currentRoom.row + dr, currentRoom.col + dc);
            if (mapMgr.HexRoomMap.TryGetValue(key, out var room) && room.walkable)
                walkable.Add(room);
        }

        return walkable.Count > 0 ? walkable[Random.Range(0, walkable.Count)] : null;
    }
}
