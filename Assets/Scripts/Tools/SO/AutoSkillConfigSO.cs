using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自动化技能配置——定义某一角色类型拥有的自动化技能列表。
/// 在编辑器中为每种角色类型创建一份资产即可。
/// 自动化技能会在战斗中自动循环释放（基础版本），消耗SP。
///
/// 配置流程：
/// 1. 在 autoSkills 中配置该角色拥有的全部自动化技能（枚举下拉选择）
/// 2. 命名为 AutoSkill_{characterType}，如 AutoSkill_LE_1、AutoSkill_P_1
/// 3. 战斗中按冷却时间自动循环释放
/// </summary>
[CreateAssetMenu(menuName = "SOData/AutoSkillConfig", fileName = "AutoSkillConfig")]
public class AutoSkillConfigSO : ScriptableObject
{
    [Header("所属角色类型")]
    public E_CharacterType characterType;

    [Header("自动化技能列表（按枚举名选择，自动循环释放）")]
    public List<E_SkillName> autoSkills = new List<E_SkillName>();

    /// <summary>
    /// 获取所有自动化技能的 SkillData 数组。
    /// </summary>
    public SkillData[] GetAutoSkillDatas()
    {
        var datas = new SkillData[autoSkills.Count];
        for (int i = 0; i < autoSkills.Count; i++)
        {
            var so = ResourcesLoader.FindSkillSOBySkillName(autoSkills[i]);
            if (so != null)
                datas[i] = new SkillData(so);
        }
        return datas;
    }
}
