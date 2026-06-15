using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 优先级/效用AI决策器 —— 为机器人评分所有可到达房间。
/// 非状态机，每回合重新评估所有目标，按加权总分排序。
/// HP低时避战，距离近优先，高价值房间优先。
/// </summary>
public class RobotAIDecisionMaker
{
    /// <summary>房间类型基础分</summary>
    static readonly Dictionary<E_HexRoomType, float> RoomBaseScore = new Dictionary<E_HexRoomType, float>
    {
        { E_HexRoomType.Battle_HighLevel, 90f },
        { E_HexRoomType.Reward,           85f },
        { E_HexRoomType.Battle_MidLevel, 70f },
        { E_HexRoomType.CityShop,         65f },
        { E_HexRoomType.NPC,              60f },
        { E_HexRoomType.UnknownEvent,     55f },
        { E_HexRoomType.Battle_LowLevel,  40f },
    };

    /// <summary>距离每步扣分</summary>
    const float DistanceCostPerStep = 5f;
    /// <summary>避战HP阈值——HP比例低于此值时大幅扣战斗分</summary>
    const float AvoidBattleHPThreshold = 0.4f;
    /// <summary>低HP时对战斗房间的额外扣分系数</summary>
    const float LowHPBattlePenalty = 50f;
    /// <summary>可到达上限——BFS路径长度超出此值忽略</summary>
    const int MaxReachableDistance = 20;

    HexPathFindingManager _pathfinder;
    CharacterData _charData;
    HexRoomTag _currentRoom;

    public RobotAIDecisionMaker(CharacterData charData, HexRoomTag currentRoom)
    {
        _charData = charData;
        _currentRoom = currentRoom;
        _pathfinder = GameRoot.GetManager<HexPathFindingManager>();
    }

    public void UpdateContext(HexRoomTag currentRoom)
    {
        _currentRoom = currentRoom;
    }

    /// <summary>选择最优目标房间，返回路径。若无合适目标则返回空列表。</summary>
    public List<HexRoomTag> DecideTarget(int availableAP)
    {
        if (_currentRoom == null || _pathfinder == null) return new List<HexRoomTag>();

        var specialRooms = _pathfinder.GetSpecialRooms();
        if (specialRooms.Count == 0) return new List<HexRoomTag>();

        float hpRatio = _charData.Maximum_Health / Mathf.Max(_charData.Maximum_Health, 1f);
        int chaosLevel = GameRoot.GetManager<ChaosLevelManager>()?.currentLevel ?? 1;
        var scored = new List<(HexRoomTag room, float score, int distance, List<HexRoomTag> path)>();

        foreach (var room in specialRooms)
        {
            if (room == _currentRoom) continue;

            var path = _pathfinder.FindPath(_currentRoom, room);
            int distance = path.Count;

            // 不可达或太远
            if (distance == 0 || distance > MaxReachableDistance || distance > availableAP)
                continue;

            var handler = room.GetComponent<HexRoomStyleHandler>();
            if (handler == null) continue;

            E_HexRoomType type = handler.RoomType;
            float score = RoomBaseScore.TryGetValue(type, out float baseScore) ? baseScore : 30f;

            // 距离衰减
            score -= distance * DistanceCostPerStep;

            // HP低时避战
            if (hpRatio < AvoidBattleHPThreshold && type.IsBattleRoom())
                score -= LowHPBattlePenalty * (1f - hpRatio);

            // 混沌等级高时略微偏好奖励/商店（规避风险）
            if (chaosLevel >= 3 && (type == E_HexRoomType.Reward || type == E_HexRoomType.CityShop))
                score += 10f;

            scored.Add((room, score, distance, path));
        }

        scored.Sort((a, b) => b.score.CompareTo(a.score));

        if (scored.Count > 0 && scored[0].score > 0)
            return scored[0].path;

        return new List<HexRoomTag>();
    }
}
