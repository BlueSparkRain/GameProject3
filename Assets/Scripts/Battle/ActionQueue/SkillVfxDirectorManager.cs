using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 技能 VFX 管理器 — MonoSceneManager 单例，统一管理所有技能释放的 UI 特效。
/// 从 SkillVfxConfigSO 读取预制件配置，使用对象池回收复用。
/// 自动获取施法者/目标的 BattleUI 屏幕坐标作为 VFX 起止点。
/// </summary>
public class SkillVfxDirectorManager : MonoSceneManager
{
    SkillVfxConfigSO _config;
    Canvas _canvas;
    Camera _cam;

    Dictionary<SkillDeliveryType, ISkillDeliveryExecutor> _executors;

    // 对象池
    Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();
    Transform _poolRoot;

    public float UiMoveOffsetY => 100f;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();

        _config = Resources.Load<SkillVfxConfigSO>("SOData/SkillVfxConfig");
        if (_config == null)
            Debug.LogWarning("[SkillVfxDirectorManager] SkillVfxConfigSO not found at Resources/SOData/SkillVfxConfig.asset — fallback VFX will be used.");

        _cam = Camera.main;

        var canvasGo = GameObject.Find("GlobalUICanvas");
        if (canvasGo != null)
            _canvas = canvasGo.GetComponent<Canvas>();

        // 池根节点
        var poolGo = new GameObject("VfxPoolRoot");
        poolGo.transform.SetParent(_canvas != null ? _canvas.transform : transform);
        poolGo.SetActive(false);
        _poolRoot = poolGo.transform;

        _executors = new Dictionary<SkillDeliveryType, ISkillDeliveryExecutor>
        {
            { SkillDeliveryType.Instant,    new InstantDelivery(this) },
            { SkillDeliveryType.Projectile, new ProjectileDelivery(this) },
            { SkillDeliveryType.SelfBuff,   new SelfBuffDelivery(this) },
            { SkillDeliveryType.AOE_Burst,  new AOEBurstDelivery(this) },
            { SkillDeliveryType.Enchant,    new EnchantDelivery(this) },
        };
    }

    public override void MgrUpdate(float deltaTime) { }

    protected override void MgrOnDispose()
    {
        base.MgrOnDispose();
        foreach (var q in _pool.Values)
            while (q.Count > 0)
                Object.Destroy(q.Dequeue());
        _pool.Clear();
    }

    // ── 公共 API ──

    public IEnumerator PlayDelivery(BattleAction action)
    {
        if (_executors.TryGetValue(action.DeliveryType, out var executor))
            yield return executor.Deliver(action);
        else
            yield return new WaitForSeconds(0.15f);
    }

    /// <summary>获取 IBattlable 的 BattleUI 屏幕坐标</summary>
    public Vector3 GetBattleUIPosition(IBattlable battler)
    {
        if (battler?.battleDamageHandler == null) return Vector3.zero;
        return ToScreenPos(battler.battleDamageHandler.transform.position);
    }

    /// <summary>世界坐标转屏幕坐标</summary>
    public Vector3 ToScreenPos(Vector3 worldPos)
    {
        if (_cam != null)
            return _cam.WorldToScreenPoint(worldPos);
        return worldPos;
    }

    /// <summary>获取指定投递类型对应的 VFX 预制件</summary>
    public GameObject GetVfxPrefab(SkillDeliveryType type)
    {
        return _config != null ? _config.GetPrefab(type) : null;
    }

    // ── 对象池 ──

    /// <summary>从池中获取 VFX 实例（或新建），定位到指定屏幕坐标</summary>
    public GameObject SpawnVfx(GameObject prefab, Vector3 screenPos, Quaternion rotation)
    {
        if (prefab == null)
            return CreateFallbackVfx(screenPos);

        // 确保该预制件有池
        if (!_pool.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            _pool[prefab] = queue;
        }

        // 找池中未激活的实例
        GameObject go = null;
        int safety = queue.Count;
        while (safety > 0 && queue.Count > 0)
        {
            var candidate = queue.Dequeue();
            if (candidate != null && !candidate.activeSelf)
            {
                go = candidate;
                break;
            }
            safety--;
        }

        // 无可用则新建
        if (go == null)
        {
            bool isUI = prefab.GetComponent<RectTransform>() != null;
            go = Instantiate(prefab, isUI && _canvas != null ? _canvas.transform : null);
        }

        go.transform.position = screenPos;
        go.transform.rotation = rotation;
        go.SetActive(true);
        return go;
    }

    /// <summary>回收 VFX 实例到池中</summary>
    public void ReturnToPool(GameObject go, GameObject prefabKey)
    {
        if (go == null || prefabKey == null) return;

        go.SetActive(false);
        go.transform.SetParent(_poolRoot);

        if (!_pool.TryGetValue(prefabKey, out var queue))
        {
            queue = new Queue<GameObject>();
            _pool[prefabKey] = queue;
        }
        queue.Enqueue(go);
    }

    /// <summary>延时回收（供 DOTween OnComplete 使用）</summary>
    public void ReturnToPoolDelayed(GameObject go, GameObject prefabKey, float delay)
    {
        DOVirtual.DelayedCall(delay, () => ReturnToPool(go, prefabKey));
    }

    GameObject CreateFallbackVfx(Vector3 screenPos)
    {
        if (_canvas != null)
        {
            var go = new GameObject("Vfx_Fallback", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_canvas.transform, false);
            go.transform.position = screenPos;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40, 40);
            rt.localScale = Vector3.one;
            var img = go.GetComponent<Image>();
            img.color = new Color(1, 0.8f, 0.2f, 0.85f);
            img.raycastTarget = false;
            return go;
        }
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "Vfx_Fallback";
        q.transform.position = screenPos;
        var mr = q.GetComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = new Color(1, 0.8f, 0.2f, 0.85f);
        q.transform.localScale = Vector3.one * 0.4f;
        return q;
    }

    // ── 动画助手（自动区分 UI/3D）──

    public Tweener TweenMove(GameObject go, Vector3 target, float duration)
    {
        return go.transform.DOMove(target, duration);
    }

    public Tweener TweenScale(GameObject go, Vector3 target, float duration)
    {
        return go.transform.DOScale(target, duration);
    }

    public Tweener TweenPunchScale(GameObject go, Vector3 punch, float duration, int vibrato = 1, float elasticity = 0.5f)
    {
        return go.transform.DOPunchScale(punch, duration, vibrato, elasticity);
    }

    public void TweenFadeOut(GameObject go, float duration)
    {
        var graphic = go.GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.DOColor(new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0), duration)
                .OnComplete(() => { if (go != null) Object.Destroy(go); });
            return;
        }
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null && mr.material != null)
        {
            mr.material.DOColor(new Color(1, 1, 1, 0), duration)
                .OnComplete(() => { if (go != null) Object.Destroy(go); });
            return;
        }
        Object.Destroy(go, duration);
    }
}
