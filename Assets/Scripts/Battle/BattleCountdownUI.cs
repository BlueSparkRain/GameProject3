using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BattleCountdownUI : MonoBehaviour
{
    [Header("倒计时文本")]
    [SerializeField] TMP_Text _countdownText;
    [SerializeField] string _goText = "开始!";

    [Header("淡入淡出")]
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] float _fadeDuration = 0.3f;

    [Header("数字缩放动画")]
    [SerializeField] float _scaleInDuration = 0.25f;
    [SerializeField] float _scaleHoldDuration = 0.35f;
    [SerializeField] float _scaleOutDuration = 0.2f;

    BattlePhaseManager _phaseManager;

    void Start()
    {
        _phaseManager = GameRoot.GetManager<BattlePhaseManager>();
        if (_phaseManager == null)
        {
            DebugManager.LogWarning(EDebugCategory.BattleState, "[BattleCountdownUI] BattlePhaseManager 未就绪");
            return;
        }

        _phaseManager.OnPhaseChanged += OnPhaseChanged;
        _phaseManager.OnCountdownTick += OnCountdownTick;
        _phaseManager.OnCountdownEnd += OnCountdownEnd;

        // 初始状态：隐藏，alpha 为 0
        if (_countdownText != null)
            _countdownText.gameObject.SetActive(false);
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
    }

    void OnDestroy()
    {
        if (_phaseManager != null)
        {
            _phaseManager.OnPhaseChanged -= OnPhaseChanged;
            _phaseManager.OnCountdownTick -= OnCountdownTick;
            _phaseManager.OnCountdownEnd -= OnCountdownEnd;
        }
    }

    void OnPhaseChanged(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Countdown:
                // 倒计时开始 → 整体淡入
                if (_canvasGroup != null)
                    _canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true);
                break;

            case BattlePhase.InProgress:
                // 淡出由 OnCountdownEnd 的动画回调接管，此处不重复处理
                break;
        }
    }

    void OnCountdownTick(int remaining)
    {
        if (_countdownText == null) return;

        // 每次数字更新：从 0 放大回弹 → 保持 → 缩回 0
        AnimateNumber(remaining > 0 ? remaining.ToString() : _goText, false);
    }

    void OnCountdownEnd()
    {
        if (_countdownText == null) return;

        // 最后的"开始!"文字：动画更大，结束后整个面板淡出
        AnimateNumber(_goText, true);
    }

    void AnimateNumber(string text, bool isLast)
    {
        // 终止上一次动画，重置到初始状态
        _countdownText.transform.DOKill();
        _countdownText.gameObject.SetActive(true);
        _countdownText.text = text;
        _countdownText.transform.localScale = Vector3.zero;
        _countdownText.alpha = 1f;

        float overshoot = isLast ? 1.5f : 1.2f;
        float blowUpScale = isLast ? 2.5f : 1.8f;

        var seq = DOTween.Sequence().SetUpdate(true);
        // ① 0 → overshoot（回弹出现）
        seq.Append(_countdownText.transform
            .DOScale(overshoot, _scaleInDuration).SetEase(Ease.OutBack));
        // ② overshoot → 1.0（归位）
        seq.Append(_countdownText.transform
            .DOScale(1f, 0.1f));
        // ③ 停在 1.0，让玩家看清
        seq.AppendInterval(_scaleHoldDuration);
        // ④ 放大 + 渐隐（并行：scale 1.0→blowUp，alpha 1→0）
        seq.Append(_countdownText.transform
            .DOScale(blowUpScale, _scaleOutDuration).SetEase(Ease.OutCubic));
        seq.Join(_countdownText
            .DOFade(0f, _scaleOutDuration).SetEase(Ease.InCubic));

        if (isLast)
        {
            seq.OnComplete(() =>
            {
                // "开始!" 消散后，整个 CanvasGroup 淡出
                if (_canvasGroup != null)
                    _canvasGroup.DOFade(0f, _fadeDuration).SetUpdate(true)
                        .OnComplete(() => _countdownText.gameObject.SetActive(false));
            });
        }
    }
}
