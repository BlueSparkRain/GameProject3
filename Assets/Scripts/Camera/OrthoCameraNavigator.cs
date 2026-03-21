using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 正交相机漫游器（修正横竖拖拽+滚轮缩放+平滑过渡 + WASD移动）
/// </summary>
public class OrthoCameraNavigator : MonoSceneManager
{
    #region Inspector可配置参数（可视化，无耦合）
    [Header("核心配置")]
    [Tooltip("目标正交相机")]
    public Camera targetOrthographicCamera;
    [Tooltip("拖拽移动速度")]
    public float dragSpeed = 50f;
    [Tooltip("拖拽灵敏度（0.1-1，1为1:1屏幕偏移）")]
    [Range(0.1f, 3f)] public float dragSensitivity = 2f;

    [Header("WASD移动配置")]
    [Tooltip("WASD移动速度")]
    public float wasdMoveSpeed = 50f;
    [Tooltip("WASD移动灵敏度（0.1-2，1为基础速度，和拖拽手感统一）")]
    [Range(0.1f, 2f)] public float wasdSensitivity = 2f;

    [Header("滚轮缩放配置")]
    [Tooltip("滚轮缩放灵敏度")]
    public float scrollSensitivity = 1.5f;
    [Tooltip("最小正交尺寸（最近距离）")]
    public float minOrthographicSize = 3f;
    [Tooltip("最大正交尺寸（最远距离）")]
    public float maxOrthographicSize = 7f;

    [Header("平滑过渡配置（衰减效果）")]
    [Tooltip("拖拽位置过渡时间（越小越灵敏，越大越丝滑）")]
    public float posSmoothTime = 0.1f;
    [Tooltip("缩放过渡时间（越小越灵敏，越大越丝滑）")]
    public float scaleSmoothTime = 0.1f;

    [Header("可选：地图边界限制（关闭则无边界）")]
    [Tooltip("是否启用地图边界限制")]
    public bool enableMapBounds = true;
    [Tooltip("地图左边界（世界坐标X）")]
    public float mapLeftBound = -50f;
    [Tooltip("地图右边界（世界坐标X）")]
    public float mapRightBound = 50f;
    [Tooltip("地图下边界（世界坐标Y）")]
    public float mapBottomBound = -30f;
    [Tooltip("地图上边界（世界坐标Y）")]
    public float mapTopBound = 30f;
    #endregion

    #region 私有缓存变量（性能核心，零冗余访问）
    private bool _isDragging;               // 是否正在拖拽
    private bool _isDragEnabled;            // 拖拽功能是否启用
    private Vector3 _lastMouseWorldPos;     // 上一帧鼠标世界坐标（缓存）
    private Transform _cachedCamTransform;  // 相机Transform缓存（避免频繁GetComponent）

    // 平滑过渡相关缓存
    private Vector3 _targetCamPos;          // 相机目标位置（用于平滑过渡）
    private float _targetOrthographicSize;  // 相机目标正交尺寸（用于平滑缩放）
    //private Coroutine _posSmoothCoroutine;  // 位置平滑协程（保证唯一）
    private Coroutine _scaleSmoothCoroutine;// 缩放平滑协程（保证唯一）
    #endregion

    #region 初始化与缓存更新（解耦+性能）
    /// <summary>
    /// 动态设置目标正交相机（外部接口，解耦核心）
    /// </summary>
    /// <param name="cam">必须是正交相机，否则自动禁用</param>
    public void SetTargetCamera(Camera cam){
        // 安全校验：非正交相机直接返回
        if (cam != null && !cam.orthographic)
        {
            Debug.LogError("[MapCameraNavigator] 目标相机不是正交相机！");
            targetOrthographicCamera = null;
            _cachedCamTransform = null;
            return;
        }

        targetOrthographicCamera = cam;
        // 缓存Transform，避免每帧GetComponent（性能优化）
        _cachedCamTransform = cam?.transform;

        // 初始化目标位置和缩放
        if (_cachedCamTransform != null){
            _targetCamPos = _cachedCamTransform.position;
            _targetOrthographicSize = targetOrthographicCamera.orthographicSize;
        }

        // 启用Update（仅当相机有效且拖拽功能开启时）
        this.enabled = _isDragEnabled && targetOrthographicCamera != null;

        Debug.Log($"[MapCameraNavigator] 已设置目标相机：{cam?.name}");
    }

    /// <summary>
    /// 启用/禁用拖拽功能（外部接口，零耦合）
    /// </summary>
    public void SetDragEnabled(bool enabled){
        _isDragEnabled = enabled;
        // 仅当相机有效且功能开启时，启用Update
        this.enabled = _isDragEnabled && targetOrthographicCamera != null;

        // 禁用时立即停止拖拽和协程
        if (!enabled){
            _isDragging = false;
        }
    }
    #endregion

    protected override void Awake(){
        base.Awake();
        // 初始化缓存（零GC分配）
        _isDragEnabled = true;
        _isDragging = false;
        _cachedCamTransform = null;
        //默认禁用Update（零空轮询消耗，仅启用拖拽时开启）
        this.enabled = false;
        targetOrthographicCamera = Camera.main;
        _cachedCamTransform = targetOrthographicCamera.transform;

        // 初始化目标位置和缩放（保证平滑过渡的初始值）
        _targetCamPos = _cachedCamTransform.position;
        _targetOrthographicSize = targetOrthographicCamera.orthographicSize;

        Debug.Log("[MapCameraNavigator] 初始化完成（高性能解耦模式）");
    }

    #region 核心拖拽&缩放&WASD逻辑（高性能，零冗余计算）
    public override void MgrUpdate(float deltaTime){
        // 仅在拖拽功能启用+相机有效时执行
        if (!_isDragEnabled || _cachedCamTransform == null) return;
        //检测鼠标滚轮：处理缩放
        HandleScrollWheel();
        //处理WASD移动（复用拖拽的过渡逻辑）
        HandleWASDMovement();

        if (Input.GetMouseButtonDown(0)){
            _isDragging = true;
            //缓存鼠标屏幕坐标（而非世界坐标），消除转换偏差
            _lastMouseWorldPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _cachedCamTransform.position.z);
            return;
        }
        //检测鼠标左键松开：停止拖拽
        if (Input.GetMouseButtonUp(0)){
            _isDragging = false;
            return;
        }

        //拖拽中：计算偏移并更新目标位置（核心逻辑）
        if (_isDragging)
            UpdateDragTargetPos();
    }

    /// <summary>
    /// 新增：处理WASD移动（完全复用现有过渡逻辑，保证手感一致）
    /// </summary>
    void HandleWASDMovement(){
        float horizontal = Input.GetAxis("Horizontal") * wasdSensitivity;
        float vertical = Input.GetAxis("Vertical") * wasdSensitivity;

        //无输入时直接返回，避免无效计算（性能优化）
        if (Mathf.Approximately(horizontal, 0) && Mathf.Approximately(vertical, 0))
            return;

        //计算正交相机「每单位输入对应的世界单位」
        float worldUnitsPerPixel = (2 * targetOrthographicCamera.orthographicSize) / Screen.height;
        float moveSpeed = wasdMoveSpeed * worldUnitsPerPixel * Time.unscaledDeltaTime;

        //计算世界坐标偏移（W=上，S=下，A=左，D=右，符合直觉）
        Vector3 moveDelta = 1.8f*8*new Vector3(horizontal * moveSpeed, vertical * moveSpeed, 0);

        //更新目标位置
        _targetCamPos += moveDelta;
        _targetCamPos.z = _cachedCamTransform.position.z; // 固定Z轴

        //复用边界限制逻辑
        if (enableMapBounds)
            _targetCamPos = ClampToMapBounds(_targetCamPos);

        //复用DOTween过渡逻辑
        _cachedCamTransform.DOKill();
        _cachedCamTransform.DOMove(_targetCamPos, 0.5f)
            .SetEase(Ease.OutCubic) // 复用拖拽的缓动曲线，保证手感统一
            .SetUpdate(true);       // 不受游戏暂停影响
    }

    /// <summary>
    /// 处理鼠标滚轮缩放（DOTween 平滑过渡+距离限制，无报错版）
    /// </summary>
    void HandleScrollWheel(){
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta == 0) return;

        // 计算目标缩放值（反向：滚轮上滑=缩小尺寸=拉近，下滑=放大尺寸=拉远）
        _targetOrthographicSize -= scrollDelta * scrollSensitivity;
        // 限制缩放范围（最近/最远距离）
        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minOrthographicSize, maxOrthographicSize);

        // 核心修正：用 DOTween.To 替代反射式的 DOFloat，彻底解决 Camera 调用 DOFloat 报错的问题；
        // 先杀死旧动画，避免叠加抖动
        targetOrthographicCamera.DOKill();
        // DOTween.To 手动插值更新 orthographicSize（稳定无报错）
        DOTween.To(
            () => targetOrthographicCamera.orthographicSize, // 取值委托（当前尺寸）
            value => targetOrthographicCamera.orthographicSize = value, // 赋值委托（更新尺寸）
            _targetOrthographicSize, // 目标尺寸
            0.5f // 过渡时长（和拖拽保持一致）
        )
        .SetTarget(targetOrthographicCamera) // 绑定到相机对象，方便DOKill管理
        .SetEase(Ease.OutCubic) // 先快后慢的缓动曲线，消除生硬感
        .SetUpdate(true); // 不受游戏暂停影响
    }

    /// <summary>
    /// 更新拖拽的目标位置（彻底统一所有方向拖拽手感 + 丝滑DOTween过渡）
    /// </summary>
    void UpdateDragTargetPos(){
        //计算正交相机「每像素对应的世界单位」（核心：统一横竖拖拽基准）
        // 屏幕高度对应的世界高度 = 2 * 正交尺寸
        float worldUnitsPerPixel = (2 * targetOrthographicCamera.orthographicSize) / Screen.height;
        // 基于像素偏移计算，彻底消除宽高比/相机尺寸对不同方向的影响

        //获取鼠标像素偏移（直接用屏幕坐标计算，避免WorldPoint转换的比例偏差）
        Vector2 mouseScreenDelta = new Vector2(
            Input.mousePosition.x - _lastMouseWorldPos.x, // 这里_lastMouseWorldPos临时存储屏幕坐标（下面会更新）
            Input.mousePosition.y - _lastMouseWorldPos.y
        );
        //反向偏移（鼠标左移=相机右移，符合直觉） + 灵敏度缩放
        mouseScreenDelta = -mouseScreenDelta * dragSensitivity;

        //转换为世界坐标偏移（统一横竖方向的像素→世界单位转换）
        Vector3 worldDelta = new Vector3(
            mouseScreenDelta.x * worldUnitsPerPixel,
            (mouseScreenDelta.y * worldUnitsPerPixel) * 2,
            0
        );

        //计算相机目标位置（叠加速度 + 固定Z轴）
        _targetCamPos = _cachedCamTransform.position + 1.8f * worldDelta * dragSpeed * Time.unscaledDeltaTime;
        _targetCamPos.z = _cachedCamTransform.position.z; // 固定Z轴

        //可选：边界限制
        if (enableMapBounds)
            _targetCamPos = ClampToMapBounds(_targetCamPos);

        //DOTween平滑过渡（核心优化：解决生硬问题）
        _cachedCamTransform.DOKill();
        // 使用OutCubic缓动（先快后慢，更自然的衰减），可根据需求调整
        _cachedCamTransform.DOMove(_targetCamPos, 0.5f)
            .SetEase(Ease.OutCubic) // 丝滑的缓动曲线，替代生硬的线性过渡
            .SetUpdate(true); // 不受Time.timeScale影响（暂停游戏也能拖拽）

        //更新缓存：存储当前鼠标屏幕坐标（而非世界坐标，避免转换偏差）
        _lastMouseWorldPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _cachedCamTransform.position.z);
    }

    /// <summary>
    /// 拖拽相机核心计算（零GC，仅基础数学运算）
    /// </summary>
    /// 废弃直接拖拽，改为更新目标位置后平滑移动
    // void DragCamera(){}

    /// <summary>
    /// 相机位置边界限制（正交相机适配）
    /// </summary>
    Vector3 ClampToMapBounds(Vector3 targetPos){
        // 基于正交相机尺寸，自动适配边界（避免相机边缘超出地图）
        float camHalfWidth = targetOrthographicCamera.orthographicSize * targetOrthographicCamera.aspect;
        float camHalfHeight = targetOrthographicCamera.orthographicSize;

        // 限制X轴（左右）
        targetPos.x = Mathf.Clamp(
            targetPos.x,
            mapLeftBound + camHalfWidth,
            mapRightBound - camHalfWidth
        );
        // 限制Y轴（上下）
        targetPos.y = Mathf.Clamp(
            targetPos.y,
            mapBottomBound + camHalfHeight,
            mapTopBound - camHalfHeight
        );
        return targetPos;
    }
    #endregion

    #region 外部扩展接口（完全解耦，按需调用）
    /// <summary>
    /// 动态设置拖拽速度（外部接口）
    /// </summary>
    public void SetDragSpeed(float speed){
        dragSpeed = Mathf.Max(0.1f, speed); // 防止速度为0
    }

    /// <summary>
    /// 新增：动态设置WASD移动速度（外部接口）
    /// </summary>
    public void SetWasdMoveSpeed(float speed){
        wasdMoveSpeed = Mathf.Max(0.1f, speed);
    }

    /// <summary>
    /// 动态配置地图边界（外部接口）
    /// </summary>
    public void SetMapBounds(bool enable, float left, float right, float bottom, float top){
        enableMapBounds = enable;
        mapLeftBound = left;
        mapRightBound = right;
        mapBottomBound = bottom;
        mapTopBound = top;
    }

    /// <summary>
    /// 立即停止拖拽（外部接口，如切换场景/暂停游戏时调用）
    /// </summary>
    public void StopDragImmediately(){
        _isDragging = false;
    }

    /// <summary>
    /// 动态设置缩放范围（外部接口）
    /// </summary>
    public void SetScaleRange(float minSize, float maxSize){
        minOrthographicSize = Mathf.Max(0.1f, minSize);
        maxOrthographicSize = Mathf.Max(minOrthographicSize, maxSize);
        // 修正目标缩放值，防止超出新范围
        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minOrthographicSize, maxOrthographicSize);
    }
    #endregion

    #region 安全释放（零内存泄漏）
    void OnDestroy(){
        _isDragging = false;
        _isDragEnabled = false;
        _cachedCamTransform = null;
        targetOrthographicCamera = null;

        Debug.Log("[MapCameraNavigator] 已销毁，资源已释放");
    }
    #endregion
}