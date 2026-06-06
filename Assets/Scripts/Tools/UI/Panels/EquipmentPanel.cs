using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 装备配置面板——点击槽位生成可选列表，选择EquipItem后直接传给EquipSlotItem存储
/// </summary>
public class EquipmentPanel : UIPanelBase
{
    [Header("8个装备槽位")]
    public EquipSlotItem[] slotItems;

    [Header("可选装备列表容器(GridLayoutGroup)")]
    public Transform equipListContainer;

    [Header("可选装备项预制件(EquipItem)")]
    public GameObject equipItemPrefab;

    [Header("关闭按钮")]
    public UnityEngine.UI.Button closeButton;

    EquipSlotItem currentSelectedSlot;
    List<GameObject> spawnedListItems = new List<GameObject>();

    protected override void OnInit()
    {
        base.OnInit();
        closeButton?.onClick.AddListener(Hide);
        for (int i = 0; i < slotItems.Length; i++)
        {
            var slot = slotItems[i];
            if (slot != null)
                slot.onClicked += OnSlotClicked;
            else
                Debug.LogWarning($"[EquipmentPanel] slotItems[{i}] 为空");
        }
    }

    public override void Show()
    {
        base.Show();
        ClearEquipList();
    }

    void OnSlotClicked(EquipSlotItem slot)
    {
        if (slot == null) return;
        Debug.Log($"[EquipmentPanel] 点击槽位: {slot.SlotType}");
        currentSelectedSlot = slot;
        PopulateEquipList(slot.SlotType);
    }

    void PopulateEquipList(E_EquipmentSlot slotType)
    {
        ClearEquipList();

        var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
        int chaosLevel = chaosMgr != null ? chaosMgr.currentLevel : 1;

        for (int i = 0; i < 3; i++)
        {
            var equip = EquipmentGenerator.GenerateForSlot(slotType, chaosLevel);
            var obj = Instantiate(equipItemPrefab, equipListContainer);
            obj.SetActive(true);
            var item = obj.GetComponent<EquipItem>();
            if (item != null)
            {
                item.SetData(equip);
                item.SetInteractable(true);
                item.onSelected += OnEquipItemSelected;
            }
            spawnedListItems.Add(obj);
        }

        if (equipListContainer != null)
            equipListContainer.gameObject.SetActive(true);
        Debug.Log($"[EquipmentPanel] 为{slotType}生成了3件可选装备");
    }

    void OnEquipItemSelected(EquipItem item)
    {
        if (item?.EquipData == null || currentSelectedSlot == null) return;

        Debug.Log($"[EquipmentPanel] 选择装备: {item.EquipData.GetEquipName()} → {currentSelectedSlot.SlotType}");

        // 直接将EquipData传给当前槽位
        currentSelectedSlot.SetEquippedItem(item.EquipData);

        ClearEquipList();
    }

    void ClearEquipList()
    {
        foreach (var obj in spawnedListItems)
        {
            var item = obj.GetComponent<EquipItem>();
            if (item != null)
                item.onSelected -= OnEquipItemSelected;
            Destroy(obj);
        }
        spawnedListItems.Clear();

        if (equipListContainer != null)
            equipListContainer.gameObject.SetActive(false);
    }
}
