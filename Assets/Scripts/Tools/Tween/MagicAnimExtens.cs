using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 对Dotween函数的二次封装
/// </summary>
public static class MagicAnimExtens
{
    #region Sequence序列构建
    // 创建空序列
    public static Sequence CreateEmptySequence() => DOTween.Sequence();

    // 序列添加移动段
    public static Sequence AddMoveSegment(this Sequence seq, Transform target, Vector3 endPos, float duration, Ease ease = Ease.Linear)
    {
        seq.Append(target.DOMove(endPos, duration).SetEase(ease));
        return seq;
    }

    // 序列添加延迟段
    public static Sequence AddDelaySegment(this Sequence seq, float delay)
    {
        seq.AppendInterval(delay);
        return seq;
    }

    // 序列添加并行段（同时播放）
    public static Sequence AddParallelSegment(this Sequence seq, Tween tween)
    {
        seq.Join(tween);
        return seq;
    }

    // 序列添加缩放段
    public static Sequence AddScaleSegment(this Sequence seq, Transform target, Vector3 endScale, float duration, Ease ease = Ease.Linear)
    {
        seq.Append(target.DOScale(endScale, duration).SetEase(ease));
        return seq;
    }
    #endregion


    /// <summary>
    /// 重置Tectransform
    /// </summary>
    public static void ResetRecTransPos(RectTransform _rectTransform, Vector3 _bornPos)
    {
        if (_rectTransform == null) return;
        Vector3 pos = _rectTransform.localPosition;
        pos = _bornPos;
        _rectTransform.localPosition = pos;
    }

    static MagicAnimationManager _animManager = null;

    public static void DoLocal_UIAnim(RectTransform _rectTransform, float _animDuration, Ease _easeType,
        Vector3 _startPos, Vector3 _targetTrans, bool _doFadeIn, bool _needAlphaFadeInOut = false)
    {
        if (_rectTransform == null) return;

        DOTween.Kill(_rectTransform.GetInstanceID(), true);
        _rectTransform.DOKill();
        CanvasGroup img = _rectTransform.GetComponent<CanvasGroup>();

        Vector2 currentAnchoredPos = _rectTransform.anchoredPosition;

        Vector2 finalTargetPos = _doFadeIn
            ? (Vector2)_startPos + (Vector2)_targetTrans
            : (Vector2)_startPos - (Vector2)_targetTrans;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

        _rectTransform.anchoredPosition = currentAnchoredPos;

        Tweener tween = _rectTransform.DOAnchorPos(finalTargetPos, _animDuration)
            .SetEase(_easeType)
            .SetUpdate(true)
            .SetAutoKill(true)
            .OnComplete(() =>
            {
                // 🚀 绝杀：动画结束 硬赋值 锁定位置！覆盖DOTween和UI系统所有计算
                _rectTransform.anchoredPosition = finalTargetPos;
            });

        if (img != null && _needAlphaFadeInOut)
        {
            float alphaTarget = _doFadeIn ? 1 : 0;
            img.DOFade(alphaTarget, _animDuration * 0.4f).From(img.alpha);
        }
    }





    /// <summary>
    /// 修复：多实例独立动画，互不打断，每个协程都能执行完成
    /// </summary>
    static IEnumerator PlayLocal_UIAnim(RectTransform _rectTransform, float _animDuration, Ease _easeType, Vector3 _startPos, Vector3 _targetTrans,
             bool _doFadeIn, bool _needAlphaFadeInOut)
    {
        // 先清空当前物体的所有旧动画
        _rectTransform.DOKill(true);
        Image img = _rectTransform.GetComponent<Image>();
        if (img != null) img.DOKill(true);

        var ui_animParams = new AnimParams
        {
            Duration = _animDuration,
            Ease = _easeType,
            LoopMode = AnimationLoopType.None,
            Interruptible = false,
            TargetType = AnimationTargetType.UI,
            SpaceMode = AnimationSpaceMode.Local
        };

        // 生成【唯一动画ID】（基于当前物体，每个面板独立）
        string uniqueAnimID = MagicAnimationManager.GetAnimID(E_TweenType.Image_UpMove) + _rectTransform.GetInstanceID();

        yield return _animManager.PlayAnimation(
            uniqueAnimID,
            _rectTransform,
            (p) =>
            {
                float targetAlpha = 1;
                if (_needAlphaFadeInOut)
                    targetAlpha = _doFadeIn ? 1 : 0;

                var moveTween = _rectTransform.DOLocalMove(_startPos + _targetTrans * (_doFadeIn ? 1 : -1), p.Duration)
                    .SetEase(p.Ease)
                    .SetId(_rectTransform);

                Tweener fadeTween = null;
                if (img != null)
                {
                    fadeTween = img.DOFade(targetAlpha, p.Duration * 1.5f)
                        .SetEase(p.Ease)
                        .SetId(_rectTransform);
                }

                // 序列：默认自动销毁（删除SetAutoKill(false)）? 修复3
                var seq = DOTween.Sequence();
                seq.Append(moveTween);
                if (fadeTween != null) seq.Join(fadeTween);

                return seq;
            },
            ui_animParams
        );
    }


}