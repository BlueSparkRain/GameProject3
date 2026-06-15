using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 装备槽位组件——配置部位枚举，内部存储EquipData
/// 点击按钮打开可选装备列表，选择后直接显示
/// </summary>
public class EquipSlotItem : MonoBehaviour{
    [Header("部位类型")]
    public E_EquipmentSlot slotType;

    [Header("部位图标")]
    public Image slotIcon;

    [Header("部位名称")]
    public TextMeshProUGUI slotNameText;

    [Header("槽位按钮(点击打开可选列表)")]
    public Button slotButton;

    [Header("已装备图标")]
    public Image equipIcon;

    [Header("已装备名称")]
    public TextMeshProUGUI equipNameText;

    [Header("已装备标记(有装备时显示)")]
    public GameObject equippedTag;

    /// <summary>当前存储的装备数据</summary>
    public EquipData CurrentEquipData { get; private set; }

    /// <summary>当前是否佩戴有装备</summary>
    public bool HasEquipped => CurrentEquipData != null;

    public E_EquipmentSlot SlotType => slotType;

    public System.Action<EquipSlotItem> onClicked;

    void Start(){
        if (slotButton != null)
            slotButton.onClick.AddListener(() => onClicked?.Invoke(this));
        LoadSlotIcon();
        UpdateSlotName();
        if (!HasEquipped)
            ShowEquippedDisplay(false);
    }

    void LoadSlotIcon(){
        if (slotIcon == null) return;
        var sprite = EquipIconPath.LoadSlotIcon(slotType);
        if (sprite != null) slotIcon.sprite = sprite;
    }

    void UpdateSlotName()
    {
        if (slotNameText != null)
            slotNameText.text = slotType switch
            {
                E_EquipmentSlot.Sword => "剑",
                E_EquipmentSlot.Spear => "枪",
                E_EquipmentSlot.Bow => "弓",
                E_EquipmentSlot.Shield => "盾",
                E_EquipmentSlot.Head => "头部防具",
                E_EquipmentSlot.Body => "身体防具",
                E_EquipmentSlot.Accessory1 => "饰品1",
                E_EquipmentSlot.Accessory2 => "饰品2",
                _ => "未知",
            };
    }

    /// <summary>直接存储EquipData并刷新显示</summary>
    public void SetEquippedItem(EquipData data)
    {
        CurrentEquipData = data;
        if (data == null)
        {
            ShowEquippedDisplay(false);
            return;
        }
        GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/ConfirmAction");
        ShowEquippedDisplay(true);
        if (equipNameText != null)
            equipNameText.text = data.GetEquipName();
        if (equipIcon != null)
        {
            var sprite = EquipIconPath.LoadEquipIcon(data.iconResourcePath)
                      ?? EquipIconPath.LoadSlotIcon(data.slot);
            if (sprite != null) equipIcon.sprite = sprite;
        }
    }

    void ShowEquippedDisplay(bool show)
    {
        if (equipIcon != null) equipIcon.gameObject.SetActive(show);
        if (equipNameText != null) equipNameText.gameObject.SetActive(show);
        if (equippedTag != null) equippedTag.SetActive(show);
    }
}
