using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// 静态事件中心（改造完成）
/// 功能：无参/单参/多参(2/3/4) + 优先级事件，全局静态调用
/// </summary>
public static class EventCenter
{
    /// <summary>
    /// 全局事件字典（静态）
    /// </summary>
    private static Dictionary<E_EventType, IEventInfo> eventDic = new Dictionary<E_EventType, IEventInfo>();

    #region 无参事件（原有功能，静态化）
    /// <summary>
    /// 添加无参监听
    /// </summary>
    public static void AddEventListener(E_EventType name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions -= action;
            (eventDic[name] as EventInfo).actions += action;
        }
        else
            eventDic.Add(name, new EventInfo(action));
    }

    /// <summary>
    /// 触发无参事件
    /// </summary>
    public static void EventTrigger(E_EventType name)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo)?.actions?.Invoke();
    }

    /// <summary>
    /// 移除无参监听
    /// </summary>
    public static void RemoveEventListener(E_EventType name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo).actions -= action;
    }
    #endregion

    #region 无参优先级事件（原有功能，静态化）
    /// <summary>
    /// 添加无参优先级监听
    /// </summary>
    public static void AddEventListener(E_EventType name, PriorityAction priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo).GetNewEvent(priorityAction);
        else
            eventDic.Add(name, new PriorityEventInfo(priorityAction));
    }

    /// <summary>
    /// 触发无参优先级事件
    /// </summary>
    public static void EventTrigger_Priority(E_EventType name)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo).allActions.ForEach(val => { val.Action?.Invoke(); });
    }

    /// <summary>
    /// 移除无参优先级监听
    /// </summary>
    public static void RemoveEventListener(E_EventType name, PriorityAction priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo).allActions.Remove(priorityAction);
    }
    #endregion

    #region 单参事件（原有功能，静态化）
    /// <summary>
    /// 添加单参监听
    /// </summary>
    public static void AddEventListener<T>(E_EventType name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T>).actions -= action;
            (eventDic[name] as EventInfo<T>).actions += action;
        }
        else
            eventDic.Add(name, new EventInfo<T>(action));
    }

    /// <summary>
    /// 触发单参事件
    /// </summary>
    public static void EventTrigger<T>(E_EventType name, T info)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>)?.actions?.Invoke(info);
    }

    /// <summary>
    /// 移除单参监听
    /// </summary>
    public static void RemoveEventListener<T>(E_EventType name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions -= action;
    }
    #endregion

    #region 单参优先级事件（原有功能，静态化）
    /// <summary>
    /// 添加单参优先级监听
    /// </summary>
    public static void AddEventListener<T>(E_EventType name, PriorityAction<T> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T>).GetNewEvent(priorityAction);
        else
            eventDic.Add(name, new PriorityEventInfo<T>(priorityAction));
    }

    /// <summary>
    /// 触发单参优先级事件
    /// </summary>
    public static void EventTrigger_Priority<T>(E_EventType name, T info)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T>).allActions.ForEach(val => { val.Action?.Invoke(info); });
    }

    /// <summary>
    /// 移除单参优先级监听
    /// </summary>
    public static void RemoveEventListener<T>(E_EventType name, PriorityAction<T> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T>).allActions.Remove(priorityAction);
    }
    #endregion

    #region 新增：双参数事件（普通+优先级）
    public static void AddEventListener<T1, T2>(E_EventType name, UnityAction<T1, T2> action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T1, T2>).actions -= action;
            (eventDic[name] as EventInfo<T1, T2>).actions += action;
        }
        else
            eventDic.Add(name, new EventInfo<T1, T2>(action));
    }
    public static void EventTrigger<T1, T2>(E_EventType name, T1 arg1, T2 arg2)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T1, T2>)?.actions?.Invoke(arg1, arg2);
    }
    public static void RemoveEventListener<T1, T2>(E_EventType name, UnityAction<T1, T2> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T1, T2>).actions -= action;
    }
    // 双参优先级
    public static void AddEventListener<T1, T2>(E_EventType name, PriorityAction<T1, T2> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2>).GetNewEvent(priorityAction);
        else
            eventDic.Add(name, new PriorityEventInfo<T1, T2>(priorityAction));
    }
    public static void EventTrigger_Priority<T1, T2>(E_EventType name, T1 arg1, T2 arg2)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2>).allActions.ForEach(val => { val.Action?.Invoke(arg1, arg2); });
    }
    public static void RemoveEventListener<T1, T2>(E_EventType name, PriorityAction<T1, T2> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2>).allActions.Remove(priorityAction);
    }
    #endregion

    #region 新增：三参数事件（普通+优先级）
    public static void AddEventListener<T1, T2, T3>(E_EventType name, UnityAction<T1, T2, T3> action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T1, T2, T3>).actions -= action;
            (eventDic[name] as EventInfo<T1, T2, T3>).actions += action;
        }
        else
            eventDic.Add(name, new EventInfo<T1, T2, T3>(action));
    }
    public static void EventTrigger<T1, T2, T3>(E_EventType name, T1 arg1, T2 arg2, T3 arg3)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T1, T2, T3>)?.actions?.Invoke(arg1, arg2, arg3);
    }
    public static void RemoveEventListener<T1, T2, T3>(E_EventType name, UnityAction<T1, T2, T3> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T1, T2, T3>).actions -= action;
    }
    // 三参优先级
    public static void AddEventListener<T1, T2, T3>(E_EventType name, PriorityAction<T1, T2, T3> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2, T3>).GetNewEvent(priorityAction);
        else
            eventDic.Add(name, new PriorityEventInfo<T1, T2, T3>(priorityAction));
    }
    public static void EventTrigger_Priority<T1, T2, T3>(E_EventType name, T1 arg1, T2 arg2, T3 arg3)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2, T3>).allActions.ForEach(val => { val.Action?.Invoke(arg1, arg2, arg3); });
    }
    public static void RemoveEventListener<T1, T2, T3>(E_EventType name, PriorityAction<T1, T2, T3> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2, T3>).allActions.Remove(priorityAction);
    }
    #endregion

    #region 新增：四参数事件（普通+优先级）
    public static void AddEventListener<T1, T2, T3, T4>(E_EventType name, UnityAction<T1, T2, T3, T4> action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T1, T2, T3, T4>).actions -= action;
            (eventDic[name] as EventInfo<T1, T2, T3, T4>).actions += action;
        }
        else
            eventDic.Add(name, new EventInfo<T1, T2, T3, T4>(action));
    }
    public static void EventTrigger<T1, T2, T3, T4>(E_EventType name, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T1, T2, T3, T4>)?.actions?.Invoke(arg1, arg2, arg3, arg4);
    }
    public static void RemoveEventListener<T1, T2, T3, T4>(E_EventType name, UnityAction<T1, T2, T3, T4> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T1, T2, T3, T4>).actions -= action;
    }
    // 四参优先级
    public static void AddEventListener<T1, T2, T3, T4>(E_EventType name, PriorityAction<T1, T2, T3, T4> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2, T3, T4>).GetNewEvent(priorityAction);
        else
            eventDic.Add(name, new PriorityEventInfo<T1, T2, T3, T4>(priorityAction));
    }
    public static void EventTrigger_Priority<T1, T2, T3, T4>(E_EventType name, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2, T3, T4>).allActions.ForEach(val => { val.Action?.Invoke(arg1, arg2, arg3, arg4); });
    }
    public static void RemoveEventListener<T1, T2, T3, T4>(E_EventType name, PriorityAction<T1, T2, T3, T4> priorityAction)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as PriorityEventInfo<T1, T2, T3, T4>).allActions.Remove(priorityAction);
    }
    #endregion

    /// <summary>
    /// 清空所有事件（静态）
    /// </summary>
    public static void ClearAllEvents()
    {
        eventDic.Clear();
    }
}

// ====================== 事件包装类（新增多参数版本） ======================
public interface IEventInfo { }

#region 基础事件包装类
public class EventInfo : IEventInfo
{
    public UnityAction actions;
    public EventInfo(UnityAction action) => actions += action;
}

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;
    public EventInfo(UnityAction<T> action) => actions += action;
}

public class EventInfo<T1, T2> : IEventInfo
{
    public UnityAction<T1, T2> actions;
    public EventInfo(UnityAction<T1, T2> action) => actions += action;
}

public class EventInfo<T1, T2, T3> : IEventInfo
{
    public UnityAction<T1, T2, T3> actions;
    public EventInfo(UnityAction<T1, T2, T3> action) => actions += action;
}

public class EventInfo<T1, T2, T3, T4> : IEventInfo
{
    public UnityAction<T1, T2, T3, T4> actions;
    public EventInfo(UnityAction<T1, T2, T3, T4> action) => actions += action;
}
#endregion

#region 优先级委托类（新增多参数版本）
public class PriorityAction
{
    public int Priority;
    public UnityAction Action;
    public PriorityAction(int priority, UnityAction action)
    {
        Priority = priority;
        Action = action;
    }
}

public class PriorityAction<T>
{
    public int Priority;
    public UnityAction<T> Action;
    public PriorityAction(int priority, UnityAction<T> action)
    {
        Priority = priority;
        Action = action;
    }
}

public class PriorityAction<T1, T2>
{
    public int Priority;
    public UnityAction<T1, T2> Action;
    public PriorityAction(int priority, UnityAction<T1, T2> action)
    {
        Priority = priority;
        Action = action;
    }
}

public class PriorityAction<T1, T2, T3>
{
    public int Priority;
    public UnityAction<T1, T2, T3> Action;
    public PriorityAction(int priority, UnityAction<T1, T2, T3> action)
    {
        Priority = priority;
        Action = action;
    }
}

public class PriorityAction<T1, T2, T3, T4>
{
    public int Priority;
    public UnityAction<T1, T2, T3, T4> Action;
    public PriorityAction(int priority, UnityAction<T1, T2, T3, T4> action)
    {
        Priority = priority;
        Action = action;
    }
}
#endregion

#region 优先级事件包装类（新增多参数版本）
public class PriorityEventInfo : IEventInfo
{
    public List<PriorityAction> allActions = new List<PriorityAction>();
    public PriorityEventInfo(PriorityAction action) => allActions.Add(action);
    public void GetNewEvent(PriorityAction action)
    {
        allActions.Add(action);
        allActions.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }
}

public class PriorityEventInfo<T> : IEventInfo
{
    public List<PriorityAction<T>> allActions = new List<PriorityAction<T>>();
    public PriorityEventInfo(PriorityAction<T> action) => allActions.Add(action);
    public void GetNewEvent(PriorityAction<T> action)
    {
        allActions.Add(action);
        allActions.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }
}

public class PriorityEventInfo<T1, T2> : IEventInfo
{
    public List<PriorityAction<T1, T2>> allActions = new List<PriorityAction<T1, T2>>();
    public PriorityEventInfo(PriorityAction<T1, T2> action) => allActions.Add(action);
    public void GetNewEvent(PriorityAction<T1, T2> action)
    {
        allActions.Add(action);
        allActions.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }
}

public class PriorityEventInfo<T1, T2, T3> : IEventInfo
{
    public List<PriorityAction<T1, T2, T3>> allActions = new List<PriorityAction<T1, T2, T3>>();
    public PriorityEventInfo(PriorityAction<T1, T2, T3> action) => allActions.Add(action);
    public void GetNewEvent(PriorityAction<T1, T2, T3> action)
    {
        allActions.Add(action);
        allActions.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }
}

public class PriorityEventInfo<T1, T2, T3, T4> : IEventInfo
{
    public List<PriorityAction<T1, T2, T3, T4>> allActions = new List<PriorityAction<T1, T2, T3, T4>>();
    public PriorityEventInfo(PriorityAction<T1, T2, T3, T4> action) => allActions.Add(action);
    public void GetNewEvent(PriorityAction<T1, T2, T3, T4> action)
    {
        allActions.Add(action);
        allActions.Sort((x, y) => y.Priority.CompareTo(x.Priority));
    }
}
#endregion

// ====================== 你的原有事件枚举（无修改） ======================
public enum E_EventType
{
    Do_EditorLogic,
    Editor_Terrain,
    Editor_Terrain_OneTime,
    Editor_Terrain_ExitEdit,

    /// <summary>
    /// 禁用鼠标漫游
    /// </summary>
    FreezeCamPan,
    /// <summary>
    /// 恢复鼠标漫游
    /// </summary>
    UnFreezeCamPan,

    /// <summary>
    ///角色升级
    /// </summary>
    Character_Upgrade,

    /// <summary>
    ///登记MapSkiller
    /// </summary>
    Character_Skiller_Regist,

    /// <summary>
    /// 登记MapMover
    /// </summary>
    Character_Mover_Regist,

    /// <summary>
    /// 选中一个角色对象,更新全局checker的操作对象
    /// </summary>
    Select_Characer,

    Map_GetPropertyGift,
    Map_GetASkill,
    Battle_LoadASkill,
    BattleEnd,
    
    /// <summary>
    /// 战斗的角色死亡
    /// </summary>
    Battle_CharacterDead,



    /// <summary>
    /// 战斗的角色力竭
    /// </summary>
    Battle_CharacterBreak,

    /// <summary>
    /// 力竭角色恢复
    /// </summary>
    Battle_CharacterBreakRefresh,

    LoadObjPool,
    E_DataSave,
    E_KillABoss,
    Mover_PlayerStartMove,
    //进入特殊房间（玩家或AI）
    Mover_IntoSpecialRoom,
    Mover_OneTimeMove,
    Mover_CheckCurrrentRoom,
    Mover_MoveStop,
    Player_RoundEnd,
    LoadMapStart,
    LoadMapEnd,
    SkillExcute,


    /// <summary>
    /// 呼叫技能面板
    /// </summary>
    CallSkillPanel,

    /// <summary>
    /// 玩家将进入战斗，注册角色数据信息
    /// </summary>
    PlayerBeforeIntoBattle,

    /// <summary>
    /// 玩家脱战/完成战斗，清空BattleManager中的数据
    /// </summary>
    PlayerOutBattle,

    /// <summary>
    /// 完成了技能配置,更新对应角色的skiller
    /// </summary>
    SkillSettle,

    /// <summary>
    /// 切换相机冻结状态
    /// </summary>
    SwitchCamFreeze,
    /// <summary>
    /// 调整玩家活力点数
    /// </summary>
    AdjustVitalityPoints,

    UpdateUIVitalityPoints,

    UpdateRoundState,

    /// <summary>
    /// 新的行动回合
    /// </summary>
    NewRound,

    /// <summary>
    /// 一个Mover结束了自身的行动回合
    /// </summary>
    OneMoverEndRound,

    /// <summary>
    /// 混沌等级提升
    /// </summary>
    ChaosLevelUP,

    /// <summary>
    /// 调整经验值
    /// </summary>
    AdjustEXP,

    /// <summary>
    /// 角色获得一个BUFF（计时消失）
    /// </summary>
    Battle_RegisteBUFF,

    /// <summary>
    /// 角色获得一个Dot（手段移除）
    /// </summary>
    Battle_RegisterDot,
    /// <summary>
    /// 进行了一次物理攻击
    /// </summary>
    Do_PhyAttack,

    /// <summary>
    /// 主动造成一次伤害（包括物理和魔法）
    /// 检查是否触发造成伤害增幅BUFF
    /// </summary>
    Do_Damage,
}