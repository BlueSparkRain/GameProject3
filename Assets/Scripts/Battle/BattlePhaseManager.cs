using System;
using System.Collections;
using Core;
using UnityEngine;

public enum BattlePhase
{
    PreBattle,   // 战斗准备：PlayerBattleBoard入场动画 + 加载角色
    Countdown,   // 倒计时阶段：3秒倒计时显示
    InProgress,  // 战斗进行：所有技能正常运转
    Ended        // 战斗结束：停止所有技能，展示结果
}

public class BattlePhaseManager : MonoSceneManager
{
    BattlePhase _phase = BattlePhase.PreBattle;
    public BattlePhase CurrentPhase => _phase;

    public event Action<BattlePhase> OnPhaseChanged;

    [Header("倒计时配置")]
    [SerializeField] int _countdownSeconds = 3;

    int _countdownRemaining;
    public int CountdownRemaining => _countdownRemaining;
    public event Action<int> OnCountdownTick;
    public event Action OnCountdownEnd;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        _phase = BattlePhase.PreBattle;
    }

    public override void MgrUpdate(float deltaTime) { }

    /// <summary>由 GameBattleManager 在所有角色加载完毕后调用</summary>
    public void OnAllCharactersLoaded()
    {
        if (_phase == BattlePhase.PreBattle)
            SetPhase(BattlePhase.Countdown);
    }

    /// <summary>由 BattleStateManager 在判定胜负后调用</summary>
    public void TriggerBattleEnd(bool playerWin)
    {
        if (_phase == BattlePhase.Ended) return;
        SetPhase(BattlePhase.Ended);
    }

    void SetPhase(BattlePhase newPhase)
    {
        if (_phase == newPhase) return;

        var oldPhase = _phase;
        _phase = newPhase;
        DebugManager.Log(EDebugCategory.BattleState,
            $"[BattlePhaseManager] 阶段切换: {oldPhase} → {newPhase}");

        switch (newPhase)
        {
            case BattlePhase.Countdown:
                BattleDebugManager.Log("——— 战斗准备就绪 ———");
                StartCountdown();
                break;
            case BattlePhase.InProgress:
                BattleDebugManager.Log("——— 战斗开始！———");
                break;
            case BattlePhase.Ended:
                BattleDebugManager.Log("——— 战斗结束 ———");
                StopAllCoroutines();
                GameRoot.GetManager<BattleActionQueue>()?.Clear();
                GameRoot.GetManager<BattleActionQueue>()?.Pause();
                break;
        }

        OnPhaseChanged?.Invoke(newPhase);
    }

    void StartCountdown()
    {
        _countdownRemaining = _countdownSeconds;
        OnCountdownTick?.Invoke(_countdownRemaining);
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        while (_countdownRemaining > 0)
        {
            BattleDebugManager.LogFormat("    {0}...", _countdownRemaining);
            yield return new WaitForSeconds(1f);
            _countdownRemaining--;
            OnCountdownTick?.Invoke(_countdownRemaining);
        }
        OnCountdownEnd?.Invoke();
        SetPhase(BattlePhase.InProgress);
    }
}
