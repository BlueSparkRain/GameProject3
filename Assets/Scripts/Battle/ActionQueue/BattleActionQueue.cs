using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class BattleActionQueue : MonoSceneManager
{
    Queue<BattleAction> _atbQueue = new Queue<BattleAction>();
    Queue<BattleAction> _normalQueue = new Queue<BattleAction>();
    bool _paused;
    bool _loopStarted;

    SkillVfxDirectorManager _vfxManager;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        _paused = false;
        if (!_loopStarted)
        {
            _loopStarted = true;
            StartCoroutine(ProcessAtbLoop());
            StartCoroutine(ProcessNormalLoop());
        }
    }

    public override void MgrUpdate(float deltaTime) { }

    public void Enqueue(BattleAction action)
    {
        _atbQueue.Enqueue(action);
    }

    public void EnqueueNormal(BattleAction action)
    {
        _normalQueue.Enqueue(action);
    }

    public void Pause() => _paused = true;
    public void Resume() => _paused = false;

    public void Clear()
    {
        _atbQueue.Clear();
        _normalQueue.Clear();
    }

    public int AtbCount => _atbQueue.Count;
    public int NormalCount => _normalQueue.Count;

    IEnumerator ProcessAtbLoop()
    {
        while (true)
        {
            while (_atbQueue.Count > 0 && !_paused)
            {
                var action = _atbQueue.Dequeue();
                yield return StartCoroutine(ProcessAction(action));
            }
            yield return null;
        }
    }

    IEnumerator ProcessNormalLoop()
    {
        while (true)
        {
            while (_normalQueue.Count > 0 && !_paused)
            {
                var action = _normalQueue.Dequeue();
                yield return StartCoroutine(ProcessAction(action));
            }
            yield return null;
        }
    }

    IEnumerator ProcessAction(BattleAction action)
    {
        yield return null;

        string tag = action.IsATB ? "(ATB)" : "";
        BattleDebugManager.LogFormat("{0} 释放 {1}{2}", action.CasterName, action.SkillName, tag);

        if (_vfxManager == null)
            _vfxManager = GameRoot.GetManager<SkillVfxDirectorManager>();

        if (_vfxManager != null)
        {
            yield return StartCoroutine(_vfxManager.PlayDelivery(action));
        }
        else
        {
            yield return new WaitForSeconds(0.15f);
        }

        try
        {
            action.Settle();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[ActionQueue] 结算异常 —— {action.CasterName}, skill={action.SkillName}: {e}");
        }

        yield return new WaitForSeconds(0.12f);
    }
}
