using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 随机事件面板——显示事件背景/名称，生成选项按钮
/// 点击选项锁定高亮，点击确认按钮执行效果并关闭
/// </summary>
public class UnknownEventPanel : UIPanelBase
{
    [Header("事件背景图")]
    public Image backgroundImage;

    [Header("事件名称")]
    public TMP_Text eventNameText;

    [Header("事件描述")]
    public TMP_Text descriptionText;

    [Header("选项按钮容器")]
    public Transform buttonsContent;

    [Header("选项预制件(EventSelectItem)")]
    public GameObject optionItemPrefab;

    [Header("确认按钮")]
    public Button confirmButton;

    [Header("暂无选项提示")]
    public TMP_Text emptyHintText;

    List<GameObject> _spawnedItems = new List<GameObject>();
    EventSelectItem _selectedItem;

    protected override void OnInit()
    {
        base.OnInit();
        if (emptyHintText != null)
            emptyHintText.gameObject.SetActive(false);
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }
    }

    public override void Show(){ 
        base.Show();
        _selectedItem = null;
        var eventMgr = GameRoot.GetManager<UnknownEventManager>();
        if (eventMgr == null)
        {
            Debug.LogError("[UnknownEventPanel] UnknownEventManager未注册");
            Hide();
            return;
        }

        var eventData = eventMgr.GetRandomEvent();
        if (eventData == null)
        {
            DebugManager.LogWarning(EDebugCategory.UIPanel,"[UnknownEventPanel] 无可用随机事件");
            ShowEmptyHint();
            return;
        }

        var options = eventMgr.GetEventOptions(eventData.eventType);
        if (options == null || options.Count == 0)
        {
            DebugManager.LogWarning(EDebugCategory.UIPanel,"[UnknownEventPanel] 事件无可用选项");
            ShowEmptyHint();
            return;
        }

        DisplayEvent(eventData, options);
    }

    void DisplayEvent(UnknownSOData eventData, List<UnknownEventOption> options)
    {
        if (backgroundImage != null && eventData.background != null)
            backgroundImage.sprite = eventData.background;

        if (eventNameText != null)
            eventNameText.text = eventData.eventName;

        if (descriptionText != null)
            descriptionText.text = eventData.description;

        ClearSpawnedItems();

        if (confirmButton != null)
            confirmButton.interactable = false;

        var soDescriptions = eventData.optionDescriptions;

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            if (option == null || option.effects == null || option.effects.Count == 0)
                continue;

            var obj = Instantiate(optionItemPrefab, buttonsContent);
            obj.SetActive(true);
            var item = obj.GetComponent<EventSelectItem>();
            if (item != null)
            {
                string btnText = (soDescriptions != null && i < soDescriptions.Count)
                    ? soDescriptions[i] : option.description;
                item.SetOption(option, btnText, OnOptionClicked);
            }
            _spawnedItems.Add(obj);
        }
    }

    void OnOptionClicked(EventSelectItem clickedItem)
    {
        if (clickedItem == null) return;

        // 取消旧选项高亮
        if (_selectedItem != null)
            _selectedItem.SetHighlighted(false);

        // 高亮新选项
        _selectedItem = clickedItem;
        _selectedItem.SetHighlighted(true);

        // 启用确认按钮
        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    void OnConfirmClicked()
    {
        if (_selectedItem == null) return;

        var option = _selectedItem.OptionData;
        DebugManager.Log(EDebugCategory.UIPanel,$"[UnknownEventPanel] 确认选项: {option.description}");
        var eventMgr = GameRoot.GetManager<UnknownEventManager>();
        eventMgr?.ExecuteOption(option);
        Hide();
    }

    void ShowEmptyHint()
    {
        if (emptyHintText != null)
            emptyHintText.gameObject.SetActive(true);
    }

    void ClearSpawnedItems()
    {
        foreach (var obj in _spawnedItems)
            Destroy(obj);
        _spawnedItems.Clear();
        _selectedItem = null;
    }

    public override void Hide()
    {
        base.Hide();
        ClearSpawnedItems();
        if (emptyHintText != null)
            emptyHintText.gameObject.SetActive(false);
        if (confirmButton != null)
            confirmButton.interactable = false;
    }
}
