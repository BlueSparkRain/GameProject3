using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 协程状态（用于管理协程生命周期）
/// </summary>
public enum CoroutineState
{
    Idle,       // 闲置（在池中）
    Running,    // 运行中
    Paused,     // 暂停
    Completed,  // 已完成
    Cancelled   // 已取消
}

/// <summary>
/// 协程句柄（引用类型，避免值拷贝GC）
/// </summary>
public class CoroutineHandle
{
    public string CoroutineId;
    public Coroutine Coroutine;
    public UnityEngine.Object Target;
    public IEnumerator CoroutineEnumerator;
    public CoroutineState State;
    public bool IsCancelled;
    public HashSet<string> ChildCoroutineIds;

    /// <summary>
    /// 复用重置
    /// </summary>
    public void Reset()
    {
        CoroutineId = string.Empty;
        Coroutine = null;
        Target = null;
        CoroutineEnumerator = null;
        State = CoroutineState.Idle;
        IsCancelled = false;
        ChildCoroutineIds?.Clear();
    }
}

public class CoroutineManager : MonoGlobalManager
{
    #region 高性能缓存
    private readonly List<string> _tempCleanIds = new List<string>(32);
    private readonly List<string> _tempStopIds = new List<string>(32);
    private const string COROUTINE_ID_PREFIX = "Cor_";
    private const int CLEANUP_INTERVAL = 10;
    private int _frameCounter;
    #endregion

    #region 核心字段
    private readonly Dictionary<string, CoroutineHandle> _activeCoroutines = new Dictionary<string, CoroutineHandle>(64);
    private readonly Stack<CoroutineHandle> _coroutinePool = new Stack<CoroutineHandle>(32);
    private int _coroutineIdCounter;
    #endregion

    #region 生命周期
    protected override void Awake()
    {
        base.Awake();
        // 基类已执行DontDestroyOnLoad，此处移除重复代码
        InitCoroutinePool(16);
    }

    public override void MgrUpdate(float deltaTime)
    {
        if (++_frameCounter >= CLEANUP_INTERVAL)
        {
            _frameCounter = 0;
            CleanupInvalidCoroutines();
        }
    }

    /// <summary>
    /// 重写销毁：仅清理协程数据，不销毁自身对象
    /// </summary>
    public override void MgrDispose()
    {
        StopAllGlobalCoroutines();
        _activeCoroutines.Clear();
        _coroutinePool.Clear();
        Debug.Log("【协程管理器】数据清理完成，对象永久保留");
    }
    #endregion

    #region 池化核心
    private void InitCoroutinePool(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            _coroutinePool.Push(new CoroutineHandle
            {
                ChildCoroutineIds = new HashSet<string>(8)
            });
        }
    }

    private CoroutineHandle GetCoroutineHandleFromPool()
    {
        if (_coroutinePool.Count > 0)
        {
            var handle = _coroutinePool.Pop();
            handle.Reset();
            return handle;
        }
        return new CoroutineHandle
        {
            ChildCoroutineIds = new HashSet<string>(8)
        };
    }

    private void ReturnCoroutineHandleToPool(CoroutineHandle handle)
    {
        handle.Reset();
        _coroutinePool.Push(handle);
    }
    #endregion

    #region 对外接口
    /// <summary>
    /// 【终极修复】增加安全校验，杜绝调用已销毁对象
    /// </summary>
    public string StartCoroutine(IEnumerator enumerator, UnityEngine.Object target = null)
    {
        // 核心安全校验：自身已销毁，直接返回，不报错
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("【协程管理器】对象已销毁，跳过协程启动");
            return string.Empty;
        }

        if (enumerator == null)
        {
            Debug.LogError("[CoroutineManager]---协程迭代器不能为空！");
            return string.Empty;
        }

        string coroutineId = GenerateCoroutineId();
        var handle = GetCoroutineHandleFromPool();

        handle.CoroutineId = coroutineId;
        handle.Target = target;
        handle.CoroutineEnumerator = WrapCoroutineWithState(enumerator, coroutineId);
        handle.State = CoroutineState.Running;

        handle.Coroutine = base.StartCoroutine(handle.CoroutineEnumerator);
        _activeCoroutines[coroutineId] = handle;

        return coroutineId;
    }

    public string StartDelayedCoroutine(float delayTime, Action action, UnityEngine.Object target = null)
    {
        return StartCoroutine(DelayedCoroutineLogic(delayTime, action), target);
    }

    public string StartDelayedCoroutine(float delayTime, IEnumerator enumerator, UnityEngine.Object target = null)
    {
        if (enumerator == null)
        {
            Debug.LogError("[CoroutineManager]---延迟协程的迭代器不能为空！");
            return string.Empty;
        }
        return StartCoroutine(DelayedCoroutineLogic(delayTime, enumerator), target);
    }

    public string StartRepeatingCoroutine(float interval, int repeatCount, Func<IEnumerator> enumeratorFunc, UnityEngine.Object target = null)
    {
        if (enumeratorFunc == null)
        {
            Debug.LogError("[CoroutineManager]---重复协程的方法引用不能为空！");
            return string.Empty;
        }

        string outerCorId = GenerateCoroutineId();
        var outerHandle = GetCoroutineHandleFromPool();
        outerHandle.CoroutineId = outerCorId;
        outerHandle.Target = target;
        outerHandle.CoroutineEnumerator = RepeatingCoroutineLogic(interval, repeatCount, enumeratorFunc, outerCorId, target);
        outerHandle.State = CoroutineState.Running;
        outerHandle.Coroutine = base.StartCoroutine(outerHandle.CoroutineEnumerator);
        _activeCoroutines[outerCorId] = outerHandle;

        return outerCorId;
    }

    public void StopGlobalCoroutine(string coroutineId)
    {
        if (string.IsNullOrEmpty(coroutineId) || !_activeCoroutines.TryGetValue(coroutineId, out var handle))
            return;

        handle.IsCancelled = true;
        handle.State = CoroutineState.Cancelled;
        if (handle.Coroutine != null)
            StopCoroutine(handle.Coroutine);

        if (handle.ChildCoroutineIds.Count > 0)
        {
            foreach (var childId in handle.ChildCoroutineIds)
                StopGlobalCoroutine(childId);
        }

        ReturnCoroutineHandleToPool(handle);
        _activeCoroutines.Remove(coroutineId);
    }

    public void StopCoroutinesByTarget(UnityEngine.Object target)
    {
        if (target == null) return;

        _tempStopIds.Clear();
        foreach (var kvp in _activeCoroutines)
        {
            if (kvp.Value.Target == target)
                _tempStopIds.Add(kvp.Key);
        }

        foreach (var id in _tempStopIds)
            StopGlobalCoroutine(id);
    }

    public IEnumerator CleanupCoroutinesByScene(Scene targetScene)
    {
        _tempStopIds.Clear();
        foreach (var kvp in _activeCoroutines)
        {
            var handle = kvp.Value;
            if (handle.Target is GameObject go && go.scene == targetScene)
                _tempStopIds.Add(kvp.Key);
            else if (handle.Target is Component comp && comp.gameObject.scene == targetScene)
                _tempStopIds.Add(kvp.Key);
        }

        foreach (var id in _tempStopIds)
            StopGlobalCoroutine(id);
        yield return null;
    }

    public void PauseGlobalCoroutine(string coroutineId)
    {
        if (string.IsNullOrEmpty(coroutineId) || !_activeCoroutines.TryGetValue(coroutineId, out var handle))
            return;

        if (handle.State == CoroutineState.Running)
            handle.State = CoroutineState.Paused;
    }

    public void ResumeGlobalCoroutine(string coroutineId)
    {
        if (string.IsNullOrEmpty(coroutineId) || !_activeCoroutines.TryGetValue(coroutineId, out var handle))
            return;

        if (handle.State == CoroutineState.Paused)
            handle.State = CoroutineState.Running;
    }

    private void StopAllGlobalCoroutines()
    {
        _tempStopIds.Clear();
        _tempStopIds.AddRange(_activeCoroutines.Keys);

        foreach (var id in _tempStopIds)
            StopGlobalCoroutine(id);
    }
    #endregion

    #region 内部辅助
    private string GenerateCoroutineId()
    {
        return $"{COROUTINE_ID_PREFIX}{++_coroutineIdCounter}";
    }

    IEnumerator WrapCoroutineWithState(IEnumerator enumerator, string coroutineId)
    {
        while (true)
        {
            if (_activeCoroutines.TryGetValue(coroutineId, out var handle))
            {
                if (handle.IsCancelled || handle.State == CoroutineState.Cancelled)
                    yield break;

                if (handle.State == CoroutineState.Paused)
                {
                    yield return null;
                    continue;
                }
            }

            if (!enumerator.MoveNext())
            {
                if (_activeCoroutines.TryGetValue(coroutineId, out var handle1))
                {
                    handle1.State = CoroutineState.Completed;
                    ReturnCoroutineHandleToPool(handle1);
                    _activeCoroutines.Remove(coroutineId);
                }
                yield break;
            }

            yield return enumerator.Current;
        }
    }

    IEnumerator DelayedCoroutineLogic(float delayTime, Action action)
    {
        yield return new WaitForSeconds(delayTime);
        action?.Invoke();
    }

    IEnumerator DelayedCoroutineLogic(float delayTime, IEnumerator enumerator)
    {
        yield return new WaitForSeconds(delayTime);
        yield return enumerator;
    }

    IEnumerator RepeatingCoroutineLogic(float interval, int repeatCount, Func<IEnumerator> enumeratorFunc, string outerCorId, UnityEngine.Object target)
    {
        int count = 0;
        yield return null;

        while (true)
        {
            if (!_activeCoroutines.ContainsKey(outerCorId) || _activeCoroutines[outerCorId].State == CoroutineState.Cancelled)
                yield break;

            // 修复无效判断逻辑
            if (target != null && target.Equals(null))
                yield break;

            yield return new WaitForSeconds(interval);

            IEnumerator enumerator = enumeratorFunc.Invoke();
            if (enumerator == null)
            {
                count++;
                continue;
            }

            string childCorId = StartCoroutine(enumerator, target);
            _activeCoroutines[outerCorId].ChildCoroutineIds.Add(childCorId);
            yield return enumerator;
            count++;

            if (repeatCount > 0 && count >= repeatCount)
                break;
        }
    }

    /// <summary>
    /// 修复无效空值判断，使用Unity标准判空
    /// </summary>
    void CleanupInvalidCoroutines()
    {
        _tempCleanIds.Clear();
        var activeScene = SceneManager.GetActiveScene();

        foreach (var kvp in _activeCoroutines)
        {
            var handle = kvp.Value;
            bool needClean = false;

            // 修复：判断Unity对象是否被销毁
            if (handle.Target != null && handle.Target.Equals(null))
                needClean = true;
            else if (handle.Target is Component comp && comp.gameObject.scene != activeScene)
                needClean = true;
            else if (handle.State is CoroutineState.Completed or CoroutineState.Cancelled)
                needClean = true;

            if (needClean)
                _tempCleanIds.Add(kvp.Key);
        }

        foreach (var id in _tempCleanIds)
        {
            if (_activeCoroutines.TryGetValue(id, out var handle))
            {
                ReturnCoroutineHandleToPool(handle);
                _activeCoroutines.Remove(id);
            }
        }
    }
    #endregion
}