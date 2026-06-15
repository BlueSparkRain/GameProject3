using Core;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 区域纹理映射器 —— 独立于 GameMapManager 的组件。
/// 通过 HexFaceTag 查找独立的面片（不再依赖 HexRoom 子物体），
/// 所有面片共用一个 MaterialPropertyBlock。
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
    public float fadeDuration = 0.5f;
    public float maxRandomDelay = 1.5f;

    [Header("面片控制")]
    public Vector3 faceScale = Vector3.one;
    public float faceHeight = -9.7f;
    public bool mappingEnabled = true;

    [Header("调试")]
    public bool showRangeGizmo = true;

    List<Renderer> _faceRenderers = new List<Renderer>();
    List<Transform> _faceTransforms = new List<Transform>();
    Material _sharedMaterial;
    MaterialPropertyBlock _sharedMPB;
    bool _materialInitialized;

    float _transitionStartTime = -1f;
    float _fromOpacity;
    float _targetOpacity = 1f;

    Texture2D _lastTex;
    Vector2 _lastTiling, _lastOffset;
    int _lastMinRow, _lastMaxRow, _lastMinCol, _lastMaxCol;
    float _lastOpacity, _lastFadeDuration, _lastMaxDelay;
    Vector3 _lastScale;
    float _lastHeight;
    bool _lastEnabled;

    static readonly int ShaderProp_RegionMin = Shader.PropertyToID("_RegionMin");
    static readonly int ShaderProp_RegionSize = Shader.PropertyToID("_RegionSize");
    static readonly int ShaderProp_MainTex = Shader.PropertyToID("_MainTex");
    static readonly int ShaderProp_Tiling = Shader.PropertyToID("_Tiling");
    static readonly int ShaderProp_Offset = Shader.PropertyToID("_Offset");
    static readonly int ShaderProp_Opacity = Shader.PropertyToID("_Opacity");
    static readonly int ShaderProp_TransitionStart = Shader.PropertyToID("_TransitionStartTime");
    static readonly int ShaderProp_FadeDuration = Shader.PropertyToID("_FadeDuration");
    static readonly int ShaderProp_FromOpacity = Shader.PropertyToID("_FromOpacity");
    static readonly int ShaderProp_TargetOpacity = Shader.PropertyToID("_TargetOpacity");
    static readonly int ShaderProp_MaxRandomDelay = Shader.PropertyToID("_MaxRandomDelay");

    void Start(){
        InitMaterial();
        EventCenter.AddEventListener(E_EventType.LoadMapEnd, OnLoadMapEnd);
        if (GameRoot.GetManager<GameMapManager>()?.HexRoomMap?.Count > 0)
            RefreshMapping();
    }
    void OnDestroy(){
        EventCenter.RemoveEventListener(E_EventType.LoadMapEnd, OnLoadMapEnd);
        ClearFaceCache();
        if (_sharedMaterial != null){
            if (Application.isPlaying) Destroy(_sharedMaterial);
            else DestroyImmediate(_sharedMaterial);
        }
    }

    void Update(){
        bool propChanged = _lastTiling != tiling || _lastOffset != offset || _lastTex != regionTexture ||
                           _lastOpacity != textureOpacity ||
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
            _lastMinCol != minCol || _lastMaxCol != maxCol){
            RefreshMapping();
            SaveLastParams();
        }
        UpdateRendererEnabled();
    }
    void InitMaterial(){
        if (_materialInitialized) return;
        Shader shader = Shader.Find("Custom/RegionTextureMapper");
        if (shader == null){
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
        // 等待HexRoomMap稳定（连续5次无变化，且>0），确保所有房间+HexFace创建完毕
        while (stable < 5)
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
            DebugManager.LogWarning(EDebugCategory.MapRoom, "[RegionTextureMapper] GameMapManager 未就绪或房间为空");
            return;
        }
        if (_sharedMaterial == null) InitMaterial();
        if (_sharedMaterial == null) return;

        // 收集所有已初始化的 HexFaceTag，按 (row, col) 建立快速查找
        // 跳过 faceRenderer==null 的（池子里未激活、未调用 Init 的实例）
        var allFaces = FindObjectsOfType<HexFaceTag>();
        var faceLookup = new Dictionary<Vector2Int, HexFaceTag>();
        foreach (var face in allFaces)
        {
            if (face == null || face.faceRenderer == null) continue;
            var key = new Vector2Int(face.row, face.col);
            if (!faceLookup.ContainsKey(key))
                faceLookup[key] = face;
        }

        // 遍历房间映射表，匹配范围内的 HexFace
        foreach (var kvp in map.HexRoomMap)
        {
            HexRoomTag room = kvp.Value;
            if (room == null) continue;

            int row = room.row;
            int col = room.col;
            if (row < minRow || row > maxRow || col < minCol || col > maxCol) continue;

            if (!faceLookup.TryGetValue(new Vector2Int(row, col), out var faceTag)) continue;

            Renderer rend = faceTag.faceRenderer;
            if (rend == null) continue;

            rend.sharedMaterial = _sharedMaterial;
            rend.enabled = true;
            _faceRenderers.Add(rend);
            _faceTransforms.Add(faceTag.transform);
        }

        //Debug.Log($"[RegionTextureMapper] 找到 {_faceRenderers.Count} 个独立 HexFace Renderer");
        ResetFadeTransition(fromZero: true);
        ApplySharedMPB();
        ApplyFaceTransforms();
        SaveLastParams();
    }

    public void ApplySharedMPB()
    {
        GameMapManager map = GameRoot.GetManager<GameMapManager>();
        if (map == null) return;

        ComputeWorldRegion(map, out Vector2 regionMin, out Vector2 regionSize);

        _sharedMPB.SetTexture(ShaderProp_MainTex, regionTexture);
        _sharedMPB.SetVector(ShaderProp_RegionMin, new Vector4(regionMin.x, regionMin.y, 0f, 0f));
        _sharedMPB.SetVector(ShaderProp_RegionSize, new Vector4(regionSize.x, regionSize.y, 0f, 0f));
        _sharedMPB.SetVector(ShaderProp_Tiling, new Vector4(tiling.x, tiling.y, 0f, 0f));
        _sharedMPB.SetVector(ShaderProp_Offset, new Vector4(offset.x, offset.y, 0f, 0f));
        _sharedMPB.SetFloat(ShaderProp_MaxRandomDelay, maxRandomDelay);

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
            Vector3 pos = t.position;
            pos.y = faceHeight;
            t.position = pos;
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
