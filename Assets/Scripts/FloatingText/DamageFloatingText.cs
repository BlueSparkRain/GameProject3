using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 单个跳字实例。动画：弹出 → 上浮 + 挤压拉伸 → 渐隐消失。
/// 各阶段重叠衔接，避免段落感。
/// </summary>
public class DamageFloatingText : MonoBehaviour
{
    RectTransform _rect;
    CanvasGroup _canvasGroup;
    Sequence _anim;
    Action _onReturn;
    bool _returning;

    [Header("弹出")]
    [SerializeField] float _popDuration = 0.12f;
    [SerializeField] float _settleDuration = 0.08f;
    [SerializeField] float _popOvershoot = 1.35f;

    [Header("上浮")]
    [SerializeField] float _floatDistance = 120f;
    [SerializeField] float _floatDuration = 0.5f;

    [Header("淡出")]
    [SerializeField] float _fadeOutDelay = 0.25f;   // 弹出后多久开始淡出
    [SerializeField] float _fadeOutDuration = 0.35f;

    [Header("水平偏移")]
    [SerializeField] float _horizontalSpread = 40f;

    [Header("挤压拉伸")]
    [SerializeField] float _squashScaleX = 1.18f;
    [SerializeField] float _squashScaleY = 0.82f;
    [SerializeField] float _stretchScaleX = 0.88f;
    [SerializeField] float _stretchScaleY = 1.15f;
    [SerializeField] float _squashDuration = 0.08f;
    [SerializeField] float _stretchDuration = 0.1f;
    [SerializeField] float _restoreDuration = 0.1f;

    TMP_Text self_tmp;
    [Header("目标TMP")]
    public TMP_Text _tmp;

    public const float ScaleMin = 1.1f;
    public const float ScaleMax = 1.6f;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        self_tmp = GetComponent<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Play(Vector3 screenPos, string text, Color color, float fontSize,
        float scale, Vector2 offset, Action onReturn)
    {
        _onReturn = onReturn;

        _rect.position = screenPos + (Vector3)offset;
        _rect.localScale = Vector3.zero;
        _canvasGroup.alpha = 0f;

        _tmp.text = text;
        _tmp.color = color;
        _tmp.fontSize = fontSize;

        self_tmp.text = text;
        self_tmp.color = Color.black;
        self_tmp.fontSize = fontSize * 1.1f;

        gameObject.SetActive(true);

        float drift = CalcHorizontalDrift();
        float startY = _rect.anchoredPosition.y;
        float startX = _rect.anchoredPosition.x;

        _anim?.Kill();
        _anim = DOTween.Sequence();

        // ── ① 弹出：0 → overshoot（OutBack 自带回弹感）──
        _anim.Append(_canvasGroup.DOFade(1f, 0.06f));
        _anim.Join(_rect.DOScale(scale * _popOvershoot, _popDuration).SetEase(Ease.OutBack));

        // ── ② 归位 + 上浮起始（与弹出收尾重叠）──
        _anim.Append(_rect.DOScale(scale, _settleDuration).SetEase(Ease.OutCubic));
        _anim.Join(_rect.DOAnchorPosY(startY + _floatDistance, _floatDuration).SetEase(Ease.OutCubic));
        _anim.Join(_rect.DOAnchorPosX(startX + drift, _floatDuration).SetEase(Ease.OutCubic));

        // ── ③ 挤压拉伸（嵌入上浮过程，微妙有机感）──
        float squashStart = _popDuration + _settleDuration * 0.5f;
        _anim.Insert(squashStart, _rect.DOScaleX(scale * _squashScaleX, _squashDuration).SetEase(Ease.OutQuad));
        _anim.Insert(squashStart, _rect.DOScaleY(scale * _squashScaleY, _squashDuration).SetEase(Ease.OutQuad));

        float stretchStart = squashStart + _squashDuration;
        _anim.Insert(stretchStart, _rect.DOScaleX(scale * _stretchScaleX, _stretchDuration).SetEase(Ease.InOutSine));
        _anim.Insert(stretchStart, _rect.DOScaleY(scale * _stretchScaleY, _stretchDuration).SetEase(Ease.InOutSine));

        float restoreStart = stretchStart + _stretchDuration;
        _anim.Insert(restoreStart, _rect.DOScaleX(scale, _restoreDuration).SetEase(Ease.OutCubic));
        _anim.Insert(restoreStart, _rect.DOScaleY(scale, _restoreDuration).SetEase(Ease.OutCubic));

        // ── ④ 淡出（延迟后开始，与上浮尾部重叠）──
        _anim.Insert(_fadeOutDelay, _canvasGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.OutQuad));

        _anim.OnComplete(Return);
    }

    float CalcHorizontalDrift()
    {
        if (_horizontalSpread <= 0f) return 0f;
        float r = UnityEngine.Random.value;
        if (r < 0.4f)
            return -UnityEngine.Random.Range(_horizontalSpread * 0.4f, _horizontalSpread);
        if (r < 0.8f)
            return UnityEngine.Random.Range(_horizontalSpread * 0.4f, _horizontalSpread);
        return UnityEngine.Random.Range(-_horizontalSpread * 0.5f, _horizontalSpread * 0.5f);
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
