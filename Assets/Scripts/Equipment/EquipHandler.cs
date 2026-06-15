using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当前装备处理器——管理角色当前各部位装备，提供绿值加成和弱点列表
/// 纯C#类，挂在CharacterData上。自动从Save_EquippedItems存档加载/保存
/// </summary>
public class EquipHandler
{
    Dictionary<E_EquipmentSlot, EquipData> currentEquips = new Dictionary<E_EquipmentSlot, EquipData>();
    public IReadOnlyDictionary<E_EquipmentSlot, EquipData> CurrentEquips => currentEquips;

    public System.Action<E_EquipmentSlot, EquipData> onEquipChanged;

    public EquipHandler()
    {
        LoadEquipped();
        onEquipChanged += (_, _) => SaveEquipped();
    }

    /// <summary>装备一件装备(同部位替换，饰品自动选空位)</summary>
    public void Equip(EquipData data)
    {
        if (data == null || !data.IsValid()) return;

        E_EquipmentSlot targetSlot = ResolveAccessorySlot(data.slot);
        ApplyEquip(data, targetSlot);
    }

    /// <summary>装备到指定槽位(面板手动选择时使用，跳过饰品自动分配)</summary>
    public void EquipToSlot(EquipData data, E_EquipmentSlot targetSlot)
    {
        if (data == null || !data.IsValid()) return;
        ApplyEquip(data, targetSlot);
    }

    E_EquipmentSlot ResolveAccessorySlot(E_EquipmentSlot slot)
    {
        if (slot != E_EquipmentSlot.Accessory1 && slot != E_EquipmentSlot.Accessory2)
            return slot;

        if (currentEquips.ContainsKey(E_EquipmentSlot.Accessory1))
            return currentEquips.ContainsKey(E_EquipmentSlot.Accessory2)
                ? E_EquipmentSlot.Accessory1
                : E_EquipmentSlot.Accessory2;
        return E_EquipmentSlot.Accessory1;
    }

    void ApplyEquip(EquipData data, E_EquipmentSlot targetSlot)
    {
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

    /// <summary>获取护盾点数绿值</summary>
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

    #region 存档 (Save_EquippedItems)
    void LoadEquipped()
    {
        var save = JsonSaver.Load<Save_EquippedItems>();
        if (save?.entries == null) return;
        foreach (var entry in save.entries)
        {
            if (entry?.data != null && entry.data.IsValid())
                currentEquips[entry.slot] = entry.data;
        }
        DebugManager.Log(EDebugCategory.Equipment,$"[EquipHandler] 从存档加载了{currentEquips.Count}件已装备");
    }

    void SaveEquipped()
    {
        var entries = new List<EquippedEntry>();
        foreach (var kv in currentEquips)
            entries.Add(new EquippedEntry { slot = kv.Key, data = kv.Value });
        JsonSaver.Save(new Save_EquippedItems(entries));
    }

    /// <summary>导出已装备列表(用于调试/编辑器)</summary>
    public List<EquippedEntry> GetEquippedList()
    {
        var list = new List<EquippedEntry>();
        foreach (var kv in currentEquips)
            list.Add(new EquippedEntry { slot = kv.Key, data = kv.Value });
        return list;
    }
    #endregion

    /// <summary>已装备条目——槽位+装备数据对</summary>
    [System.Serializable]
    public class EquippedEntry
    {
        public E_EquipmentSlot slot;
        public EquipData data;
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

/// <summary>
/// [Save]已装备数据存档——独立存档文件，与EquipBacketManager类似
/// </summary>
[System.Serializable]
public class Save_EquippedItems : IValidatable
{
    public List<EquipHandler.EquippedEntry> entries;

    public Save_EquippedItems() { }

    public Save_EquippedItems(List<EquipHandler.EquippedEntry> list)
    {
        entries = list;
    }

    public bool IsValid() => entries != null && entries.TrueForAll(e =>
        e?.data != null && e.data.IsValid());
}
