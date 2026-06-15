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

    [Header("相对生成点的屏幕偏移")]
    [SerializeField] Vector2 _screenOffset;

    [Header("过滤阈值")]
    [SerializeField] float _minAbsDelta = 0f;
    [SerializeField] bool _negativeOnly = false;

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

        // 同一 BattleHandler 下共享一个 FloatTextCanvas，避免重复创建
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

        Show(Mathf.RoundToInt(delta));
    }

    bool MatchesStatType(E_BattleModelType modelType) => _statType switch
    {
        FloatingTextStatType.HP     => modelType == E_BattleModelType.HP,
        FloatingTextStatType.SP     => modelType == E_BattleModelType.SP,
        FloatingTextStatType.Shield => modelType == E_BattleModelType.ShieldPoints,
        FloatingTextStatType.AG     => modelType == E_BattleModelType.AG,
        _ => false,
    };

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
        float scale = Random.Range(DamageFloatingText.ScaleMin, DamageFloatingText.ScaleMax);

        ft.Play(screenPos, text, color, _fontSize, scale, _screenOffset, () =>
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
        float scale = Random.Range(DamageFloatingText.ScaleMin, DamageFloatingText.ScaleMax);

        ft.Play(screenPos, text, color, _fontSize, scale, _screenOffset, () =>
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
