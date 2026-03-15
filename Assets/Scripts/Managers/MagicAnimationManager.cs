using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public enum E_TweenType
{
    Swing_Box,
    Image_UpMove,

}
public class MagicAnimationManager : MonoGlobalManager
{
    /// <summary>
    /// 动画句柄
    /// 封装单个动画的核心信息，避免零散存储：
    /// TweenObj：DOTween 的动画对象（Tween/Sequence）
    /// Params：动画参数（如时长、缓动、循环等）
    /// TargetInstanceId：动画目标的唯一 ID（Unity 对象用 InstanceID，普通对象用 HashCode）
    /// Tcs：异步完成源，用于异步等待动画结束
    /// </summary>
    struct AnimationHandle{
        public Tween TweenObj;
        public AnimParams Params;
        public string TargetInstanceId;
        public TaskCompletionSource<bool> Tcs;
    }

    /// <summary>
    /// 活跃动画缓存
    /// 缓存所有活跃状态的动画，Key 为动画唯一 ID，Value 为动画句柄
    /// </summary>
    readonly Dictionary<string, AnimationHandle> _activeAnimations = new();

    // 新增：锁对象，保证线程安全
    readonly object _lockObj = new object();

    protected override void Awake(){
        base.Awake();
        //初始化DOTween
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(200, 50);
        //Dotween.Init()参数解析如下
        //true（第一个）：启用安全模式（避免无效对象的 Tween 报错）；
        //true（第二个）：启用回收池（提升 Tween 性能）；
        //LogBehaviour.ErrorsOnly：仅在出错时打印日志；
        //SetCapacity(200, 50)：预设 Tween 池容量（200 个 Tween，50 个 Sequence），减少内存分配。

        InitTweenerDic();
    }

  
    public static Dictionary<E_TweenType, string> tweenDic = new Dictionary<E_TweenType, string>();
    void InitTweenerDic(){
        tweenDic.Add(E_TweenType.Swing_Box, GenerateUniqueAnimId(E_TweenType.Swing_Box.ToString()));
        tweenDic.Add(E_TweenType.Image_UpMove, GenerateUniqueAnimId(E_TweenType.Image_UpMove.ToString()));

    }
    public static string GetAnimID(E_TweenType tweenType){
        return tweenDic.ContainsKey(tweenType)?tweenDic[tweenType]:null;
    }
    public override void MgrUpdate(float deltaTime){
        CleanupDestroyedTargetAnimations();
    }

    //防止内存泄漏
    void OnApplicationQuit(){
        //停止所有动画 
        StopAllAnimations();

        //清空活跃动画缓存
        _activeAnimations.Clear();

        //主动销毁单例对象
        Destroy(gameObject);
    }

    void OnDestroy(){
        //加锁保护 
        lock (_lockObj){
            //停止所有动画
            StopAllAnimations();

            //清空缓存
            _activeAnimations.Clear();

            //强制杀死 DOTween 的所有动画
            DOTween.KillAll();
        }
    }

    #region 对外核心API
    /// <summary>
    /// 生成全局唯一的动画ID
    /// </summary>
    /// <param name="prefix">动画ID前缀(便于调试区分)</param>
    /// <returns></returns>
    public string GenerateUniqueAnimId(string prefix = "Anim_"){
        //Guid.NewGuid() 用于生成无连字符的唯一字符串
        return $"{prefix}{Guid.NewGuid():N}";
    }

    /// <summary>
    /// 异步播放[单个]Tween动画——最核心的动画播放接口
    /// </summary>
    /// <param name="animId">动画唯一ID（由上面的GenerateUniqueAnimId()生成）</param>
    /// <param name="target">动画目标对象(如Transform,RectTransform)</param>
    /// <param name="tweenCreator">动画创建委托(外部传入具体的Tween逻辑，比如移动、缩放)</param>
    /// <param name="animationParams">动画参数</param>
    /// <returns></returns>
    public async Task PlayAnimationAsync(string animId, object target, Func<AnimParams, Tween> tweenCreator, AnimParams animationParams){
        //先中断同名 ID 的旧动画（避免同 ID 动画冲突）；
        if (_activeAnimations.ContainsKey(animId)){
            InterruptAnimation(animId);
        }
        //调用内部方法 InternalPlayAnimationAsync 执行实际播放逻辑
        await InternalPlayAnimationAsync(animId, target, () => tweenCreator(animationParams), animationParams);
    }

    /// <summary>
    /// 异步播放[Sequence]序列动画————多个Tween按顺序/并行执行
    /// 和PlayAnimationAsync 几乎一致，仅适配 Sequence 类型
    /// </summary>
    /// <param name="sequenceId">动画唯一ID(同)</param>
    /// <param name="sequenceCreator">动画目标对象</param>
    /// <param name="animationParams">动画参数</param>
    /// <returns></returns>
    public async Task PlaySequenceAsync(string sequenceId, Func<AnimParams, Sequence> sequenceCreator, AnimParams animationParams){
        if (_activeAnimations.ContainsKey(sequenceId)){
            InterruptAnimation(sequenceId);
        }
        await InternalPlaySequenceAsync(sequenceId, () => sequenceCreator(animationParams), animationParams);
    }


    /// <summary>
    /// 强制中断指定ID的动画
    /// </summary>
    /// <param name="animId">要中断的动画ID</param>
    /// <param name="completeImmediately">是否让动画完成:true->动画直接跳到结束状态，false->动画停在当前状态</param>
    public void InterruptAnimation(string animId, bool completeImmediately = false){
        lock (_lockObj){
            if (!_activeAnimations.ContainsKey(animId)){
                return;
            }

            var handle = _activeAnimations[animId];
            handle.TweenObj?.Kill(completeImmediately);

            // 修复：先移除动画句柄，再完成Task，避免回调冲突
            RemoveAnimation(animId);
            handle.Tcs.TrySetResult(true);
        }
    }

    /// <summary>
    /// 暂停指定ID的动画
    /// </summary>
    /// <param name="animId">c</param>
    public void PauseAnimation(string animId){
        lock (_lockObj){
            if (_activeAnimations.TryGetValue(animId, out var handle))
                handle.TweenObj?.Pause();
            else
                Debug.Log($"动画ID：{animId} 暂未注册，无需暂停");
        }
    }

    /// <summary>
    /// 恢复暂停的动画
    /// </summary>
    /// <param name="animId">要恢复的动画ID</param>
    public void ResumeAnimation(string animId)
    {
        lock (_lockObj)
        {
            if (_activeAnimations.TryGetValue(animId, out var handle))
                handle.TweenObj?.Play();
            else
                Debug.Log($"动画ID：{animId} 暂未注册，无需恢复");
        }
    }

    /// <summary>
    /// 停止所有活跃动画，“批量中断” 的核心接口；
    /// </summary>
    /// <param name="completeImmediately">是否让动画完成:true->动画直接跳到结束状态，false->动画停在当前状态</param>
    public void StopAllAnimations(bool completeImmediately = false)
    {
        lock (_lockObj){
            // 复制所有Key到数组，遍历数组而非原字典
            string[] allAnimIds = new string[_activeAnimations.Keys.Count];
            _activeAnimations.Keys.CopyTo(allAnimIds, 0);

            // 遍历复制的Key数组，避免遍历原字典时修改
            foreach (string animId in allAnimIds){
                if (_activeAnimations.TryGetValue(animId, out var handle)){
                    // 终止Tween
                    handle.TweenObj?.Kill(!completeImmediately);
                    // 完成Task
                    handle.Tcs.TrySetResult(true);
                    // 安全移除（此时遍历的是数组，修改字典不会报错）
                    RemoveAnimation(animId);
                }
            }
            // 最后清空字典（可选，确保无残留）
            _activeAnimations.Clear();
        }
    }
    /// <summary>
    /// 停止指定 Unity 对象的所有动画（比如某个 UI 被销毁前，停止它的所有动画）；
    /// </summary>
    /// <param name="target"></param>
    public void StopTargetAllAnimations(UnityEngine.Object target)
    {
        string targetId = target.GetInstanceID().ToString();
        var toRemove = new List<string>();

        lock (_lockObj)
        {
            // 第一步：收集要移除的Key
            foreach (var kvp in _activeAnimations)
            {
                if (kvp.Value.TargetInstanceId == targetId)
                {
                    kvp.Value.TweenObj?.Kill();
                    kvp.Value.Tcs.TrySetResult(true);
                    toRemove.Add(kvp.Key);
                }
            }

            // 第二步：统一移除
            foreach (var id in toRemove)
            {
                RemoveAnimation(id);
            }
        }
    }
    #endregion

    #region 内部核心逻辑

    /// <summary>
    ///PlayAnimationAsync 的内部实现，封装动画播放的核心逻辑
    /// </summary>
    /// <param name="animId">目标的动画ID</param>
    /// <param name="target">动画应用的组件</param>
    /// <param name="tweenCreator">实际的动画委托</param>
    /// <param name="animationParams">动画参数</param>
    /// <returns></returns>
    async Task InternalPlayAnimationAsync(string animId, object target, Func<Tween> tweenCreator, AnimParams animationParams){
        Debug.Log(animationParams.Ease+"-666677");
        
        //获取目标对象的唯一 ID（Unity 对象用 InstanceID，普通对象用 HashCode）
        string targetId = string.Empty;
        if (target is UnityEngine.Object unityObj)
            targetId = unityObj.GetInstanceID().ToString();
        else
            targetId = target.GetHashCode().ToString();
        
        // 临时变量：存储要await的Tcs（避免lock内await）
        TaskCompletionSource<bool> tcs = null;
        
        try{
            //调用 tweenCreator 创建 Tween 对象，为空则抛错；
            Tween tween = tweenCreator.Invoke();
            if (tween == null){
                //仅抛错相关逻辑
                Debug.LogError($"动画创建失败：tweenCreator返回null（ID：{animId}）");
                lock (_lockObj){
                    if (_activeAnimations.ContainsKey(animId))
                        _activeAnimations[animId].Tcs.TrySetException(new ArgumentNullException("tweenCreator返回null"));
                }
                return;
            }

            //配置 Tween 的延迟+缓动+回调（OnUpdate/OnComplete/OnKill），是一段链式调用；
            tween.SetDelay(animationParams.Delay)
                 .SetEase(animationParams.Ease)
                 //动画运行中每帧执行OnUpdate()————传递自定义曲线进度，支撑外部灵活的动画逻辑
                 .OnUpdate(() =>{
                     float progress = tween.ElapsedPercentage();
                     //float curveProgress = animationParams.Curve.Evaluate(progress);
                     //animationParams.OnUpdate?.Invoke(curveProgress);
                 })
                 //动画自然播放完毕后 执行OnComplete()————触发完成回调 + 清理资源
                 .OnComplete(() =>{
                     //触发外部传入的动画完成回调方法
                     animationParams.OnComplete?.Invoke();
                     lock (_lockObj){
                         if (_activeAnimations.ContainsKey(animId)){
                             //标记异步任务完成
                             _activeAnimations[animId].Tcs.TrySetResult(true);
                             //从活跃动画缓存中移除该动画
                             RemoveAnimation(animId);
                         }
                     }
                 })
                 //动画被强制终止时 执行OnKill()————触发中断回调 + 兜底清理资源
                 .OnKill(() =>{
                     //触发外部传入的动画打断回调方法
                     animationParams.OnInterrupt?.Invoke();
                     lock (_lockObj){
                         if (_activeAnimations.ContainsKey(animId)){
                             //标记异步任务完成
                             _activeAnimations[animId].Tcs.TrySetResult(true);
                             //从活跃动画缓存中移除该动画
                             RemoveAnimation(animId);
                         }
                     }
                 });

            //处理循环模式（适配自定义的 AnimationLoopType 到 DOTween 的 LoopType）；
            if (animationParams.LoopMode != AnimationLoopType.None){
                var dotweenLoopType = animationParams.LoopMode == AnimationLoopType.Restart
                    ? DG.Tweening.LoopType.Restart
                    : DG.Tweening.LoopType.Yoyo;
                tween.SetLoops(animationParams.LoopCount, dotweenLoopType);
            }

            //处理动画曲线（自定义 AnimationCurve 覆盖默认缓动）；
            //if (animationParams.Curve != AnimationCurve.Linear(0, 0, 1, 1)){
            //    tween.SetEase((time, duration, _, _) =>{
            //        float progress = time / duration;
            //        return animationParams.Curve.Evaluate(progress);
            //    });
            //}

            //加锁创建 TaskCompletionSource，将动画缓存到 _activeAnimations；
            lock (_lockObj){
                tcs = new TaskCompletionSource<bool>();
                _activeAnimations[animId] = new AnimationHandle{
                    TweenObj = tween,
                    Params = animationParams,
                    TargetInstanceId = targetId,
                    Tcs = tcs // 绑定到临时变量
                };
            }
            //播放 Tween，异步等待任务完成；
            tween.Play();

            //移出lock，await临时变量的Task
            if (tcs != null)
                await tcs.Task;
        }
        //异常处理：捕获创建 / 播放过程中的错误，通知异步任务并清理缓存。
        catch (Exception ex){
            Debug.LogError($"动画播放失败（ID：{animId}）：{ex.Message}\n{ex.StackTrace}");
            lock (_lockObj){
                if (_activeAnimations.ContainsKey(animId)){
                    _activeAnimations[animId].Tcs.TrySetException(ex);
                    RemoveAnimation(animId);
                }
            }
            // 同时完成临时Tcs，避免阻塞
            if (tcs != null)
                tcs.TrySetException(ex);
            throw;
        }
    }

    /// <summary>
    /// PlaySequenceAsync的内部实现，逻辑和 InternalPlayAnimationAsync 几乎一致，仅适配 Sequence 类型；
    ///区别：Sequence的目标ID 设为 Sequence_ + sequenceId（因为 Sequence 无具体目标对象）。
    /// </summary>
    /// <param name="sequenceId"></param>
    /// <param name="sequenceCreator"></param>
    /// <param name="animationParams"></param>
    /// <returns></returns>
    async Task InternalPlaySequenceAsync(string sequenceId, Func<Sequence> sequenceCreator, AnimParams animationParams){
        // 临时变量存储Tcs
        TaskCompletionSource<bool> tcs = null;
        try{
            Sequence sequence = sequenceCreator.Invoke();
            if (sequence == null){
                Debug.LogError($"序列创建失败：sequenceCreator返回null（ID：{sequenceId}）");
                lock (_lockObj){
                    if (_activeAnimations.ContainsKey(sequenceId)){
                        _activeAnimations[sequenceId].Tcs.TrySetException(new ArgumentNullException("sequenceCreator返回null"));
                    }
                }
                return;
            }

            sequence.SetDelay(animationParams.Delay)
                    .OnUpdate(() =>{
                        float progress = sequence.ElapsedPercentage();
                        //float curveProgress = animationParams.Curve.Evaluate(progress);
                        //animationParams.OnUpdate?.Invoke(curveProgress);
                    })
                    .OnComplete(() =>{
                        animationParams.OnComplete?.Invoke();
                        lock (_lockObj)
                        {
                            if (_activeAnimations.ContainsKey(sequenceId))
                            {
                                _activeAnimations[sequenceId].Tcs.TrySetResult(true);
                                RemoveAnimation(sequenceId);
                            }
                        }
                    })
                    .OnKill(() =>{
                        animationParams.OnInterrupt?.Invoke();
                        lock (_lockObj){
                            if (_activeAnimations.ContainsKey(sequenceId)){
                                _activeAnimations[sequenceId].Tcs.TrySetResult(true);
                                RemoveAnimation(sequenceId);
                            }
                        }
                    });

            if (animationParams.LoopMode != AnimationLoopType.None){
                var dotweenLoopType = animationParams.LoopMode == AnimationLoopType.Restart
                    ? DG.Tweening.LoopType.Restart
                    : DG.Tweening.LoopType.Yoyo;
                sequence.SetLoops(animationParams.LoopCount, dotweenLoopType);
            }

            //if (animationParams.Curve != AnimationCurve.Linear(0, 0, 1, 1)){
            //    sequence.SetEase((time, duration, _, _) =>{
            //        float progress = time / duration;
            //        return animationParams.Curve.Evaluate(progress);
            //    });
            //}

            //lock内创建Tcs并赋值给临时变量
            lock (_lockObj){
                tcs = new TaskCompletionSource<bool>();
                _activeAnimations[sequenceId] = new AnimationHandle{
                    TweenObj = sequence,
                    Params = animationParams,
                    TargetInstanceId = "Sequence_" + sequenceId,
                    Tcs = tcs
                };
            }
            sequence.Play();
            //移出lock，await临时变量（核心修复）
            if (tcs != null)
                await tcs.Task;
        }
        catch (Exception ex){
            Debug.LogError($"序列动画播放失败（ID：{sequenceId}）：{ex.Message}\n{ex.StackTrace}");
            lock (_lockObj){
                if (_activeAnimations.ContainsKey(sequenceId)){
                    _activeAnimations[sequenceId].Tcs.TrySetException(ex);
                    RemoveAnimation(sequenceId);
                }
            }
            // 完成临时Tcs
            if (tcs != null)
                tcs.TrySetException(ex);
            throw;
        }
    }
    /// <summary>
    /// 自动清理无效动画（每帧调用）
    /// 清理两类无效动画：
    /// Tween对象为空且异步任务已完成的动画（无意义的缓存）；
    /// 目标Unity对象已销毁的动画（避免对销毁对象执行动画导致报错）；
    /// </summary>
    private void CleanupDestroyedTargetAnimations()
    {
        var toRemove = new List<string>();
        lock (_lockObj){
            // 1. 清理无效Key（TweenObj为空且Tcs已完成）
            // 即Tween对象为空且异步任务已完成的动画（无意义的缓存）；
            foreach (var kvp in _activeAnimations){
                if (kvp.Value.TweenObj == null && kvp.Value.Tcs.Task.IsCompleted){
                    toRemove.Add(kvp.Key);
                }
            }

            // 2. 清理已销毁目标的动画Key
            // 即目标Unity对象已销毁的动画（避免对销毁对象执行动画导致报错）
            foreach (var kvp in _activeAnimations){
                if (int.TryParse(kvp.Value.TargetInstanceId, out int instanceId)){
                    bool isTargetAlive = false;
                    foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll<UnityEngine.Object>()){
                        if (obj.GetInstanceID() == instanceId){
                            isTargetAlive = true;
                            break;
                        }
                    }
                    if (!isTargetAlive){
                        kvp.Value.TweenObj?.Kill();
                        kvp.Value.Tcs.TrySetResult(true);
                        toRemove.Add(kvp.Key);
                    }
                }
            }
            // 统一移除（遍历完成后再修改字典）
            foreach (var id in toRemove){
                if (_activeAnimations.ContainsKey(id)){
                    RemoveAnimation(id);
                }
            }
        }
    }

    /// <summary>
    /// 从动画句柄字典中移除目标元素
    /// </summary>
    /// <param name="animId">句柄ID</param>
    void RemoveAnimation(string animId){
        lock (_lockObj){
            if (_activeAnimations.ContainsKey(animId)){
                _activeAnimations.Remove(animId);
            }
        }
    }
    #endregion
















    #region 新增：基于配置的动画扩展示例（不影响原有功能）

    private Tween CreateMoveTween(UnityEngine.Object target, Vector3 targetPos, AnimParams p)
    {
        Tween tween = null;

        // 根据物体类型和空间模式适配不同的动画逻辑
        switch (p.TargetType)
        {
            case AnimationTargetType.UI:
                if (target is RectTransform rectTrans)
                {
                    // UI使用锚点位置，空间模式不影响（UI自身的本地/世界逻辑）
                    tween = rectTrans.DOAnchorPos((Vector2)targetPos, p.Duration)
                                     .SetEase(p.Ease)
                                     .SetDelay(p.Delay);
                }
                break;

            case AnimationTargetType.Sprite2D:
            case AnimationTargetType.Object3D:
                if (target is Transform trans)
                {
                    if (p.SpaceMode == AnimationSpaceMode.Local)
                    {
                        tween = trans.DOLocalMove(targetPos, p.Duration)
                                     .SetEase(p.Ease)
                                     .SetDelay(p.Delay);
                    }
                    else
                    {
                        tween = trans.DOMove(targetPos, p.Duration)
                                     .SetEase(p.Ease)
                                     .SetDelay(p.Delay);
                    }
                }
                break;

            case AnimationTargetType.Auto:
                // 自动识别类型
                if (target is RectTransform autoRect)
                {
                    tween = autoRect.DOAnchorPos((Vector2)targetPos, p.Duration)
                                     .SetEase(p.Ease)
                                     .SetDelay(p.Delay);
                }
                else if (target is Transform autoTrans)
                {
                    if (p.SpaceMode == AnimationSpaceMode.Local)
                    {
                        tween = autoTrans.DOLocalMove(targetPos, p.Duration)
                                         .SetEase(p.Ease)
                                         .SetDelay(p.Delay);
                    }
                    else
                    {
                        tween = autoTrans.DOMove(targetPos, p.Duration)
                                         .SetEase(p.Ease)
                                         .SetDelay(p.Delay);
                    }
                }
                break;
        }

        return tween;
    }
    #endregion

}