using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弱点类型 → 图标的单条映射
/// </summary>
[Serializable]
public class WeaknessIconEntry
{
    [Tooltip("弱点类型")]
    public E_WeaknessType weaknessType;
    [Tooltip("对应的图标精灵")]
    public Sprite iconSprite;
}

/// <summary>
/// 弱点图标配置 —— 定义每种弱点类型对应的显示图标。
/// 通过 ResourcesLoader 加载，路径：SOData/WeaknessIconConfig/WeaknessIconConfig
///
/// 配置流程：
/// 1. 在 Unity 中创建资产（右键 → Create → SOData → WeaknessIconConfig）
/// 2. 在 iconEntries 中为每种弱点类型配置对应的 Sprite
/// 3. 资产放入 Resources/SOData/WeaknessIconConfig/ 下
/// </summary>
[CreateAssetMenu(menuName = "SOData/WeaknessIconConfig", fileName = "WeaknessIconConfig")]
public class WeaknessIconConfigSO : ScriptableObject
{
    [Header("弱点类型 → 图标精灵 映射列表")]
    public List<WeaknessIconEntry> iconEntries = new List<WeaknessIconEntry>();

    /// <summary>
    /// 根据弱点类型获取对应的图标精灵
    /// </summary>
    public Sprite GetSprite(E_WeaknessType type)
    {
        foreach (var entry in iconEntries)
        {
            if (entry.weaknessType == type)
                return entry.iconSprite;
        }
        return null;
    }
}
