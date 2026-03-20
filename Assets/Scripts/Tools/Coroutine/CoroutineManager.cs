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
/// 协程句柄（池化复用核心，存储协程关键信息）
/// </summary>
public struct CoroutineHandle
{
    /// <summary>
    /// 全局唯一协程ID
    /// </summary>
    public string CoroutineId;
    /// <summary>
    /// Unity原生协程对象
    /// </summary>
    public Coroutine Coroutine;
    /// <summary>
    /// 协程所属目标对象（为空则全局协程）
    /// </summary>
    public UnityEngine.Object Target;
    /// <summary>
    /// 协程执行的迭代器
    /// </summary>
    public IEnumerator CoroutineEnumerator;
    /// <summary>
    /// 协程当前状态
    /// </summary>
    public CoroutineState State;
    /// <summary>
    /// 取消令牌（用于主动取消协程）
    /// </summary>
    public bool IsCancelled;
    /// <summary>
    /// 新增：子协程ID列表（管理重复协程的子协程）
    /// </summary>
    public List<string> ChildCoroutineIds;
}
////使用示例：
//// 1. 启动普通协程
//string corId = CoroutineManager.Instance.StartCoroutine(MyCoroutineLogic(), this);

//// 2. 启动延迟协程（无需写IEnumerator，直接传方法）
//string delayId = CoroutineManager.Instance.StartDelayedCoroutine(2f, () =>
//{
//    Debug.Log("2秒后执行！");
//}, this);

//// 3. 启动重复协程（间隔1秒，重复5次）
//string repeatId = CoroutineManager.Instance.StartRepeatingCoroutine(1f, 5, () =>
//{
//    Debug.Log("重复执行！");
//}, this);

//// 4. 停止协程（按ID）
//CoroutineManager.Instance.StopGlobalCoroutine(corId);

//// 5. 停止当前对象的所有协程（比如OnDestroy时）
//private void OnDestroy()
//{
//    CoroutineManager.Instance.StopCoroutinesByTarget(this);
//}

//// 自定义协程逻辑（和原生写法一致）
//private IEnumerator MyCoroutineLogic()
//{
//    Debug.Log("协程开始");
//    yield return new WaitForSeconds(1f);
//    Debug.Log("协程执行中");
//    yield return new WaitForEndOfFrame();
//    Debug.Log("协程结束");
//}

/// <summary>
/// 全局协程管理器（带协程池，安全、高效、极简）
/// </summary>
public class CoroutineManager : MonoGlobalManager
{
    #region 核心字段
    /// <summary>
    /// 活跃协程字典（Key：协程ID，Value：协程句柄）
    /// </summary>
    readonly Dictionary<string, CoroutineHandle> _activeCoroutines = new();

    /// <summary>
    /// 协程池（复用CoroutineHandle，减少GC）
    /// </summary>
    readonly Stack<CoroutineHandle> _coroutinePool = new();

    /// <summary>
    /// 锁对象（保证线程安全）
    /// </summary>
    readonly object _lockObj = new();

    /// <summary>
    /// 协程ID计数器（简化ID生成，比Guid更轻量）
    /// </summary>
    int _coroutineIdCounter = 0;
    #endregion

    #region 生命周期（保证单例+自动清理）
    protected override void Awake(){
        base.Awake();
        // 设为不销毁，保证全局唯一
        DontDestroyOnLoad(gameObject);
        // 初始化协程池（预设10个句柄，减少首次调用GC）
        InitCoroutinePool(10);
    }

    public override void MgrUpdate(float deltaTime){
        CleanupInvalidCoroutines();
    }

    public override void MgrDispose(){
        base.MgrDispose();
        // 销毁时停止所有协程
        StopAllCoroutines();
        StopAllGlobalCoroutines();
        Debug.Log("[CoroutineManager]---已停止所有协程");
        // 清空池和字典
        lock (_lockObj)
        {
            _activeCoroutines.Clear();
            _coroutinePool.Clear();
        }
    }
    #endregion

    #region 池化核心方法
    /// <summary>
    /// 初始化协程池
    /// </summary>
    /// <param name="initCount">初始池大小</param>
    void InitCoroutinePool(int initCount){
        lock (_lockObj){
            for (int i = 0; i < initCount; i++){
                _coroutinePool.Push(new CoroutineHandle{
                    CoroutineId = string.Empty,
                    Coroutine = null,
                    Target = null,
                    CoroutineEnumerator = null,
                    State = CoroutineState.Idle,
                    IsCancelled = false,
                    ChildCoroutineIds = new List<string>() // 初始化子列表
                });
            }
        }
    }

    /// <summary>
    /// 从池中获取协程句柄（复用，零GC）
    /// </summary>
    /// <returns>复用的句柄</returns>
    CoroutineHandle GetCoroutineHandleFromPool(){
        lock (_lockObj){
            if (_coroutinePool.Count > 0){
                var handle = _coroutinePool.Pop();
                // 重置句柄状态
                handle.IsCancelled = false;
                handle.State = CoroutineState.Idle;
                return handle;
            }
            // 池空时创建新句柄（兜底）
            return new CoroutineHandle();
        }
    }

    /// <summary>
    /// 将句柄归还到池中（复用）
    /// </summary>
    /// <param name="handle">要归还的句柄</param>
    void ReturnCoroutineHandleToPool(CoroutineHandle handle){
        lock (_lockObj){
            // 清空关键信息，重置状态（新增：清空子协程列表）
            handle.CoroutineId = string.Empty;
            handle.Coroutine = null;
            handle.Target = null;
            handle.CoroutineEnumerator = null;
            handle.State = CoroutineState.Idle;
            handle.IsCancelled = false;
            handle.ChildCoroutineIds?.Clear(); // 重置子协程列表
            _coroutinePool.Push(handle);
        }
    }
    #endregion

    #region 对外核心接口（极简、无脑调用）
    /// <summary>
    /// 启动全局协程（核心接口，最简调用）
    /// </summary>
    /// <param name="enumerator">协程迭代器</param>
    /// <param name="target">关联的目标对象（销毁时自动停止协程）</param>
    /// <returns>协程ID（用于后续控制）</returns>
    public  string StartCoroutine(IEnumerator enumerator, UnityEngine.Object target = null){
        if (enumerator == null){
            Debug.LogError("[CoroutineManager]---协程迭代器不能为空！");
            return string.Empty;
        }

        lock (_lockObj){
            // 1. 生成唯一ID
            string coroutineId = GenerateCoroutineId();

            // 2. 从池获取句柄
            var handle = GetCoroutineHandleFromPool();
            handle.CoroutineId = coroutineId;
            handle.Target = target;
            handle.CoroutineEnumerator = WrapCoroutineWithState(enumerator, coroutineId);
            handle.State = CoroutineState.Running;

            // 3. 启动协程并记录
            handle.Coroutine = base.StartCoroutine(handle.CoroutineEnumerator);
            _activeCoroutines[coroutineId] = handle;

            return coroutineId;
        }
    }

    /// <summary>
    /// 重载1：启动延迟执行的协程（常用快捷接口）
    /// </summary>
    /// <param name="delayTime">延迟时间（秒）</param>
    /// <param name="action">延迟后执行的方法</param>
    /// <param name="target">关联的目标对象</param>
    /// <returns>协程ID</returns>
    public string StartDelayedCoroutine(float delayTime, Action action, UnityEngine.Object target = null){
        return StartCoroutine(DelayedCoroutineLogic(delayTime, action), target);
    }

    /// <summary>
    /// 启动延迟执行的协程（重载2：传入自定义协程，灵活扩展）
    /// </summary>
    /// <param name="delayTime">延迟时间（秒）</param>
    /// <param name="enumerator">延迟后执行的协程迭代器</param>
    /// <param name="target">关联的目标对象</param>
    /// <returns>协程ID</returns>
    public string StartDelayedCoroutine(float delayTime, IEnumerator enumerator, UnityEngine.Object target = null){
        if (enumerator == null){
            Debug.LogError("[CoroutineManager]---延迟协程的迭代器不能为空！");
            return string.Empty;
        }
        // 先延迟，再执行自定义协程
        return StartCoroutine(DelayedCoroutineLogic(delayTime, enumerator), target);
    }

    /// <summary>
    /// 启动重复执行的协程（重载2：接收方法引用，子协程也注册到字典）
    /// </summary>
    /// <param name="interval">执行间隔（秒）</param>
    /// <param name="repeatCount">重复次数（-1=无限）</param>
    /// <param name="enumeratorFunc">返回迭代器的方法引用</param>
    /// <param name="target">关联的目标对象</param>
    /// <returns>外层协程ID（用于后续控制）</returns>
    public string StartRepeatingCoroutine(float interval, int repeatCount, Func<IEnumerator> enumeratorFunc, UnityEngine.Object target = null){
        if (enumeratorFunc == null){
            Debug.LogError("[CoroutineManager]---重复协程的方法引用不能为空！");
            return string.Empty;
        }

        // 1. 生成外层协程ID，并注册外层协程
        lock (_lockObj){
            string outerCorId = GenerateCoroutineId();
            var outerHandle = GetCoroutineHandleFromPool();
            outerHandle.CoroutineId = outerCorId;
            outerHandle.Target = target;
            // 初始化子协程ID列表
            outerHandle.ChildCoroutineIds = new List<string>();
            // 包装重复逻辑，传入外层ID和子协程生成方法
            outerHandle.CoroutineEnumerator = RepeatingCoroutineLogic(interval, repeatCount, enumeratorFunc, outerCorId, target);
            outerHandle.State = CoroutineState.Running;
            outerHandle.Coroutine = base.StartCoroutine(outerHandle.CoroutineEnumerator);
            _activeCoroutines[outerCorId] = outerHandle;
            return outerCorId;
        }
    }

    /// <summary>
    /// 停止指定ID的协程(包括子协程)
    /// </summary>
    /// <param name="coroutineId">协程ID</param>
    public void StopGlobalCoroutine(string coroutineId){
        if (string.IsNullOrEmpty(coroutineId))
            return;
        lock (_lockObj){
            if (_activeCoroutines.TryGetValue(coroutineId, out var handle)){
                // 第一步：停止当前协程
                handle.IsCancelled = true;
                handle.State = CoroutineState.Cancelled;
                if (handle.Coroutine != null)
                    StopCoroutine(handle.Coroutine);
                
                // 第二步：递归停止所有子协程（核心修复）
                if (handle.ChildCoroutineIds != null && handle.ChildCoroutineIds.Count > 0)
                    foreach (var childId in handle.ChildCoroutineIds)
                        StopGlobalCoroutine(childId); // 递归停止子协程    

                // 第三步：归还句柄到池（重置子协程列表）
                handle.ChildCoroutineIds?.Clear();
                ReturnCoroutineHandleToPool(handle);
                // 从活跃字典移除
                _activeCoroutines.Remove(coroutineId);
            }
        }
    }

    /// <summary>
    /// 停止指定目标对象的所有协程
    /// </summary>
    /// <param name="target">目标对象</param>
    public void StopCoroutinesByTarget(UnityEngine.Object target){
        if (target == null)
            return;

        lock (_lockObj){
            var toStopIds = new List<string>();
            // 收集目标关联的所有协程ID
            foreach (var kvp in _activeCoroutines)
                if (kvp.Value.Target == target)
                    toStopIds.Add(kvp.Key);
            // 批量停止
            foreach (var id in toStopIds)
                StopGlobalCoroutine(id);
        }
    }

    // 新增：清理指定场景的所有协程
    public IEnumerator CleanupCoroutinesByScene(Scene targetScene){
        lock (_lockObj){
            var toStopIds = new List<string>();
            foreach (var kvp in _activeCoroutines){
                var handle = kvp.Value;
                // 仅处理场景内的对象（排除全局对象）
                if (handle.Target is GameObject go && go.scene == targetScene)
                    toStopIds.Add(kvp.Key);
                else if (handle.Target is Component comp && comp.gameObject.scene == targetScene)
                    toStopIds.Add(kvp.Key);
            }
            // 批量停止并清理
            foreach (var id in toStopIds)
                StopGlobalCoroutine(id);
            yield return null;
            Debug.Log($"[CoroutineManager]---已停止{targetScene.name}（上一个场景）内所有协程");
        }
    }

    /// <summary>
    /// 暂停指定ID的协程
    /// </summary>
    /// <param name="coroutineId">协程ID</param>
    public void PauseGlobalCoroutine(string coroutineId){
        if (string.IsNullOrEmpty(coroutineId))
            return;
        lock (_lockObj){
            if (_activeCoroutines.TryGetValue(coroutineId, out var handle) && handle.State == CoroutineState.Running){
                handle.State = CoroutineState.Paused;
                // 暂停原生协程（Unity原生协程无法直接暂停，需通过状态控制）
                _activeCoroutines[coroutineId] = handle;
            }
        }
    }
    /// <summary>
    /// 恢复暂停的协程
    /// </summary>
    /// <param name="coroutineId">协程ID</param>
    public void ResumeGlobalCoroutine(string coroutineId){
        if (string.IsNullOrEmpty(coroutineId))
            return;
        lock (_lockObj){
            if (_activeCoroutines.TryGetValue(coroutineId, out var handle) && handle.State == CoroutineState.Paused){
                handle.State = CoroutineState.Running;
                // 重新启动协程（需配合包装逻辑）
                handle.Coroutine = base.StartCoroutine(handle.CoroutineEnumerator);
                _activeCoroutines[coroutineId] = handle;
            }
        }
    }

    /// <summary>
    /// 停止所有全局协程
    /// </summary>
    void StopAllGlobalCoroutines(){
        lock (_lockObj){
            var allIds = new List<string>(_activeCoroutines.Keys);
            foreach (var id in allIds)
                StopGlobalCoroutine(id);
        }
    }
    #endregion

    #region 内部辅助逻辑
    /// <summary>
    /// 生成轻量级协程ID（比Guid高效）
    /// </summary>
    /// <returns>唯一ID</returns>
    string GenerateCoroutineId(){
        lock (_lockObj){
            _coroutineIdCounter++;
            return $"Cor_{_coroutineIdCounter}_{Time.frameCount}";
        }
    }

    /// <summary>
    /// 包装协程迭代器，增加状态控制（暂停/取消）
    /// </summary>
    /// <param name="enumerator">原始迭代器</param>
    /// <param name="coroutineId">协程ID</param>
    /// <returns>包装后的迭代器</returns>
    IEnumerator WrapCoroutineWithState(IEnumerator enumerator, string coroutineId){
        while (true){
            // 检查是否取消
            lock (_lockObj){
                if (_activeCoroutines.TryGetValue(coroutineId, out var handle) && (handle.IsCancelled || handle.State == CoroutineState.Cancelled))
                    yield break;
                
                // 检查是否暂停
                if (handle.State == CoroutineState.Paused){
                    yield return null;
                    continue;
                }
            }

            // 执行原始协程的一步
            if (!enumerator.MoveNext()){
                // 协程完成
                lock (_lockObj){
                    if (_activeCoroutines.TryGetValue(coroutineId, out var handle)){
                        handle.State = CoroutineState.Completed;
                        ReturnCoroutineHandleToPool(handle);
                        _activeCoroutines.Remove(coroutineId);
                    }
                }
                yield break;
            }
            // 返回当前迭代结果
            yield return enumerator.Current;
        }
    }

    /// <summary>
    /// 延迟执行的协程逻辑
    /// </summary>
    /// <param name="delayTime">延迟时间</param>
    /// <param name="action">执行方法</param>
    /// <returns>迭代器</returns>
    IEnumerator DelayedCoroutineLogic(float delayTime, Action action){
        yield return new WaitForSeconds(delayTime);
        action?.Invoke();
    }

    /// <summary>
    /// 延迟执行的协程逻辑（重载2：执行自定义协程）
    /// </summary>
    /// <param name="delayTime">延迟时间</param>
    /// <param name="enumerator">自定义协程迭代器</param>
    /// <returns>迭代器</returns>
    IEnumerator DelayedCoroutineLogic(float delayTime, IEnumerator enumerator){
        // 先等待指定延迟
        yield return new WaitForSeconds(delayTime);
        // 再执行自定义协程的完整逻辑
        yield return enumerator;
    }

    // 配套修正内部逻辑：接收Func<IEnumerator>，每次循环重新生成迭代器
    // 重构内部重复逻辑：增加外层ID和target，子协程注册到字典
    IEnumerator RepeatingCoroutineLogic(float interval, int repeatCount, Func<IEnumerator> enumeratorFunc, string outerCorId, UnityEngine.Object target){
        int count = 0;
        Debug.Log($"【重复协程外层】开始循环（外层ID：{outerCorId}，间隔：{interval}秒，重复次数：{repeatCount}）");

        // 核心修复1：延迟1帧再执行第一次检查（等外层协程完全注册到_activeCoroutines）
        yield return null;

        while (true){
            // 核心修复2：优化检查逻辑——先检查是否存在，再检查是否被取消，避免误判
            bool isOuterCorValid = false;
            lock (_lockObj) {
                isOuterCorValid = _activeCoroutines.ContainsKey(outerCorId)
                                  && _activeCoroutines[outerCorId].State != CoroutineState.Cancelled;
            }

            if (!isOuterCorValid){
                //Debug.LogError($"【重复协程外层】外层协程无效（ID：{outerCorId}），当前_activeCoroutines是否包含该ID：{_activeCoroutines.ContainsKey(outerCorId)}");
                // 仅当明确被取消时才终止，避免误终止
                if (_activeCoroutines.ContainsKey(outerCorId) && _activeCoroutines[outerCorId].State == CoroutineState.Cancelled)
                    yield break;
                else{
                    //Debug.LogWarning($"【重复协程外层】外层协程未注册，等待1帧重试（ID：{outerCorId}）");
                    yield return null; // 等待1帧，让外层协程完成注册
                    continue; 
                }
            }

            if (target != null && target == null){
                //Debug.LogError($"【重复协程外层】目标对象已销毁（ID：{outerCorId}）");
                yield break;
            }

            //Debug.Log($"【重复协程外层】等待{interval}秒后执行第{count + 1}次子协程（外层ID：{outerCorId}）");
            yield return new WaitForSeconds(interval);

            IEnumerator enumerator = enumeratorFunc.Invoke();
            if (enumerator == null){
                //Debug.LogError($"【重复协程外层】子协程迭代器为空（外层ID：{outerCorId}）");
                count++;
                continue;
            }

            string childCorId = StartCoroutine(enumerator, target);
            //Debug.Log($"【重复协程外层】启动第{count + 1}次子协程（外层ID：{outerCorId}，子ID：{childCorId}）");

            lock (_lockObj){
                if (_activeCoroutines.TryGetValue(outerCorId, out var outerHandle)){
                    outerHandle.ChildCoroutineIds.Add(childCorId);
                    _activeCoroutines[outerCorId] = outerHandle;
                }
            }

            yield return enumerator;
            count++;
            //Debug.Log($"【重复协程外层】第{count}次子协程执行完成（外层ID：{outerCorId}）");

            if (repeatCount > 0 && count >= repeatCount){
                Debug.Log($"【重复协程外层】达到重复次数，终止循环（外层ID：{outerCorId}，总次数：{count}）");
                break;
            }
        }
    }

    /// <summary>
    /// 自动清理无效协程（每帧调用）
    /// </summary>
    void CleanupInvalidCoroutines(){
        lock (_lockObj){
            var toCleanIds = new List<string>();
            foreach (var kvp in _activeCoroutines){
                var handle = kvp.Value;
                // 清理1：目标对象已销毁（核心）
                if (handle.Target != null && handle.Target == null)
                    toCleanIds.Add(kvp.Key);
                // 清理2：目标对象属于非当前激活场景（新增）
                else if (handle.Target is Component comp && comp.gameObject.scene != SceneManager.GetActiveScene())
                    toCleanIds.Add(kvp.Key);
                // 清理3：协程已完成/取消但未清理
                else if (handle.State is CoroutineState.Completed or CoroutineState.Cancelled)
        
                toCleanIds.Add(kvp.Key);
            }

            // 批量清理
            foreach (var id in toCleanIds){
                if (_activeCoroutines.TryGetValue(id, out var handle)){ 
                    ReturnCoroutineHandleToPool(handle);
                    _activeCoroutines.Remove(id);
                }
            }
        }
    }
    #endregion
}
