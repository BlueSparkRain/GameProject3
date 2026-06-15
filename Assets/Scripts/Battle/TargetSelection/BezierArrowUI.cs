using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 贝塞尔曲线箭头UI组件 — 纯UI渲染，在起点和终点之间生成节点Dot形成曲线，末端放置箭头。
/// 节点数控制曲线"软硬"：越少越刚直，越多越柔顺。
/// </summary>
public class BezierArrowUI : MonoBehaviour
{
    [Header("Dot预制件")]
    public GameObject dotPrefab;
    [Header("箭头预制件（放置于曲线末端）")]
    public GameObject arrowHeadPrefab;
    [Header("节点数量（控制曲线软硬）")]
    [Range(5, 40)]
    public int nodeCount = 15;
    [Header("控制点水平张力")]
    public float horizontalTension = 0.4f;
    [Header("垂直弧高（正=上拱，负=下凹）")]
    [Range(-1f, 1f)]
    public float verticalArc = 0.3f;
    [Header("Dot起点水平偏移")]
    public float startOffsetX = 0f;
    [Header("Dot起点垂直偏移")]
    public float startOffsetY = 0f;
    [Header("Dot最小缩放（根部）")]
    public float minDotScale = 0.2f;
    [Header("Dot最大缩放（头部）")]
    public float maxDotScale = 1.5f;
    [Header("Dot旋转偏移（补偿预制件尖角朝向）")]
    [Tooltip("0=尖角在X+方向, -90=尖角在Y+方向, 90=尖角在Y-方向")]
    public float dotRotationOffset = -90f;
    [Header("箭头旋转偏移")]
    public float arrowRotationOffset = -90f;
    [Header("全局旋转（所有Dot+箭头统一附加角度）")]
    public float globalRotation = 0f;
    [Header("级联强度（Dot依次受前方角度影响）")]
    [Range(0, 1)] public float cascadeStrength = 0.6f;
    [Header("颜色渐变")]
    public Color startColor = Color.white;
    public Color endColor = Color.white;
    [Range(0, 1)] public float colorTransition = 0.5f;

    List<GameObject> _dots = new List<GameObject>();
    GameObject _arrowHead;
    Vector2 _startScreenPos;
    Vector2 _endScreenPos;

    /// <summary>设置起点（SkillIcon在屏幕上的位置），同时将ArrowRoot定位到起点</summary>
    public void SetStartPoint(Vector2 screenPos)
    {
        _startScreenPos = screenPos;
        // 将ArrowRoot移动到起点屏幕位置
        RectTransform parentRt = transform.parent as RectTransform;
        if (parentRt != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, screenPos, null, out Vector2 localPos);
            ((RectTransform)transform).anchoredPosition = localPos;
        }
    }

    /// <summary>同时更新起点和终点并重算曲线（每帧调用）</summary>
    public void UpdateCurve(Vector2 startScreenPos, Vector2 endScreenPos)
    {
        _startScreenPos = startScreenPos;
        _endScreenPos = endScreenPos;
        // 每帧同步 ArrowRoot 位置到起点
        RectTransform parentRt = transform.parent as RectTransform;
        if (parentRt != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, startScreenPos, null, out Vector2 localPos);
            ((RectTransform)transform).anchoredPosition = localPos;
        }
        EnsureDotCount();
        EnsureArrowHead();
        RecalculateCurve();
    }

    /// <summary>更新终点并重算曲线（start 不动时用）</summary>
    public void UpdateCurve(Vector2 endScreenPos)
    {
        _endScreenPos = endScreenPos;
        EnsureDotCount();
        EnsureArrowHead();
        RecalculateCurve();
    }

    /// <summary>显示曲线</summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>隐藏并回收所有节点</summary>
    public void Hide()
    {
        ClearAllDots();
        ClearArrowHead();
        gameObject.SetActive(false);
    }

    void EnsureDotCount()
    {
        while (_dots.Count < nodeCount)
        {
            var dot = Instantiate(dotPrefab, transform);
            dot.transform.localScale = Vector3.one;
            _dots.Add(dot);
        }
        while (_dots.Count > nodeCount)
        {
            var last = _dots[_dots.Count - 1];
            _dots.RemoveAt(_dots.Count - 1);
            Destroy(last);
        }
    }

    void EnsureArrowHead()
    {
        if (arrowHeadPrefab == null) return;
        if (_arrowHead == null)
        {
            _arrowHead = Instantiate(arrowHeadPrefab, transform);
            _arrowHead.transform.localScale = Vector3.one;
        }
    }

    void ClearAllDots()
    {
        foreach (var d in _dots)
            if (d != null) Destroy(d);
        _dots.Clear();
    }

    void ClearArrowHead()
    {
        if (_arrowHead != null)
        {
            Destroy(_arrowHead);
            _arrowHead = null;
        }
    }

    void RecalculateCurve()
    {
        // p0：曲线从ArrowRoot自身位置 + 偏移开始
        Vector2 p0 = new Vector2(startOffsetX, startOffsetY);
        // p3：将屏幕坐标终点转为ArrowRoot本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, _endScreenPos, null, out Vector2 p3);

        float dx = Mathf.Abs(p3.x - p0.x);
        float dy = p3.y - p0.y;
        float hPull = dx * horizontalTension;
        float vPull = dx * verticalArc * 0.4f;
        int sign = p3.x >= p0.x ? 1 : -1;

        // 控制点同时拉水平和垂直，形成真正的三次贝塞尔弧线
        Vector2 p1 = p0 + new Vector2(hPull * sign, vPull);
        Vector2 p2 = p3 - new Vector2(hPull * sign, -vPull);

        // ① 先放置所有 Dot 位置 + 缩放 + 颜色
        for (int i = 0; i < _dots.Count; i++)
        {
            float t = _dots.Count == 1 ? 0.5f : (float)i / (_dots.Count - 1);
            Vector2 bezierPt = CubicBezier(p0, p1, p2, p3, t);
            var dotRt = _dots[i].transform as RectTransform;
            dotRt.anchoredPosition = bezierPt;

            float scale = Mathf.Lerp(minDotScale, maxDotScale, t);
            dotRt.localScale = new Vector3(scale, scale, 1f);

            // 颜色根据 colorTransition 控制过渡区间
            float colorT = Mathf.InverseLerp(0, Mathf.Max(0.001f, colorTransition), t);
            Color dotColor = Color.Lerp(startColor, endColor, colorT);
            SetRendererColor(_dots[i], dotColor);
        }

        // ② 箭头始终朝向目标
        float headAngle = 0f;
        Vector2 toTarget = p3 - p0;
        if (toTarget.magnitude > 0.001f)
            headAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + arrowRotationOffset + globalRotation;

        if (_arrowHead != null)
        {
            var arrowRt = _arrowHead.transform as RectTransform;
            arrowRt.anchoredPosition = p3;
            arrowRt.rotation = Quaternion.Euler(0, 0, headAngle);
            SetRendererColor(_arrowHead, endColor);
        }

        // ③ Dot 级联反向传播：从箭头往前，每个 Dot 混合贝塞尔切线 + 前方角度
        float aheadAngle = headAngle;
        for (int i = _dots.Count - 1; i >= 0; i--)
        {
            var dotRt = _dots[i].transform as RectTransform;
            float t = _dots.Count == 1 ? 0.5f : (float)i / (_dots.Count - 1);
            Vector2 tangent = CubicBezierTangent(p0, p1, p2, p3, t);

            float tangentAngle = headAngle; // fallback
            if (tangent.magnitude > 0.001f)
                tangentAngle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg + dotRotationOffset + globalRotation;

            // 向箭头方向混合，级联感
            float dotAngle = Mathf.LerpAngle(tangentAngle, aheadAngle, cascadeStrength);
            dotRt.rotation = Quaternion.Euler(0, 0, dotAngle);
            aheadAngle = dotAngle;
        }
    }

    /// <summary>三次贝塞尔: B(t) = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3</summary>
    public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1f - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;
        return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
    }

    void SetRendererColor(GameObject go, Color color)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.color = color; return; }
        var img = go.GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.color = color;
    }

    /// <summary>三次贝塞尔切线: B'(t) = 3(1-t)²(P1-P0) + 6(1-t)t(P2-P1) + 3t²(P3-P2)</summary>
    public static Vector2 CubicBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1f - t;
        return 3f * u * u * (p1 - p0) + 6f * u * t * (p2 - p1) + 3f * t * t * (p3 - p2);
    }
}
