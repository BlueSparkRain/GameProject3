using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 随机事件类型枚举——在SO上选择即可关联对应的选项逻辑
/// </summary>
public enum E_UnknownEventType
{
    潘多拉魔盒,
    锻造大师,
    奥秘,
    财富权力名望,
    贪婪,
    精通,
    祝福,
    抉择,
}

/// <summary>
/// 随机事件效果类型枚举
/// </summary>
public enum E_UnknownEventEffectType
{
    ReplaceAllSkills_Random,
    SkillSelectReward,
    GrantEquipment_BySlot,
    GrantRandomEquipment,
    LoseVitality,
    GainGold,
    RestoreVitalityAndActionPct,
    GainExp,
    UnlockAutoSkillSlot,
    UnlockATBSlot,
    GainActionPoints,
    GainVitality,
}

/// <summary>
/// 单个事件效果定义（纯代码数据，不序列化到SO）
/// </summary>
public class UnknownEventEffect
{
    public E_UnknownEventEffectType type;
    public int param1;
    public int param2;

    public UnknownEventEffect(E_UnknownEventEffectType type, int param1 = 0, int param2 = 0)
    {
        this.type = type;
        this.param1 = param1;
        this.param2 = param2;
    }
}

/// <summary>
/// 随机事件选项定义（纯代码数据，不序列化到SO）
/// </summary>
public class UnknownEventOption
{
    public string description;
    public List<UnknownEventEffect> effects;

    public UnknownEventOption(string description, List<UnknownEventEffect> effects)
    {
        this.description = description;
        this.effects = effects;
    }
}

/// <summary>
/// 随机事件SO数据——仅存储展示信息，选项逻辑在 UnknownEventManager.BuildEventRegistry() 中定义
/// </summary>
[CreateAssetMenu(menuName = "SOData/UnknownEventSO", fileName = "UnknownEvent_", order = 0)]
public class UnknownSOData : ScriptableObject
{
    [Header("事件类型(关联代码中的选项逻辑)")]
    public E_UnknownEventType eventType;

    [Header("事件描述文本")]
    public string description;

    [Header("事件背景图")]
    public Sprite background;

    [Header("各选项文本(仅展示,逻辑由代码注册表控制)")]
    public List<string> optionDescriptions;

    /// <summary>事件展示名称（由枚举自动生成）</summary>
    public string eventName => eventType.ToString();
}
