using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 给技能类标记 ID 的特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SkillIDAttribute : Attribute
{
    public int ID { get; }
    public SkillIDAttribute(int id) => ID = id;
}

/// <summary>
/// 【全局唯一】技能工厂管理器
/// 所有角色（玩家/敌人）统一用它创建技能，无任何多余逻辑
/// </summary>
public static class BattleSkillFactory
{
    // 核心缓存：技能ID → 技能创建委托（最高效方式，无反射）
    private static readonly Dictionary<int, Func<SkillBase>> _skillMap = new();

    // 缓存：ID → 构造函数 (TargetType)
    private static readonly Dictionary<int, Func<E_SkillTargetType, SkillBase>> _skillConstructors = new();

    static BattleSkillFactory()
    {
        // 静态构造：程序启动时扫描一次所有技能类
        ScanAllSkillClasses();
    }

    /// <summary>
    /// 扫描所有继承 SkillBase 的类，并读取 [SkillID] 特性
    /// </summary>
    private static void ScanAllSkillClasses()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var skillTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(SkillBase)) && !t.IsAbstract);

        foreach (var type in skillTypes)
        {
            var idAttr = type.GetCustomAttribute<SkillIDAttribute>();
            if (idAttr == null) continue;

            // 获取带 TargetType 参数的构造函数
            var ctor = type.GetConstructor(new[] { typeof(E_SkillTargetType) });
            if (ctor == null)
            {
                Debug.LogError($"技能 {type.Name} 没有带 TargetType 的构造函数！");
                continue;
            }

            _skillConstructors[idAttr.ID] = targetType =>
                (SkillBase)ctor.Invoke(new object[] { targetType });
        }
    }

    /// <summary>
    /// 全自动注册所有技能（根据 Resources 中的 SkillSO）
    /// 外部接口完全不变
    /// </summary>
    public static void RegisterAllSkills()
    {
        _skillMap.Clear();

        // 加载所有技能SO（你原来的加载方式不变）
        for (int i = 0; i < 1000; i++)
        {
            var skillSo = ResourcesLoader.FindSkillSOByID(i);
            if (skillSo == null) break; // 找不到就停止

            int skillId = skillSo.skill_ID;
            E_SkillTargetType targetType = skillSo.skill_targetType;

            // 从构造器缓存中自动创建对应技能
            if (_skillConstructors.TryGetValue(skillId, out var ctor))
            {
                _skillMap[skillId] = () => ctor(targetType);
                Debug.Log($"✅ 自动注册技能 ID:{skillId} 名称:{skillSo.skill_Name}");
            }
            else
            {
                Debug.LogError($" 未找到技能 ID {skillId} 对应的类，请检查 [SkillID] 特性！");
            }
        }
    }

    public static SkillBase CreateBattleSkill(int skillId, IBattlable caster)
    {
        var skill = Create(skillId);
        skill.GetCaster(caster);
        return skill;
    }

    public static List<SkillBase> CreateBattleSkillsBatch(List<int> skillidlist, IBattlable caster)
    {
        var skills = CreateBatch(skillidlist);
        foreach (var skill in skills)
        {
            skill.GetCaster(caster);
        }
        return skills;
    }

    /// <summary>
    /// 根据单个ID创建技能（供任意角色使用） 
    /// </summary>
    static SkillBase Create(int skillId)
    {
        if (_skillMap.TryGetValue(skillId, out var creator))
        {
            return creator();
        }

        throw new KeyNotFoundException($"技能ID {skillId} 未注册");
    }

    /// <summary>
    /// 【核心方法】根据ID列表批量创建技能（对局前直接调用）
    /// </summary>
    static List<SkillBase> CreateBatch(List<int> skillIdList)
    {
        List<SkillBase> skills = new List<SkillBase>(skillIdList.Count);
        foreach (int id in skillIdList) skills.Add(Create(id));
        return skills;
    }
}