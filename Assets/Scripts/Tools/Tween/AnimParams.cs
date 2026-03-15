using DG.Tweening;
using System;
using UnityEngine;

// 轻量级参数类：用于代码内快速创建简单动画
[Serializable]
public class AnimParams
{
    [Header("基础配置")]
    public float Duration = 1f;
    public float Delay = 0f;
    //public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);
    public Ease Ease = Ease.Linear;
    [Header("循环配置")]
    public AnimationLoopType LoopMode = AnimationLoopType.None;
    public int LoopCount = 0;
    [Header("是否支持打断")]
    public bool Interruptible = true;
    [Header("回调配置（仅运行时）")]
    [HideInInspector] public Action OnComplete;
    [HideInInspector] public Action OnInterrupt;
    [HideInInspector] public Action<float> OnUpdate;

    // 新增：物体类型配置
    public AnimationTargetType TargetType = AnimationTargetType.Auto;
    // 新增：动画空间模式配置
    public AnimationSpaceMode SpaceMode = AnimationSpaceMode.Local;
}

// 新增：物体类型枚举
public enum AnimationTargetType
{
    Auto,       // 自动识别（默认）
    UI,         // UI元素（RectTransform）
    Sprite2D,   // 2D精灵（SpriteRenderer）
    Object3D    // 3D物体（Transform）
}

// 新增：动画空间模式枚举
public enum AnimationSpaceMode
{
    Local,      // 本地坐标系（默认）
    World       // 世界坐标系
}


public enum AnimationLoopType
{
    None,       // 无循环
    Restart,    // 重复播放（从头开始）
    Yoyo        // 来回播放（如：放大→缩小→放大）
}
