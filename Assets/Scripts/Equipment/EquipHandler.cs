using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当前装备处理器——管理角色当前各部位装备，提供绿值加成和弱点列表
/// 纯C#类，挂在CharacterData上
/// </summary>
public class EquipHandler
{
    Dictionary<E_EquipmentSlot, EquipData> currentEquips = new Dictionary<E_EquipmentSlot, EquipData>();
    public IReadOnlyDictionary<E_EquipmentSlot, EquipData> CurrentEquips => currentEquips;

    public System.Action<E_EquipmentSlot, EquipData> onEquipChanged;

    /// <summary>装备一件装备(同部位替换，饰品自动选空位)</summary>
    public void Equip(EquipData data)
    {
        if (data == null || !data.IsValid()) return;

        // 饰品自动选空位
        E_EquipmentSlot targetSlot = data.slot;
        if (targetSlot == E_EquipmentSlot.Accessory1 || targetSlot == E_EquipmentSlot.Accessory2)
        {
            if (currentEquips.ContainsKey(E_EquipmentSlot.Accessory1))
                targetSlot = currentEquips.ContainsKey(E_EquipmentSlot.Accessory2)
                    ? E_EquipmentSlot.Accessory1  // 两个都满了，替换Accessory1
                    : E_EquipmentSlot.Accessory2; // Accessory1满了，用Accessory2
            else
                targetSlot = E_EquipmentSlot.Accessory1; // Accessory1空闲
        }

        // 同部位旧装备先卸下
        if (currentEquips.TryGetValue(targetSlot, out var old))
            Unequip(targetSlot);

        currentEquips[targetSlot] = data;
        onEquipChanged?.Invoke(targetSlot, data);
    }

    /// <summary>卸下指定部位的装备</summary>
    public void Unequip(E_EquipmentSlot slot)
    {
        if (currentEquips.TryGetValue(slot, out var old))
        {
            currentEquips.Remove(slot);
            onEquipChanged?.Invoke(slot, null);
        }
    }

    /// <summary>获取指定部位的当前装备</summary>
    public EquipData GetEquipped(E_EquipmentSlot slot)
        => currentEquips.TryGetValue(slot, out var eq) ? eq : null;

    /// <summary>计算装备提供的绿值加成总和(映射到CharacterProperty)</summary>
    public float GetGreenBonus(E_CharacterPropertyType propertyType)
    {
        float total = 0f;
        foreach (var kv in currentEquips)
        {
            if (kv.Value?.affixes == null) continue;
            foreach (var affix in kv.Value.affixes)
            {
                var mapped = MapToProperty(affix.type);
                if (mapped == propertyType)
                    total += affix.value;
            }
        }
        return total;
    }

    /// <summary>获取护盾点数绿值(不映射到CharacterProperty,战斗时直接加给Max_ShieldPoints)</summary>
    public int GetShieldBonus()
    {
        int total = 0;
        foreach (var kv in currentEquips)
        {
            if (kv.Value?.affixes == null) continue;
            foreach (var affix in kv.Value.affixes)
            {
                if (affix.type == E_EquipAffixType.ShieldPoints)
                    total += affix.value;
            }
        }
        return total;
    }

    /// <summary>获取当前所有装备的弱点列表</summary>
    public List<E_WeaknessType> GetWeaknesses()
    {
        var list = new List<E_WeaknessType>();
        foreach (var kv in currentEquips)
        {
            if (kv.Value != null)
                list.Add(kv.Value.weakness);
        }
        return list;
    }

    /// <summary>词条类型→角色属性类型的映射</summary>
    public static E_CharacterPropertyType MapToProperty(E_EquipAffixType affixType)
    {
        switch (affixType)
        {
            case E_EquipAffixType.HP:      return E_CharacterPropertyType.Maximum_Health;
            case E_EquipAffixType.Mana:    return E_CharacterPropertyType.Maximum_Mana;
            case E_EquipAffixType.PhysATK: return E_CharacterPropertyType.Phy_Attack;
            case E_EquipAffixType.MagATK:  return E_CharacterPropertyType.Mag_Attack;
            case E_EquipAffixType.PhysDEF: return E_CharacterPropertyType.Phy_Resistance;
            case E_EquipAffixType.MagDEF:  return E_CharacterPropertyType.Mag_Resistance;
            default: return E_CharacterPropertyType.Maximum_Health;
        }
    }
}
