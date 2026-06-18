using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 全局唯一目标筛选器（无实例、无管理器，极简高性能）
/// 自动根据 阵营+技能类型 筛选目标，自动处理死亡
/// </summary>
public static class BattleTargetSelector{
    // 全局战斗单位池（战斗管理器注册所有存活单位，O(1)获取）
    public static List<IBattlable> AllBattleUnits { get; set; } = new();

    /// <summary>
    /// 核心：根据施法者+技能类型，获取合法目标（自动处理死亡+阵营）
    /// </summary>
    public static List<IBattlable> GetValidTargets(IBattlable caster, E_SkillTargetType_Auto type){
        var targets = new List<IBattlable>();
        bool isPlayerCaster = caster.Camp == E_Camp.玩家方;
        switch (type){
            case E_SkillTargetType_Auto.对单体:
                var first = GetFirstAliveTarget(isPlayerCaster ? E_Camp.敌方 : E_Camp.玩家方);
                if (first != null) targets.Add(first);break;
            case E_SkillTargetType_Auto.对全体:
                targets.AddRange(GetAllAliveTargets(isPlayerCaster ? E_Camp.敌方 : E_Camp.玩家方));break;
            case E_SkillTargetType_Auto.对N目标:
                targets.AddRange(GetNAliveTargets(isPlayerCaster ? E_Camp.敌方 : E_Camp.玩家方, 3));break;
        }
        return targets;
    }
    public static void ClearAll()
    {
        AllBattleUnits.Clear();
        DebugManager.Log(EDebugCategory.General, "[BattleTargetSelector]---已清空所有Battler");
    }

    public static void RegisteNewBattler(IBattlable battler){
        AllBattleUnits.Add(battler);
        DebugManager.Log(EDebugCategory.General, $"[BattleTargetSelector]---新增Battler：{battler.Camp},当前战斗人数{AllBattleUnits.Count} ");
    }
    // 获取第一个存活目标
    private static IBattlable GetFirstAliveTarget(E_Camp camp){
        return AllBattleUnits.FirstOrDefault(u => u.Camp == camp && u.IsAlive);
    }
    // 获取所有存活目标
    public static List<IBattlable> GetAllAliveTargets(E_Camp camp) =>
        AllBattleUnits.Where(u => u.Camp == camp && u.IsAlive).ToList();

    // 获取N个存活目标
    private static List<IBattlable> GetNAliveTargets(E_Camp camp, int count) =>
        AllBattleUnits.Where(u => u.Camp == camp && u.IsAlive).Take(count).ToList();

    // 获取随机N个存活目标
    public static List<IBattlable> GetRandomNAliveTargets(E_Camp camp, int count) =>
        AllBattleUnits.Where(u => u.Camp == camp && u.IsAlive)
                      .OrderBy(_ => Random.value) // 核心：随机排序
                      .Take(count)
                      .ToList();
}

