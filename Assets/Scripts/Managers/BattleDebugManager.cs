using System;
using System.Collections.Generic;
using Core;

/// <summary>
/// 战斗日志管理器 — 统一收集本场战斗的逐行事件（技能释放、伤害、Buff/Dot、死亡等），
/// 供 BattleLogDisplay 订阅展示，同时转发到 DebugManager 控制台。
/// </summary>
public class BattleDebugManager : MonoSceneManager
{
    public event Action<string> OnNewEntry;

    List<string> _entries = new List<string>();
    public IReadOnlyList<string> Entries => _entries;

    public static void Log(string message)
    {
        var instance = GameRoot.GetManager<BattleDebugManager>();
        if (instance != null)
            instance.AddEntry(message);
        else
            DebugManager.Log(EDebugCategory.BattleState, $"[BattleLog] {message}");
    }

    public static void LogFormat(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    void AddEntry(string message)
    {
        _entries.Add(message);
        OnNewEntry?.Invoke(message);
        DebugManager.Log(EDebugCategory.BattleState, $"[BattleLog] {message}");
    }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        _entries.Clear();
    }

    public override void MgrUpdate(float deltaTime) { }
}
