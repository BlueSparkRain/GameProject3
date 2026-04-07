using UnityEngine;

/// <summary>
/// 全视角适配 角色射线检测器（正交/透视通用·2D/3D自适应·极致性能）
/// </summary>
public class CharacterRayCaster : MonoSceneManager
{
    //[Header("相机配置（为空自动使用主相机）")]
    //[Tooltip("指定检测相机（正交/透视均可），留空自动使用Camera.main")]
    //public Camera targetCamera;

    [Header("检测配置")]
    [Tooltip("角色所在层")]
    public LayerMask characterLayer;

    // 性能优化：无GC缓存数组 + 相机缓存（避免每帧查找）
    private readonly RaycastHit[] _3DHitCache = new RaycastHit[1];
    private readonly RaycastHit2D[] _2DHitCache = new RaycastHit2D[1];
    private Camera _cachedCamera; // 最终使用的相机（缓存）


    protected override void Awake()
    {
        base.Awake();
    //    // 自动初始化相机
        InitializeCamera();

    }

    /// <summary>
    /// 自动初始化相机：手动指定为空 → 自动使用场景主相机
    /// </summary>
    private void InitializeCamera()
    {
        characterLayer = LayerMask.GetMask("Character");
        _cachedCamera ??= Camera.main;

        if (_cachedCamera == null){
            Debug.LogError("射线检测失败：场景中没有找到主相机！请给相机添加MainCamera标签，或手动指定targetCamera", this);
        }
    }

    /// <summary>
    /// 核心点击检测（正交/透视全适配 · 2D/3D自适应 · 无GC）
    /// </summary>
    private void CheckCharacterClick()
    {
        // 相机为空直接返回（防止报错）
        if (_cachedCamera == null) return;

        // ✅ 关键：Unity原生API，自动适配 正交相机 / 透视相机，无需任何额外判断！
        Ray ray = _cachedCamera.ScreenPointToRay(Input.mousePosition);

        // 1. 3D角色检测（无GC）
        int hit3DCount = Physics.RaycastNonAlloc(ray, _3DHitCache, Mathf.Infinity, characterLayer);
        if (hit3DCount > 0)
        {
            ExecuteClick(_3DHitCache[0].collider);
            return;
        }

        // 2. 2D角色检测（无GC）
        int hit2DCount = Physics2D.GetRayIntersectionNonAlloc(ray, _2DHitCache, Mathf.Infinity, characterLayer);
        if (hit2DCount > 0)
        {
            ExecuteClick(_2DHitCache[0].collider);
        }
    }
    IClickableCharacter currentCharacter;
    /// <summary>
    /// 执行点击回调（3D碰撞体）
    /// </summary>
    private void ExecuteClick(Collider collider)
    {
        if (collider.TryGetComponent(out IClickableCharacter character))
        {
            if (currentCharacter == null) { 
                currentCharacter=character;
            }
            else if (currentCharacter!=null && currentCharacter != character) {
                currentCharacter.OffClick();
                currentCharacter=character;
            }
            character.OnClick();
        }
    }

    /// <summary>
    /// 执行点击回调（2D碰撞体）
    /// </summary>
    private void ExecuteClick(Collider2D collider)
    {
        if (collider.TryGetComponent(out IClickableCharacter character))
        {
            if (currentCharacter == null)
            {
                currentCharacter = character;
            }
            else if (currentCharacter != null && currentCharacter != character)
            {
                currentCharacter.OffClick();
                currentCharacter = character;
            }
            character.OnClick();
        }
    }

    public override void MgrUpdate(float deltaTime)
    {
        // 仅点击时检测，0 idle消耗
        if (Input.GetMouseButtonDown(0)) CheckCharacterClick();
    }
}