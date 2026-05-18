using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 全局唯一目标筛选器（无实例、无管理器，极简高性能）
/// 自动根据 阵营+技能类型 筛选目标，自动处理死亡
/// </summary>
public static class BattleTargetSelector
{
    // 全局战斗单位池（战斗管理器注册所有存活单位，O(1)获取）
    public static List<IBattlable> AllBattleUnits { get; set; } = new();

    /// <summary>
    /// 核心：根据施法者+技能类型，获取合法目标（自动处理死亡+阵营）
    /// </summary>
    public static List<IBattlable> GetValidTargets(IBattlable caster, E_SkillTargetType type)
    {
        var targets = new List<IBattlable>();
        bool isPlayerCaster = caster.Camp == E_Camp.玩家方;

        switch (type)
        {
            // 玩家方技能：目标=敌方
            case E_SkillTargetType.对单体:
                targets.Add(GetFirstAliveTarget(isPlayerCaster ? E_Camp.敌方 : E_Camp.玩家方));
                break;
            case E_SkillTargetType.对全体:
                targets.AddRange(GetAllAliveTargets(isPlayerCaster ? E_Camp.敌方 : E_Camp.玩家方));
                break;
            case E_SkillTargetType.对N目标:
                targets.AddRange(GetNAliveTargets(isPlayerCaster ? E_Camp.敌方 : E_Camp.玩家方, 3));
                break;

                //// 敌方技能：目标=玩家方
                //case E_SkillTargetType.对玩家单体:
                //    targets.Add(GetFirstAliveTarget(isPlayerCaster ? E_Camp.玩家方 : E_Camp.敌方));
                //    break;
                //case E_SkillTargetType.对玩家全体:
                //    targets.AddRange(GetAllAliveTargets(isPlayerCaster ? E_Camp.玩家方 : E_Camp.敌方));
                //    break;
                //case E_SkillTargetType.对玩家N目标:
                //    targets.AddRange(GetNAliveTargets(isPlayerCaster ? E_Camp.玩家方 : E_Camp.敌方, 3));
                //    break;
        }

        return targets;
    }

    public static void RegisteABattler(IBattlable battler)
    {
        AllBattleUnits.Add(battler);
        Debug.Log("好的哈电话" + AllBattleUnits.Count + ":" + battler);
    }

    // 工具：获取第一个存活目标
    private static IBattlable GetFirstAliveTarget(E_Camp camp)
    {
        Debug.Log(AllBattleUnits.First(u => u.Camp == camp && u.IsAlive) + "????");
        return AllBattleUnits.First(u => u.Camp == camp && u.IsAlive);
    }
    // 工具：获取所有存活目标
    private static List<IBattlable> GetAllAliveTargets(E_Camp camp) =>
        AllBattleUnits.Where(u => u.Camp == camp && u.IsAlive).ToList();

    // 工具：获取N个存活目标
    private static List<IBattlable> GetNAliveTargets(E_Camp camp, int count) =>
        AllBattleUnits.Where(u => u.Camp == camp && u.IsAlive).Take(count).ToList();
}

