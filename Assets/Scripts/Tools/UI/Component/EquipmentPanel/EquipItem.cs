using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 装备展示组件——显示一件装备的图标和名称
/// 可在EquipSlotItem中作为子物体(展示已装备), 也可在选择列表中(可点击选择)
/// </summary>
public class EquipItem : MonoBehaviour{
    [Header("装备图标")]
    public Image iconImage;
    [Header("装备名称")]
    public TextMeshProUGUI nameText;
    [Header("选择按钮(列表模式下使用)")]
    public Button selectButton;
    EquipData _equipData;
    public EquipData EquipData => _equipData;
    public System.Action<EquipItem> onSelected;

    void Awake(){
        if (selectButton == null)
            selectButton = GetComponent<Button>();
        if (selectButton == null)
            selectButton = GetComponentInChildren<Button>();

        if (selectButton != null)
            selectButton.onClick.AddListener(
                () => { onSelected?.Invoke(this);
                GameRoot.GetManager<AudioManager>().PlaySFX("Music/SFX/ClickButton");
            });
        else
            DebugManager.LogWarning(EDebugCategory.UIPanel,$"[EquipItem] 未找到Button组件，对象:{name}，请检查预制件是否挂载Button");
    }

    /// <summary>填充装备数据</summary>
    public void SetData(EquipData data){
        _equipData = data;
        if (data == null){
            if (nameText != null) nameText.text = "";
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        if (nameText != null) nameText.text = data.GetEquipName();
        TryLoadIcon(data);
    }
    /// <summary>设置是否可交互(列表模式下按钮可点)</summary>
    public void SetInteractable(bool interactable){
        if (selectButton != null)
            selectButton.interactable = interactable;
    }

    void TryLoadIcon(EquipData data){
        if (iconImage == null || data == null) return;
        // 优先使用SO配置的装备图标，fallback到通用部位图标
        var sprite = EquipIconPath.LoadEquipIcon(data.iconResourcePath)
                  ?? EquipIconPath.LoadSlotIcon(data.slot);
        if (sprite != null) iconImage.sprite = sprite;
    }
}
