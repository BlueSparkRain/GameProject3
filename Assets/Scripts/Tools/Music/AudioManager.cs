using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频类型枚举（区分背景音乐/音效）
/// </summary>
public enum AudioType{
    BGM,  // 背景音乐（支持多轨道共存、缓入缓出）
    SFX   // 音效（对象池复用，高性能播放）
}

/// <summary>
/// BGM轨道通道封装类
/// 【核心设计】每个轨道独立管理AudioSource和渐变状态，实现多BGM共存
/// </summary>
public class BgmChannel
{
    public AudioSource audioSource;       // 该轨道专属的音频播放组件
    public Coroutine fadeCoroutine;       // 该轨道当前的音量渐变协程（淡入/淡出）
    public bool isFading;                 // 标记：该轨道是否正在执行音量渐变
    public string trackName;              // 轨道名称（区分不同BGM轨道，如Main/Ambient）
}

/// <summary>
/// 多轨道音频管理器（单例模式）
/// 核心能力：1.多BGM轨道共存+独立缓入缓出 2.SFX对象池高性能播放 3.全流程安全校验
/// 不修改任何功能，仅添加关键注释
/// </summary>
public class AudioManager :MonoGlobalManager
{
    #region Inspector可配置参数（可视化调整，无需改代码）
    [Header("BGM全局配置")]
    [Range(0f, 1f)] public float bgmBaseVolume = 0.8f; // BGM基础音量（0-1）
    public float bgmFadeDuration = 1.5f;               // BGM淡入/淡出时长（秒）
    public bool bgmLoop = true;                        // BGM是否默认循环

    [Header("SFX配置")]
    [Range(0f, 1f)] public float sfxBaseVolume = 1f;     // SFX基础音量（0-1）
    public int sfxPoolInitSize = 5;                      // SFX对象池初始容量
    public int sfxPoolMaxSize = 10;                      // SFX对象池最大容量（防止内存溢出）
    [Range(0f, 1f)] public float sfxDefaultSpatialBlend; // SFX默认3D混合度（0=2D，1=3D）
    #endregion

    #region 核心私有变量（音频状态管理）
    // 关键：多BGM轨道字典 → Key=轨道名（如Main/Ambient），Value=轨道实例
    // 不同轨道名对应不同AudioSource，实现多BGM同时播放
    private Dictionary<string, BgmChannel> _bgmChannels;

    // SFX对象池核心 → 复用AudioSource，避免频繁创建/销毁导致GC卡顿
    private List<AudioSource> _sfxPool;       // 空闲的SFX播放组件池
    private List<AudioSource> _sfxInUse;      // 正在使用的SFX播放组件
    #endregion

    protected override void Awake()
    {
        base.Awake();
        InitAudioComponents(); // 初始化音频核心组件
    }

    #region 初始化核心方法（仅在启动时执行）
    /// <summary>
    /// 初始化BGM轨道字典和SFX对象池（核心初始化，仅执行一次）
    /// </summary>
    void InitAudioComponents(){
        // 初始化BGM轨道字典（存储多轨道BGM状态）
        _bgmChannels = new Dictionary<string, BgmChannel>();

        // 初始化SFX对象池（预创建指定数量的SFX播放组件）
        _sfxPool = new List<AudioSource>();
        _sfxInUse = new List<AudioSource>();
        // 预创建初始容量的SFX组件，避免首次播放卡顿
        for (int i = 0; i < sfxPoolInitSize; i++) CreateSfxAudioSource();
    }
    #endregion

    #region BGM轨道核心方法（多轨道共存的核心逻辑）
    /// <summary>
    /// 获取/创建指定名称的BGM轨道（核心：轨道不存在则自动创建）
    /// </summary>
    /// <param name="trackName">轨道名称（如Main/Ambient）</param>
    BgmChannel GetOrCreateBgmChannel(string trackName){
        // 优先复用已存在的轨道
        if (_bgmChannels.TryGetValue(trackName, out var channel)) return channel;

        // 轨道不存在时，创建新轨道的GameObject和AudioSource
        var channelObj = new GameObject($"BGM_Track_{trackName}");
        channelObj.transform.SetParent(transform); // 挂载到管理器下，方便管理

        var source = channelObj.AddComponent<AudioSource>();
        // 轨道音频组件基础配置（仅初始化时设置）
        source.playOnAwake = false; // 关键：不自动播放
        source.loop = bgmLoop;      // 继承全局循环配置
        source.volume = 0f;         // 初始音量0，方便淡入
        source.spatialBlend = 0f;   // BGM默认2D音效

        // 封装轨道状态并加入字典
        channel = new BgmChannel{
            trackName = trackName,
            audioSource = source,
            isFading = false,
            fadeCoroutine = null
        };
        _bgmChannels.Add(trackName, channel);
        return channel;
    }

    /// <summary>
    /// 播放指定轨道的BGM（多BGM共存核心API）
    /// </summary>
    /// <param name="bgmClip">要播放的BGM音频文件</param>
    /// <param name="trackName">轨道名（不同名则共存）</param>
    /// <param name="fadeIn">是否启用淡入效果</param>
    /// <param name="forceRestart">同一轨道是否强制重启（避免重复播放）</param>
    public void PlayBGM(AudioClip bgmClip, string trackName = "Main", bool fadeIn = true, bool forceRestart = false){
        // 安全校验：音频文件为空则报错
        if (bgmClip == null){
            Debug.LogError($"[BGM] 轨道{trackName}音频片段为空，无法播放");
            return;
        }

        // 获取/创建目标轨道
        var channel = GetOrCreateBgmChannel(trackName);
        var source = channel.audioSource;

        // 关键：同一轨道+同一BGM+正在播放 → 不重复播放（除非强制重启）
        if (source.clip == bgmClip && source.isPlaying && !forceRestart) return;

        // 停止该轨道当前的渐变协程（防止淡入/淡出冲突）
        if (channel.fadeCoroutine != null)
        {
            StopCoroutine(channel.fadeCoroutine);
            channel.isFading = false;
        }

        // 逻辑：当前轨道有播放内容 → 先淡出再播放新BGM；无内容 → 直接播放
        if (fadeIn && source.isPlaying)
        {
            channel.fadeCoroutine = StartCoroutine(FadeOutThenPlayNewBgm(channel, bgmClip, fadeIn));
        }
        else
        {
            source.clip = bgmClip;
            source.volume = fadeIn ? 0f : bgmBaseVolume; // 淡入则初始音量0
            source.Play(); // 开始播放

            if (fadeIn) channel.fadeCoroutine = StartCoroutine(FadeInBgm(channel));
        }
    }

    /// <summary>
    /// 停止指定轨道的BGM
    /// </summary>
    /// <param name="trackName">轨道名</param>
    /// <param name="fadeOut">是否启用淡出效果</param>
    public void StopBGM(string trackName = "Main", bool fadeOut = true)
    {
        // 轨道不存在则直接返回
        if (!_bgmChannels.TryGetValue(trackName, out var channel)) return;
        var source = channel.audioSource;

        // 轨道无播放内容且未渐变 → 无需处理
        if (!source.isPlaying && !channel.isFading) return;

        // 停止当前渐变协程
        if (channel.fadeCoroutine != null) StopCoroutine(channel.fadeCoroutine);

        // 淡出停止 或 立即停止
        if (fadeOut) channel.fadeCoroutine = StartCoroutine(FadeOutBgm(channel));
        else
        {
            source.Stop();
            source.volume = 0f;
            channel.isFading = false;
        }
    }

    /// <summary>
    /// 停止所有轨道的BGM（全局静音）
    /// </summary>
    public void StopAllBGM(bool fadeOut = true)
    {
        foreach (var trackName in _bgmChannels.Keys) StopBGM(trackName, fadeOut);
    }

    /// <summary>
    /// 设置指定轨道BGM的音量（仅非渐变状态生效）
    /// </summary>
    public void SetBgmVolume(float newVolume, string trackName = "Main")
    {
        newVolume = Mathf.Clamp01(newVolume); // 关键：限制音量范围0-1，防止异常
        if (_bgmChannels.TryGetValue(trackName, out var channel) && !channel.isFading)
        {
            channel.audioSource.volume = newVolume;
        }
    }
    #endregion

    #region BGM淡入淡出协程（平滑音量渐变核心）
    /// <summary>
    /// BGM淡入协程（核心：线性插值实现平滑音量提升）
    /// </summary>
    private IEnumerator FadeInBgm(BgmChannel channel)
    {
        channel.isFading = true;
        float elapsed = 0f;          // 已流逝时间
        float startVol = channel.audioSource.volume; // 起始音量

        // 渐变逻辑：每帧更新音量，直到达到目标时长
        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            // 关键：Mathf.Lerp线性插值 → 从起始音量到基础音量平滑过渡
            channel.audioSource.volume = Mathf.Lerp(startVol, bgmBaseVolume, elapsed / bgmFadeDuration);
            yield return null; // 等待下一帧
        }

        // 确保音量最终达到目标值
        channel.audioSource.volume = bgmBaseVolume;
        channel.isFading = false;
        channel.fadeCoroutine = null;
    }

    /// <summary>
    /// BGM淡出协程（核心：线性插值实现平滑音量降低）
    /// </summary>
    private IEnumerator FadeOutBgm(BgmChannel channel)
    {
        channel.isFading = true;
        float elapsed = 0f;
        float startVol = channel.audioSource.volume;

        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            // 从当前音量平滑降到0
            channel.audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
            yield return null;
        }

        // 淡出完成后停止播放，重置音量
        channel.audioSource.Stop();
        channel.audioSource.volume = 0f;
        channel.isFading = false;
        channel.fadeCoroutine = null;
    }

    /// <summary>
    /// 先淡出当前BGM，再淡入新BGM（轨道切换核心逻辑）
    /// </summary>
    private IEnumerator FadeOutThenPlayNewBgm(BgmChannel channel, AudioClip newClip, bool fadeIn)
    {
        yield return FadeOutBgm(channel); // 等待淡出完成
        // 播放新BGM并淡入
        channel.audioSource.clip = newClip;
        channel.audioSource.volume = 0f;
        channel.audioSource.Play();
        if (fadeIn) yield return FadeInBgm(channel);
    }
    #endregion

    #region SFX对象池核心（高性能音效播放）
    /// <summary>
    /// 创建新的SFX播放组件并加入对象池（仅池未满时创建）
    /// </summary>
    private AudioSource CreateSfxAudioSource()
    {
        // 关键：池容量校验 → 防止创建过多组件导致内存溢出
        if (_sfxPool.Count + _sfxInUse.Count >= sfxPoolMaxSize) return null;

        // 创建SFX专用GameObject，挂载到管理器下
        var obj = new GameObject($"SFX_Source_{_sfxPool.Count + _sfxInUse.Count + 1}");
        obj.transform.SetParent(transform);

        var source = obj.AddComponent<AudioSource>();
        // SFX组件基础配置
        source.playOnAwake = false;
        source.loop = false;       // 音效默认不循环
        source.volume = sfxBaseVolume;
        source.spatialBlend = sfxDefaultSpatialBlend;

        _sfxPool.Add(source); // 加入空闲池
        return source;
    }

    /// <summary>
    /// 从对象池获取空闲的SFX播放组件（核心：优先复用，减少GC）
    /// </summary>
    private AudioSource GetFreeSfxSource()
    {
        // 优先复用空闲组件
        if (_sfxPool.Count > 0)
        {
            var s = _sfxPool[0];
            _sfxPool.RemoveAt(0);
            _sfxInUse.Add(s);
            return s;
        }
        // 无空闲则创建新组件（池未满时）
        return CreateSfxAudioSource();
    }

    /// <summary>
    /// 回收SFX播放组件到对象池（播放完成后调用）
    /// </summary>
    private void RecycleSfxSource(AudioSource source)
    {
        // 安全校验：组件为空/不在使用中 → 不处理
        if (source == null || !_sfxInUse.Contains(source)) return;

        source.Stop();       // 停止播放
        source.clip = null;  // 清空音频文件
        _sfxInUse.Remove(source);

        // 池未满则回收复用，否则销毁（防止内存占用）
        if (_sfxPool.Count < sfxPoolMaxSize) _sfxPool.Add(source);
        else Destroy(source.gameObject);
    }

    /// <summary>
    /// 播放音效（外部调用核心API）
    /// </summary>
    /// <param name="sfxClip">音效文件</param>
    /// <param name="pos">播放位置（3D音效生效）</param>
    /// <param name="volumeScale">音量缩放（基于基础音量）</param>
    /// <param name="spatialBlend">3D混合度（0=2D，1=3D）</param>
    public void PlaySFX(AudioClip sfxClip, Vector3 pos = default, float volumeScale = 1f, float spatialBlend = -1f)
    {
        // 安全校验：音效文件为空 → 报错返回
        if (sfxClip == null)
        {
            Debug.LogError("[SFX] 音效片段为空");
            return;
        }

        // 获取空闲SFX组件
        var source = GetFreeSfxSource();
        if (source == null) return;

        // 配置音效参数
        source.clip = sfxClip;
        source.volume = Mathf.Clamp01(sfxBaseVolume * volumeScale); // 限制音量范围
        source.transform.position = pos;
        // 3D混合度：传值则用自定义值，否则用默认值
        source.spatialBlend = spatialBlend >= 0 ? Mathf.Clamp01(spatialBlend) : sfxDefaultSpatialBlend;

        source.Play(); // 播放音效
        // 关键：音效播放完成后自动回收组件到池
        StartCoroutine(RecycleSfxAfterPlay(source, sfxClip.length));
    }

    /// <summary>
    /// 音效播放完成后回收组件（延迟回收，匹配音效时长）
    /// </summary>
    private IEnumerator RecycleSfxAfterPlay(AudioSource source, float length)
    {
        yield return new WaitForSeconds(length); // 等待音效播放完毕
        RecycleSfxSource(source);
    }

    /// <summary>
    /// 设置SFX基础音量（实时更新所有SFX组件）
    /// </summary>
    public void SetSfxVolume(float newVolume)
    {
        newVolume = Mathf.Clamp01(newVolume);
        sfxBaseVolume = newVolume;
        // 更新正在使用的SFX音量
        foreach (var s in _sfxInUse) s.volume = newVolume;
        // 更新空闲池中的SFX音量
        foreach (var s in _sfxPool) s.volume = newVolume;
    }
    #endregion

    protected override void MgrOnDispose()
    {
        base.MgrOnDispose();
        StopAllCoroutines(); // 停止所有协程

        // 释放所有BGM轨道
        foreach (var channel in _bgmChannels.Values)
        {
            if (channel.audioSource != null) Destroy(channel.audioSource.gameObject);
        }
        _bgmChannels.Clear();

        // 释放所有SFX组件
        foreach (var s in _sfxPool) Destroy(s.gameObject);
        foreach (var s in _sfxInUse) Destroy(s.gameObject);
        _sfxPool.Clear();
        _sfxInUse.Clear();
    }

    #region 资源释放（安全退出，防止内存泄漏）
    ///// <summary>
    ///// 销毁时释放所有音频资源（核心：防止内存泄漏）
    ///// </summary>
    //private void OnDestroy()
    //{
    //    StopAllCoroutines(); // 停止所有协程

    //    // 释放所有BGM轨道
    //    foreach (var channel in _bgmChannels.Values)
    //    {
    //        if (channel.audioSource != null) Destroy(channel.audioSource.gameObject);
    //    }
    //    _bgmChannels.Clear();

    //    // 释放所有SFX组件
    //    foreach (var s in _sfxPool) Destroy(s.gameObject);
    //    foreach (var s in _sfxInUse) Destroy(s.gameObject);
    //    _sfxPool.Clear();
    //    _sfxInUse.Clear();
    //}

    public override void MgrUpdate(float deltaTime){}
    #endregion
}