using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectPanel : UIPanelBase
{
    [Header("选择器容器")]
    public Transform skillContent;

    public List<SkillSelector_UIItem> skillSelectorUIs = new List<SkillSelector_UIItem>();
    public List<int> skillIDList = new List<int>();
    [Header("细节板RectTransform")]
    public RectTransform detialBoardRect;

    [Header("DetailBoard-技能图标")]
    public Image detail_SkillImge;
    [Header("DetailBoard-技能名称")]
    public TMP_Text detail_SkillNameText;
    [Header("DetailBoard-技能描述")]
    public TMP_Text detail_SkillDescriptionText;

    /// <summary>
    /// 如果鼠标移入
    /// </summary>
    public void ShowDetailBoard(RectTransform selector, Vector3 offset, SkillPropertySO skillSO)
    {
        detialBoardRect.position = selector.position + offset;
        detail_SkillImge.sprite = skillSO.skill_Sprite;
        detail_SkillNameText.text = skillSO.skill_Name;
        detail_SkillDescriptionText.text = skillSO.skill_Description;
    }

    public void HideDetailBoard(){
        detialBoardRect.position = Vector3.zero;
    }
    void AssignSkillData(SkillSelector_UIItem skillSelectorUI, SkillPropertySO skillPropertySO){
        skillSelectorUI.InitSelf(skillPropertySO);
    }

    /// <summary>
    /// 获取一个不同于skillIDList的一个新的SkillID，放置到目标Index
    /// </summary>
    SkillPropertySO GetNewSkill(){
        int newSkillID = GetAvailableId(0,10);
        Debug.Log($"当前List内{skillIDList[0]},{skillIDList[1]},{skillIDList[2]},新skillID：{newSkillID}");
        return ResourcesLoader.FindSkillSOByID(newSkillID);
    }

    int GetAvailableId(int min, int max){
        // 将现有列表转为 HashSet 以提高查找效率
        var existing = new HashSet<int>(skillIDList);
        // 遍历区间，找到第 一个不在集合中的数
        for (int id = min; id <= max; id++){
            if (!existing.Contains(id))
                return id;
        }
        return -1;
    }

    public void AssignNewSkillData(SkillSelector_UIItem skillSelectorUI){
        SkillPropertySO newSkillSo = GetNewSkill();
        AssignSkillData(skillSelectorUI,newSkillSo);
    }

    protected override void OnInit(){
        base.OnInit();
        //收集所有Selector
        for (int i = 0; i < skillContent.childCount; i++)
            skillSelectorUIs.Add(skillContent.GetChild(i).GetComponent<SkillSelector_UIItem>());
    }
    /// <summary>
    /// 随机3种不同技能，分配给skillSelectorUIs
    /// </summary>
    List<int> GetBatchSkillIDS(int skillCount)
    {
        //随机三个数
        skillIDList = RandomUtility.GetUniqueRandomList(skillCount, 0, 10);
        return skillIDList;
    }
    /// <summary>
    /// 打开Skill选择面板就自动分配三种技能
    /// </summary>
    public void SetttleSelect()
    {
        List<int> skillIDs = GetBatchSkillIDS(skillSelectorUIs.Count);
        for (int i = 0; i < skillSelectorUIs.Count; i++)
        {
            SkillPropertySO skillData = ResourcesLoader.FindSkillSOByID(skillIDs[i]);
            AssignSkillData(skillSelectorUIs[i], skillData);
        }
    }
}
