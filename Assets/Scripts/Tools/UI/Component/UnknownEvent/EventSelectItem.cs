using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用选择项组件——挂载到选项预制件上
/// 点击后高亮锁定，确认按钮统一执行
/// 可用于 UnknownEventPanel / RewardPanel 等场景
/// </summary>
public class EventSelectItem : MonoBehaviour
{
    [Header("选项描述文本")]
    public Text descriptionText;

    [Header("选项按钮")]
    public Button selectButton;

    [Header("高亮标记(选中时显示)")]
    public GameObject highlightTag;

    UnknownEventOption _optionData;
    System.Action<EventSelectItem> _onClicked;

    void Awake()
    {
        if (selectButton == null)
            selectButton = GetComponent<Button>();
        if (selectButton != null)
            selectButton.onClick.AddListener(OnClick);
        if (highlightTag != null)
            highlightTag.SetActive(false);
    }

    /// <summary>
    /// UnknownEvent 专用：绑定事件选项数据
    /// </summary>
    public void SetOption(UnknownEventOption option, string descriptionOverride, System.Action<EventSelectItem> onClicked)
    {
        _optionData = option;
        _onClicked = onClicked;
        if (descriptionText != null)
            descriptionText.text = descriptionOverride ?? option?.description ?? "";
    }

    /// <summary>
    /// 通用重载：仅设置描述文本与点击回调，不携带 UnknownEventOption
    /// </summary>
    public void SetOption(string description, System.Action<EventSelectItem> onClicked)
    {
        _optionData = null;
        _onClicked = onClicked;
        if (descriptionText != null)
            descriptionText.text = description;
    }

    public UnknownEventOption OptionData => _optionData;

    public void SetHighlighted(bool highlighted)
    {
        if (highlightTag != null)
            highlightTag.SetActive(highlighted);
    }

    void OnClick()
    {
        _onClicked?.Invoke(this);
    }

    void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(OnClick);
    }
}
