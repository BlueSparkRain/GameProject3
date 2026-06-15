using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 随机事件管理器——负责选取随机事件并执行选项效果
/// </summary>
public class UnknownEventManager : MonoGlobalManager
{
    const string soLoadPath = "SOData/UnknownEventSOData";

    List<UnknownSOData> _eventPool;
    Dictionary<E_UnknownEventType, UnknownSOData> _eventDict;
    Dictionary<E_UnknownEventType, List<UnknownEventOption>> _optionRegistry;
    bool _poolLoaded;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        LoadEventPool();
        BuildEventRegistry();
    }

    void LoadEventPool()
    {
        var all = Resources.LoadAll<UnknownSOData>(soLoadPath);
        _eventPool = new List<UnknownSOData>(all);
        _eventDict = new Dictionary<E_UnknownEventType, UnknownSOData>();
        foreach (var so in all)
            _eventDict[so.eventType] = so;
        _poolLoaded = true;
        DebugManager.Log(EDebugCategory.MapRoom, $"[UnknownEventManager] 从 Resources/{soLoadPath} 加载了{_eventPool.Count}个随机事件SO");
    }

    // ============================================================
    // 事件选项注册表（所有随机事件的选项逻辑集中定义于此）
    // ============================================================
    void BuildEventRegistry()
    {
        _optionRegistry = new Dictionary<E_UnknownEventType, List<UnknownEventOption>>();

        _optionRegistry[E_UnknownEventType.潘多拉魔盒] = new List<UnknownEventOption>
        {
            new UnknownEventOption("将所有技能替换成随机技能", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.ReplaceAllSkills_Random, 0)
            }),
            new UnknownEventOption("获得一次技能选择奖励", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.SkillSelectReward, 1)
            }),
        };

        _optionRegistry[E_UnknownEventType.锻造大师] = new List<UnknownEventOption>
        {
            new UnknownEventOption("获得1件剑类型的随机装备", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GrantEquipment_BySlot, (int)E_EquipmentSlot.Sword)
            }),
            new UnknownEventOption("获得1件枪类型的随机装备", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GrantEquipment_BySlot, (int)E_EquipmentSlot.Spear)
            }),
            new UnknownEventOption("获得1件弓类型的随机装备", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GrantEquipment_BySlot, (int)E_EquipmentSlot.Bow)
            }),
        };

        _optionRegistry[E_UnknownEventType.奥秘] = new List<UnknownEventOption>
        {
            new UnknownEventOption("获得一次技能选择奖励", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.SkillSelectReward, 1)
            }),
            new UnknownEventOption("失去4点活力值，获得两次技能选择奖励", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.LoseVitality, 4),
                new UnknownEventEffect(E_UnknownEventEffectType.SkillSelectReward, 2),
            }),
        };

        _optionRegistry[E_UnknownEventType.财富权力名望] = new List<UnknownEventOption>
        {
            new UnknownEventOption("获得1500金币", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GainGold, 1500)
            }),
            new UnknownEventOption("回复25%最大活力值&行动值", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.RestoreVitalityAndActionPct, 25)
            }),
            new UnknownEventOption("获得5000经验值", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GainExp, 5000)
            }),
        };

        _optionRegistry[E_UnknownEventType.贪婪] = new List<UnknownEventOption>
        {
            new UnknownEventOption("获得1000金币", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GainGold, 1000)
            }),
            new UnknownEventOption("失去2点活力值，获得3000金币", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.LoseVitality, 2),
                new UnknownEventEffect(E_UnknownEventEffectType.GainGold, 3000),
            }),
        };

        _optionRegistry[E_UnknownEventType.精通] = new List<UnknownEventOption>
        {
            new UnknownEventOption("失去6点活力值，获得1个自动化技能槽", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.LoseVitality, 6),
                new UnknownEventEffect(E_UnknownEventEffectType.UnlockAutoSkillSlot, 1),
            }),
            new UnknownEventOption("失去6点活力值，获得1个ATB技能槽", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.LoseVitality, 6),
                new UnknownEventEffect(E_UnknownEventEffectType.UnlockATBSlot, 1),
            }),
            new UnknownEventOption("回复2点活力值", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GainVitality, 2)
            }),
        };

        _optionRegistry[E_UnknownEventType.祝福] = new List<UnknownEventOption>
        {
            new UnknownEventOption("获得5点行动值", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GainActionPoints, 5)
            }),
            new UnknownEventOption("获得1000经验值", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GainExp, 1000)
            }),
            new UnknownEventOption("获得5点活力值", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.GainVitality, 5)
            }),
        };

        _optionRegistry[E_UnknownEventType.抉择] = new List<UnknownEventOption>
        {
            new UnknownEventOption("失去8点活力值，获得2个ATB技能槽", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.LoseVitality, 8),
                new UnknownEventEffect(E_UnknownEventEffectType.UnlockATBSlot, 2),
            }),
            new UnknownEventOption("失去8点活力值，获得三次技能选择奖励", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.LoseVitality, 8),
                new UnknownEventEffect(E_UnknownEventEffectType.SkillSelectReward, 3),
            }),
            new UnknownEventOption("失去8点活力值，获得3件随机装备", new List<UnknownEventEffect>
            {
                new UnknownEventEffect(E_UnknownEventEffectType.LoseVitality, 8),
                new UnknownEventEffect(E_UnknownEventEffectType.GrantRandomEquipment, 3),
            }),
        };
    }

    /// <summary>获取指定事件的所有选项（从代码注册表读取）</summary>
    public List<UnknownEventOption> GetEventOptions(E_UnknownEventType eventType)
    {
        if (_optionRegistry == null) BuildEventRegistry();
        _optionRegistry.TryGetValue(eventType, out var options);
        return options;
    }

    public void ReloadPool()
    {
        _poolLoaded = false;
        _eventDict?.Clear();
        LoadEventPool();
    }

    /// <summary>按事件类型精确获取事件SO</summary>
    public UnknownSOData GetEventByType(E_UnknownEventType eventType)
    {
        if (!_poolLoaded) LoadEventPool();
        _eventDict.TryGetValue(eventType, out var so);
        return so;
    }

    /// <summary>随机获取一个事件SO</summary>
    public UnknownSOData GetRandomEvent()
    {
        if (!_poolLoaded) LoadEventPool();
        if (_eventPool == null || _eventPool.Count == 0)
        {
            Debug.LogError("[UnknownEventManager] 随机事件池为空");
            return null;
        }
        return _eventPool[Random.Range(0, _eventPool.Count)];
    }

    /// <summary>执行一个选项的所有效果</summary>
    public void ExecuteOption(UnknownEventOption option)
    {
        if (option?.effects == null) return;
        foreach (var effect in option.effects)
            ExecuteEffect(effect);
    }

    void ExecuteEffect(UnknownEventEffect effect)
    {
        switch (effect.type)
        {
            case E_UnknownEventEffectType.ReplaceAllSkills_Random:
                ReplaceAllSkillsRandom();
                break;

            case E_UnknownEventEffectType.SkillSelectReward:
                OpenSkillSelectReward(effect.param1 > 0 ? effect.param1 : 1);
                break;

            case E_UnknownEventEffectType.GrantEquipment_BySlot:
                GrantEquipmentBySlot((E_EquipmentSlot)effect.param1);
                break;

            case E_UnknownEventEffectType.GrantRandomEquipment:
                GrantRandomEquipment(effect.param1 > 0 ? effect.param1 : 1);
                break;

            case E_UnknownEventEffectType.LoseVitality:
                GameRoot.GetManager<VitalityPointsManager>()?.AdjustVolityPoints(-Mathf.Abs(effect.param1));
                break;

            case E_UnknownEventEffectType.GainGold:
                GameRoot.GetManager<GoldManager>()?.AddGold(effect.param1);
                break;

            case E_UnknownEventEffectType.RestoreVitalityAndActionPct:
                RestoreVitalityAndActionPct(effect.param1 > 0 ? effect.param1 : 25);
                break;

            case E_UnknownEventEffectType.GainExp:
                GainExp(effect.param1);
                break;

            case E_UnknownEventEffectType.UnlockAutoSkillSlot:
                UnlockAutoSkillSlot(effect.param1 > 0 ? effect.param1 : 1);
                break;

            case E_UnknownEventEffectType.UnlockATBSlot:
                UnlockATBSlot(effect.param1 > 0 ? effect.param1 : 1);
                break;

            case E_UnknownEventEffectType.GainActionPoints:
                GameRoot.GetManager<ActionPointsManager>()?.AddActionPoints(effect.param1);
                break;

            case E_UnknownEventEffectType.GainVitality:
                GameRoot.GetManager<VitalityPointsManager>()?.AdjustVolityPoints(effect.param1);
                break;

            default:
                DebugManager.LogWarning(EDebugCategory.MapRoom, $"[UnknownEventManager] 未处理的效果类型: {effect.type}");
                break;
        }
    }

    #region 效果实现

    void ReplaceAllSkillsRandom()
    {
        // TODO: 接入技能系统后实现——随机选3个技能ID(0-59)替换全部技能
        var newIds = RandomUtility.GetUniqueRandomList(3, 0, 59);
        DebugManager.Log(EDebugCategory.MapRoom, $"[UnknownEventManager] [TODO] 技能替换为: {string.Join(", ", newIds)}");
    }

    void OpenSkillSelectReward(int count)
    {
        var uiMgr = GameRoot.GetManager<UIManager>();
        if (uiMgr == null) return;
        int rewardCount = count > 0 ? count : 1;
        uiMgr.OpenPanel<SkillSelectPanel>(E_UIPanelType.SkillSelectPanel,
            panel =>
            {
                panel.maxSelectCount = rewardCount;
                panel.SetttleSelect();
            });
    }

    void GrantEquipmentBySlot(E_EquipmentSlot slot)
    {
        int chaosLevel = GameRoot.GetManager<ChaosLevelManager>()?.currentLevel ?? 1;
        var equip = EquipmentGenerator.GenerateForSlot(slot, chaosLevel);
        if (equip != null)
            GameRoot.GetManager<EquipBacketManager>()?.AddEquipment(equip);
    }

    void GrantRandomEquipment(int count)
    {
        int chaosLevel = GameRoot.GetManager<ChaosLevelManager>()?.currentLevel ?? 1;
        for (int i = 0; i < count; i++)
        {
            var equip = EquipmentGenerator.Generate(chaosLevel);
            if (equip != null)
                GameRoot.GetManager<EquipBacketManager>()?.AddEquipment(equip);
        }
    }

    void RestoreVitalityAndActionPct(int percent)
    {
        var vitalityMgr = GameRoot.GetManager<VitalityPointsManager>();
        if (vitalityMgr != null)
        {
            int restoreV = Mathf.RoundToInt(vitalityMgr.max_VitalityPoints * percent / 100f);
            vitalityMgr.AdjustVolityPoints(restoreV);
        }

        var apMgr = GameRoot.GetManager<ActionPointsManager>();
        if (apMgr != null)
        {
            int restoreA = Mathf.RoundToInt(apMgr.MaxActionPoints * percent / 100f);
            apMgr.AddActionPoints(restoreA);
        }
    }

    void GainExp(int amount)
    {
        var playerTag = FindPlayerTag();
        if (playerTag == null) return;
        var levelHandler = playerTag.GetComponent<CharacterLevelUpHandler>();
        levelHandler?.AdjustEXP(amount);
    }

    void UnlockAutoSkillSlot(int count)
    {
        var playerTag = FindPlayerTag();
        playerTag?.CharacterData?.UnlockAutoSlot(count);
    }

    void UnlockATBSlot(int count)
    {
        var playerTag = FindPlayerTag();
        if (playerTag == null) return;
        playerTag.CharacterData?.UnlockAtbSlot(count);
    }

    CharacterHandler FindPlayerTag()
    {
        var tags = Object.FindObjectsOfType<CharacterHandler>();
        foreach (var t in tags)
            if (t.isPlayer) return t;
        return null;
    }

    #endregion

    public override void MgrUpdate(float deltaTime) { }
}
