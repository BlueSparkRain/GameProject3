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
        SceneManager.sceneUnloaded += OnSceneUnloaded;
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
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        StopAllGlobalCoroutines();
        _activeCoroutines.Clear();
        _coroutinePool.Clear();
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

        _activeCoroutines[coroutineId] = handle;
        handle.Coroutine = base.StartCoroutine(handle.CoroutineEnumerator);

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
        _activeCoroutines[outerCorId] = outerHandle;
        outerHandle.Coroutine = base.StartCoroutine(outerHandle.CoroutineEnumerator);

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

    /// <summary>
    /// 检测Unity对象是否存活（未被销毁）
    /// ReferenceEquals过滤"未设置目标"的情况，Unity重载的!=过滤"已销毁"的情况
    /// </summary>
    private static bool IsTargetAlive(UnityEngine.Object target)
    {
        if (ReferenceEquals(target, null)) return true;
        return target != null;
    }

    private string GenerateCoroutineId()
    {
        return $"{COROUTINE_ID_PREFIX}{++_coroutineIdCounter}";
    }

    IEnumerator WrapCoroutineWithState(IEnumerator enumerator, string coroutineId)
    {
        while (true)
        {
            if (!_activeCoroutines.TryGetValue(coroutineId, out var handle))
            {
                yield break;
            }

            if (handle.IsCancelled || handle.State == CoroutineState.Cancelled)
                yield break;

            if (handle.State == CoroutineState.Paused)
            {
                yield return null;
                continue;
            }

            // 每次迭代前验活：目标已销毁则安全终止
            if (!IsTargetAlive(handle.Target))
            {
                Debug.LogWarning($"[CoroutineManager] 协程 {coroutineId} 的目标对象已销毁，安全终止");
                handle.State = CoroutineState.Completed;
                ReturnCoroutineHandleToPool(handle);
                _activeCoroutines.Remove(coroutineId);
                yield break;
            }

            if (!enumerator.MoveNext())
            {
                handle.State = CoroutineState.Completed;
                ReturnCoroutineHandleToPool(handle);
                _activeCoroutines.Remove(coroutineId);
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

            if (!IsTargetAlive(target))
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
    /// 场景卸载时主动回收该场景的所有协程
    /// </summary>
    void OnSceneUnloaded(Scene scene)
    {
        _tempCleanIds.Clear();
        foreach (var kvp in _activeCoroutines)
        {
            var target = kvp.Value.Target;
            // 从未设置target → 跳过（ReferenceEquals区分"真null"和"Unity已销毁对象"）
            if (ReferenceEquals(target, null)) continue;
            // target已被销毁（属于已卸载场景）或target明确来自该场景 → 清理
            if (target == null
                || (target is GameObject go && go.scene == scene)
                || (target is Component comp && comp.gameObject.scene == scene))
            {
                _tempCleanIds.Add(kvp.Key);
            }
        }

        foreach (var id in _tempCleanIds)
        {
            if (_activeCoroutines.TryGetValue(id, out var handle))
            {
                handle.State = CoroutineState.Cancelled;
                if (handle.Coroutine != null)
                    StopCoroutine(handle.Coroutine);
                ReturnCoroutineHandleToPool(handle);
                _activeCoroutines.Remove(id);
            }
        }
    }

    /// <summary>
    /// 定时清理已失效的协程（目标销毁 / 已完成 / 已取消）
    /// </summary>
    void CleanupInvalidCoroutines()
    {
        _tempCleanIds.Clear();

        foreach (var kvp in _activeCoroutines)
        {
            var handle = kvp.Value;
            if (!IsTargetAlive(handle.Target)
                || handle.State is CoroutineState.Completed or CoroutineState.Cancelled)
            {
                _tempCleanIds.Add(kvp.Key);
            }
        }

        foreach (var id in _tempCleanIds)
        {
            if (_activeCoroutines.TryGetValue(id, out var handle))
            {
                if (handle.Coroutine != null)
                    StopCoroutine(handle.Coroutine);
                ReturnCoroutineHandleToPool(handle);
                _activeCoroutines.Remove(id);
            }
        }
    }
    #endregion
}