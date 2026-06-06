using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗技能管理器 —— 专门管理当前的技能Icon的冷却循环
/// 在战斗前会从某个CharacterSkiller中将当前已配置的技能数据传递过来（第一次创建时）。
/// 生成对应的技能图标，并注册到skiller中
/// </summary>
public class BattleSkiller
{
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
    Dictionary<SkillIcon, SkillBase> skillIconDic = new Dictionary<SkillIcon, SkillBase>();

    public BattleSkiller(SkillIconSpawner _normalSkillIconSpawner,SkillIconSpawner _atbSkillIconSpawner,IBattlable self, BattlerStateTag stateTag){
        normalSkillIconSpawner=_normalSkillIconSpawner;
        atbSkillIconSpawner=_atbSkillIconSpawner;
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
    void InitSkillsBatch(List<int> skillIDList){
        var _normalSkills = BattleSkillFactory.CreateBattleSkillsBatch(skillIDList, self);
        foreach (var skill in _normalSkills){
            normalSkills.Add(skill);
        }
    }

    void InitATBSkillsBatch(List<int> skillIDList){
        var _atbSkills = BattleSkillFactory.CreateBattleSkillsBatch(skillIDList, self);
        foreach (var skill in _atbSkills){
            atbSkills.Add(skill);
        }
    }

    void InitSkiller(IBattlable self){
        this.self = self;
        DoCycle = true;

        EventCenter.AddEventListener<BattlerStateTag>(E_EventType.Battle_CharacterBreak,SkillsBreakCheck);
        EventCenter.AddEventListener(E_EventType.Battle_CharacterBreakRefresh, OnBreakRefresh);
    }

    void OnBreakRefresh()
    {
        if (batterStateTag != null && !batterStateTag.State_Break)
            atbExecutor?.Resume();
    }

    public void InitATBIntention(Battle_Controller controller, ATBIntentionConfigSO config)
    {
        if (config == null || config.atbIntentionIndices.Count == 0)
        {
            Debug.LogWarning($"[BattleSkiller] {self.Camp} ATB config为空或意图列表为空，将不会初始化");
            return;
        }
        atbExecutor = new ATBIntentionExecutor(self, controller, config);
        Debug.Log($"[BattleSkiller] {self.Camp} ATB执行器已初始化");
    }

    /// <summary>
    /// 从AutoSkillConfigSO加载自动技能 到 normalSkillIconSpawner
    /// 这些技能会在战斗中自动循环释放（基础版本，消耗SP）
    /// </summary>
    public void LoadAutoSkillsFromConfig(AutoSkillConfigSO config, int autoSlotCount = 9)
    {
        if (config == null || config.autoSkills.Count == 0) return;

        normalSkillIconSpawner.UnloadSkills();

        SkillData[] skillDatas = config.GetAutoSkillDatas();
        normalSkillIDs.Clear();
        for (int i = 0; i < skillDatas.Length; i++)
        {
            if (skillDatas[i] != null)
                normalSkillIDs.Add(skillDatas[i].skill_ID);
        }

        var dataList = new List<SkillData>();
        for (int i = 0; i < skillDatas.Length; i++)
        {
            if (skillDatas[i] != null)
                dataList.Add(skillDatas[i]);
        }

        normalSkills.Clear();
        normalSkillIcons = normalSkillIconSpawner.LoadSlotsAndSkills(autoSlotCount, dataList, false, true);
        InitSkillsBatch(normalSkillIDs);

        for (int i = 0; i < normalSkillIcons.Count; i++)
        {
            if (i < normalSkills.Count)
            {
                normalSkillIcons[i].InitBattleSkill(normalSkills[i]);
                skillIconDic[normalSkillIcons[i]] = normalSkills[i];
            }
        }

        Debug.Log($"[BattleSkiller] {self.Camp} 从AutoSkillConfig加载了{normalSkillIDs.Count}个自动技能");
    }

    /// <summary>
    /// 从ATBIntentionConfigSO加载主动技能 到 atbSkillIconSpawner
    /// 根据 atbIntentionIndices 从 activeSkills 中筛选，由ATBIntentionExecutor进行释放（加强版本，消耗ATB）
    /// </summary>
    public void LoadActiveSkillsFromConfig(ATBIntentionConfigSO config, int atbSlotCount = 5)
    {
        if (config == null || config.activeSkills.Count == 0) return;

        atbSkillIconSpawner.UnloadSkills();

        // 按 atbIntentionIndices（1-based）筛选ATB主动技能
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

        // 截断技能数据以适配可用槽位
        int slotCount = Mathf.Min(atbSlotCount, atbSkillDatas.Count);
        if (atbSkillDatas.Count > slotCount)
        {
            atbSkillDatas = atbSkillDatas.GetRange(0, slotCount);
            atbSkillIDs = atbSkillIDs.GetRange(0, slotCount);
        }

        atbSkills.Clear();
        if (atbSkillDatas.Count > 0)
        {
            atbSkillIcons = atbSkillIconSpawner.LoadSlotsAndSkills(slotCount, atbSkillDatas, false, true);
            InitATBSkillsBatch(atbSkillIDs);
            for (int i = 0; i < atbSkillIcons.Count; i++)
            {
                if (i < atbSkills.Count)
                {
                    atbSkillIcons[i].InitBattleSkill(atbSkills[i]);
                    skillIconDic[atbSkillIcons[i]] = atbSkills[i];
                }
            }
        }

        Debug.Log($"[BattleSkiller] {self.Camp} 从ATBConfig加载了{atbSkillIDs.Count}个主动技能, ID列表:[{string.Join(",", atbSkillIDs)}]");
    }

    bool _atbUpdateLoggedOnce;
    public void OnATBUpdate(float deltaTime)
    {
        if (atbExecutor == null)
        {
            if (!_atbUpdateLoggedOnce)
            {
                Debug.LogWarning($"[BattleSkiller] {self.Camp} OnATBUpdate: atbExecutor为null，跳过ATB更新");
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

    public void OnSkillUpdate(float currentSP){
        if (DoCycle)
            DoSkillsUpdate(currentSP);
    }
    void FreezeSkill(int ID, bool freeze)
    {
        foreach (var icon in normalSkillIcons)
            if (icon.SkillData.skill_ID == ID)
                icon.FreezeIcon(freeze);

        foreach (var icon in atbSkillIcons)
            if (icon.SkillData.skill_ID == ID)
                icon.FreezeIcon(freeze);
    }

    /// <summary>
    /// 力竭时冻结所有技能icon的冷却
    /// </summary>
    void SkillsBreakCheck(BattlerStateTag tag) {

        if (batterStateTag == tag){
            atbExecutor?.Pause();
            foreach (var icon in normalSkillIcons){
                icon.SkillBreak();
            }
            foreach (var icon in atbSkillIcons){
                icon.SkillBreak();
            }
        }
        else
        Debug.Log("没被冻结");

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

    void DoSkillsUpdate(float currentSP){
      foreach (var SkillIcon in normalSkillIcons){
          SkillIcon.IconCycleUpdate(currentSP);
      }
    }
}
