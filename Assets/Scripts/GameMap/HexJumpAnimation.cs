using UnityEngine;
using DG.Tweening;

/// <summary>
/// 六边形跳动动画组件（抽离动画逻辑，可挂载到任意物体）
/// </summary>
[RequireComponent(typeof(Transform))]
public class HexJumpAnimation : MonoBehaviour
{
    // 基础配置（可在Inspector面板调整）
    [Header("跳动配置")]
    [Tooltip("基础跳动高度（距离为0时的最大高度）")]
    public float baseJumpHeight = 0.6f;
    [Tooltip("基础动画时长")]
    public float baseDuration = 0.15f;
    [Tooltip("动画缓动曲线")]
    public Ease jumpEase = Ease.OutCubic;

    private Transform _selfTrans;
    private Vector3 _originalPos; // 记录初始位置，避免跳动后偏移

    void Awake(){
        _selfTrans = transform;
        _originalPos = _selfTrans.localPosition;
    }

    /// <summary>
    /// 触发跳动动画（外部调用）
    /// </summary>
    /// <param name="distanceRatio">距离系数（0~1，越远越小）</param>
    /// <param name="delay">延迟执行时间</param>
    public void TriggerJump(float distanceRatio, float delay = 0f){
        // 重置位置，避免多次触发导致偏移
        _selfTrans.localPosition = _originalPos;

        // 计算实际跳动高度（距离越远，高度越小）
        float actualHeight = baseJumpHeight * (1 - distanceRatio);
        if (actualHeight < 0.01f) actualHeight = 0.01f; // 避免高度为0

        float rand_Height = Random.Range(0.7f, 1.3f);
        float rand_Duration = Random.Range(0.7f,1.3f);

        // 执行跳动动画
        _selfTrans.DOLocalMoveY(_originalPos.y + actualHeight* rand_Height, baseDuration*rand_Duration * 0.5f)
            .SetEase(jumpEase)
            .SetDelay(delay)
            .OnComplete(() =>{
                // 回落动画
                _selfTrans.DOLocalMoveY(_originalPos.y, baseDuration * 0.5f)
                    .SetEase(jumpEase);
            });
    }
}