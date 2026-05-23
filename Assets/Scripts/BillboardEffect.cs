using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 高性能布告板效果：使物体始终朝向相机。
/// 通过静态缓存Camera.main + Transform缓存避免每帧GC开销。
/// </summary>
//public class BillboardEffect : MonoBehaviour
//{
[ExecuteAlways]
public class BillboardEffectCom : MonoBehaviour
{
    [Header("朝向模式")]
    [Tooltip("完全朝向相机（LookAt）")]
    public bool fullFaceCamera = true;

    [Header("轴锁定（仅 fullFaceCamera=false 时生效）")]
    [Tooltip("锁定Y轴旋转（垂直布告板，常用于角色血条）")]
    public bool lockYAxis = true;
    [Tooltip("锁定X轴旋转")]
    public bool lockXAxis;
    [Tooltip("锁定Z轴旋转")]
    public bool lockZAxis;

    [Header("高级")]
    [Tooltip("反转朝向（背面朝相机）")]
    public bool reverseFace;
    [Tooltip("仅在运行时刻更新")]
    public bool onlyWhenRunning = true;

    // 静态相机缓存，所有BillboardEffect实例共享
    private static Camera _cachedMainCamera;
    private static int _lastFrameCameraChecked;

    private Transform _cachedTransform;
    private bool _hasStarted;

    /// <summary>
    /// 强制刷新静态相机缓存（相机切换时调用）
    /// </summary>
    public static void InvalidateCameraCache()
    {
        _cachedMainCamera = null;
        _lastFrameCameraChecked = -1;
    }

    void Awake()
    {
        _cachedTransform = transform;
    }

    void Start()
    {
        _hasStarted = true;
    }

    void OnEnable()
    {
        if (_hasStarted || !onlyWhenRunning)
            return;
        // 编辑器模式下首次启用时确保Transform已缓存
        if (_cachedTransform == null)
            _cachedTransform = transform;
    }

    void LateUpdate()
    {
        if (onlyWhenRunning && !Application.isPlaying)
            return;

        UpdateCameraCache();
        if (_cachedMainCamera == null)
            return;

        ApplyBillboard();
    }

    void UpdateCameraCache()
    {
        // 每帧检查一次，避免同一帧内多次调用Camera.main
        int currentFrame = Time.frameCount;
        if (_lastFrameCameraChecked != currentFrame || _cachedMainCamera == null)
        {
            _lastFrameCameraChecked = currentFrame;
            // Camera.main内部有缓存，开销可接受
            if (_cachedMainCamera == null || !_cachedMainCamera.isActiveAndEnabled)
                _cachedMainCamera = Camera.main;
        }
    }

    void ApplyBillboard()
    {
        if (fullFaceCamera)
        {
            Vector3 dir = _cachedTransform.position - _cachedMainCamera.transform.position;
            if (reverseFace) dir = -dir;
            if (dir.sqrMagnitude > 0.0001f)
                _cachedTransform.rotation = Quaternion.LookRotation(dir, _cachedMainCamera.transform.up);
        }
        else
        {
            Quaternion camRot = _cachedMainCamera.transform.rotation;
            if (reverseFace) camRot = Quaternion.Inverse(camRot);

            Vector3 euler = camRot.eulerAngles;
            Vector3 currentEuler = _cachedTransform.rotation.eulerAngles;

            if (lockXAxis) euler.x = currentEuler.x;
            if (lockYAxis) euler.y = currentEuler.y;
            if (lockZAxis) euler.z = currentEuler.z;

            _cachedTransform.rotation = Quaternion.Euler(euler);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 编辑器值变更时立即应用
        if (_cachedTransform == null)
        {
            _cachedTransform = transform;
            UpdateCameraCache();
        }
    }
#endif
}

