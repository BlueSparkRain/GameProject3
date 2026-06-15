using Core;
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

    public static void PerfectJump_WorldAnim(Transform charcaterTrans, Vector3 targetPos)
    {
        // 清空旧动画，防止卡顿重叠
        charcaterTrans.DOKill();
        float totalDuration = 0.2f;   // 总时长（0.4~0.6最丝滑）
        float jumpPower = 0.8f;      // 弹跳高度
                                     // 挤压/拉伸幅度（数值越小越柔和，越大越Q弹）
        float squeezeXZ = 1.15f;     // 起跳/落地 XZ挤压
        float squeezeY = 0.8f;      // 起跳/落地 Y压缩
        float stretchXZ = 0.9f;      // 空中 XZ拉伸
        float stretchY = 1.2f;      // 空中 Y拉长

        // 创建序列动画
        Sequence seq = DOTween.Sequence();

        // 1. 抛物线跳跃（基础位移，丝滑曲线）
        seq.Join(charcaterTrans.DOJump(targetPos, jumpPower, 1, totalDuration)
            .SetEase(Ease.InOutSine)); // 【关键】最丝滑的正弦曲线，抛弃Flash

        // ====================== 无缝缩放动画（完美同步跳跃）======================
        // 阶段1：起跳快速挤压 (0 ~ 20% 总时长)
        seq.Insert(0, charcaterTrans.DOScale(
            new Vector3(squeezeXZ, squeezeY, squeezeXZ),
            totalDuration * 0.3f
        ).SetEase(Ease.OutSine));

        // 阶段2：腾空缓慢拉伸 (20% ~ 50% 总时长)
        seq.Insert(totalDuration * 0.2f, charcaterTrans.DOScale(
            new Vector3(stretchXZ, stretchY, stretchXZ),
            totalDuration * 0.3f
        ).SetEase(Ease.InOutSine));

        // 阶段3：落地前挤压 (50% ~ 85% 总时长)
        seq.Insert(totalDuration * 0.5f, charcaterTrans.DOScale(
            new Vector3(squeezeXZ, squeezeY, squeezeXZ),
            totalDuration * 0.4f
        ).SetEase(Ease.InOutSine));

        GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/OneMove",default,0.5f,1.3f);
        // 阶段4：落地回弹复原 (85% ~ 100% 总时长)
        seq.Insert(totalDuration * 0.85f, charcaterTrans.DOScale(
            Vector3.one,
            totalDuration * 0.3f
        ).SetEase(Ease.OutSine));

        // 播放
        seq.Play();
    }

    /// <summary>
    /// 战败踢飞动画变种：跳跃 + 360° 翻滚 + 三轴缩放
    /// 翻滚180°顶点: X=0.4 Y=0.2 Z=0.4（压扁感）
    /// </summary>
    public static void RollingKick_WorldAnim(Transform charcaterTrans, Vector3 targetPos)
    {
        charcaterTrans.DOKill();
        Vector3 originalEuler = charcaterTrans.eulerAngles;
        float totalDur = 0.25f;
        float halfDur = totalDur * 0.5f;

        Sequence seq = DOTween.Sequence();

        seq.Join(charcaterTrans.DOJump(targetPos, 0.6f, 1, totalDur).SetEase(Ease.InOutSine));
        // 转 2 圈 (720°)
        seq.Join(charcaterTrans
            .DORotate(new Vector3(originalEuler.x, originalEuler.y + 360f, originalEuler.z),
                totalDur, RotateMode.FastBeyond360).SetEase(Ease.InOutSine));

        // XZ: 1 → 0.4 → 0.8 → 1
        seq.Insert(0,       charcaterTrans.DOScaleX(0.4f, halfDur).SetEase(Ease.InSine));
        seq.Insert(0,       charcaterTrans.DOScaleZ(0.4f, halfDur).SetEase(Ease.InSine));
        seq.Insert(halfDur, charcaterTrans.DOScaleX(0.8f, halfDur).SetEase(Ease.InOutSine));
        seq.Insert(halfDur, charcaterTrans.DOScaleZ(0.8f, halfDur).SetEase(Ease.InOutSine));

        // Y:  1 → 0.4 → 0.4 → 1（顶点更扁）
        seq.Insert(0,       charcaterTrans.DOScaleY(0.4f, halfDur).SetEase(Ease.InSine));

        // 统一回弹
        seq.Append(charcaterTrans.DOScale(1f, 0.08f).SetEase(Ease.OutBack));
        seq.Append(charcaterTrans.DORotate(originalEuler, 0.06f).SetEase(Ease.OutSine));

        seq.Play();
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