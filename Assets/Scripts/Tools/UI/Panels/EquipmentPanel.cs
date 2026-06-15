using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;

/// <summary>
/// 装备配置面板——从EquipBacketManager取可选装备，选中后同步到EquipSlotItem和EquipHandler
/// </summary>
public class EquipmentPanel : UIPanelBase
{
    [Header("8个装备槽位")]
    public EquipSlotItem[] slotItems;

    [Header("可选装备列表容器(GridLayoutGroup)")]
    public Transform equipListContainer;

    [Header("可选装备项预制件(EquipItem)")]
    public GameObject equipItemPrefab;

    [Header("无可选装备提示文本")]
    public TMP_Text emptyHintText;

    [Header("当前选中部位文本")]
    public TMP_Text currentSlotLabel;

    [Header("属性展示")]
    public PropertyItem[] propertyItems;

    [Header("关闭按钮")]
    public UnityEngine.UI.Button closeButton;

    EquipSlotItem currentSelectedSlot;
    EquipHandler playerEquipHandler;
    CharacterData playerCharacterData;
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
                DebugManager.LogWarning(EDebugCategory.UIPanel,$"[EquipmentPanel] slotItems[{i}] 为空");
        }
    }

    public override void Show(){
        base.Show();
        FindPlayerEquipHandler();
        RestoreSlotDisplays();
        ClearEquipList();
        RefreshPropertyItems();
    }
    void OnSlotClicked(EquipSlotItem slot)
    {
        if (slot == null) return;
        DebugManager.Log(EDebugCategory.UIPanel,$"[EquipmentPanel] 点击槽位: {slot.SlotType}");
        currentSelectedSlot = slot;
        PopulateEquipList(slot.SlotType);
        if (currentSlotLabel != null)
        {
            currentSlotLabel.text = $"{GetSlotChineseName(slot.SlotType)}";
            currentSlotLabel.gameObject.SetActive(true);
        }
    }

    void PopulateEquipList(E_EquipmentSlot slotType)
    {
        ClearEquipList();

        var backetMgr = GameRoot.GetManager<EquipBacketManager>();
        if (backetMgr == null)
        {
            DebugManager.LogWarning(EDebugCategory.UIPanel,"[EquipmentPanel] EquipBacketManager未注册");
            return;
        }

        var candidates = backetMgr.GetBySlot(slotType);
        if (candidates.Count == 0){
            DebugManager.Log(EDebugCategory.UIPanel,$"[EquipmentPanel] 背包中没有{slotType}部位的装备");
            ShowEmptyHint(slotType);
            return;
        }
        HideEmptyHint();
        // 收集已在其他槽位装备的物品ID，避免重复装备
        var equippedIds = new HashSet<string>();
        foreach (var slot in slotItems){
            if (slot != null && slot != currentSelectedSlot && slot.HasEquipped)
                equippedIds.Add(slot.CurrentEquipData.equipId);
        }
        foreach (var equip in candidates){
            if (equippedIds.Contains(equip.equipId)) continue;
            var obj = Instantiate(equipItemPrefab, equipListContainer);
            obj.SetActive(true);
            var item = obj.GetComponent<EquipItem>();
            if (item != null){
                item.SetData(equip);
                item.SetInteractable(true);
                item.onSelected += OnEquipItemSelected;
            }
            spawnedListItems.Add(obj);
        }
        if (equipListContainer != null)
            equipListContainer.gameObject.SetActive(true);
        DebugManager.Log(EDebugCategory.UIPanel,$"[EquipmentPanel] 为{slotType}显示了{spawnedListItems.Count}件背包装备");
    }

    void OnEquipItemSelected(EquipItem item)
    {
        if (item?.EquipData == null || currentSelectedSlot == null) return;

        var equipData = item.EquipData;
        DebugManager.Log(EDebugCategory.UIPanel,$"[EquipmentPanel] 选择装备: {equipData.GetEquipName()} → {currentSelectedSlot.SlotType}");

        // 同步到EquipSlotItem(UI显示)
        currentSelectedSlot.SetEquippedItem(equipData);

        // 同步到EquipHandler(绿值加成，指定槽位)
        playerEquipHandler?.EquipToSlot(equipData, currentSelectedSlot.SlotType);

        ClearEquipList();
        RefreshPropertyItems();
    }

    void FindPlayerEquipHandler()
    {
        if (playerEquipHandler != null) return;
        var tags = FindObjectsOfType<CharacterHandler>();
        foreach (var tag in tags)
        {
            if (tag.isPlayer && tag.CharacterData != null)
            {
                playerEquipHandler = tag.CharacterData.EquipHandler;
                playerCharacterData = tag.CharacterData;
                DebugManager.Log(EDebugCategory.UIPanel,"[EquipmentPanel] 已绑定玩家EquipHandler");
                return;
            }
        }
        DebugManager.LogWarning(EDebugCategory.UIPanel,"[EquipmentPanel] 未找到玩家CharacterDataTag，装备将不会同步绿值");
    }

    void RestoreSlotDisplays()
    {
        if (playerEquipHandler == null) return;
        foreach (var slot in slotItems)
        {
            if (slot == null) continue;
            var equipped = playerEquipHandler.GetEquipped(slot.SlotType);
            slot.SetEquippedItem(equipped);
        }
    }

    void ShowEmptyHint(E_EquipmentSlot slotType)
    {
        if (emptyHintText != null)
        {
            emptyHintText.text = $"目前尚无可选装备";
            emptyHintText.gameObject.SetActive(true);
        }
    }

    void HideEmptyHint()
    {
        if (emptyHintText != null)
            emptyHintText.gameObject.SetActive(false);
    }

    static string GetSlotChineseName(E_EquipmentSlot slot) => slot switch
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
        HideEmptyHint();
        if (currentSlotLabel != null)
            currentSlotLabel.text = "无_";
    }

    void RefreshPropertyItems()
    {
        if (propertyItems == null) return;
        if (playerCharacterData == null) return;
        foreach (var item in propertyItems)
        {
            if (item != null)
                item.Refresh(playerCharacterData);
        }
    }
}
