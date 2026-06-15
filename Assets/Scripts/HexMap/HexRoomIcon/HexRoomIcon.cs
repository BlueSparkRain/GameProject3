using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core;

/// <summary>
/// 印章组件 —— 挂载在 HexRoomIcon 预制体上。
/// 负责设置精灵、保持世界位置和旋转跟随房间。
/// 通过静态方法 CreateForRoom 统一创建，与 ObjectPoolManager 对接。
/// </summary>
public class HexRoomIcon : MonoBehaviour
{
    const string SpriteDir = "Sprite/HexRoomIcon/";
    static Dictionary<E_HexRoomType, Sprite> _spriteCache = new Dictionary<E_HexRoomType, Sprite>();

    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] float _heightAboveRoom = 3.5f;

    [Header("漂浮动画")]
    [SerializeField] float _floatAmplitude = 0.25f;
    [SerializeField] float _floatFrequency = 1.8f;

    [Header("悬停缩放")]
    [SerializeField] float _hoverScale = 1.5f;
    [SerializeField] float _hoverDelay = 0.1f;
    [SerializeField] float _scaleLerpSpeed = 12f;

    Transform _roomTransform;
    public Transform RoomTransform => _roomTransform;

    Vector3 _originalScale;
    float _floatPhaseOffset;
    float _currentScaleTarget = 1f;
    Coroutine _hoverCoroutine;
    bool _isHovered;

    void Awake()
    {
        // _spriteRenderer 在预制体中手动拖拽赋值
        _originalScale = transform.localScale;
        _floatPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnMouseEnter()
    {
        _isHovered = true;
        if (_hoverCoroutine != null)
            StopCoroutine(_hoverCoroutine);
        _hoverCoroutine = StartCoroutine(HoverScaleRoutine(true));
    }

    void OnMouseExit()
    {
        _isHovered = false;
        if (_hoverCoroutine != null)
            StopCoroutine(_hoverCoroutine);
        _hoverCoroutine = StartCoroutine(HoverScaleRoutine(false));
    }

    IEnumerator HoverScaleRoutine(bool entering)
    {
        if (entering)
        {
            // 等待悬停延迟
            yield return new WaitForSeconds(_hoverDelay);
            // 仅当延迟后鼠标仍在图标上时，才应用放大
            if (_isHovered)
                _currentScaleTarget = _hoverScale;
        }
        else
        {
            // 鼠标离开立即还原目标，无延迟
            _currentScaleTarget = 1f;
        }
        _hoverCoroutine = null;
    }

    void Update()
    {
        // 平滑缩放至目标值，杜绝快速移入移出导致的缩放异常
        Vector3 targetScale = _originalScale * _currentScaleTarget;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, _scaleLerpSpeed * Time.deltaTime);
    }

    /// <summary>为指定房间创建图标（由 HexRoomStyleHandler.SetRoomType 调用）</summary>
    public static void CreateForRoom(Transform room, E_HexRoomType roomType)
    {
        var pool = GameRoot.GetManager<ObjectPoolManager>();
        if (pool == null) return;

        var go = pool.GetInstance(E_PoolType.HexRoomIcon_房间图标);
        if (go == null) return;

        var stamp = go.GetComponent<HexRoomIcon>();
        if (stamp == null)
        {
            pool.ReturnPool(E_PoolType.HexRoomIcon_房间图标, go);
            return;
        }

        var sprite = GetSprite(roomType);
        if (sprite == null)
            DebugManager.LogWarning(EDebugCategory.MapRoom, $"[HexRoomIcon] {roomType}: 精灵缺失，图标将以无精灵状态显示");

        stamp.AttachToRoom(room, sprite);
    }

    public void AttachToRoom(Transform room, Sprite icon)
    {
        _roomTransform = room;
        _spriteRenderer.sprite = icon;

        if (room != null)
        {
            transform.position = room.position + Vector3.up * _heightAboveRoom;
            transform.rotation = room.rotation;
        }
    }

    /// <summary>跟随房间位置和旋转，叠加漂浮动画</summary>
    void LateUpdate()
    {
        if (_roomTransform != null)
        {
            float floatOffset = Mathf.Sin(Time.time * _floatFrequency + _floatPhaseOffset) * _floatAmplitude;
            transform.position = _roomTransform.position + Vector3.up * _heightAboveRoom + Vector3.forward * floatOffset;
            transform.rotation = _roomTransform.rotation;
        }
    }

    static Sprite GetSprite(E_HexRoomType type)
    {
        if (_spriteCache.TryGetValue(type, out var cached))
            return cached;

        var sprite = Resources.Load<Sprite>($"{SpriteDir}{type}");
        if (sprite != null)
            _spriteCache[type] = sprite;

        return sprite;
    }

    /// <summary>对象池回收时重置状态</summary>
    void OnDisable()
    {
        _isHovered = false;
        _currentScaleTarget = 1f;
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
        transform.localScale = _originalScale;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }
#endif
}
