using System.Collections.Generic;
using UnityEngine;
using Core;
/// <summary>
/// ATB意图执行器——按配置顺序循环执行ATB增强技能。独立于普通技能冷却循环，仅受ATB点数驱动。
/// ATB通过技能释放产生AG（怒气值），AG达到100后自动转化为1点ATB，不存在被动充能。
/// </summary>
public class ATBIntentionExecutor
{
    IBattlable self;
    Battle_Controller controller;
    ATBIntentionConfigSO config;

    /// <summary>地图配置模式：直接使用技能ID列表</summary>
    List<SkillPropertySO> mapSkillSOs;
    int atbSpendPerSkill;

    int currentIndex;

    bool isPaused;

    /// <summary>释放后随机间隔，避免每帧检测立刻连发</summary>
    float _nextCheckCooldown;
    static readonly Vector2 CooldownRange = new Vector2(2f, 5f);

    // 调试用：避免每帧刷屏
    float debugLogTimer;
    const float DEBUG_LOG_INTERVAL = 2f;

    public ATBIntentionExecutor(IBattlable self, Battle_Controller controller, ATBIntentionConfigSO config)
    {
        this.self = self;
        this.controller = controller;
        this.config = config;
        mapSkillSOs = null;
        currentIndex = 0;
        debugLogTimer = 0f;

        DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] 初始化完成 {controller.CharacterData.Character_Name}" +
                  $", 意图数{config.atbIntentionIndices.Count}" +
                  $", 每次消耗ATB={config.atbSpendPerSkill}" +
                  $", 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
    }

    /// <summary>
    /// 地图配置路径：直接使用技能ID列表构造
    /// </summary>
    public ATBIntentionExecutor(IBattlable self, Battle_Controller controller, List<int> skillIDs, int spendPerSkill = 1)
    {
        this.self = self;
        this.controller = controller;
        this.config = null;
        this.atbSpendPerSkill = spendPerSkill;

        mapSkillSOs = new List<SkillPropertySO>();
        foreach (int id in skillIDs)
        {
            var so = ResourcesLoader.FindSkillSOByID(id);
            if (so != null)
                mapSkillSOs.Add(so);
        }

        currentIndex = 0;
        debugLogTimer = 0f;

        DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] 地图配置初始化完成 {controller.CharacterData.Character_Name}" +
                  $", 意图数{mapSkillSOs.Count}" +
                  $", 每次消耗ATB={spendPerSkill}" +
                  $", 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
    }

    int SkillCount => config != null ? config.atbIntentionIndices.Count : (mapSkillSOs?.Count ?? 0);

    SkillPropertySO GetCurrentSkillSO()
    {
        if (mapSkillSOs != null)
        {
            if (currentIndex >= mapSkillSOs.Count) return null;
            return mapSkillSOs[currentIndex];
        }
        if (config != null)
        {
            int oneBasedIdx = config.atbIntentionIndices[currentIndex];
            return config.GetSkillSOByIntentionIndex(oneBasedIdx);
        }
        return null;
    }

    int GetSpendPerSkill()
    {
        if (mapSkillSOs != null) return atbSpendPerSkill;
        return config != null ? config.atbSpendPerSkill : 1;
    }

    public void OnUpdate(float deltaTime)
    {
        if (SkillCount == 0)
            return;

        if (isPaused)
        {
            debugLogTimer += deltaTime;
            if (debugLogTimer >= DEBUG_LOG_INTERVAL)
            {
                debugLogTimer = 0f;
                DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] {controller.CharacterData.Character_Name} ATB执行器暂停中(isPaused=true), 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
            }
            return;
        }

        // 释放后冷却倒计时
        if (_nextCheckCooldown > 0f)
        {
            _nextCheckCooldown -= deltaTime;
            return;
        }

        debugLogTimer += deltaTime;
        if (debugLogTimer >= DEBUG_LOG_INTERVAL)
        {
            debugLogTimer = 0f;
            DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] {controller.CharacterData.Character_Name} 模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}, 模型AG={controller.GetCharacterModelValue(E_BattleModelType.AG)}");
        }

        var skillSO = GetCurrentSkillSO();
        if (skillSO == null) return;

        int modelATB = (int)controller.GetCharacterModelValue(E_BattleModelType.ATBPoints);
        if (modelATB < skillSO.skill_AtbCost_ATB)
            return;

        DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] {controller.CharacterData.Character_Name} ATB满足释放条件! 模型ATB={modelATB}, 技能消耗{skillSO.skill_AtbCost_ATB}, 意图索引={currentIndex}");
        ExecuteCurrentIntention();
        _nextCheckCooldown = Random.Range(CooldownRange.x, CooldownRange.y);
    }

    void ExecuteCurrentIntention()
    {
        var skillSO = GetCurrentSkillSO();
        if (skillSO == null)
        {
            Debug.LogError($"[ATBExecutor] {controller.CharacterData.Character_Name} 意图索引[{currentIndex}]对应的SkillSO为null!");
            AdvanceIndex();
            return;
        }

        DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] {self.Camp} 释放主动技能 {skillSO.skill_Name}(ID:{skillSO.skill_ID}), " +
                  $"技能ATB消耗{skillSO.skill_AtbCost_ATB}, 意图索引={currentIndex}");

        var skill = BattleSkillFactory.CreateBattleSkill(skillSO.skill_ID, self);
        if (skill != null)
        {
            // 预解析技能目标（用于 VFX 定位）
            var targets = BattleTargetSelector.GetValidTargets(self, skillSO.skill_targetType_Auto);
            IBattlable target = targets != null && targets.Count > 0 ? targets[0] : null;

            DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] 技能实例创建成功 AtbCost={skill.AtbCost}, 提交到行动队列...");
            var queue = GameRoot.GetManager<BattleActionQueue>();
            if (queue != null)
            {
                var action = new BattleAction(skill,
                    controller.CharacterData.Character_Name, skillSO.skill_Name,
                    skillSO.skill_DeliveryType, E_SkillLevel.加强版本,
                    henceTime: GetSpendPerSkill(), isATB: true, target: target);
                queue.Enqueue(action);
            }
        }
        else
        {
            Debug.LogError($"[ATBExecutor] BattleSkillFactory.CreateBattleSkill返回null! skillID={skillSO.skill_ID}");
        }

        AdvanceIndex();
    }

    void AdvanceIndex()
    {
        currentIndex++;
        if (currentIndex >= SkillCount)
            currentIndex = 0;
        DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] 意图索引推进: {currentIndex}/{SkillCount}");
    }

    public void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] {controller.CharacterData.Character_Name} ATB执行器已暂停, 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            isPaused = false;
            debugLogTimer = 0f;
            DebugManager.Log(EDebugCategory.ATBIntention,$"[ATBExecutor] {controller.CharacterData.Character_Name} ATB执行器已恢复, 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
        }
    }

    public int CurrentIndex => currentIndex;
    public ATBIntentionConfigSO Config => config;
}
