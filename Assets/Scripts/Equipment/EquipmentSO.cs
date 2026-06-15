using UnityEngine;

/// <summary>
/// 装备配置SO——在编辑器中创建，定义装备的固定属性(部位/名称/售价/图标)
/// 词条仍由EquipmentGenerator按规则随机生成
/// </summary>
[CreateAssetMenu(menuName = "装备/EquipmentSO", fileName = "Equip_", order = 0)]
public class EquipmentSO : ScriptableObject
{
    [Header("部位")]
    public E_EquipmentSlot slot;

    [Header("装备名称")]
    public string equipName;

    [Header("基础售价")]
    public int basePrice = 100;

    [Header("图标(仅编辑器预览)")]
    public Sprite icon;

    [Header("图标路径(Resources相对路径, 如 Sprite/EquipmentIcons/Icon_Sword)")]
    public string iconResourcePath;
}
