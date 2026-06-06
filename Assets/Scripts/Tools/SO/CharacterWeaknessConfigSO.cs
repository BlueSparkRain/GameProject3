using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色弱点配置——定义某一角色类型的基础弱点列表（可多选）。
/// 战斗中可通过技能添加/移除弱点，此SO仅定义初始状态。
///
/// 配置流程：
/// 1. 在 Unity 中为每种角色类型创建一份资产（右键 → Create → SOData → CharacterWeaknessConfig）
/// 2. 在 weaknesses 列表中勾选该角色拥有的所有基础弱点
/// 3. 资产放入 Resources/SOData/CharacterWeaknessConfig/ 下
/// </summary>
[CreateAssetMenu(menuName = "SOData/CharacterWeaknessConfig", fileName = "CharacterWeaknessConfig")]
public class CharacterWeaknessConfigSO : ScriptableObject
{
    [Header("所属角色类型")]
    public E_CharacterType characterType;

    [Header("基础弱点列表（可多选）")]
    public List<E_WeaknessType> weaknesses = new List<E_WeaknessType>();

    [Header("初始护盾点数")]
    [Tooltip("战斗开始时角色的护盾值，弱点被命中时会消耗护盾")]
    public int initialShieldPoints = 5;
}
