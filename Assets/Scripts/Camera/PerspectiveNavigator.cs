using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 透视相机漫游器 - XZ平面移动，FOV缩放
/// 接口与OrthoCameraNavigator保持一致
/// </summary>
public class PerspectiveNavigator : MonoSceneManager
{
    #region Inspector
    [Header("核心配置")]
    public Camera targetPerspectiveCamera;
    public float dragSpeed = 50f;
    [Range(0.1f, 3f)] public float dragSensitivity = 2f;

    [Header("WASD移动")]
    public float wasdMoveSpeed = 50f;
    [Range(0.1f, 2f)] public float wasdSensitivity = 2f;

    [Header("鼠标边缘移动")]
    public float edgeTriggerPixel = 200f;
    public float edgeMoveSpeed = 30f;
    [Range(0.1f, 2f)] public float edgeSensitivity = 2f;

    [Header("FOV缩放")]
    public float scrollSensitivity = 5f;
    public float minFOV = 20f;
    public float maxFOV = 80f;

    [Header("平滑过渡")]
    public float posSmoothTime = 0.4f;
    public float fovSmoothTime = 0.4f;

    [Header("地平面Y（用于计算透视世界单位转换）")]
    public float groundPlaneY = 0f;

    [Header("地图边界")]
    public bool enableMapBounds = true;
    public float mapLeftBound = -5f;
    public float mapRightBound = 60f;
    public float mapBackBound = -15f;
    public float mapFrontBound = 50f;
    #endregion

    #region 私有变量
    private bool _isDragging;
    private bool _isDragEnabled;
    private Vector3 _lastMouseScreenPos;
    private Transform _cachedCamTransform;

    private Vector3 _targetCamPos;
    private float _targetFOV;
    private Vector2 _edgeMoveDirection;
    private bool _isFocusing;
    private bool use_CamPan;
    #endregion

    #region 初始化
    public void SetTargetCamera(Camera cam)
    {
        if (cam != null && cam.orthographic)
        {
            Debug.LogError("非透视相机！");
            targetPerspectiveCamera = null;
            _cachedCamTransform = null;
            return;
        }

        targetPerspectiveCamera = cam;
        _cachedCamTransform = cam?.transform;

        if (_cachedCamTransform != null)
        {
            _targetCamPos = _cachedCamTransform.position;
            _targetFOV = targetPerspectiveCamera.fieldOfView;
        }

        this.enabled = _isDragEnabled && targetPerspectiveCamera != null;
    }

    public void SetDragEnabled(bool enabled)
    {
        _isDragEnabled = enabled;
        this.enabled = _isDragEnabled && targetPerspectiveCamera != null;
        if (!enabled) _isDragging = false;
    }

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener(E_EventType.FreezeCamPan, Freeze);
        EventCenter.AddEventListener(E_EventType.UnFreezeCamPan,UnFreeze);
    }
    void Freeze()
    {
        use_CamPan = false;
        Debug.Log("-------冻结吧");
    }
    void UnFreeze() {
        use_CamPan = true;
        Debug.Log("-------解冻啦");
    }

    protected override void MgrOnDispose()
    {
        base.MgrOnDispose();
        EventCenter.RemoveEventListener(E_EventType.FreezeCamPan, Freeze);
        EventCenter.RemoveEventListener(E_EventType.UnFreezeCamPan, UnFreeze);
    }

    protected override void Awake()
    {
        base.Awake();
        _isDragEnabled = true;
        this.enabled = false;
        use_CamPan = true;

        targetPerspectiveCamera = Camera.main;
        if (targetPerspectiveCamera != null)
        {
            _cachedCamTransform = targetPerspectiveCamera.transform;
            _targetCamPos = _cachedCamTransform.position;
            _targetFOV = targetPerspectiveCamera.fieldOfView;
        }
        _isFocusing = false;
    }
    #endregion

    #region 核心更新
    public override void MgrUpdate(float deltaTime)
    {
        if (!use_CamPan) return;
        if (_isFocusing) return;
        if (!_isDragEnabled || _cachedCamTransform == null) return;

        HandleScrollWheel();
        HandleWASDMovement();
        HandleMouseEdgeMovement();

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

    #region 移动功能

    /// <summary>
    /// 透视相机世界单位换算：基于相机到地平面的距离
    /// </summary>
    float GetWorldUnitsPerPixel()
    {
        Vector3 camForward = _cachedCamTransform.forward;
        float heightAboveGround = _cachedCamTransform.position.y - groundPlaneY;
        if (heightAboveGround <= 0.01f) heightAboveGround = 0.01f;

        float dot = Vector3.Dot(camForward, Vector3.down);
        if (dot <= 0.001f) dot = 0.001f;

        float distanceToGround = heightAboveGround / dot;
        return 2f * distanceToGround * Mathf.Tan(targetPerspectiveCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
    }

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

        float worldUnits = GetWorldUnitsPerPixel();
        float speed = edgeMoveSpeed * worldUnits * Time.unscaledDeltaTime;

        Vector3 moveDelta = new Vector3(
            _edgeMoveDirection.x * speed,
            0,
            _edgeMoveDirection.y * speed
        );

        _targetCamPos += moveDelta * 30;
        _targetCamPos.y = _cachedCamTransform.position.y;

        if (enableMapBounds) _targetCamPos = ClampCameraBounds(_targetCamPos);

        _cachedCamTransform.DOKill();
        _cachedCamTransform.DOMove(_targetCamPos, posSmoothTime).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    void HandleWASDMovement()
    {
        float h = Input.GetAxis("Horizontal") * wasdSensitivity;
        float v = Input.GetAxis("Vertical") * wasdSensitivity;

        if (Mathf.Approximately(h, 0) && Mathf.Approximately(v, 0)) return;

        float worldUnits = GetWorldUnitsPerPixel();
        float speed = wasdMoveSpeed * worldUnits * Time.unscaledDeltaTime;

        Vector3 moveDelta = new Vector3(h * speed, 0, v * speed);
        _targetCamPos += moveDelta * 10;
        _targetCamPos.y = _cachedCamTransform.position.y;

        if (enableMapBounds) _targetCamPos = ClampCameraBounds(_targetCamPos);

        _cachedCamTransform.DOKill();
        _cachedCamTransform.DOMove(_targetCamPos, posSmoothTime).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    void UpdateDragTargetPos()
    {
        Vector2 delta = (Vector2)Input.mousePosition - (Vector2)_lastMouseScreenPos;
        delta = -delta * dragSensitivity;

        float worldUnits = GetWorldUnitsPerPixel();

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

    #region 缩放（FOV）
    void HandleScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        _targetFOV -= scroll * scrollSensitivity;
        _targetFOV = Mathf.Clamp(_targetFOV, minFOV, maxFOV);

        targetPerspectiveCamera.DOKill();
        DOTween.To(
            () => targetPerspectiveCamera.fieldOfView,
            v => targetPerspectiveCamera.fieldOfView = v,
            _targetFOV,
            fovSmoothTime
        ).SetTarget(targetPerspectiveCamera).SetEase(Ease.OutCubic).SetUpdate(true);
    }
    #endregion

    #region 边界限制
    Vector3 ClampCameraBounds(Vector3 pos)
    {
        float worldUnits = GetWorldUnitsPerPixel();
        float halfW = worldUnits * Screen.width * 0.5f;
        float halfH = worldUnits * Screen.height * 0.5f;
        pos.x = Mathf.Clamp(pos.x, mapLeftBound + halfW, mapRightBound - halfW);
        pos.z = Mathf.Clamp(pos.z, mapBackBound + halfH, mapFrontBound - halfH);
        return pos;
    }
    #endregion

    #region 聚焦功能
    public void FocusOnTarget(GameObject target, float focusSmoothTime = 1.5f)
    {
        if (target == null || _cachedCamTransform == null) return;
        _isFocusing = true;

        _cachedCamTransform.DOKill();
        _isDragging = false;

        Vector3 finalPos = new Vector3(
            target.transform.position.x + 1,
            _cachedCamTransform.position.y,
            target.transform.position.z - 3
        );

        _targetCamPos = finalPos;
        _cachedCamTransform.DOMove(finalPos, focusSmoothTime)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => _isFocusing = false);
    }

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

    /// <summary>
    /// 设置FOV缩放范围（与OrthoCameraNavigator.SetScaleRange接口一致）
    /// </summary>
    public void SetScaleRange(float min, float max)
    {
        minFOV = Mathf.Max(1f, min);
        maxFOV = Mathf.Max(minFOV, max);
        _targetFOV = Mathf.Clamp(_targetFOV, minFOV, maxFOV);
    }
    #endregion

    void OnDestroy()
    {
        _isDragging = false;
        _isDragEnabled = false;
        _isFocusing = false;
        _cachedCamTransform = null;
        targetPerspectiveCamera = null;
    }
}
