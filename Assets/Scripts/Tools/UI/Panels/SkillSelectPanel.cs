using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSelectPanel :UIPanelBase
{
    [Header("选择器容器")]
    public Transform skillContent;

    public List<SkillSelectorUI> skillSelectorUIs=new List<SkillSelectorUI>();

    protected override void OnInit()
    {
        base.OnInit();
        //收集所有Selector
        for (int i = 0; i < skillContent.childCount; i++)
            skillSelectorUIs.Add(skillContent.GetChild(i).GetComponent<SkillSelectorUI>());
    }

    /// <summary>
    /// 随机3种不同技能，分配给skillSelectorUIs
    /// </summary>
    List<int> GetBatchSkillIDS(int skillCount) { 
        //随机三个数
        var list = new List<int>();
        list = RandomUtility.GetUniqueRandomList(skillCount,0,10);
        return list;
    }


    public  void SetttleSelect() {
        List<int> skillIDs=GetBatchSkillIDS(skillSelectorUIs.Count);    
        for (int i = 0; i < skillSelectorUIs.Count; i++) {
            SkillPropertySO skillData = ResourcesLoader.FindSkillSOByID(skillIDs[i]);
            skillSelectorUIs[i].InitSelf(skillData);
        }
    
    }
}
