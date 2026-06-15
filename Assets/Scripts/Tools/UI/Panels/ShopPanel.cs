using Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : UIPanelBase
{
    [Header("商品列表容器")]
    public Transform itemContainer;
    [Header("装备商品预制体(含ShopGoodItem组件)")]
    public GameObject equipGoodItemPrefab;
    [Header("金币文本")]
    public TMP_Text goldText;
    [Header("关闭按钮")]
    public Button closeButton;

    List<EquipData> generatedEquipments;

    protected override void OnInit()
    {
        base.OnInit();
        closeButton?.onClick.AddListener(Hide);
        EventCenter.AddEventListener(E_EventType.UpdateUIGold, RefreshGoldDisplay);
    }

    public override void Show()
    {
        base.Show();
        GenerateEquipments();
        RefreshGoldDisplay();
        RefreshShopItems();
    }

    void GenerateEquipments()
    {
        var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
        int chaosLevel = chaosMgr != null ? chaosMgr.currentLevel : 1;
        generatedEquipments = EquipmentGenerator.GenerateBatch(5, chaosLevel);
        DebugManager.Log(EDebugCategory.UIPanel,$"[ShopPanel] 生成了{generatedEquipments.Count}件随机装备(混沌等级:{chaosLevel})");
    }

    int GetChaosAdjustedPrice(int basePrice)
    {
        var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
        float multiplier = chaosMgr != null ? chaosMgr.ShopPriceMultiplier : 1f;
        return Mathf.RoundToInt(basePrice * multiplier);
    }

    void RefreshShopItems()
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        // 装备商品
        if (equipGoodItemPrefab != null && generatedEquipments != null)
        {
            foreach (var equip in generatedEquipments)
            {
                var obj = Instantiate(equipGoodItemPrefab, itemContainer);
                obj.SetActive(true);
                int price = GetChaosAdjustedPrice(equip.price);
                var goodItem = obj.GetComponent<ShopGoodItem>();
                if (goodItem != null)
                    goodItem.Populate(equip, price);
            }
        }

    }

    public void RefreshGoldDisplay()
    {
        if (goldText != null)
        {
            var gm = GameRoot.GetManager<GoldManager>();
            int gold = gm != null ? gm.Gold : 0;
            goldText.text = $"{gold}";
        }
    }
}
