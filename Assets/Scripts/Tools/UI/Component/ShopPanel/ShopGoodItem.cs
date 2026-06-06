using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店装备商品UI组件——挂载在装备选择预制件上
/// 显示装备部位图标、名称、词条描述、售价，处理购买逻辑
/// </summary>
public class ShopGoodItem : MonoBehaviour
{
    [Header("装备部位图标")]
    public Image slotImage;

    [Header("装备名称")]
    public TextMeshProUGUI equipNameText;

    [Header("词条加成描述")]
    public TextMeshProUGUI affixDescText;

    [Header("售价")]
    public TextMeshProUGUI priceText;

    [Header("选择按钮")]
    public Button selectButton;

    [Header("售罄遮罩图")]
    public Image soldImage;

    [Header("购买/售罄文字")]
    public Text soldText;

    EquipData equipData;
    int adjustedPrice;
    bool isSold;

    void Start()
    {
        selectButton?.onClick.AddListener(OnSelectClicked);
        ResetSoldState();
    }

    void ResetSoldState()
    {
        isSold = false;
        if (selectButton != null)
            selectButton.interactable = true;
        if (soldImage != null)
            soldImage.gameObject.SetActive(false);
        if (soldText != null)
            soldText.text = "购买";
    }

    /// <summary>填充装备数据到UI</summary>
    public void Populate(EquipData data, int price)
    {
        equipData = data;
        adjustedPrice = price;
        ResetSoldState();

        if (equipNameText != null)
            equipNameText.text = data.GetEquipName();

        if (affixDescText != null)
            affixDescText.text = data.GetAffixDescription();

        if (priceText != null)
            priceText.text = $"{price} G";

        if (slotImage != null)
            TryLoadSlotSprite(data.slot);
    }

    void TryLoadSlotSprite(E_EquipmentSlot slot)
    {
        if (slotImage == null) return;
        var sprite = EquipIconPath.LoadSlotIcon(slot);
        if (sprite != null)
            slotImage.sprite = sprite;
    }

    void OnSelectClicked()
    {
        if (equipData == null || !equipData.IsValid() || isSold) return;

        var goldMgr = GameRoot.GetManager<GoldManager>();
        if (goldMgr == null) return;

        if (!goldMgr.SpendGold(adjustedPrice))
        {
            GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(
                E_UIPanelType.MessagePanel,
                p => p.SetMessage("金币不足，无法购买", null));
            return;
        }

        var backetMgr = GameRoot.GetManager<EquipBacketManager>();
        backetMgr?.AddEquipment(equipData);

        PrintEquipDetails();


        // 播放购买音效
        GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/SoldItem");

        // 更新金币显示
        EventCenter.EventTrigger(E_EventType.UpdateUIGold);

        // 售出状态
        isSold = true;
        if (selectButton != null)
            selectButton.interactable = false;
        if (soldImage != null)
            soldImage.gameObject.SetActive(true);
        if (soldText != null)
            soldText.text = "已售罄";
    }

    void PrintEquipDetails()
    {
        string affixStr = "";
        if (equipData.affixes != null)
        {
            foreach (var affix in equipData.affixes)
                affixStr += $"\n    +{affix.value} {EquipData.GetAffixTypeName(affix.type)}";
        }
        int remainGold = GameRoot.GetManager<GoldManager>()?.Gold ?? 0;

        string msg = $"[购买装备] {equipData.GetEquipName()};[部位]: {equipData.GetSlotName()};[混沌等级]: {equipData.chaosLevel};[词条]:{affixStr};[弱点]: {equipData.weakness}\n;[价格]: {adjustedPrice}";
        Debug.Log(msg);
    }
}
