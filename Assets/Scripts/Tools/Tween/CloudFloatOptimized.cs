using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
public class CloudFloatOptimized : MonoBehaviour
{
    [Header("漂浮（无Update极致性能）")]
    public float floatRange = 0.5f;
    public float minFloatDuration = 1.2f;
    public float maxFloatDuration = 2.8f;

    [Header("缩放（随机平滑）")]
    public float scaleRange = 0.12f;
    public float minScaleDuration = 1.8f;
    public float maxScaleDuration = 3.2f;

    private Vector3 _origPos;
    private Vector3 _origScale;

    public void DoCloudeAnim()
    {
        // 一次性缓存，永不重复获取
        _origPos = transform.localPosition;
        _origScale = transform.localScale;

        // 启动无限循环缓动，之后组件不再执行任何代码
        StartFloatLoop();
        StartScaleLoop();
    }

    /// <summary>
    /// 上下随机非匀速漂浮（无限循环）
    /// </summary>
    private void StartFloatLoop()
    {
        float duration = Random.Range(minFloatDuration, maxFloatDuration);
        float targetY = _origPos.y + Random.Range(-floatRange, floatRange);

        // 随机曲线 = 非恒定速度（关键）
        Ease ease = Random.value > 0.5f ? Ease.InOutSine : Ease.InOutQuad;

        _floatTween=transform.DOLocalMoveY(targetY, duration)
                 .SetEase(ease)
                 .SetLoops(-1, LoopType.Yoyo) // 无限往返
                 .SetLink(gameObject); // 随物体销毁自动回收，防内存泄漏
    }
    Tween _floatTween;
    Tween _scaleTween;
    /// <summary>
    /// 随机缩放循环
    /// </summary>
    private void StartScaleLoop()
    {
        float duration = Random.Range(minScaleDuration, maxScaleDuration);
        float scale = 1f + Random.Range(-scaleRange, scaleRange);

        Ease ease = Random.value > 0.5f ? Ease.InOutCubic : Ease.InOutSine;

        _scaleTween= transform.DOScale(_origScale * scale, duration)
                 .SetEase(ease)
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetLink(gameObject);
    }

    /// <summary>
    /// 【外部调用】停止所有无限动画，并重置云朵状态
    /// </summary>
    public void StopCloudAnim()
    {
        // 销毁漂浮动画（complete=true：立即完成并重置）
        if (_floatTween != null && _floatTween.IsActive())
            _floatTween.Kill(complete: true);

        // 销毁缩放动画
        if (_scaleTween != null && _scaleTween.IsActive())
            _scaleTween.Kill(complete: true);

        transform.localPosition = _origPos;
        transform.localScale = _origScale;
    }
}