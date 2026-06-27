using UnityEngine;
using Core;

/// <summary>
/// 伤害跳字生成器。每个组件只负责一种数值类型（HP/SP/Shield/AG），
/// 创建 ScreenSpace-Overlay Canvas，跳字实例从 ObjectPoolManager 获取。
/// 自动监听战斗事件，按 StatType + 所属角色过滤。
///
/// 挂在角色 GameObject 下即可，BattleHandler 初始化后自动生效。
/// </summary>
public class FloatingTextSpawner : MonoBehaviour
{
    [Header("负责的数值类型")]
    [SerializeField] FloatingTextStatType _statType;

    [Header("跳字生成位置（世界坐标参考点）")]
    [SerializeField] Transform _spawnOrigin;

    [Header("正数（恢复/获得）颜色")]
    [SerializeField] Color _positiveColor = new Color(0.15f, 1f, 0.25f);
    [Header("负数（损失/消耗）颜色")]
    [SerializeField] Color _negativeColor = new Color(1f, 0.15f, 0.15f);
    [Header("字号")]
    [SerializeField] float _fontSize = 32f;

    [Header("过滤阈值")]
    [SerializeField] float _minAbsDelta = 0f;
    [SerializeField] bool _negativeOnly = false;

    // ── 内部：同时跳字防重叠 ──
    const float StaggerWindow = 0.25f;      // 这个时间窗口内的生成视为"同时"
    const float StaggerStepX = 55f;         // 每个后续跳字在 X 轴偏移的步长
    const float StaggerStepY = 18f;         // Y 轴微调步长
    const float BaseScreenOffsetY = 40f;    // 基础 Y 偏移（头上方）
    const int   MaxStaggerSlots = 6;        // 同时最多 staggered 数量

    int   _staggerIndex;                    // 当前窗口内的序号
    float _lastSpawnTime = float.MinValue;

    Canvas _canvas;
    Camera _cam;

    IBattlable Owner{
        get{
            if (__owner == null){
                var handler = GetComponentInParent<BattleHandler>();
                if (handler != null) __owner = handler.Self;
            }
            return __owner;
        }
    }
    IBattlable __owner;

    public FloatingTextStatType StatType => _statType;
    public Transform SpawnOrigin => _spawnOrigin;

    void Awake()
    {
        _cam = Camera.main;

        var root = GetComponentInParent<BattleHandler>();
        Transform parent = root != null ? root.transform : transform;
        Transform existing = parent.Find("FloatTextCanvas");

        if (existing != null)
        {
            _canvas = existing.GetComponent<Canvas>();
        }
        else
        {
            var canvasGo = new GameObject("FloatTextCanvas",
                typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            canvasGo.transform.SetParent(parent, false);
            canvasGo.transform.localScale = Vector3.one;
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999;
        }
    }

    void OnEnable()
    {
        EventCenter.AddEventListener<IBattlable, Vector3, E_BattleModelType, float>(
            E_EventType.Battle_ModelValueChanged, OnModelValueChanged);
    }

    void OnDisable(){
        EventCenter.RemoveEventListener<IBattlable, Vector3, E_BattleModelType, float>(
            E_EventType.Battle_ModelValueChanged, OnModelValueChanged);
    }

    void OnModelValueChanged(IBattlable battler, Vector3 worldPos, E_BattleModelType modelType, float delta)
    {
        var owner = Owner;
        if (owner == null || battler != owner) return;
        if (!MatchesStatType(modelType)) return;

        if (_negativeOnly && delta > 0) return;
        if (_minAbsDelta > 0 && Mathf.Abs(delta) < _minAbsDelta) return;

        Show(Mathf.FloorToInt(delta));
    }

    bool MatchesStatType(E_BattleModelType modelType) => _statType switch
    {
        FloatingTextStatType.HP     => modelType == E_BattleModelType.HP,
        FloatingTextStatType.SP     => modelType == E_BattleModelType.SP,
        FloatingTextStatType.Shield => modelType == E_BattleModelType.ShieldPoints,
        FloatingTextStatType.AG     => modelType == E_BattleModelType.AG,
        _ => false,
    };

    /// <summary>计算本次生成的交错偏移，确保同时出现的跳字不重叠。</summary>
    Vector2 GetStaggerOffset()
    {
        float now = Time.unscaledTime;
        if (now - _lastSpawnTime > StaggerWindow)
            _staggerIndex = 0;

        int idx = _staggerIndex % MaxStaggerSlots;
        _staggerIndex++;
        _lastSpawnTime = now;

        if (idx == 0)
            return new Vector2(0f, BaseScreenOffsetY);

        // 交替左右偏移：1→右, 2→左, 3→更右, 4→更左 ...
        int side = (idx % 2 == 1) ? 1 : -1;
        int step = (idx + 1) / 2;
        float x = side * step * StaggerStepX;
        float y = BaseScreenOffsetY - step * StaggerStepY;
        return new Vector2(x, y);
    }

    /// <summary>显示跳字。正数 = 获得/恢复，负数 = 损失/消耗。</summary>
    public void Show(int amount)
    {
        var poolMgr = GameRoot.GetManager<ObjectPoolManager>();
        if (poolMgr == null) return;

        var go = poolMgr.GetInstance(E_PoolType.FloatingText_跳字);
        if (go == null) return;

        var ft = go.GetComponent<DamageFloatingText>();
        if (ft == null) return;

        go.transform.SetParent(_canvas.transform, false);

        Vector3 screenPos = GetSpawnScreenPos();
        bool positive = amount >= 0;
        Color color = positive ? _positiveColor : _negativeColor;
        string text = $"{(positive ? "+" : "")}{amount}";

        Vector2 staggerOffset = GetStaggerOffset();
        int staggerIdx = _staggerIndex - 1; // 传给动画层用于微调 timing

        ft.Play(screenPos, text, color, _fontSize, staggerOffset, staggerIdx, () =>
        {
            poolMgr.ReturnPool(E_PoolType.FloatingText_跳字, go);
        });
    }

    /// <summary>自定义文字</summary>
    public void ShowCustom(string text, Color? colorOverride = null)
    {
        var poolMgr = GameRoot.GetManager<ObjectPoolManager>();
        if (poolMgr == null) return;

        var go = poolMgr.GetInstance(E_PoolType.FloatingText_跳字);
        if (go == null) return;

        var ft = go.GetComponent<DamageFloatingText>();
        if (ft == null) return;

        go.transform.SetParent(_canvas.transform, false);

        Vector3 screenPos = GetSpawnScreenPos();
        Color color = colorOverride ?? _positiveColor;

        Vector2 staggerOffset = GetStaggerOffset();
        int staggerIdx = _staggerIndex - 1;

        ft.Play(screenPos, text, color, _fontSize, staggerOffset, staggerIdx, () =>
        {
            poolMgr.ReturnPool(E_PoolType.FloatingText_跳字, go);
        });
    }

    Vector3 GetSpawnScreenPos()
    {
        Vector3 worldPos = _spawnOrigin != null ? _spawnOrigin.position : transform.position;
        if (_cam != null && worldPos.z > 0.1f)
            return _cam.WorldToScreenPoint(worldPos);
        return worldPos;
    }
}
