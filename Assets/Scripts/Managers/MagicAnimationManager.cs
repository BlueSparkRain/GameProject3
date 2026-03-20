using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AmplifyShaderEditor.Preferences.User;

public enum E_TweenType
{
    Swing_Box,
    Image_UpMove,

}

public class MagicAnimationManager : MonoGlobalManager
{
    /// <summary>
    /// 动画句柄（移除TCS，新增Coroutine字段管理协程）
    /// </summary>
    struct AnimationHandle
    {
        public Tween TweenObj;
        public AnimParams Params;
        public string TargetInstanceId;
        public Coroutine Coroutine; // 新增：管理协程生命周期
    }

    /// <summary>
    /// 活跃动画缓存
    /// </summary>
    readonly Dictionary<string, AnimationHandle> _activeAnimations = new();

    // 锁对象，保证线程安全
    readonly object _lockObj = new object();

    protected override void Awake()
    {
        base.Awake();
        //初始化DOTween
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(200, 50);

        InitTweenerDic();
    }

    public static Dictionary<E_TweenType, string> tweenDic = new Dictionary<E_TweenType, string>();
    void InitTweenerDic(){
        RegisterTweenDic(E_TweenType.Swing_Box);
        RegisterTweenDic(E_TweenType.Image_UpMove);
    }
    void RegisterTweenDic(E_TweenType e_TweenType) {
        if (!tweenDic.ContainsKey(e_TweenType)) // 先检查键是否存在
            tweenDic.Add(e_TweenType, GenerateUniqueAnimId(e_TweenType.ToString()));
    }

    public static string GetAnimID(E_TweenType tweenType){
        return tweenDic.ContainsKey(tweenType) ? tweenDic[tweenType] : null;
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
        return $"{prefix}{Guid.NewGuid():N}";
    }

    /// <summary>
    /// 协程播放[单个]Tween动画——替换原有异步接口
    /// 外部调用：StartCoroutine(MagicAnimationManager.Instance.PlayAnimation(...))
    /// </summary>
    /// <param name="animId">动画唯一ID</param>
    /// <param name="target">动画目标对象</param>
    /// <param name="tweenCreator">动画创建委托</param>
    /// <param name="animationParams">动画参数</param>
    /// <returns>协程对象（便于外部管理）</returns>
    public Coroutine PlayAnimation(string animId, object target, Func<AnimParams, Tween> tweenCreator, AnimParams animationParams){
        if (_activeAnimations.ContainsKey(animId))
            InterruptAnimation(animId);
       
        var coroutine = StartCoroutine(InternalPlayAnimationCoroutine(animId, target, () => tweenCreator(animationParams), animationParams));
        // 兼容写法：更新协程引用
        lock (_lockObj){
            if (_activeAnimations.ContainsKey(animId)){
                var oldHandle = _activeAnimations[animId];
                _activeAnimations[animId] = new AnimationHandle{
                    TweenObj = oldHandle.TweenObj,
                    Params = oldHandle.Params,
                    TargetInstanceId = oldHandle.TargetInstanceId,
                    Coroutine = coroutine
                };
            }
        }
        return coroutine;
    }

    /// <summary>
    /// 协程播放[Sequence]序列动画——替换原有异步接口
    /// </summary>
    /// <param name="sequenceId">动画唯一ID</param>
    /// <param name="sequenceCreator">序列创建委托</param>
    /// <param name="animationParams">动画参数</param>
    /// <returns>协程对象</returns>
    public Coroutine PlaySequence(string sequenceId, Func<AnimParams, Sequence> sequenceCreator, AnimParams animationParams){
        if (_activeAnimations.ContainsKey(sequenceId)){
            InterruptAnimation(sequenceId);
        }
        var coroutine = StartCoroutine(InternalPlaySequenceCoroutine(sequenceId, () => sequenceCreator(animationParams), animationParams));
        // 兼容写法：更新协程引用
        lock (_lockObj){
            if (_activeAnimations.ContainsKey(sequenceId)){
                var oldHandle = _activeAnimations[sequenceId];
                _activeAnimations[sequenceId] = new AnimationHandle{
                    TweenObj = oldHandle.TweenObj,
                    Params = oldHandle.Params,
                    TargetInstanceId = oldHandle.TargetInstanceId,
                    Coroutine = coroutine
                };
            }
        }
        return coroutine;
    }

    /// <summary>
    /// 强制中断指定ID的动画
    /// </summary>
    /// <param name="animId">要中断的动画ID</param>
    /// <param name="completeImmediately">是否让动画完成:true->动画直接跳到结束状态，false->动画停在当前状态</param>
    public void InterruptAnimation(string animId, bool completeImmediately = false){
        lock (_lockObj){
            if (!_activeAnimations.ContainsKey(animId))
                return;

            var handle = _activeAnimations[animId];
            // 停止协程
            if (handle.Coroutine != null)
                StopCoroutine(handle.Coroutine);
            
            // 停止Tween
            handle.TweenObj?.Kill(completeImmediately);
            // 触发中断回调
            handle.Params.OnInterrupt?.Invoke();
            // 移除动画
            RemoveAnimation(animId);
        }
    }

    /// <summary>
    /// 暂停指定ID的动画
    /// </summary>
    /// <param name="animId">动画ID</param>
    public void PauseAnimation(string animId){
        lock (_lockObj){
            if (_activeAnimations.TryGetValue(animId, out var handle))
                handle.TweenObj?.Pause();
            else
                Debug.Log($"[MagicAnimationManager]---动画ID：{animId} 暂未注册，无需暂停");
            
        }
    }

    /// <summary>
    /// 恢复暂停的动画
    /// </summary>
    /// <param name="animId">要恢复的动画ID</param>
    public void ResumeAnimation(string animId){
        lock (_lockObj){
            if (_activeAnimations.TryGetValue(animId, out var handle))
                handle.TweenObj?.Play();
            else
                Debug.Log($"[MagicAnimationManager]---动画ID：{animId} 暂未注册，无需恢复");
        }
    }

    /// <summary>
    /// 停止所有活跃动画
    /// </summary>
    /// <param name="completeImmediately">是否让动画完成</param>
    public void StopAllAnimations(bool completeImmediately = false){
        lock (_lockObj){
            string[] allAnimIds = new string[_activeAnimations.Keys.Count];
            _activeAnimations.Keys.CopyTo(allAnimIds, 0);

            foreach (string animId in allAnimIds){
                if (_activeAnimations.TryGetValue(animId, out var handle)){
                    // 停止协程
                    if (handle.Coroutine != null)
                        StopCoroutine(handle.Coroutine);
                    
                    // 停止Tween
                    handle.TweenObj?.Kill(!completeImmediately);
                    // 触发中断回调
                    handle.Params.OnInterrupt?.Invoke();
                    // 移除动画
                    RemoveAnimation(animId);
                }
            }
            _activeAnimations.Clear();
        }
    }

    /// <summary>
    /// 停止指定 Unity 对象的所有动画
    /// </summary>
    /// <param name="target">目标对象</param>
    public void StopTargetAllAnimations(UnityEngine.Object target){
        string targetId = target.GetInstanceID().ToString();
        var toRemove = new List<string>();

        lock (_lockObj){
            foreach (var kvp in _activeAnimations){
                if (kvp.Value.TargetInstanceId == targetId){
                    // 停止协程
                    if (kvp.Value.Coroutine != null)
                        StopCoroutine(kvp.Value.Coroutine);
                    
                    // 停止Tween
                    kvp.Value.TweenObj?.Kill();
                    // 触发中断回调
                    kvp.Value.Params.OnInterrupt?.Invoke();
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
                RemoveAnimation(id);
            
        }
    }
    #endregion


    #region 内部核心逻辑（协程实现）
    /// <summary>
    /// 播放单个动画的协程逻辑
    /// </summary>
    /// <param name="animId">动画ID</param>
    /// <param name="target">目标对象</param>
    /// <param name="tweenCreator">动画创建委托</param>
    /// <param name="animationParams">动画参数</param>
    /// <returns>协程迭代器</returns>
    IEnumerator InternalPlayAnimationCoroutine(string animId, object target, Func<Tween> tweenCreator, AnimParams animationParams){
        Debug.Log(animationParams.Ease + "-666677");

        //获取目标对象的唯一 ID
        string targetId = string.Empty;
        if (target is UnityEngine.Object unityObj)
            targetId = unityObj.GetInstanceID().ToString();
        else
            targetId = target.GetHashCode().ToString();

            //创建 Tween 对象
            Tween tween = tweenCreator.Invoke();
            if (tween == null){
                Debug.LogError($"[MagicAnimationManager]---动画创建失败：tweenCreator返回null（ID：{animId}）");
                RemoveAnimation(animId);
                yield break;
            }

            //配置 Tween 参数
            tween.SetDelay(animationParams.Delay)
                 .SetEase(animationParams.Ease)
                 .OnUpdate(() =>{
                     float progress = tween.ElapsedPercentage();
                     animationParams.OnUpdate?.Invoke(progress);
                 })
                 // 移除OnComplete/OnKill的回调绑定（改由协程控制）
                 .OnComplete(() => { })
                 .OnKill(() => { });

            //处理循环模式
            if (animationParams.LoopMode != AnimationLoopType.None){
                var dotweenLoopType = animationParams.LoopMode == AnimationLoopType.Restart
                    ? DG.Tweening.LoopType.Restart
                    : DG.Tweening.LoopType.Yoyo;
                tween.SetLoops(animationParams.LoopCount, dotweenLoopType);
            }

            //记录动画句柄
            lock (_lockObj){
                _activeAnimations[animId] = new AnimationHandle{
                    TweenObj = tween,
                    Params = animationParams,
                    TargetInstanceId = targetId,
                    Coroutine = null // 协程引用后续由外部赋值
                };
            }

            //播放动画
            tween.Play();

            // 协程等待动画完成（核心：替代异步await）
            yield return tween.WaitForCompletion();

            // 动画完成后触发回调
            animationParams.OnComplete?.Invoke();

            // 清理动画
            lock (_lockObj)
                RemoveAnimation(animId);
    }

    /// <summary>
    /// 播放序列动画的协程逻辑
    /// </summary>
    /// <param name="sequenceId">序列ID</param>
    /// <param name="sequenceCreator">序列创建委托</param>
    /// <param name="animationParams">动画参数</param>
    /// <returns>协程迭代器</returns>
    IEnumerator InternalPlaySequenceCoroutine(string sequenceId, Func<Sequence> sequenceCreator, AnimParams animationParams){
            Sequence sequence = sequenceCreator.Invoke();
            if (sequence == null){
                Debug.LogError($"[MagicAnimationManager]---序列创建失败：sequenceCreator返回null（ID：{sequenceId}）");
                RemoveAnimation(sequenceId);
                yield break;
            }

            //配置序列参数
            sequence.SetDelay(animationParams.Delay)
                    .OnUpdate(() =>{
                        float progress = sequence.ElapsedPercentage();
                        animationParams.OnUpdate?.Invoke(progress);
                    })
                    .OnComplete(() => { })
                    .OnKill(() => { });

            //处理循环模式
            if (animationParams.LoopMode != AnimationLoopType.None){
                var dotweenLoopType = animationParams.LoopMode == AnimationLoopType.Restart
                    ? DG.Tweening.LoopType.Restart
                    : DG.Tweening.LoopType.Yoyo;
                sequence.SetLoops(animationParams.LoopCount, dotweenLoopType);
            }

            //记录动画句柄
            lock (_lockObj){
                _activeAnimations[sequenceId] = new AnimationHandle{
                    TweenObj = sequence,
                    Params = animationParams,
                    TargetInstanceId = "Sequence_" + sequenceId,
                    Coroutine = null
                };
            }

            //播放序列
            sequence.Play();

            // 协程等待序列完成
            yield return sequence.WaitForCompletion();

            // 触发完成回调
            animationParams.OnComplete?.Invoke();

            // 清理动画
            lock (_lockObj)
                RemoveAnimation(sequenceId);
    }

    /// <summary>
    /// 自动清理无效动画（每帧调用）
    /// </summary>
    void CleanupDestroyedTargetAnimations(){
        var toRemove = new List<string>();
        lock (_lockObj){
            // 1. 清理无效Key（TweenObj为空）
            foreach (var kvp in _activeAnimations){
                if (kvp.Value.TweenObj == null)
                    toRemove.Add(kvp.Key);
            }
            // 2. 清理已销毁目标的动画Key
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
                        // 停止协程
                        if (kvp.Value.Coroutine != null)
                            StopCoroutine(kvp.Value.Coroutine);
                        
                        // 停止Tween
                        kvp.Value.TweenObj?.Kill();
                        // 触发中断回调
                        kvp.Value.Params.OnInterrupt?.Invoke();
                        toRemove.Add(kvp.Key);
                    }
                }
            }

            // 统一移除
            foreach (var id in toRemove){
                RemoveAnimation(id);
            }
        }
    }

    /// <summary>
    /// 从动画句柄字典中移除目标元素
    /// </summary>
    /// <param name="animId">句柄ID</param>
    void RemoveAnimation(string animId){
        lock (_lockObj){
            if (_activeAnimations.ContainsKey(animId))
                _activeAnimations.Remove(animId);
        }
    }
    #endregion
}