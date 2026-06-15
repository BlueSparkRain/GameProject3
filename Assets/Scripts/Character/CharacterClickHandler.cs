using UnityEngine;

/// <summary>
/// 角色点击处理器（挂载到2D/3D角色身上）
/// </summary>
[RequireComponent(typeof(GameObject))]
public class CharacterClickHandler : MonoBehaviour, IClickableCharacter
{
    [Header("角色配置")]
    [Tooltip("勾选=3D角色 | 取消=2D角色")]
    public bool is3DCharacter = true;

    public GameObject ModelLayer;

    public void OffClick()
    {
        int layerID = LayerMask.NameToLayer("Default");
        ModelLayer.layer = layerID;
    }

    /// <summary>
    /// 点击回调（你可以在这里扩展任何逻辑：选中、移动、技能等）
    /// </summary>
    public void OnClick()
    {
        int layerID = LayerMask.NameToLayer("OutLine");
        ModelLayer.layer =layerID;
        // 需求：打印角色名字
        DebugManager.Log(EDebugCategory.Character, $"【点击角色】：{gameObject.name}");
    }

    // 编辑器校验：自动检查碰撞体（避免漏加导致检测失效）
    private void OnValidate()
    {
        if (is3DCharacter && GetComponent<Collider>() == null)
            DebugManager.LogWarning(EDebugCategory.Character, $"角色 {name} 是3D类型，请添加 Collider（胶囊/盒子/球体）");

        if (!is3DCharacter && GetComponent<Collider2D>() == null)
            DebugManager.LogWarning(EDebugCategory.Character, $"角色 {name} 是2D类型，请添加 Collider2D");
    }
}