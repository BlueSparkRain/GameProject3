using Core;
using UnityEngine;

/// <summary>
/// 机器人房间自动结算器 —— 不打开任何面板，直接应用房间效果。
/// 战斗→自动胜利拿奖励 / 随机事件→随机选一个选项 / 商店→跳过 /
/// 奖励→自动领取 / NPC→自动交互(技能奖励等)
/// </summary>
public class RobotRoomAutoResolver
{
    CharacterLevelUpHandler _levelHandler;
    CharacterMapSkiller _skiller;

    public RobotRoomAutoResolver(CharacterLevelUpHandler levelHandler, CharacterMapSkiller skiller)
    {
        _levelHandler = levelHandler;
        _skiller = skiller;
    }

    /// <summary>自动结算机器人进入的房间。返回效果描述文本。</summary>
    public string Resolve(HexRoomTag roomTag)
    {
        var handler = roomTag.GetComponent<HexRoomStyleHandler>();
        if (handler == null) return "无房间类型";

        E_HexRoomType type = handler.RoomType;
        switch (type)
        {
            case E_HexRoomType.Battle_LowLevel:
            case E_HexRoomType.Battle_MidLevel:
            case E_HexRoomType.Battle_HighLevel:
                return ResolveBattle(roomTag, type);
            case E_HexRoomType.UnknownEvent:
                return ResolveRandomEvent();
            case E_HexRoomType.CityShop:
                return ResolveShop();
            case E_HexRoomType.Reward:
                return ResolveReward();
            case E_HexRoomType.NPC:
                return ResolveNPC();
            default:
                return "普通地块";
        }
    }

    string ResolveBattle(HexRoomTag roomTag, E_HexRoomType roomType)
    {
        var battleLogic = roomTag.GetComponentInChildren<BattleRoomLogic>();
        if (battleLogic == null) return "战斗房间无BattleRoomLogic";

        E_CharacterType enemyType = battleLogic.EnemyType;
        string s = enemyType.ToString();
        string prefix = s.StartsWith("LE_") ? "LE_" : s.StartsWith("ME_") ? "ME_" : s.StartsWith("BOSS_") ? "BOSS_" : "";

        int baseGold = prefix switch { "LE_" => 150, "ME_" => 500, "BOSS_" => 2000, _ => 100 };
        float baseEXP = prefix switch { "LE_" => 1000f, "ME_" => 5000f, "BOSS_" => 20000f, _ => 500f };

        var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
        float rewardMulti = chaosMgr != null ? chaosMgr.RewardMultiplier : 1f;

        int gold = Mathf.RoundToInt(baseGold * rewardMulti);
        int exp = Mathf.RoundToInt(baseEXP * rewardMulti);

        GameRoot.GetManager<GoldManager>()?.AddGold(gold);
        GameRoot.GetManager<VitalityPointsManager>()?.AdjustVolityPoints(-1);
        _levelHandler?.AdjustEXP(exp);

        if (_skiller != null)
            EventCenter.EventTrigger(E_EventType.Mover_IntoSpecialRoom, _skiller, roomType);

        DebugManager.Log(EDebugCategory.BattleAI, $"[RobotResolver] 战斗胜利: {enemyType}, 金币+{gold}, EXP+{exp}, 活力-1");
        return $"战斗胜利: +{gold}G, +{exp}EXP";
    }

    string ResolveRandomEvent()
    {
        var eventMgr = GameRoot.GetManager<UnknownEventManager>();
        if (eventMgr == null) return "UnknownEventManager未就绪";

        var so = eventMgr.GetRandomEvent();
        if (so == null) return "无可用随机事件";

        var options = eventMgr.GetEventOptions(so.eventType);
        if (options == null || options.Count == 0) return "事件无选项";

        var picked = options[Random.Range(0, options.Count)];
        eventMgr.ExecuteOption(picked);

        DebugManager.Log(EDebugCategory.BattleAI, $"[RobotResolver] 随机事件: {so.eventType}, 选择: {picked.description}");
        return $"随机事件[{so.eventType}]: {picked.description}";
    }

    string ResolveShop()
    {
        DebugManager.Log(EDebugCategory.BattleAI, "[RobotResolver] 商店: 跳过(机器人不购物)");
        return "商店: 跳过";
    }

    string ResolveReward()
    {
        EventCenter.EventTrigger(E_EventType.Map_GetPropertyGift);
        DebugManager.Log(EDebugCategory.BattleAI, "[RobotResolver] 奖励房间: 自动领取");
        return "奖励: 自动领取";
    }

    string ResolveNPC()
    {
        EventCenter.EventTrigger(E_EventType.Map_GetASkill);
        DebugManager.Log(EDebugCategory.BattleAI, "[RobotResolver] NPC: 自动交互");
        return "NPC: 自动交互";
    }
}
