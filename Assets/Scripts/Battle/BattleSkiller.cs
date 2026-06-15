using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 战斗技能管理器 —— 管理技能的冷却循环与释放。
/// 核心循环使用 SkillCharger（纯数据），SkillIcon 仅在有 Spawner 时创建（玩家路径）。
/// 敌人路径无 Spawner，仅在 Charger 内部充能，不生成图标。
/// </summary>
public class BattleSkiller
{
    List<ISkillMode> normalSkillModes = new List<ISkillMode>();
    List<ISkillMode> atbSkillModes = new List<ISkillMode>();

    // 仅玩家路径使用（有 Spawner 时保留图标引用）
    List<SkillIcon> normalSkillIcons = new List<SkillIcon>();
    List<SkillIcon> atbSkillIcons = new List<SkillIcon>();

    List<int> normalSkillIDs = new List<int>();
    List<int> atbSkillIDs = new List<int>();

    SkillIconSpawner normalSkillIconSpawner;
    SkillIconSpawner atbSkillIconSpawner;

    Battle_Controller battleController;
    BattlerStateTag batterStateTag;

    List<SkillBase> normalSkills = new List<SkillBase>();
    List<SkillBase> atbSkills = new List<SkillBase>();
    Dictionary<ISkillMode, SkillBase> skillModeSkillDic = new Dictionary<ISkillMode, SkillBase>();

    /// <summary>
    /// Spawner 参数可为 null（敌人路径），此时仅创建 Charger 不生成图标。
    /// </summary>
    public BattleSkiller(SkillIconSpawner _normalSkillIconSpawner, SkillIconSpawner _atbSkillIconSpawner, IBattlable self, BattlerStateTag stateTag)
    {
        normalSkillIconSpawner = _normalSkillIconSpawner;
        atbSkillIconSpawner = _atbSkillIconSpawner;
        batterStateTag = stateTag;
        InitSkiller(self);
    }

    IBattlable self;
    public IBattlable Self => self;

    ATBIntentionExecutor atbExecutor;

    bool DoCycle;

    /// <summary>
    /// 注册每个实际技能效果逻辑
    /// </summary>
    void InitSkillsBatch(List<int> skillIDList)
    {
        var _normalSkills = BattleSkillFactory.CreateBattleSkillsBatch(skillIDList, self);
        foreach (var skill in _normalSkills)
            normalSkills.Add(skill);
    }

    void InitATBSkillsBatch(List<int> skillIDList)
    {
        var _atbSkills = BattleSkillFactory.CreateBattleSkillsBatch(skillIDList, self);
        foreach (var skill in _atbSkills)
            atbSkills.Add(skill);
    }

    void InitSkiller(IBattlable self)
    {
        this.self = self;
        DoCycle = true;

        EventCenter.AddEventListener<BattlerStateTag>(E_EventType.Battle_CharacterBreak, SkillsBreakCheck);
        EventCenter.AddEventListener(E_EventType.Battle_CharacterBreakRefresh, OnBreakRefresh);
    }

    void OnBreakRefresh()
    {
        if (batterStateTag != null && !batterStateTag.State_Break)
        {
            atbExecutor?.Resume();
            // 力竭恢复后解冻所有技能充能
            foreach (var mode in normalSkillModes)
                mode.Freeze(false);
            foreach (var mode in atbSkillModes)
                mode.Freeze(false);
        }
    }

    public void InitATBIntention(Battle_Controller controller, ATBIntentionConfigSO config)
    {
        if (config == null || config.atbIntentionIndices.Count == 0)
        {
            DebugManager.LogWarning(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} ATB config为空或意图列表为空，将不会初始化");
            return;
        }
        atbExecutor = new ATBIntentionExecutor(self, controller, config);
        DebugManager.Log(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} ATB执行器已初始化");
    }

    public void SetATBExecutor(ATBIntentionExecutor executor)
    {
        atbExecutor = executor;
        DebugManager.Log(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} ATB执行器已设置(地图配置)");
    }

    /// <summary>
    /// 从AutoSkillConfigSO加载自动技能。有 Spawner 时生成图标，否则仅创建 Charger。
    /// </summary>
    public void LoadAutoSkillsFromConfig(AutoSkillConfigSO config, int autoSlotCount = 9)
    {
        if (config == null || config.autoSkills.Count == 0) return;

        SkillData[] skillDatas = config.GetAutoSkillDatas();
        normalSkillIDs.Clear();
        var dataList = new List<SkillData>();
        for (int i = 0; i < skillDatas.Length; i++)
        {
            if (skillDatas[i] != null)
            {
                normalSkillIDs.Add(skillDatas[i].skill_ID);
                dataList.Add(skillDatas[i]);
            }
        }

        normalSkills.Clear();
        normalSkillIcons.Clear();
        normalSkillModes.Clear();

        // 有 Spawner：生成图标 + 从图标提取 ISkillMode
        if (normalSkillIconSpawner != null)
        {
            normalSkillIconSpawner.skillMode = E_SkillMode.Auto;
            normalSkillIconSpawner.UnloadSkills();
            normalSkillIcons = normalSkillIconSpawner.LoadSlotsAndSkills(autoSlotCount, dataList, false, true);
            InitSkillsBatch(normalSkillIDs);

            for (int i = 0; i < normalSkillIcons.Count; i++)
            {
                if (i < normalSkills.Count)
                {
                    normalSkillIcons[i].InitBattleSkill(normalSkills[i]);
                    var mode = normalSkillIcons[i].SkillMode;
                    normalSkillModes.Add(mode);
                    skillModeSkillDic[mode] = normalSkills[i];
                }
            }
        }
        else
        {
            // 无_ Spawner（敌人）：直接创建 AutoMode
            InitSkillsBatch(normalSkillIDs);
            for (int i = 0; i < dataList.Count; i++)
            {
                if (i < normalSkills.Count)
                {
                    var mode = new AutoMode();
                    mode.Init(dataList[i], normalSkills[i]);
                    normalSkillModes.Add(mode);
                    skillModeSkillDic[mode] = normalSkills[i];
                }
            }
        }

        DebugManager.Log(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} 从AutoSkillConfig加载了{normalSkillIDs.Count}个自动技能 (icons={normalSkillIcons.Count}), ID列表:[{string.Join(",", normalSkillIDs)}]");
    }

    /// <summary>
    /// 从ATBIntentionConfigSO加载主动技能。有 Spawner 时生成图标，否则仅创建 Charger。
    /// </summary>
    public void LoadActiveSkillsFromConfig(ATBIntentionConfigSO config, int atbSlotCount = 5)
    {
        if (config == null || config.activeSkills.Count == 0) return;

        var atbSkillDatas = new List<SkillData>();
        atbSkillIDs.Clear();

        foreach (int oneBasedIdx in config.atbIntentionIndices)
        {
            int zeroBased = oneBasedIdx - 1;
            if (zeroBased >= 0 && zeroBased < config.activeSkills.Count)
            {
                var so = ResourcesLoader.FindSkillSOBySkillName(config.activeSkills[zeroBased]);
                if (so != null)
                {
                    atbSkillDatas.Add(new SkillData(so));
                    atbSkillIDs.Add(so.skill_ID);
                }
            }
        }

        int slotCount = Mathf.Min(atbSlotCount, atbSkillDatas.Count);
        if (atbSkillDatas.Count > slotCount)
        {
            atbSkillDatas = atbSkillDatas.GetRange(0, slotCount);
            atbSkillIDs = atbSkillIDs.GetRange(0, slotCount);
        }

        atbSkills.Clear();
        atbSkillIcons.Clear();
        atbSkillModes.Clear();

        if (atbSkillDatas.Count == 0) return;

        if (atbSkillIconSpawner != null)
        {
            atbSkillIconSpawner.skillMode = E_SkillMode.ATB;
            atbSkillIconSpawner.UnloadSkills();
            atbSkillIcons = atbSkillIconSpawner.LoadSlotsAndSkills(slotCount, atbSkillDatas, false, true);
            InitATBSkillsBatch(atbSkillIDs);
            for (int i = 0; i < atbSkillIcons.Count; i++)
            {
                if (i < atbSkills.Count)
                {
                    atbSkillIcons[i].InitBattleSkill(atbSkills[i]);
                    var mode = atbSkillIcons[i].SkillMode;
                    atbSkillModes.Add(mode);
                    skillModeSkillDic[mode] = atbSkills[i];
                }
            }
        }
        else
        {
            // 敌人路径：ATB技能仍需 ISkillMode（目前用Auto占位，实际由ATBIntentionExecutor驱动）
            InitATBSkillsBatch(atbSkillIDs);
            for (int i = 0; i < atbSkillDatas.Count; i++)
            {
                if (i < atbSkills.Count)
                {
                    var mode = new AutoMode();
                    mode.Init(atbSkillDatas[i], atbSkills[i]);
                    atbSkillModes.Add(mode);
                    skillModeSkillDic[mode] = atbSkills[i];
                }
            }
        }

        DebugManager.Log(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} 从ATBConfig加载了{atbSkillIDs.Count}个主动技能 (icons={atbSkillIcons.Count}), ID列表:[{string.Join(",", atbSkillIDs)}]");
    }

    /// <summary>
    /// 从技能ID列表加载自动技能（地图配置路径）
    /// </summary>
    public void LoadAutoSkillsFromIDs(List<int> skillIDs, int autoSlotCount = 9)
    {
        if (skillIDs == null || skillIDs.Count == 0) return;

        normalSkillIDs.Clear();
        normalSkillIDs.AddRange(skillIDs);

        var dataList = new List<SkillData>();
        foreach (int id in skillIDs)
        {
            var so = ResourcesLoader.FindSkillSOByID(id);
            if (so != null)
                dataList.Add(new SkillData(so));
        }

        normalSkills.Clear();
        normalSkillIcons.Clear();
        normalSkillModes.Clear();

        if (normalSkillIconSpawner != null)
        {
            normalSkillIconSpawner.skillMode = E_SkillMode.Auto;
            normalSkillIconSpawner.UnloadSkills();
            normalSkillIcons = normalSkillIconSpawner.LoadSlotsAndSkills(autoSlotCount, dataList, false, true);
            InitSkillsBatch(normalSkillIDs);

            for (int i = 0; i < normalSkillIcons.Count; i++)
            {
                if (i < normalSkills.Count)
                {
                    normalSkillIcons[i].InitBattleSkill(normalSkills[i]);
                    var mode = normalSkillIcons[i].SkillMode;
                    normalSkillModes.Add(mode);
                    skillModeSkillDic[mode] = normalSkills[i];
                }
            }
        }
        else
        {
            InitSkillsBatch(normalSkillIDs);
            for (int i = 0; i < dataList.Count; i++)
            {
                if (i < normalSkills.Count)
                {
                    var mode = new AutoMode();
                    mode.Init(dataList[i], normalSkills[i]);
                    normalSkillModes.Add(mode);
                    skillModeSkillDic[mode] = normalSkills[i];
                }
            }
        }

        DebugManager.Log(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} 从ID列表加载了{normalSkillIDs.Count}个自动技能 (icons={normalSkillIcons.Count}), ID列表:[{string.Join(",", normalSkillIDs)}]");
    }

    /// <summary>
    /// 从技能ID列表加载主动技能（地图配置路径）
    /// </summary>
    public void LoadActiveSkillsFromIDs(List<int> skillIDs, int atbSlotCount = 5)
    {
        if (skillIDs == null || skillIDs.Count == 0) return;

        atbSkillIDs.Clear();
        atbSkillIDs.AddRange(skillIDs);

        var atbSkillDatas = new List<SkillData>();
        foreach (int id in skillIDs)
        {
            var so = ResourcesLoader.FindSkillSOByID(id);
            if (so != null)
                atbSkillDatas.Add(new SkillData(so));
        }

        int slotCount = Mathf.Min(atbSlotCount, atbSkillDatas.Count);
        if (atbSkillDatas.Count > slotCount)
        {
            atbSkillDatas = atbSkillDatas.GetRange(0, slotCount);
            atbSkillIDs = atbSkillIDs.GetRange(0, slotCount);
        }

        atbSkills.Clear();
        atbSkillIcons.Clear();
        atbSkillModes.Clear();

        if (atbSkillDatas.Count == 0) return;

        if (atbSkillIconSpawner != null)
        {
            atbSkillIconSpawner.skillMode = E_SkillMode.ATB;
            atbSkillIconSpawner.UnloadSkills();
            atbSkillIcons = atbSkillIconSpawner.LoadSlotsAndSkills(slotCount, atbSkillDatas, false, true);
            InitATBSkillsBatch(atbSkillIDs);
            for (int i = 0; i < atbSkillIcons.Count; i++)
            {
                if (i < atbSkills.Count)
                {
                    atbSkillIcons[i].InitBattleSkill(atbSkills[i]);
                    var mode = atbSkillIcons[i].SkillMode;
                    atbSkillModes.Add(mode);
                    skillModeSkillDic[mode] = atbSkills[i];
                }
            }
        }
        else
        {
            InitATBSkillsBatch(atbSkillIDs);
            for (int i = 0; i < atbSkillDatas.Count; i++)
            {
                if (i < atbSkills.Count)
                {
                    var mode = new AutoMode();
                    mode.Init(atbSkillDatas[i], atbSkills[i]);
                    atbSkillModes.Add(mode);
                    skillModeSkillDic[mode] = atbSkills[i];
                }
            }
        }

        DebugManager.Log(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} 从ID列表加载了{atbSkillIDs.Count}个主动技能 (icons={atbSkillIcons.Count}), ID列表:[{string.Join(",", atbSkillIDs)}]");
    }

    bool _atbUpdateLoggedOnce;
    public void OnATBUpdate(float deltaTime)
    {
        if (atbExecutor == null)
        {
            if (!_atbUpdateLoggedOnce)
            {
                DebugManager.LogWarning(EDebugCategory.BattleSkiller, $"[BattleSkiller] {self.Camp} OnATBUpdate: atbExecutor为null，跳过ATB更新");
                _atbUpdateLoggedOnce = true;
            }
            return;
        }
        atbExecutor.OnUpdate(deltaTime);
    }

    /// <summary>
    /// 由AI指令直接触发技能，绕过普通冷却循环流程
    /// </summary>
    public void DoSkillExecutionNow(int skillID, E_SkillLevel level, int henceTime = 0)
    {
        var skill = BattleSkillFactory.CreateBattleSkill(skillID, self);
        skill?.SkillExcute(level, henceTime);
    }

    public void OnSkillUpdate(float currentSP)
    {
        if (DoCycle)
            DoSkillsUpdate(currentSP);
    }

    void FreezeSkill(int ID, bool freeze)
    {
        foreach (var mode in normalSkillModes)
            if (mode.SkillData.skill_ID == ID)
                mode.Freeze(freeze);

        foreach (var mode in atbSkillModes)
            if (mode.SkillData.skill_ID == ID)
                mode.Freeze(freeze);
    }

    /// <summary>
    /// 力竭时冻结所有技能冷却
    /// </summary>
    void SkillsBreakCheck(BattlerStateTag tag)
    {
        if (batterStateTag == tag)
        {
            atbExecutor?.Pause();
            foreach (var mode in normalSkillModes)
                mode.SkillBreak();
            foreach (var mode in atbSkillModes)
                mode.SkillBreak();
        }
        else
            DebugManager.Log(EDebugCategory.BattleSkiller, "没被冻结");
    }

    public void StopATB() => atbExecutor?.Pause();
    public void ResumeATB() => atbExecutor?.Resume();

    /// <summary>
    /// 追加次级技能效果
    /// </summary>
    void AppendBattleSkillEffect(int skillID)
    {
        normalSkills.Add(BattleSkillFactory.CreateBattleSkill(skillID, self));
    }

    void DoSkillsUpdate(float currentSP)
    {
        float dt = Time.deltaTime;
        foreach (var mode in normalSkillModes)
            mode.Update(currentSP, dt);
    }

    /// <summary>
    /// ATB主动技能模式更新（处理Q/W/E输入、时缓等）
    /// </summary>
    public void OnATBModeUpdate(float currentSP)
    {
        float dt = Time.deltaTime;
        foreach (var mode in atbSkillModes)
            mode.Update(currentSP, dt);
    }
}
