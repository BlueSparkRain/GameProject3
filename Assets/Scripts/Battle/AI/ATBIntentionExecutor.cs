using UnityEngine;

/// <summary>
/// ATB意图执行器——按配置顺序循环执行ATB增强技能。独立于普通技能冷却循环，仅受ATB点数驱动。
/// ATB通过技能释放产生AG（怒气值），AG达到100后自动转化为1点ATB，不存在被动充能。
/// </summary>
public class ATBIntentionExecutor
{
    IBattlable self;
    Battle_Controller controller;
    ATBIntentionConfigSO config;

    int currentIndex;

    bool isPaused;

    // 调试用：避免每帧刷屏
    float debugLogTimer;
    const float DEBUG_LOG_INTERVAL = 2f;

    public ATBIntentionExecutor(IBattlable self, Battle_Controller controller, ATBIntentionConfigSO config)
    {
        this.self = self;
        this.controller = controller;
        this.config = config;
        currentIndex = 0;
        debugLogTimer = 0f;

        Debug.Log($"[ATBExecutor] 初始化完成 {controller.CharacterData.Character_Name}" +
                  $", 意图数{config.atbIntentionIndices.Count}" +
                  $", 每次消耗ATB={config.atbSpendPerSkill}" +
                  $", 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
    }

    public void OnUpdate(float deltaTime)
    {
        if (config == null || config.atbIntentionIndices.Count == 0)
            return;

        if (isPaused)
        {
            debugLogTimer += deltaTime;
            if (debugLogTimer >= DEBUG_LOG_INTERVAL)
            {
                debugLogTimer = 0f;
                Debug.Log($"[ATBExecutor] {controller.CharacterData.Character_Name} ATB执行器暂停中(isPaused=true), 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
            }
            return;
        }

        // 每秒输出一次ATB状态
        debugLogTimer += deltaTime;
        if (debugLogTimer >= DEBUG_LOG_INTERVAL)
        {
            debugLogTimer = 0f;
            Debug.Log($"[ATBExecutor] {controller.CharacterData.Character_Name} 模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}, 模型AG={controller.GetCharacterModelValue(E_BattleModelType.AG)}");
        }

        // 检查当前意图技能是否有足够ATB点数
        int oneBasedIdx = config.atbIntentionIndices[currentIndex];
        var skillSO = config.GetSkillSOByIntentionIndex(oneBasedIdx);
        if (skillSO == null) return;

        int modelATB = (int)controller.GetCharacterModelValue(E_BattleModelType.ATBPoints);
        if (modelATB < skillSO.skill_atb_cost)
            return;

        Debug.Log($"[ATBExecutor] {controller.CharacterData.Character_Name} ATB满足释放条件! 模型ATB={modelATB}, 技能消耗{skillSO.skill_atb_cost}, 意图索引={currentIndex}");
        ExecuteCurrentIntention();
    }

    void ExecuteCurrentIntention()
    {
        int oneBasedIndex = config.atbIntentionIndices[currentIndex];
        Debug.Log($"[ATBExecutor] 执行意图索引[{currentIndex}]: activeSkills位置={oneBasedIndex}(1-based)");

        var skillSO = config.GetSkillSOByIntentionIndex(oneBasedIndex);
        if (skillSO == null)
        {
            Debug.LogError($"[ATBExecutor] {controller.CharacterData.Character_Name} " +
                           $"意图索引[{currentIndex}]位置{oneBasedIndex}对应的SkillSO为null! " +
                           $"activeSkills总数={config.activeSkills.Count}, 请检查ATBIntentionConfigSO配置");
            AdvanceIndex();
            return;
        }

        Debug.Log($"[ATBExecutor] {self.Camp} 释放主动技能 {skillSO.skill_Name}(ID:{skillSO.skill_ID}), " +
                  $"技能ATB消耗{skillSO.skill_atb_cost}, 意图索引={currentIndex}, 技能位置{oneBasedIndex}");

        var skill = BattleSkillFactory.CreateBattleSkill(skillSO.skill_ID, self);
        if (skill != null)
        {
            Debug.Log($"[ATBExecutor] 技能实例创建成功 AtbCost={skill.AtbCost}, 准备执行加强版本...");
            skill.SkillExcute(E_SkillLevel.加强版本, config.atbSpendPerSkill);
            Debug.Log($"[ATBExecutor] {controller.CharacterData.Character_Name} ATB技能执行完毕 [{skillSO.skill_Name}] " +
                      $"执行后模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
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
        if (currentIndex >= config.atbIntentionIndices.Count)
            currentIndex = 0;
        Debug.Log($"[ATBExecutor] 意图索引推进: {currentIndex}/{config.atbIntentionIndices.Count}");
    }

    public void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            Debug.Log($"[ATBExecutor] {controller.CharacterData.Character_Name} ATB执行器已暂停, 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            isPaused = false;
            debugLogTimer = 0f;
            Debug.Log($"[ATBExecutor] {controller.CharacterData.Character_Name} ATB执行器已恢复, 当前模型ATB={controller.GetCharacterModelValue(E_BattleModelType.ATBPoints)}");
        }
    }

    public int CurrentIndex => currentIndex;
    public ATBIntentionConfigSO Config => config;
}
