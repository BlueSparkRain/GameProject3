using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSelector_UIItem : MonoBehaviour, IPointerEnterHandler
{
    [Header("技能Image")]
    public Image skillImage;

    [Header("技能名称")]
    public TMP_Text skillNameText;
    [Header("技能x效果")]
    public TMP_Text skillDescriptionText;
    [Header("刷新按钮")]
    public Button refreshButton;

    /// <summary>
    /// 当前持有的技能数据
    /// </summary>
    SkillPropertySO skillData;
    //随机一种技能数据,由Panel传入分配


    /// <summary>
    ///根据传入的数据来初始化选择器
    /// </summary>
    public void InitSelf(SkillPropertySO skillData)
    {

        this.skillData = skillData;
        skillImage.sprite = skillData.skill_Sprite;
        skillNameText.text = skillData.skill_Name;
        skillDescriptionText.text = skillData.skill_Description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInParent<SkillSelectPanel>().ShowDetailBoard(GetComponent<RectTransform>(), Vector3.down, skillData);
    }
    /// <summary>
    /// 刷新一种新的技能（通知SkillSelectPanel来分配）
    /// </summary>
    void OnClickRefreshButton()
    {
        GetComponentInParent<SkillSelectPanel>().AssignNewSkillData(this);

    }
    void Start()
    {
        refreshButton.onClick.AddListener(OnClickRefreshButton);
    }

}
