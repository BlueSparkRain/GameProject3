using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSelector_UIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("技能Image")]
    public Image skillImage;
    [Header("技能名称Text")]
    public Text skillNameText;
    
    [Header("刷新按钮Button")]
    public Button refreshButton;
    [Header("冻结刷新挡板Obj")]
    public GameObject freezeRefreshImage;

    [Header("选中高亮标记Obj")]
    public GameObject highlightTag;
    [Header("选中按钮Button")]
    public Button selectButton;

    [Header("悬浮提示")]
    [SerializeField] float _hoverDelay = 0.3f;
    [SerializeField] Vector2 _tooltipOffset = new Vector2(0, 80f);

    SkillPropertySO skillData;
    public SkillPropertySO SkillData => skillData;
    public int SkillID => skillData?.skill_ID ?? -1;
    SkillTooltipHover _hoverTooltip;
    System.Action<SkillSelector_UIItem> _onClicked;



    public void InitSelf(SkillPropertySO skillData)
    {
        this.skillData = skillData;
        skillImage.sprite = skillData.skill_Sprite;
        skillNameText.text = skillData.skill_Name;
        _hoverTooltip?.Dispose();
        _hoverTooltip = new SkillTooltipHover(this, transform, skillData.skill_Description, _hoverDelay, _tooltipOffset);
    }

    public void SetClickCallback(System.Action<SkillSelector_UIItem> onClicked)
    {
        _onClicked = onClicked;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightTag != null)
            highlightTag.SetActive(highlighted);
    }

    void Awake()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(OnClickSelect);
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnClickRefreshButton);
        if (highlightTag != null)
            highlightTag.SetActive(false);
        freezeRefreshImage.SetActive(false);
    }

    void OnClickSelect()
    {
        _onClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData){
        _hoverTooltip?.Enter();
    }
    public void OnPointerExit(PointerEventData eventData){
        _hoverTooltip?.Exit();
    }
    void OnClickRefreshButton(){
        GetComponentInParent<SkillSelectPanel>().AssignNewSkillData(this);
        _hoverTooltip?.SetDescription(skillData.skill_Description);
        freezeRefreshImage.SetActive(true);
    }

    void OnDisable()
    {
        _hoverTooltip?.Exit();
    }

    void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(OnClickSelect);
        _hoverTooltip?.Dispose();
        _hoverTooltip = null;
    }
}
