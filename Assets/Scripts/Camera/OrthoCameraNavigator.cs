using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 俯视角正交相机漫游器 - XZ平面移动（Y轴固定高度）
/// 支持：拖拽、WASD、鼠标边缘、滚轮缩放、目标聚焦
/// </summary>
public class OrthoCameraNavigator : MonoSceneManager
{
    #region Inspector可配置参数
    [Header("核心配置")]
    [Tooltip("目标正交相机")]
    public Camera targetOrthographicCamera;
    [Tooltip("拖拽移动速度")]
    public float dragSpeed = 50f;
    [Tooltip("拖拽灵敏度")]
    [Range(0.1f, 3f)] public float dragSensitivity = 2f;

    [Header("WASD移动配置")]
    [Tooltip("WASD移动速度")]
    public float wasdMoveSpeed = 50f;
    [Tooltip("WASD灵敏度")]
    [Range(0.1f, 2f)] public float wasdSensitivity = 2f;

    [Header("鼠标边缘移动配置")]
    [Tooltip("边缘触发像素宽度")]
    public float edgeTriggerPixel = 200f;
    [Tooltip("边缘移动速度")]
    public float edgeMoveSpeed = 30f;
    [Tooltip("边缘灵敏度")]
    [Range(0.1f, 2f)] public float edgeSensitivity = 2f;

    [Header("滚轮缩放配置")]
    [Tooltip("缩放灵敏度")]
    public float scrollSensitivity = 1.5f;
    public float minOrthographicSize = 3f;
    public float maxOrthographicSize = 6f;

    [Header("平滑过渡")]
    public float posSmoothTime = 0.4f;
    public float scaleSmoothTime = 0.4f;

    [Header("地图边界（XZ平面）")]
    public bool enableMapBounds = true;
    public float mapLeftBound = -5f;    // X最小
    public float mapRightBound = 60f;   // X最大
    public float mapBackBound = -15f;    // Z最小
    public float mapFrontBound = 50f;   // Z最大

    //[Header("聚焦配置")]
    //public float focusSmoothTime = 1.5f;
    #endregion

    #region 私有变量
    private bool _isDragging;
    private bool _isDragEnabled;
    private Vector3 _lastMouseScreenPos;
    private Transform _cachedCamTransform;

    private Vector3 _targetCamPos;
    private float _targetOrthographicSize;
    private Vector2 _edgeMoveDirection;
    private bool _isFocusing;

    //启用相机漫游
    bool use_CamPan;
    #endregion

    #region 初始化
    public void SetTargetCamera(Camera cam)
    {
        if (cam != null && !cam.orthographic)
        {
            Debug.LogError("非正交相机！");
            targetOrthographicCamera = null;
            _cachedCamTransform = null;
            return;
        }

        targetOrthographicCamera = cam;
        _cachedCamTransform = cam?.transform;

        if (_cachedCamTransform != null)
        {
            _targetCamPos = _cachedCamTransform.position;
            _targetOrthographicSize = targetOrthographicCamera.orthographicSize;
        }

        this.enabled = _isDragEnabled && targetOrthographicCamera != null;
    }

    public void SetDragEnabled(bool enabled)
    {
        _isDragEnabled = enabled;
        this.enabled = _isDragEnabled && targetOrthographicCamera != null;
        if (!enabled) _isDragging = false;
    }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener(E_EventType.FreezeCamPan, () => use_CamPan = false);
        EventCenter.AddEventListener(E_EventType.UnFreezeCamPan, () => use_CamPan = true);
    }
    protected override void MgrOnDispose()
    {
        base.MgrOnDispose();
        EventCenter.RemoveEventListener(E_EventType.FreezeCamPan, () => use_CamPan = false);
        EventCenter.RemoveEventListener(E_EventType.UnFreezeCamPan, () => use_CamPan = true);
    }

    protected override void Awake()
    {
        base.Awake();
        _isDragEnabled = true;
        this.enabled = false;

        targetOrthographicCamera = Camera.main;
        _cachedCamTransform = targetOrthographicCamera.transform;

        _targetCamPos = _cachedCamTransform.position;
        _targetOrthographicSize = targetOrthographicCamera.orthographicSize;
        _isFocusing = false;
    }
    #endregion

    #region 核心更新
    public override void MgrUpdate(float deltaTime)
    {
        if (_isFocusing) return;
        if (!_isDragEnabled || _cachedCamTransform == null) return;

        //滚轮放大
        HandleScrollWheel();
        //WASD漫游
        HandleWASDMovement();
        //屏幕边缘漫游
        HandleMouseEdgeMovement();
        //鼠标拖拽漫游
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lastMouseScreenPos = Input.mousePosition;
            return;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            return;
        }
        if (_isDragging) UpdateDragTargetPos();
    }
    #endregion

    #region 移动功能（全改为 XZ 平面）
    /// <summary>
    /// 鼠标边缘移动 - XZ平面
    /// </summary>
    void HandleMouseEdgeMovement()
    {
        _edgeMoveDirection = Vector2.zero;
        Vector2 mousePos = Input.mousePosition;

        if (mousePos.x < edgeTriggerPixel) _edgeMoveDirection.x = -1;
        else if (mousePos.x > Screen.width - edgeTriggerPixel) _edgeMoveDirection.x = 1;

        if (mousePos.y < edgeTriggerPixel) _edgeMoveDirection.y = -1;
        else if (mousePos.y > Screen.height - edgeTriggerPixel) _edgeMoveDirection.y = 1;

        if (_edgeMoveDirection.magnitude < 0.1f) return;
        _edgeMoveDirection.Normalize();

        float worldUnits = (2 * targetOrthographicCamera.orthographicSize) / Screen.height;
        float speed = edgeMoveSpeed * worldUnits * Time.unscaledDeltaTime;

        // 核心修改：XZ移动，Y固定
        Vector3 moveDelta = new Vector3(
            _edgeMoveDirection.x * speed,
            0,
            _edgeMoveDirection.y * speed
        );

        _targetCamPos += moveDelta * 30;
        _targetCamPos.y = _cachedCamTransform.position.y; // 锁死Y

        if (enableMapBounds) _targetCamPos = ClampCameraBounds(_targetCamPos);

        _cachedCamTransform.DOKill();
        _cachedCamTransform.DOMove(_targetCamPos, posSmoothTime).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    /// <summary>
    /// WASD移动 - XZ平面
    /// </summary>
    void HandleWASDMovement()
    {
        float h = Input.GetAxis("Horizontal") * wasdSensitivity;
        float v = Input.GetAxis("Vertical") * wasdSensitivity;

        if (Mathf.Approximately(h, 0) && Mathf.Approximately(v, 0)) return;

        float worldUnits = (2 * targetOrthographicCamera.orthographicSize) / Screen.height;
        float speed = wasdMoveSpeed * worldUnits * Time.unscaledDeltaTime;

        // 核心修改：XZ移动
        Vector3 moveDelta = new Vector3(h * speed, 0, v * speed);
        _targetCamPos += moveDelta * 10;
        _targetCamPos.y = _cachedCamTransform.position.y;

        if (enableMapBounds) _targetCamPos = ClampCameraBounds(_targetCamPos);

        _cachedCamTransform.DOKill();
        _cachedCamTransform.DOMove(_targetCamPos, posSmoothTime).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    /// <summary>
    /// 鼠标拖拽 - XZ平面
    /// </summary>
    void UpdateDragTargetPos()
    {
        Vector2 delta = (Vector2)Input.mousePosition - (Vector2)_lastMouseScreenPos;
        delta = -delta * dragSensitivity;

        float worldUnits = (2 * targetOrthographicCamera.orthographicSize) / Screen.height;

        // 核心修改：拖拽控制 XZ
        Vector3 moveDelta = new Vector3(
            delta.x * worldUnits,
            0,
            delta.y * worldUnits
        );

        _targetCamPos = _cachedCamTransform.position + moveDelta * dragSpeed * Time.unscaledDeltaTime;
        _targetCamPos.y = _cachedCamTransform.position.y;

        if (enableMapBounds) _targetCamPos = ClampCameraBounds(_targetCamPos);

        _cachedCamTransform.DOKill();
        _cachedCamTransform.DOMove(_targetCamPos, posSmoothTime).SetEase(Ease.OutCubic).SetUpdate(true);
        _lastMouseScreenPos = Input.mousePosition;
    }
    #endregion

    #region 缩放功能
    void HandleScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        _targetOrthographicSize -= scroll * scrollSensitivity;
        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minOrthographicSize, maxOrthographicSize);

        targetOrthographicCamera.DOKill();
        DOTween.To(
            () => targetOrthographicCamera.orthographicSize,
            v => targetOrthographicCamera.orthographicSize = v,
            _targetOrthographicSize,
            scaleSmoothTime
        ).SetTarget(targetOrthographicCamera).SetEase(Ease.OutCubic).SetUpdate(true);
    }
    #endregion

    #region 边界限制（XZ平面）
    Vector3 ClampCameraBounds(Vector3 pos)
    {
        float halfW = targetOrthographicCamera.orthographicSize * targetOrthographicCamera.aspect;
        pos.x = Mathf.Clamp(pos.x, mapLeftBound + halfW, mapRightBound - halfW);
        pos.z = Mathf.Clamp(pos.z, mapBackBound + halfW, mapFrontBound - halfW);
        return pos;
    }
    #endregion

    #region 聚焦功能（终极修复！XZ对齐，Y固定）
    /// <summary>
    /// 平滑聚焦：相机XZ=目标XZ，Y保持俯视高度，目标居中屏幕
    /// </summary>
    public void FocusOnTarget(GameObject target,float focusSmoothTime = 1.5f)
    {
        //if (target == null || _cachedCamTransform == null || _isFocusing) return;
        if (target == null || _cachedCamTransform == null ) return;
        _isFocusing = true;

        _cachedCamTransform.DOKill();
        _isDragging = false;

        // 相机 Y 固定，只移动 XZ 对齐目标
        Vector3 targetPos = target.transform.position;
        Vector3 finalPos = new Vector3(
            targetPos.x + 1,
            _cachedCamTransform.position.y, // 固定高空Y
            targetPos.z - 3
        );

        _targetCamPos = finalPos;
        _cachedCamTransform.DOMove(finalPos, focusSmoothTime)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
                   //.OnComplete(() => { });
            .OnComplete(() => _isFocusing = false);
    }
    IEnumerator Flash()
    {
        _isFocusing = true;
        yield return new WaitForSeconds(0.3f);
        _isFocusing = false;
    }

    /// <summary>
    /// 立即聚焦
    /// </summary>
    public void FocusOnTargetImmediate(GameObject target)
    {
        if (target == null || _cachedCamTransform == null) return;

        _isFocusing = true;
        _cachedCamTransform.DOKill();

        Vector3 finalPos = new Vector3(
            target.transform.position.x,
            _cachedCamTransform.position.y,
            target.transform.position.z - 5
        );

        _cachedCamTransform.position = finalPos;
        _targetCamPos = finalPos;
        _isFocusing = false;
    }
    #endregion

    #region 工具函数
    public void SetEdgeMoveSpeed(float speed) => edgeMoveSpeed = Mathf.Max(0.1f, speed);
    public void SetDragSpeed(float speed) => dragSpeed = Mathf.Max(0.1f, speed);
    public void SetWasdMoveSpeed(float speed) => wasdMoveSpeed = Mathf.Max(0.1f, speed);
    public void StopDragImmediately() => _isDragging = false;

    public void SetMapBounds(bool enable, float left, float right, float back, float front)
    {
        enableMapBounds = enable;
        mapLeftBound = left;
        mapRightBound = right;
        mapBackBound = back;
        mapFrontBound = front;
    }

    public void SetScaleRange(float min, float max)
    {
        minOrthographicSize = Mathf.Max(0.1f, min);
        maxOrthographicSize = Mathf.Max(minOrthographicSize, max);
        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minOrthographicSize, maxOrthographicSize);
    }
    #endregion

    #region 销毁
    void OnDestroy()
    {
        _isDragging = false;
        _isDragEnabled = false;
        _isFocusing = false;
        _cachedCamTransform = null;
        targetOrthographicCamera = null;
    }
    #endregion
}