using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// 【全局唯一】技能工厂管理器
/// 所有角色（玩家/敌人）统一用它创建技能，无任何多余逻辑
/// </summary>
public static class BattleSkillFactory
{
        // 核心缓存：技能ID → 技能创建委托（最高效方式，无反射）
        private static readonly Dictionary<int, Func<ISkill>> _skillMap = new();
        private static int skillNum=3;
        public static void RegisterAllSkills()
        {
            for (int i = 0; i < skillNum; i++){
                var skillSo = ResourcesLoader.FindSkillSOByID(i);
                _skillMap[i] = () => new Skill_BaseAttack(skillSo.skill_targetType);
            }
            // 注册格式：ID => new 技能()
        //_skillMap[102] = () => new IceSkill();
        //_skillMap[201] = () => new EnemyBiteSkill();
        // 新增技能只加这一行，完全解耦
    }

        /// <summary>
        /// 根据单个ID创建技能（供任意角色使用）
        /// </summary>
        public static ISkill Create(int skillId)
        {
            if (_skillMap.TryGetValue(skillId, out var creator))
                return creator();
            UnityEngine.Debug.Log(skillId+ ": "+ creator()); ;
            throw new KeyNotFoundException($"技能ID {skillId} 未注册");
        }

        /// <summary>
        /// 【核心方法】根据ID列表批量创建技能（对局前直接调用）
        /// 玩家/敌人/任何单位都能用
        /// </summary>
        public static List<ISkill> CreateBatch(List<int> skillIdList)
        {
            List<ISkill> skills = new List<ISkill>(skillIdList.Count); // 预分配内存，无GC
            foreach (int id in skillIdList) skills.Add(Create(id));
            return skills;
        }
    } 
