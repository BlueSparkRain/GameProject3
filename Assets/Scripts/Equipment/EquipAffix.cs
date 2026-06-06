using System;

/// <summary>
/// 装备词条结构体——一条词条的类型+数值
/// </summary>
[Serializable]
public struct EquipAffix
{
    public E_EquipAffixType type;
    public int value;
}
