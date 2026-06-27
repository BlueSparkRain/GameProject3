using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 单个跳字实例。动画：弹出 → 上浮 + 随机偏移 → 渐隐消失。
/// 所有偏移和随机化参数由脚本内部控制，不暴露给 Inspector。
/// 同时生成多个跳字时，通过 staggerOffset / staggerIndex 防止位置和动画重叠。
/// </summary>
public class DamageFloatingText : MonoBehaviour
{
    RectTransform _rect;
    CanvasGroup _canvasGroup;
    Sequence _anim;
    Action _onReturn;
    bool _returning;

    TMP_Text self_tmp;
    public TMP_Text _tmp;

    // ── 内部动画参数（不暴露 Inspector）──
    const float PopDuration       = 0.12f;
    const float SettleDuration    = 0.08f;
    const float PopOvershoot      = 1.35f;

    const float FloatDistanceMin  = 90f;
    const float FloatDistanceMax  = 140f;
    const float FloatDurationMin  = 0.45f;
    const float FloatDurationMax  = 0.65f;

    const float FadeOutDelayMin   = 0.18f;
    const float FadeOutDelayMax   = 0.32f;
    const float FadeOutDuration   = 0.35f;

    // 水平随机漂移
    const float DriftMin = 25f;
    const float DriftMax = 55f;

    // 挤压拉伸
    const float SquashScaleX   = 1.18f;
    const float SquashScaleY   = 0.82f;
    const float StretchScaleX  = 0.88f;
    const float StretchScaleY  = 1.15f;
    const float SquashDuration = 0.08f;
    const float StretchDuration = 0.1f;
    const float RestoreDuration = 0.1f;

    // 字号随机
    public const float ScaleMin = 0.95f;
    public const float ScaleMax = 1.35f;

    // ── 交错延迟：防止同时跳字动画完全同步 ──
    const float StaggerAnimDelay = 0.04f; // 每个后续跳字额外延迟

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        self_tmp = GetComponent<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// 播放跳字动画。
    /// </summary>
    /// <param name="screenPos">屏幕坐标基准点</param>
    /// <param name="text">显示文字</param>
    /// <param name="color">颜色</param>
    /// <param name="fontSize">字号</param>
    /// <param name="staggerOffset">由 Spawner 计算的交错偏移（屏幕坐标）</param>
    /// <param name="staggerIndex">交错序号，用于微调动画 timing</param>
    /// <param name="onReturn">回收回调</param>
    public void Play(Vector3 screenPos, string text, Color color, float fontSize,
        Vector2 staggerOffset, int staggerIndex, Action onReturn)
    {
        _onReturn = onReturn;

        // 起始位置 = 基准点 + 交错偏移
        _rect.position = screenPos + (Vector3)staggerOffset;
        _rect.localScale = Vector3.zero;
        _canvasGroup.alpha = 0f;

        _tmp.text = text;
        _tmp.color = color;
        _tmp.fontSize = fontSize;

        self_tmp.text = text;
        self_tmp.color = Color.black;
        self_tmp.fontSize = fontSize * 1.1f;

        gameObject.SetActive(true);

        // ── 每实例随机化动画参数，防止同时跳字完全同步 ──
        float floatDist  = UnityEngine.Random.Range(FloatDistanceMin, FloatDistanceMax);
        float floatDur   = UnityEngine.Random.Range(FloatDurationMin, FloatDurationMax);
        float fadeDelay  = UnityEngine.Random.Range(FadeOutDelayMin, FadeOutDelayMax);
        float drift      = CalcHorizontalDrift(staggerIndex);
        float scale      = UnityEngine.Random.Range(ScaleMin, ScaleMax);

        float startY = _rect.anchoredPosition.y;
        float startX = _rect.anchoredPosition.x;

        // 交错延迟：同一帧的后续跳字稍晚启动
        float staggerDelay = staggerIndex * StaggerAnimDelay;

        _anim?.Kill();
        _anim = DOTween.Sequence();

        // 交错启动延迟
        if (staggerDelay > 0f)
            _anim.AppendInterval(staggerDelay);

        // ── ① 弹出：0 → overshoot ──
        _anim.Append(_canvasGroup.DOFade(1f, 0.06f));
        _anim.Join(_rect.DOScale(scale * PopOvershoot, PopDuration).SetEase(Ease.OutBack));

        // ── ② 归位 + 上浮（与弹出收尾重叠）──
        _anim.Append(_rect.DOScale(scale, SettleDuration).SetEase(Ease.OutCubic));
        _anim.Join(_rect.DOAnchorPosY(startY + floatDist, floatDur).SetEase(Ease.OutCubic));
        _anim.Join(_rect.DOAnchorPosX(startX + drift, floatDur).SetEase(Ease.OutCubic));

        // ── ③ 挤压拉伸 ──
        float squashStart = PopDuration + SettleDuration * 0.5f + staggerDelay;
        _anim.Insert(squashStart, _rect.DOScaleX(scale * SquashScaleX, SquashDuration).SetEase(Ease.OutQuad));
        _anim.Insert(squashStart, _rect.DOScaleY(scale * SquashScaleY, SquashDuration).SetEase(Ease.OutQuad));

        float stretchStart = squashStart + SquashDuration;
        _anim.Insert(stretchStart, _rect.DOScaleX(scale * StretchScaleX, StretchDuration).SetEase(Ease.InOutSine));
        _anim.Insert(stretchStart, _rect.DOScaleY(scale * StretchScaleY, StretchDuration).SetEase(Ease.InOutSine));

        float restoreStart = stretchStart + StretchDuration;
        _anim.Insert(restoreStart, _rect.DOScaleX(scale, RestoreDuration).SetEase(Ease.OutCubic));
        _anim.Insert(restoreStart, _rect.DOScaleY(scale, RestoreDuration).SetEase(Ease.OutCubic));

        // ── ④ 淡出 ──
        _anim.Insert(fadeDelay + staggerDelay, _canvasGroup.DOFade(0f, FadeOutDuration).SetEase(Ease.OutQuad));

        _anim.OnComplete(Return);
    }

    /// <summary>基于 stagger 序号计算水平漂移方向，相邻序号走向相反方向。</summary>
    float CalcHorizontalDrift(int staggerIndex)
    {
        float mag = UnityEngine.Random.Range(DriftMin, DriftMax);
        // 偶数序号偏右，奇数序号偏左，避免重叠
        bool goRight = (staggerIndex % 2 == 0);
        // 加入少量随机避免机械感
        float jitter = UnityEngine.Random.Range(-mag * 0.2f, mag * 0.2f);
        return goRight ? (mag + jitter) : -(mag + jitter);
    }

    void Return()
    {
        _returning = true;
        _anim?.Kill();
        _anim = null;
        _onReturn?.Invoke();
        _onReturn = null;
        gameObject.SetActive(false);
        _returning = false;
    }

    void OnDisable()
    {
        if (_returning) return;
        _anim?.Kill();
        _anim = null;
        if (transform.parent != null)
        {
            var ps = transform.parent.gameObject.scene;
            if (!ps.isLoaded || !transform.parent.gameObject.activeSelf)
            {
                _onReturn = null;
                return;
            }
        }
        _onReturn?.Invoke();
        _onReturn = null;
    }

    void OnDestroy()
    {
        _anim?.Kill();
    }
}
