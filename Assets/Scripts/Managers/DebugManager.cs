using UnityEngine;

public enum EDebugCategory
{
    SkillExecution,   // 技能释放 & 伤害
    ATBIntention,     // ATB意图
    BattleBuff,       // Buff增减
    BattleDOT,        // DOT增减
    BattleDamage,     // 伤害计算
    BattleState,      // 战斗状态（死亡/力竭）
    BattleAI,         // 怪物AI
    BattleSkiller,    // 技能加载
    SkillUI,          // 技能UI拖拽
    PlayerBattleBoard,// 面板
    General,          // 通用/核心系统
    MapRoom,          // 地图房间流程
    Equipment,        // 装备系统
    UIPanel,          // UI面板
    Character,        // 角色数据/升级
}

public static class DebugManager
{
    public static void Log(EDebugCategory cat, string msg)
    {
        Debug.Log(msg);
    }

    public static void LogWarning(EDebugCategory cat, string msg)
    {
        Debug.LogWarning(msg);
    }

    public static void LogError(EDebugCategory cat, string msg)
    {
        Debug.LogError(msg);
    }
}
