using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频类型枚举（区分背景音乐/音效）
/// </summary>
public enum AudioType
{
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
/// 核心能力：
/// 1. 多BGM轨道共存+独立缓入缓出
/// 2. SFX对象池高性能播放
/// 3. AudioClip路径缓存（对象池），避免重复加载
/// 4. 支持字符串路径播放BGM/SFX
/// 5. 全流程安全校验
/// </summary>
public class AudioManager : MonoGlobalManager
{
    protected override void Awake()
    {
        base.Awake();
        InitAudioComponents();
    }


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

    [Header("AudioClip缓存配置")]
    public bool enableClipCache = true; // 是否启用AudioClip缓存（建议开启）
    public int maxCacheCount = 50;      // 最大缓存Clip数量（防止内存过载）
    #endregion

    #region 核心私有变量（音频状态管理）
    // 多BGM轨道字典 → Key=轨道名（如Main/Ambient），Value=轨道实例
    private Dictionary<string, BgmChannel> _bgmChannels;

    // SFX对象池核心 → 复用AudioSource，避免频繁创建/销毁导致GC卡顿
    private List<AudioSource> _sfxPool;       // 空闲的SFX播放组件池
    private List<AudioSource> _sfxInUse;      // 正在使用的SFX播放组件

    // AudioClip对象池（缓存）→ Key=资源路径，Value=AudioClip（避免重复加载）
    private Dictionary<string, AudioClip> _audioClipCache;
    // 缓存的访问顺序（用于LRU清理策略）
    private List<string> _clipAccessOrder;
    #endregion

    #region 初始化核心方法（仅在启动时执行）
    /// <summary>
    /// 初始化BGM轨道、SFX对象池、AudioClip缓存（核心初始化，仅执行一次）
    /// </summary>
    void InitAudioComponents()
    {
        // 初始化BGM轨道字典
        _bgmChannels = new Dictionary<string, BgmChannel>();

        // 初始化SFX对象池（预创建指定数量的SFX播放组件）
        _sfxPool = new List<AudioSource>();
        _sfxInUse = new List<AudioSource>();
        for (int i = 0; i < sfxPoolInitSize; i++)
            CreateSfxAudioSource();

        // 初始化AudioClip缓存池
        if (enableClipCache)
        {
            _audioClipCache = new Dictionary<string, AudioClip>();
            _clipAccessOrder = new List<string>();
        }
    }
    #endregion

    #region AudioClip缓存核心方法（对象池）
    /// <summary>
    /// 从Resources加载AudioClip并缓存（核心：路径加载+缓存复用）
    /// </summary>
    /// <param name="path">Resources下的音频路径（无需扩展名）</param>
    /// <returns>加载的AudioClip（失败返回null）</returns>
    public AudioClip LoadAudioClip(string path)
    {
        // 空路径校验
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[AudioManager] 音频路径为空，无法加载");
            return null;
        }

        // 缓存启用时，优先从缓存获取
        if (enableClipCache && _audioClipCache.TryGetValue(path, out AudioClip cachedClip))
        {
            // 更新访问顺序（LRU策略）
            UpdateClipAccessOrder(path);
            return cachedClip;
        }

        // 从Resources加载（需确保音频文件放在Resources目录下）
        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debug.LogError($"[AudioManager] 路径{path}未找到AudioClip，请检查路径是否正确（无需扩展名）");
            return null;
        }

        // 缓存启用时，将加载的Clip加入缓存
        if (enableClipCache)
        {
            // 缓存满时，清理最久未使用的Clip
            if (_audioClipCache.Count >= maxCacheCount)
                CleanupLeastUsedClip();

            _audioClipCache.Add(path, clip);
            _clipAccessOrder.Add(path);
        }

        return clip;
    }

    /// <summary>
    /// 更新Clip访问顺序（LRU策略：最近访问的放最后）
    /// </summary>
    private void UpdateClipAccessOrder(string path)
    {
        if (_clipAccessOrder.Contains(path))
        {
            _clipAccessOrder.Remove(path);
        }
        _clipAccessOrder.Add(path);
    }

    /// <summary>
    /// 清理最久未使用的AudioClip（防止缓存溢出）
    /// </summary>
    private void CleanupLeastUsedClip()
    {
        if (_clipAccessOrder.Count == 0) return;

        string leastUsedPath = _clipAccessOrder[0];
        _audioClipCache.Remove(leastUsedPath);
        _clipAccessOrder.RemoveAt(0);

        // 可选：卸载未使用的音频资源（减少内存占用）
        Resources.UnloadUnusedAssets();
        Debug.Log($"[AudioManager] 清理缓存：{leastUsedPath}（缓存数量已达上限）");
    }

    /// <summary>
    /// 手动清理指定路径的Clip缓存
    /// </summary>
    /// <param name="path">要清理的音频路径</param>
    public void ClearClipCache(string path)
    {
        if (enableClipCache && _audioClipCache.ContainsKey(path))
        {
            _audioClipCache.Remove(path);
            _clipAccessOrder.Remove(path);
            Debug.Log($"[AudioManager] 清理指定缓存：{path}");
        }
    }

    /// <summary>
    /// 清空所有AudioClip缓存（切换场景时建议调用）
    /// </summary>
    public void ClearAllClipCache()
    {
        if (enableClipCache)
        {
            _audioClipCache.Clear();
            _clipAccessOrder.Clear();
            Resources.UnloadUnusedAssets();
            Debug.Log("[AudioManager] 清空所有AudioClip缓存");
        }
    }
    #endregion

    #region BGM轨道核心方法（多轨道共存+路径播放）
    /// <summary>
    /// 获取/创建指定名称的BGM轨道（核心：轨道不存在则自动创建）
    /// </summary>
    /// <param name="trackName">轨道名称（如Main/Ambient）</param>
    BgmChannel GetOrCreateBgmChannel(string trackName)
    {
        if (_bgmChannels.TryGetValue(trackName, out var channel))
            return channel;

        // 创建新轨道的GameObject和AudioSource
        var channelObj = new GameObject($"BGM_Track_{trackName}");
        channelObj.transform.SetParent(transform);

        var source = channelObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = bgmLoop;
        source.volume = 0f;
        source.spatialBlend = 0f; // BGM默认2D

        // 封装轨道状态并加入字典
        channel = new BgmChannel
        {
            trackName = trackName,
            audioSource = source,
            isFading = false,
            fadeCoroutine = null
        };
        _bgmChannels.Add(trackName, channel);
        return channel;
    }

    /// <summary>
    /// 【重载】通过路径播放指定轨道的BGM（外部核心调用接口）
    /// </summary>
    /// <param name="bgmPath">Resources下的BGM路径（无需扩展名）</param>
    /// <param name="trackName">轨道名（不同名则共存）</param>
    /// <param name="fadeIn">是否启用淡入效果</param>
    /// <param name="forceRestart">同一轨道是否强制重启</param>
    public void PlayBGM(string bgmPath, string trackName = "Main", bool fadeIn = true, bool forceRestart = false)
    {
        // 加载音频Clip
        AudioClip bgmClip = LoadAudioClip(bgmPath);
        if (bgmClip == null) return;

        // 调用原始PlayBGM方法
        PlayBGM(bgmClip, trackName, fadeIn, forceRestart);
    }

    /// <summary>
    /// 播放指定轨道的BGM（核心逻辑）
    /// </summary>
    /// <param name="bgmClip">要播放的BGM音频文件</param>
    /// <param name="trackName">轨道名</param>
    /// <param name="fadeIn">是否淡入</param>
    /// <param name="forceRestart">是否强制重启</param>
    public void PlayBGM(AudioClip bgmClip, string trackName = "Main", bool fadeIn = true, bool forceRestart = false)
    {
        if (bgmClip == null)
        {
            Debug.LogError($"[BGM] 轨道{trackName}音频片段为空，无法播放");
            return;
        }

        var channel = GetOrCreateBgmChannel(trackName);
        var source = channel.audioSource;

        // 同一轨道+同一BGM+正在播放 → 不重复播放（除非强制重启）
        if (source.clip == bgmClip && source.isPlaying && !forceRestart)
            return;

        // 停止当前渐变协程
        if (channel.fadeCoroutine != null)
        {
            StopCoroutine(channel.fadeCoroutine);
            channel.isFading = false;
        }

        // 有播放内容则先淡出再播放，无则直接播放
        if (fadeIn && source.isPlaying)
        {
            channel.fadeCoroutine = StartCoroutine(FadeOutThenPlayNewBgm(channel, bgmClip, fadeIn));
        }
        else
        {
            source.clip = bgmClip;
            source.volume = fadeIn ? 0f : bgmBaseVolume;
            source.Play();

            if (fadeIn)
                channel.fadeCoroutine = StartCoroutine(FadeInBgm(channel));
        }
    }

    /// <summary>
    /// 停止指定轨道的BGM
    /// </summary>
    /// <param name="trackName">轨道名</param>
    /// <param name="fadeOut">是否淡出</param>
    public void StopBGM(string trackName = "Main", bool fadeOut = true)
    {
        if (!_bgmChannels.TryGetValue(trackName, out var channel))
            return;

        var source = channel.audioSource;
        if (!source.isPlaying && !channel.isFading)
            return;

        if (channel.fadeCoroutine != null)
            StopCoroutine(channel.fadeCoroutine);

        if (fadeOut)
            channel.fadeCoroutine = StartCoroutine(FadeOutBgm(channel));
        else
        {
            source.Stop();
            source.volume = 0f;
            channel.isFading = false;
        }
    }

    /// <summary>
    /// 停止所有轨道的BGM
    /// </summary>
    public void StopAllBGM(bool fadeOut = true)
    {
        foreach (var trackName in _bgmChannels.Keys)
            StopBGM(trackName, fadeOut);
    }

    /// <summary>
    /// 设置指定轨道BGM的音量（仅非渐变状态生效）
    /// </summary>
    public void SetBgmVolume(float newVolume, string trackName = "Main")
    {
        newVolume = Mathf.Clamp01(newVolume);
        if (_bgmChannels.TryGetValue(trackName, out var channel) && !channel.isFading)
        {
            channel.audioSource.volume = newVolume;
        }
    }
    #endregion

    #region BGM淡入淡出协程
    /// <summary>
    /// BGM淡入协程（线性插值实现平滑音量提升）
    /// </summary>
    private IEnumerator FadeInBgm(BgmChannel channel)
    {
        channel.isFading = true;
        float elapsed = 0f;
        float startVol = channel.audioSource.volume;

        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            channel.audioSource.volume = Mathf.Lerp(startVol, bgmBaseVolume, elapsed / bgmFadeDuration);
            yield return null;
        }

        channel.audioSource.volume = bgmBaseVolume;
        channel.isFading = false;
        channel.fadeCoroutine = null;
    }

    /// <summary>
    /// BGM淡出协程（线性插值实现平滑音量降低）
    /// </summary>
    private IEnumerator FadeOutBgm(BgmChannel channel)
    {
        channel.isFading = true;
        float elapsed = 0f;
        float startVol = channel.audioSource.volume;

        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            channel.audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
            yield return null;
        }

        channel.audioSource.Stop();
        channel.audioSource.volume = 0f;
        channel.isFading = false;
        channel.fadeCoroutine = null;
    }

    /// <summary>
    /// 先淡出当前BGM，再淡入新BGM
    /// </summary>
    private IEnumerator FadeOutThenPlayNewBgm(BgmChannel channel, AudioClip newClip, bool fadeIn)
    {
        yield return FadeOutBgm(channel);
        channel.audioSource.clip = newClip;
        channel.audioSource.volume = 0f;
        channel.audioSource.Play();
        if (fadeIn)
            yield return FadeInBgm(channel);
    }
    #endregion

    #region SFX对象池核心（路径播放+高性能）
    /// <summary>
    /// 创建新的SFX播放组件并加入对象池
    /// </summary>
    private AudioSource CreateSfxAudioSource()
    {
        if (_sfxPool.Count + _sfxInUse.Count >= sfxPoolMaxSize)
            return null;

        var obj = new GameObject($"SFX_Source_{_sfxPool.Count + _sfxInUse.Count + 1}");
        obj.transform.SetParent(transform);

        var source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.volume = sfxBaseVolume;
        source.spatialBlend = sfxDefaultSpatialBlend;

        _sfxPool.Add(source);
        return source;
    }

    /// <summary>
    /// 从对象池获取空闲的SFX播放组件
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
    /// 回收SFX播放组件到对象池
    /// </summary>
    private void RecycleSfxSource(AudioSource source)
    {
        if (source == null || !_sfxInUse.Contains(source))
            return;

        source.Stop();
        source.clip = null;
        _sfxInUse.Remove(source);

        if (_sfxPool.Count < sfxPoolMaxSize)
            _sfxPool.Add(source);
        else
            Destroy(source.gameObject);
    }

    /// <summary>
    /// 【重载】通过路径播放音效（外部核心调用接口）
    /// </summary>
    /// <param name="sfxPath">Resources下的音效路径（无需扩展名）</param>
    /// <param name="pos">播放位置（3D音效生效）</param>
    /// <param name="volumeScale">音量缩放</param>
    /// <param name="spatialBlend">3D混合度</param>
    public void PlaySFX(string sfxPath, Vector3 pos = default, float volumeScale = 1f, float spatialBlend = -1f)
    {
        // 加载音频Clip
        AudioClip sfxClip = LoadAudioClip(sfxPath);
        if (sfxClip == null) return;

        // 调用原始PlaySFX方法
        PlaySFX(sfxClip, pos, volumeScale, spatialBlend);
    }

    /// <summary>
    /// 播放音效（核心逻辑）
    /// </summary>
    public void PlaySFX(AudioClip sfxClip, Vector3 pos = default, float volumeScale = 1f, float spatialBlend = -1f)
    {
        if (sfxClip == null)
        {
            Debug.LogError("[SFX] 音效片段为空");
            return;
        }

        var source = GetFreeSfxSource();
        if (source == null)
        {
            Debug.LogWarning("[SFX] 对象池已满，无法播放音效");
            return;
        }

        // 配置音效参数
        source.clip = sfxClip;
        source.volume = Mathf.Clamp01(sfxBaseVolume * volumeScale);
        source.transform.position = pos;
        source.spatialBlend = spatialBlend >= 0 ? Mathf.Clamp01(spatialBlend) : sfxDefaultSpatialBlend;

        source.Play();
        // 播放完成后自动回收
        StartCoroutine(RecycleSfxAfterPlay(source, sfxClip.length));
    }

    /// <summary>
    /// 音效播放完成后回收组件
    /// </summary>
    private IEnumerator RecycleSfxAfterPlay(AudioSource source, float length)
    {
        yield return new WaitForSeconds(length);
        RecycleSfxSource(source);
    }

    /// <summary>
    /// 设置SFX基础音量（实时更新所有SFX组件）
    /// </summary>
    public void SetSfxVolume(float newVolume)
    {
        newVolume = Mathf.Clamp01(newVolume);
        sfxBaseVolume = newVolume;

        foreach (var s in _sfxInUse)
            s.volume = newVolume;
        foreach (var s in _sfxPool)
            s.volume = newVolume;
    }
    #endregion

    #region 资源释放（防止内存泄漏）
    private void OnDestroy()
    {
        StopAllCoroutines();

        // 释放BGM轨道
        foreach (var channel in _bgmChannels.Values)
        {
            if (channel.audioSource != null)
                Destroy(channel.audioSource.gameObject);
        }
        _bgmChannels.Clear();

        // 释放SFX组件
        foreach (var s in _sfxPool)
            Destroy(s.gameObject);
        foreach (var s in _sfxInUse)
            Destroy(s.gameObject);
        _sfxPool.Clear();
        _sfxInUse.Clear();

        // 清空Clip缓存
        ClearAllClipCache();
    }
    #endregion
    public override void MgrUpdate(float deltaTime)
    { 
    }
}