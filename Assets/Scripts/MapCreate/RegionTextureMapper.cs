using System.Collections.Generic;
using UnityEngine;
using Core;

/// <summary>
/// 区域纹理映射器 —— 独立于 GameMapManager 的组件。
/// 按 (row, col) 范围找到房间面片 Renderer，所有面片共用一个 MaterialPropertyBlock。
/// 每面随机延迟由 Shader 根据物体 pivot 世界坐标哈希在 GPU 端计算，
/// 全局面片合用单 MPB — SRP Batcher 合批，CPU 零逐面开销。
/// </summary>
public class RegionTextureMapper : MonoBehaviour
{
    [Header("房间坐标范围")]
    public int minRow;
    public int maxRow = 10;
    public int minCol;
    public int maxCol = 10;

    [Header("纹理")]
    public Texture2D regionTexture;

    [Header("运行时映射")]
    public Vector2 tiling = Vector2.one;
    public Vector2 offset = Vector2.zero;
    [Range(0f, 1f)]
    public float textureOpacity = 1f;

    [Header("渐变")]
    [Tooltip("单个面片从 0→1 的渐变时长（秒）")]
    public float fadeDuration = 0.5f;
    [Tooltip("面片随机延迟的最大值（秒）")]
    public float maxRandomDelay = 1.5f;

    [Header("面片控制")]
    [Tooltip("面片缩放")]
    public Vector3 faceScale = Vector3.one;
    [Tooltip("面片在房间上方的 Y 轴高度偏移")]
    public float faceHeight = 0.15f;
    [Tooltip("开启/关闭贴图映射（带动画过渡）")]
    public bool mappingEnabled = true;

    [Header("面片查找")]
    [Tooltip("房间预制件中面片子物体的名称")]
    public string faceChildName = "HexFace";

    [Header("调试")]
    public bool showRangeGizmo = true;

    List<Renderer> _faceRenderers = new List<Renderer>();
    List<Transform> _faceTransforms = new List<Transform>();
    Material _sharedMaterial;
    MaterialPropertyBlock _sharedMPB;
    bool _materialInitialized;

    // 渐变状态
    float _transitionStartTime = -1f;
    float _fromOpacity;
    float _targetOpacity = 1f;

    // 轮询缓存
    Texture2D _lastTex;
    Vector2 _lastTiling, _lastOffset;
    int _lastMinRow, _lastMaxRow, _lastMinCol, _lastMaxCol;
    float _lastOpacity, _lastFadeDuration, _lastMaxDelay;
    Vector3 _lastScale;
    float _lastHeight;
    bool _lastEnabled;
    string _lastFaceName;

    static readonly int ShaderProp_RegionMin       = Shader.PropertyToID("_RegionMin");
    static readonly int ShaderProp_RegionSize      = Shader.PropertyToID("_RegionSize");
    static readonly int ShaderProp_MainTex         = Shader.PropertyToID("_MainTex");
    static readonly int ShaderProp_Tiling          = Shader.PropertyToID("_Tiling");
    static readonly int ShaderProp_Offset          = Shader.PropertyToID("_Offset");
    static readonly int ShaderProp_Opacity         = Shader.PropertyToID("_Opacity");
    static readonly int ShaderProp_TransitionStart = Shader.PropertyToID("_TransitionStartTime");
    static readonly int ShaderProp_FadeDuration    = Shader.PropertyToID("_FadeDuration");
    static readonly int ShaderProp_FromOpacity     = Shader.PropertyToID("_FromOpacity");
    static readonly int ShaderProp_TargetOpacity   = Shader.PropertyToID("_TargetOpacity");
    static readonly int ShaderProp_MaxRandomDelay  = Shader.PropertyToID("_MaxRandomDelay");

    void Start()
    {
        InitMaterial();
        EventCenter.AddEventListener(E_EventType.LoadMapEnd, OnLoadMapEnd);
        if (GameRoot.GetManager<GameMapManager>()?.HexRoomMap?.Count > 0)
            RefreshMapping();
    }

    void OnDestroy()
    {
        EventCenter.RemoveEventListener(E_EventType.LoadMapEnd, OnLoadMapEnd);
        ClearFaceCache();
        if (_sharedMaterial != null)
        {
            if (Application.isPlaying) Destroy(_sharedMaterial);
            else DestroyImmediate(_sharedMaterial);
        }
    }

    void Update()
    {
        bool propChanged = _lastTiling != tiling || _lastOffset != offset || _lastTex != regionTexture ||
                           _lastOpacity != textureOpacity || _lastFaceName != faceChildName ||
                           _lastFadeDuration != fadeDuration || _lastMaxDelay != maxRandomDelay;

        bool transformChanged = _lastScale != faceScale || _lastHeight != faceHeight;
        bool enabledChanged = _lastEnabled != mappingEnabled;

        if (propChanged || enabledChanged)
            ApplySharedMPB();

        if (transformChanged)
            ApplyFaceTransforms();

        if (propChanged || transformChanged || enabledChanged)
            SaveLastParams();

        if (_lastMinRow != minRow || _lastMaxRow != maxRow ||
            _lastMinCol != minCol || _lastMaxCol != maxCol)
        {
            RefreshMapping();
            SaveLastParams();
        }

        UpdateRendererEnabled();
    }

    void InitMaterial()
    {
        if (_materialInitialized) return;

        Shader shader = Shader.Find("Custom/RegionTextureMapper");
        if (shader == null)
        {
            Debug.LogError("[RegionTextureMapper] 找不到 Custom/RegionTextureMapper 着色器");
            return;
        }
        _sharedMaterial = new Material(shader);
        _sharedMPB = new MaterialPropertyBlock();
        _materialInitialized = true;
    }

    void OnLoadMapEnd()
    {
        if (!isActiveAndEnabled) return;
        StartCoroutine(DelayedRefreshCoro());
    }

    System.Collections.IEnumerator DelayedRefreshCoro()
    {
        GameMapManager map = GameRoot.GetManager<GameMapManager>();
        int lastCount = 0;
        int stable = 0;
        while (stable < 3)
        {
            yield return new WaitForSeconds(0.1f);
            int count = map?.HexRoomMap?.Count ?? 0;
            if (count == lastCount && count > 0)
                stable++;
            else
                stable = 0;
            lastCount = count;
        }
        RefreshMapping();
    }

    [ContextMenu("Refresh Mapping")]
    public void RefreshMapping()
    {
        ClearFaceCache();

        GameMapManager map = GameRoot.GetManager<GameMapManager>();
        if (map == null || map.HexRoomMap == null || map.HexRoomMap.Count == 0)
        {
            Debug.LogWarning("[RegionTextureMapper] GameMapManager 未就绪或房间为空");
            return;
        }
        if (_sharedMaterial == null) InitMaterial();
        if (_sharedMaterial == null) return;

        foreach (var kvp in map.HexRoomMap)
        {
            HexRoomTag room = kvp.Value;
            if (room == null) continue;

            int row = room.row;
            int col = room.col;
            if (row < minRow || row > maxRow || col < minCol || col > maxCol) continue;

            Transform faceTrans = room.transform.Find(faceChildName);
            if (faceTrans == null) continue;

            Renderer rend = faceTrans.GetComponent<Renderer>();
            if (rend == null) continue;

            rend.sharedMaterial = _sharedMaterial;
            rend.enabled = true;
            _faceRenderers.Add(rend);
            _faceTransforms.Add(faceTrans);
        }

        Debug.Log($"[RegionTextureMapper] 找到 {_faceRenderers.Count} 个面片 Renderer");
        ResetFadeTransition(fromZero: true);
        ApplySharedMPB();
        ApplyFaceTransforms();
        SaveLastParams();
    }

    /// <summary>
    /// 写入共享 MPB + 材质属性（仅参数变化时触发，非每帧）。
    /// CBUFFER 内属性（_Opacity / _Transition 等）直接设到 Material，
    /// CBUFFER 外属性（_RegionMin / _Tiling / _MaxRandomDelay 等）设到 MPB。
    /// </summary>
    public void ApplySharedMPB()
    {
        GameMapManager map = GameRoot.GetManager<GameMapManager>();
        if (map == null) return;

        ComputeWorldRegion(map, out Vector2 regionMin, out Vector2 regionSize);

        // CBUFFER 外 — 通过 MPB，SRP Batcher 合批不冲突
        _sharedMPB.SetTexture(ShaderProp_MainTex, regionTexture);
        _sharedMPB.SetVector(ShaderProp_RegionMin, new Vector4(regionMin.x, regionMin.y, 0f, 0f));
        _sharedMPB.SetVector(ShaderProp_RegionSize, new Vector4(regionSize.x, regionSize.y, 0f, 0f));
        _sharedMPB.SetVector(ShaderProp_Tiling, new Vector4(tiling.x, tiling.y, 0f, 0f));
        _sharedMPB.SetVector(ShaderProp_Offset, new Vector4(offset.x, offset.y, 0f, 0f));
        _sharedMPB.SetFloat(ShaderProp_MaxRandomDelay, maxRandomDelay);

        // CBUFFER 内 — 直接设到 Material 实例
        _sharedMaterial.SetFloat(ShaderProp_Opacity, textureOpacity);
        _sharedMaterial.SetFloat(ShaderProp_FadeDuration, Mathf.Max(0.001f, fadeDuration));

        if (_lastEnabled != mappingEnabled)
            ResetFadeTransition(fromZero: false);

        _sharedMaterial.SetFloat(ShaderProp_TransitionStart, _transitionStartTime);
        _sharedMaterial.SetFloat(ShaderProp_FromOpacity, _fromOpacity);
        _sharedMaterial.SetFloat(ShaderProp_TargetOpacity, _targetOpacity);

        foreach (var rend in _faceRenderers)
        {
            if (rend != null)
                rend.SetPropertyBlock(_sharedMPB);
        }
    }

    void ResetFadeTransition(bool fromZero)
    {
        _transitionStartTime = Time.time;
        _fromOpacity = fromZero ? 0f : (mappingEnabled ? 0f : 1f);
        _targetOpacity = mappingEnabled ? 1f : 0f;
    }

    void UpdateRendererEnabled()
    {
        float elapsed = Time.time - _transitionStartTime;
        float maxDelay = maxRandomDelay + fadeDuration;

        foreach (var rend in _faceRenderers)
        {
            if (rend == null) continue;
            if (_targetOpacity <= 0f && elapsed >= maxDelay)
                rend.enabled = false;
            else if (_targetOpacity > 0f)
                rend.enabled = true;
        }
    }

    void ApplyFaceTransforms()
    {
        for (int i = _faceTransforms.Count - 1; i >= 0; i--)
        {
            Transform t = _faceTransforms[i];
            if (t == null)
            {
                _faceTransforms.RemoveAt(i);
                continue;
            }
            t.localScale = faceScale;
            Vector3 lp = t.localPosition;
            lp.z = faceHeight;
            t.localPosition = lp;
        }
    }

    void ComputeWorldRegion(GameMapManager map, out Vector2 regionMin, out Vector2 regionSize)
    {
        float worldMinX = map.CalculateRoomWorldPos(0, minCol).x;
        float worldMaxX = map.CalculateRoomWorldPos(1, maxCol).x;
        float worldMinZ = map.CalculateRoomWorldPos(minRow, 0).z;
        float worldMaxZ = map.CalculateRoomWorldPos(maxRow, 0).z;

        if (worldMinX > worldMaxX) (worldMinX, worldMaxX) = (worldMaxX, worldMinX);
        if (worldMinZ > worldMaxZ) (worldMinZ, worldMaxZ) = (worldMaxZ, worldMinZ);

        regionMin = new Vector2(worldMinX, worldMinZ);
        regionSize = new Vector2(worldMaxX - worldMinX, worldMaxZ - worldMinZ);

        if (regionSize.x <= 0f) regionSize.x = 0.01f;
        if (regionSize.y <= 0f) regionSize.y = 0.01f;
    }

    [ContextMenu("Clear Face Cache")]
    public void ClearFaceCache()
    {
        foreach (var rend in _faceRenderers)
        {
            if (rend != null)
                rend.SetPropertyBlock(null);
        }
        _faceRenderers.Clear();
        _faceTransforms.Clear();
    }

    void SaveLastParams()
    {
        _lastTex = regionTexture;
        _lastTiling = tiling;
        _lastOffset = offset;
        _lastMinRow = minRow;
        _lastMaxRow = maxRow;
        _lastMinCol = minCol;
        _lastMaxCol = maxCol;
        _lastOpacity = textureOpacity;
        _lastFadeDuration = fadeDuration;
        _lastMaxDelay = maxRandomDelay;
        _lastScale = faceScale;
        _lastHeight = faceHeight;
        _lastEnabled = mappingEnabled;
        _lastFaceName = faceChildName;
    }

    void OnDrawGizmosSelected()
    {
        if (!showRangeGizmo) return;

        GameMapManager map = GameRoot.GetManager<GameMapManager>();
        if (map == null) return;

        ComputeWorldRegion(map, out Vector2 regionMin, out Vector2 regionSize);

        Vector3 center = new Vector3(regionMin.x + regionSize.x * 0.5f, 0.05f, regionMin.y + regionSize.y * 0.5f);
        Vector3 size = new Vector3(regionSize.x, 0.05f, regionSize.y);

        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}
