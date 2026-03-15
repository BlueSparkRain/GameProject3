using DG.Tweening;
using UnityEngine;
/// <summary>
/// 对Dotween函数的二次封装
/// </summary>
public static class MagicAnimExtens 
{
    #region Transform动画（3D/2D物体）
    // 世界坐标移动
    public static Tween CreateMoveTween(this Transform target, Vector3 endPos, float duration)
        => target.DOMove(endPos, duration);

    // 本地坐标移动
    public static Tween CreateLocalMoveTween(this Transform target, Vector3 endPos, float duration)
        => target.DOLocalMove(endPos, duration);

    // 缩放
    public static Tween CreateScaleTween(this Transform target, Vector3 endScale, float duration)
        => target.DOScale(endScale, duration);

    // 旋转
    public static Tween CreateRotateTween(this Transform target, Vector3 endEuler, float duration, RotateMode mode = RotateMode.Fast)
        => target.DORotate(endEuler, duration, mode);
    #endregion

    #region UI动画
    // CanvasGroup透明度
    public static Tween CreateFadeTween(this CanvasGroup target, float endAlpha, float duration)
        => target.DOFade(endAlpha, duration);

    // RectTransform锚点移动
    public static Tween CreateAnchorPosTween(this RectTransform target, Vector2 endPos, float duration)
        => target.DOAnchorPos(endPos, duration);

    // RectTransform缩放
    public static Tween CreateScaleTween(this RectTransform target, Vector2 endScale, float duration)
        => target.DOScale(endScale, duration);
    #endregion

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
}
