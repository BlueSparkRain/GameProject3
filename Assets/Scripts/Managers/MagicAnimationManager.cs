using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_TweenType
{
    Swing_Box,
    Image_UpMove,

}

public class MagicAnimationManager : MonoGlobalManager
{
    /// <summary>
    /// ����������Ƴ�TCS������Coroutine�ֶι���Э�̣�
    /// </summary>
    struct AnimationHandle
    {
        public Tween TweenObj;
        public AnimParams Params;
        public string TargetInstanceId;
        public Coroutine Coroutine; // ����������Э����������
    }

    /// <summary>
    /// ��Ծ��������
    /// </summary>
    readonly Dictionary<string, AnimationHandle> _activeAnimations = new();

    // �����󣬱�֤�̰߳�ȫ
    readonly object _lockObj = new object();

    protected override void Awake(){
        base.Awake();
        //��ʼ��DOTween
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(200, 50);

        InitTweenerDic();
    }

    public static Dictionary<E_TweenType, string> tweenDic = new Dictionary<E_TweenType, string>();
    
    /// <summary>
    /// ��ʼ�������ֵ�
    /// </summary>
    void InitTweenerDic(){
        RegisterTweenDic(E_TweenType.Swing_Box);
        RegisterTweenDic(E_TweenType.Image_UpMove);
    }

    /// <summary>
    /// ע�Ỻ������ö��
    /// </summary>
    /// <param name="e_TweenType"></param>
    void RegisterTweenDic(E_TweenType e_TweenType){
        if (!tweenDic.ContainsKey(e_TweenType)) // �ȼ����Ƿ����
            tweenDic.Add(e_TweenType, GenerateUniqueAnimId(e_TweenType.ToString()));
    }

    public static string GetAnimID(E_TweenType tweenType){
        return tweenDic.ContainsKey(tweenType) ? tweenDic[tweenType] : null;
    }

    public override void MgrUpdate(float deltaTime){
        CleanupDestroyedTargetAnimations();
    }

    //��ֹ�ڴ�й©
    void OnApplicationQuit(){
        //ֹͣ���ж��� 
        StopAllAnimations();
        //��ջ�Ծ��������
        _activeAnimations.Clear();
        //�������ٵ�������
        Destroy(gameObject);
    }

    void OnDestroy(){
        //�������� 
        lock (_lockObj){
            //ֹͣ���ж���
            StopAllAnimations();
            //��ջ���
            _activeAnimations.Clear();

            //ǿ��ɱ�� DOTween �����ж���
            DOTween.KillAll();
        }
    }

    #region �������API
    /// <summary>
    /// ����ȫ��Ψһ�Ķ���ID
    /// </summary>
    /// <param name="prefix">����IDǰ׺(���ڵ�������)</param>
    /// <returns></returns>
    public string GenerateUniqueAnimId(string prefix = "Anim_"){
        return $"{prefix}{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Э�̲���[����]Tween���������滻ԭ���첽�ӿ�
    /// �ⲿ���ã�StartCoroutine(MagicAnimationManager.Instance.PlayAnimation(...))
    /// </summary>
    /// <param name="animId">����ΨһID</param>
    /// <param name="target">����Ŀ�����</param>
    /// <param name="tweenCreator">��������ί��</param>
    /// <param name="animationParams">��������</param>
    /// <returns>Э�̶��󣨱����ⲿ������</returns>
    public Coroutine PlayAnimation(string animId, object target, Func<AnimParams, Tween> tweenCreator, AnimParams animationParams){
        if (_activeAnimations.ContainsKey(animId))
            InterruptAnimation(animId);

        var coroutine = StartCoroutine(InternalPlayAnimationCoroutine(animId, target, () => tweenCreator(animationParams), animationParams));
        // ����д��������Э������
        lock (_lockObj){
            if (_activeAnimations.ContainsKey(animId)){
                var oldHandle = _activeAnimations[animId];
                _activeAnimations[animId] = new AnimationHandle
                {
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
    /// Э�̲���[Sequence]���ж��������滻ԭ���첽�ӿ�
    /// </summary>
    /// <param name="sequenceId">����ΨһID</param>
    /// <param name="sequenceCreator">���д���ί��</param>
    /// <param name="animationParams">��������</param>
    /// <returns>Э�̶���</returns>
    public Coroutine PlaySequence(string sequenceId, Func<AnimParams, Sequence> sequenceCreator, AnimParams animationParams){
        if (_activeAnimations.ContainsKey(sequenceId))
            InterruptAnimation(sequenceId);
        
        var coroutine = StartCoroutine(InternalPlaySequenceCoroutine(sequenceId, () => sequenceCreator(animationParams), animationParams));


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
    /// ǿ���ж�ָ��ID�Ķ���
    /// </summary>
    /// <param name="animId">Ҫ�жϵĶ���ID</param>
    /// <param name="completeImmediately">�Ƿ��ö������:true->����ֱ����������״̬��false->����ͣ�ڵ�ǰ״̬</param>
    public void InterruptAnimation(string animId, bool completeImmediately = false){
        lock (_lockObj){
            if (!_activeAnimations.ContainsKey(animId))
                return;

            var handle = _activeAnimations[animId];
            // ֹͣЭ��
            if (handle.Coroutine != null)
                StopCoroutine(handle.Coroutine);

            // ֹͣTween
            handle.TweenObj?.Kill(completeImmediately);
            // �����жϻص�
            handle.Params.OnInterrupt?.Invoke();
            // �Ƴ�����
            RemoveAnimation(animId);
        }
    }

    /// <summary>
    /// ��ָͣ��ID�Ķ���
    /// </summary>
    /// <param name="animId">����ID</param>
    public void PauseAnimation(string animId){
        lock (_lockObj){
            if (_activeAnimations.TryGetValue(animId, out var handle))
                handle.TweenObj?.Pause();
            else
                DebugManager.Log(EDebugCategory.General, $"[MagicAnimationManager]---����ID��{animId} ��δע�ᣬ������ͣ");
        }
    }

    /// <summary>
    /// �ָ���ͣ�Ķ���
    /// </summary>
    /// <param name="animId">Ҫ�ָ��Ķ���ID</param>
    public void ResumeAnimation(string animId){
        lock (_lockObj){
            if (_activeAnimations.TryGetValue(animId, out var handle))
                handle.TweenObj?.Play();
            else
                DebugManager.Log(EDebugCategory.General, $"[MagicAnimationManager]---����ID��{animId} ��δע�ᣬ����ָ�");
        }
    }

    /// <summary>
    /// ֹͣ���л�Ծ����
    /// </summary>
    /// <param name="completeImmediately">�Ƿ��ö������</param>
    public void StopAllAnimations(bool completeImmediately = false)
    {
        lock (_lockObj)
        {
            string[] allAnimIds = new string[_activeAnimations.Keys.Count];
            _activeAnimations.Keys.CopyTo(allAnimIds, 0);

            foreach (string animId in allAnimIds)
            {
                if (_activeAnimations.TryGetValue(animId, out var handle))
                {
                    // ֹͣЭ��
                    if (handle.Coroutine != null)
                        StopCoroutine(handle.Coroutine);

                    // ֹͣTween
                    handle.TweenObj?.Kill(!completeImmediately);
                    // �����жϻص�
                    handle.Params.OnInterrupt?.Invoke();
                    // �Ƴ�����
                    RemoveAnimation(animId);
                }
            }
            _activeAnimations.Clear();
        }
    }

    /// <summary>
    /// ָֹͣ�� Unity ��������ж���
    /// </summary>
    /// <param name="target">Ŀ�����</param>
    public void StopTargetAllAnimations(UnityEngine.Object target)
    {
        string targetId = target.GetInstanceID().ToString();
        var toRemove = new List<string>();

        lock (_lockObj)
        {
            foreach (var kvp in _activeAnimations)
            {
                if (kvp.Value.TargetInstanceId == targetId)
                {
                    // ֹͣЭ��
                    if (kvp.Value.Coroutine != null)
                        StopCoroutine(kvp.Value.Coroutine);

                    // ֹͣTween
                    kvp.Value.TweenObj?.Kill();
                    // �����жϻص�
                    kvp.Value.Params.OnInterrupt?.Invoke();
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
                RemoveAnimation(id);

        }
    }
    #endregion


    #region �ڲ������߼���Э��ʵ�֣�
    /// <summary>
    /// ���ŵ���������Э���߼�
    /// </summary>
    /// <param name="animId">����ID</param>
    /// <param name="target">Ŀ�����</param>
    /// <param name="tweenCreator">��������ί��</param>
    /// <param name="animationParams">��������</param>
    /// <returns>Э�̵�����</returns>
    IEnumerator InternalPlayAnimationCoroutine(string animId, object target, Func<Tween> tweenCreator, AnimParams animationParams)
    {

        //��ȡĿ������Ψһ ID
        string targetId = string.Empty;
        if (target is UnityEngine.Object unityObj)
            targetId = unityObj.GetInstanceID().ToString();
        else
            targetId = target.GetHashCode().ToString();

        //���� Tween ����
        Tween tween = tweenCreator.Invoke();
        if (tween == null)
        {
            Debug.LogError($"[MagicAnimationManager]---��������ʧ�ܣ�tweenCreator����null��ID��{animId}��");
            RemoveAnimation(animId);
            yield break;
        }

        //���� Tween ����
        tween.SetDelay(animationParams.Delay)
             .SetEase(animationParams.Ease)
             .OnUpdate(() =>
             {
                 float progress = tween.ElapsedPercentage();
                 animationParams.OnUpdate?.Invoke(progress);
             })
             // �Ƴ�OnComplete/OnKill�Ļص��󶨣�����Э�̿��ƣ�
             .OnComplete(() => { })
             .OnKill(() => { });

        //����ѭ��ģʽ
        if (animationParams.LoopMode != AnimationLoopType.None)
        {
            var dotweenLoopType = animationParams.LoopMode == AnimationLoopType.Restart
                ? DG.Tweening.LoopType.Restart
                : DG.Tweening.LoopType.Yoyo;
            tween.SetLoops(animationParams.LoopCount, dotweenLoopType);
        }

        //��¼�������
        lock (_lockObj)
        {
            _activeAnimations[animId] = new AnimationHandle
            {
                TweenObj = tween,
                Params = animationParams,
                TargetInstanceId = targetId,
                Coroutine = null // Э�����ú������ⲿ��ֵ
            };
        }

        //���Ŷ���
        tween.Play();

        // Э�̵ȴ�������ɣ����ģ�����첽await��
        yield return tween.WaitForCompletion();

        // ������ɺ󴥷��ص�
        animationParams.OnComplete?.Invoke();

        // ��������
        lock (_lockObj)
            RemoveAnimation(animId);
    }

    /// <summary>
    /// �������ж�����Э���߼�
    /// </summary>
    /// <param name="sequenceId">����ID</param>
    /// <param name="sequenceCreator">���д���ί��</param>
    /// <param name="animationParams">��������</param>
    /// <returns>Э�̵�����</returns>
    IEnumerator InternalPlaySequenceCoroutine(string sequenceId, Func<Sequence> sequenceCreator, AnimParams animationParams)
    {
        Sequence sequence = sequenceCreator.Invoke();
        if (sequence == null)
        {
            Debug.LogError($"[MagicAnimationManager]---���д���ʧ�ܣ�sequenceCreator����null��ID��{sequenceId}��");
            RemoveAnimation(sequenceId);
            yield break;
        }

        //�������в���
        sequence.SetDelay(animationParams.Delay)
                .OnUpdate(() =>
                {
                    float progress = sequence.ElapsedPercentage();
                    animationParams.OnUpdate?.Invoke(progress);
                })
                .OnComplete(() => { })
                .OnKill(() => { });

        //����ѭ��ģʽ
        if (animationParams.LoopMode != AnimationLoopType.None)
        {
            var dotweenLoopType = animationParams.LoopMode == AnimationLoopType.Restart
                ? DG.Tweening.LoopType.Restart
                : DG.Tweening.LoopType.Yoyo;
            sequence.SetLoops(animationParams.LoopCount, dotweenLoopType);
        }

        //��¼�������
        lock (_lockObj)
        {
            _activeAnimations[sequenceId] = new AnimationHandle
            {
                TweenObj = sequence,
                Params = animationParams,
                TargetInstanceId = "Sequence_" + sequenceId,
                Coroutine = null
            };
        }

        //��������
        sequence.Play();

        // Э�̵ȴ��������
        yield return sequence.WaitForCompletion();

        // ������ɻص�
        animationParams.OnComplete?.Invoke();

        // ��������
        lock (_lockObj)
            RemoveAnimation(sequenceId);
    }

    /// <summary>
    /// �Զ�������Ч������ÿ֡���ã�
    /// </summary>
    void CleanupDestroyedTargetAnimations()
    {
        var toRemove = new List<string>();
        lock (_lockObj)
        {
            // 1. ������ЧKey��TweenObjΪ�գ�
            foreach (var kvp in _activeAnimations)
            {
                if (kvp.Value.TweenObj == null)
                    toRemove.Add(kvp.Key);
            }
            // 2. ����������Ŀ��Ķ���Key
            foreach (var kvp in _activeAnimations)
            {
                if (int.TryParse(kvp.Value.TargetInstanceId, out int instanceId))
                {
                    bool isTargetAlive = false;
                    foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll<UnityEngine.Object>())
                    {
                        if (obj.GetInstanceID() == instanceId)
                        {
                            isTargetAlive = true;
                            break;
                        }
                    }
                    if (!isTargetAlive)
                    {
                        // ֹͣЭ��
                        if (kvp.Value.Coroutine != null)
                            StopCoroutine(kvp.Value.Coroutine);

                        // ֹͣTween
                        kvp.Value.TweenObj?.Kill();
                        // �����жϻص�
                        kvp.Value.Params.OnInterrupt?.Invoke();
                        toRemove.Add(kvp.Key);
                    }
                }
            }

            // ͳһ�Ƴ�
            foreach (var id in toRemove)
            {
                RemoveAnimation(id);
            }
        }
    }

    /// <summary>
    /// �Ӷ�������ֵ����Ƴ�Ŀ��Ԫ��
    /// </summary>
    /// <param name="animId">���ID</param>
    void RemoveAnimation(string animId)
    {
        lock (_lockObj)
        {
            if (_activeAnimations.ContainsKey(animId))
                _activeAnimations.Remove(animId);
        }
    }
    #endregion
}