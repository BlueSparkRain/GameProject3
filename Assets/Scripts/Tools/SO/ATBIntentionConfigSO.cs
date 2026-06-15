using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ATB意图配置——定义某一角色类型的主动技能池与ATB释放顺序。
/// 在编辑器中为每种角色类型创建一份资产即可。
///
/// ATB获取方式：通过技能释放产生AG（怒气值），AG达到100后自动转化为1点ATB。
/// 不存在被动ATB充能，ATB仅由AG溢出驱动。
///
/// 配置流程：
/// 1. 在 activeSkills 中配置该角色拥有的全部主动技能（枚举下拉选择）
/// 2. 在 atbIntentionIndices 中按 1-based 索引配置ATB技能的释放顺序
///    （1 = activeSkills[0], 2 = activeSkills[1], ...）
/// 3. 列表执行完毕后自动循环
///
/// 技能解析链路：E_SkillName → ResourcesLoader.FindSkillSOBySkillName() → SkillPropertySO → SkillData
/// 所有技能获取都经过SO体系验证，不使用裸ID强转。
/// </summary>
[CreateAssetMenu(menuName = "SOData/ATBIntentionConfig", fileName = "ATBIntentionConfig")]
public class ATBIntentionConfigSO : ScriptableObject
{
    [Header("所属角色类型")]
    public E_CharacterType characterType;

    [Header("主动技能列表（该角色拥有的全部技能，用于普通自动循环 + ATB意图）")]
    public List<E_SkillName> activeSkills = new List<E_SkillName>();

    [Header("ATB意图索引（1-based，引用activeSkills中的技能位置，顺序循环）")]
    public List<int> atbIntentionIndices = new List<int>();

    [Header("每次释放ATB技能消耗点数")]
    public int atbSpendPerSkill = 1;

    /// <summary>
    /// 根据意图索引获取对应的 SkillPropertySO。索引无效返回 null。
    /// </summary>
    public SkillPropertySO GetSkillSOByIntentionIndex(int oneBasedIndex)
    {
        int arrayIndex = oneBasedIndex - 1;
        if (arrayIndex < 0 || arrayIndex >= activeSkills.Count){
            Debug.LogError($"[ATBIntentionConfig] 意图索引 {oneBasedIndex} 超出主动技能列表范围(1~{activeSkills.Count})");
            return null;
        }
        return ResourcesLoader.FindSkillSOBySkillName(activeSkills[arrayIndex]);
    }
    /// <summary>
    /// 根据意图索引获取对应的 SkillData。索引无效返回 null。
    /// </summary>
    public SkillData GetSkillDataByIntentionIndex(int oneBasedIndex){
        var so = GetSkillSOByIntentionIndex(oneBasedIndex);
        return so != null ? new SkillData(so) : null;
    }

    /// <summary>
    /// 获取 activeSkills 中所有技能的 SkillPropertySO 数组。
    /// </summary>
    public SkillPropertySO[] GetActiveSkillSOs(){
        var list = new List<SkillPropertySO>();
        foreach (var skillName in activeSkills){
            var so = ResourcesLoader.FindSkillSOBySkillName(skillName);
            if (so != null) list.Add(so);
        }
        return list.ToArray();
    }

    /// <summary>
    /// 获取 activeSkills 中所有技能的 SkillData 数组。
    /// </summary>
    public SkillData[] GetActiveSkillDatas()
    {
        var sos = GetActiveSkillSOs();
        var datas = new SkillData[sos.Length];
        for (int i = 0; i < sos.Length; i++)
            datas[i] = new SkillData(sos[i]);
        return datas;
    }
}
