using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装备图标路径统一维护
/// </summary>
public static class EquipIconPath
{
    static string iconDir = "Sprite/EquipmentIcons";

    static string GetSpriteName(E_EquipmentSlot slot) => slot switch
    {
        E_EquipmentSlot.Sword => "Icon_Sword",
        E_EquipmentSlot.Spear => "Icon_Spear",
        E_EquipmentSlot.Bow => "Icon_Bow",
        E_EquipmentSlot.Shield => "Icon_Shield",
        E_EquipmentSlot.Head => "Icon_Head",
        E_EquipmentSlot.Body => "Icon_Body",
        E_EquipmentSlot.Accessory1 => "Icon_Accessory",
        E_EquipmentSlot.Accessory2 => "Icon_Accessory",
        _ => "Icon_Default",
    };

    /// <summary>加载部位图标Sprite —— 路径入口统一在此修改</summary>
    public static Sprite LoadSlotIcon(E_EquipmentSlot slot)
        => Resources.Load<Sprite>($"{iconDir}/{GetSpriteName(slot)}");
}

/// <summary>
/// 装备生成器——根据混沌等级随机生成装备
/// </summary>
public static class EquipmentGenerator
{
    static readonly E_EquipmentSlot[] allSlots =
    {
        E_EquipmentSlot.Sword, E_EquipmentSlot.Spear, E_EquipmentSlot.Bow,
        E_EquipmentSlot.Shield,
        E_EquipmentSlot.Head, E_EquipmentSlot.Body,
        E_EquipmentSlot.Accessory1, E_EquipmentSlot.Accessory2,
    };

    static readonly E_EquipAffixType[] weaponAffixPool =  { E_EquipAffixType.PhysATK, E_EquipAffixType.HP, E_EquipAffixType.Mana };
    static readonly E_EquipAffixType[] shieldAffixPool =  { E_EquipAffixType.ShieldPoints, E_EquipAffixType.HP, E_EquipAffixType.Mana, E_EquipAffixType.PhysDEF, E_EquipAffixType.MagDEF };
    static readonly E_EquipAffixType[] armorAffixPool =   { E_EquipAffixType.MagATK, E_EquipAffixType.PhysDEF, E_EquipAffixType.MagDEF, E_EquipAffixType.HP, E_EquipAffixType.Mana };

    /// <summary>获取指定部位可用的词条池</summary>
    public static E_EquipAffixType[] GetAffixPool(E_EquipmentSlot slot)
    {
        switch (slot)
        {
            case E_EquipmentSlot.Sword:
            case E_EquipmentSlot.Spear:
            case E_EquipmentSlot.Bow:
                return weaponAffixPool;
            case E_EquipmentSlot.Shield:
                return shieldAffixPool;
            case E_EquipmentSlot.Head:
            case E_EquipmentSlot.Body:
            case E_EquipmentSlot.Accessory1:
            case E_EquipmentSlot.Accessory2:
                return armorAffixPool;
            default:
                return armorAffixPool;
        }
    }

    /// <summary>获取词条的随机数值区间 (min, max) —— X = 混沌等级</summary>
    public static (int min, int max) GetAffixRange(E_EquipAffixType type, int chaosLevel)
    {
        int x = Mathf.Max(1, chaosLevel);
        switch (type)
        {
            case E_EquipAffixType.HP:           return (100 * x, 150 * x);
            case E_EquipAffixType.Mana:          return (100 * x, 150 * x);
            case E_EquipAffixType.PhysATK:       return (50 * x, 75 * x);
            case E_EquipAffixType.MagATK:        return (50 * x, 75 * x);
            case E_EquipAffixType.PhysDEF:       return (100 * x, 150 * x);
            case E_EquipAffixType.MagDEF:        return (100 * x, 150 * x);
            case E_EquipAffixType.ShieldPoints:  return (2 * x, 3 * x);
            default:                             return (10 * x, 50 * x);
        }
    }

    /// <summary>获取各部位预设装备名称(每部位4个)</summary>
    public static string[] GetSlotNames(E_EquipmentSlot slot)
    {
        switch (slot)
        {
            case E_EquipmentSlot.Sword: return new[] { "铁剑", "钢剑", "秘银剑", "龙鳞剑" };
            case E_EquipmentSlot.Spear: return new[] { "铁枪", "钢枪", "秘银枪", "龙牙枪" };
            case E_EquipmentSlot.Bow:   return new[] { "短弓", "长弓", "秘银弓", "凤翼弓" };
            case E_EquipmentSlot.Shield: return new[] { "木盾", "铁盾", "秘银盾", "龙鳞盾" };
            case E_EquipmentSlot.Head:  return new[] { "布帽", "皮盔", "铁盔", "秘银盔" };
            case E_EquipmentSlot.Body:  return new[] { "布衣", "皮甲", "铁甲", "秘银甲" };
            case E_EquipmentSlot.Accessory1:
            case E_EquipmentSlot.Accessory2: return new[] { "铜戒指", "银项链", "金手镯", "宝石耳环" };
            default: return new[] { "铁剑", "钢剑", "秘银剑", "龙鳞剑" };
        }
    }

    /// <summary>生成指定部位的装备(带随机预设名称)</summary>
    public static EquipData GenerateForSlot(E_EquipmentSlot slot, int chaosLevel)
    {
        var pool = GetAffixPool(slot);
        var pickedIndices = RandomUtility.GetUniqueRandomList(3, 0, pool.Length - 1);
        var affixes = new EquipAffix[3];
        for (int i = 0; i < 3; i++)
        {
            var affixType = pool[pickedIndices[i]];
            var (min, max) = GetAffixRange(affixType, chaosLevel);
            affixes[i] = new EquipAffix { type = affixType, value = Random.Range(min, max + 1) };
        }

        // 处理饰品: 用Accessory1作为槽位(统一归类)
        E_EquipmentSlot storeSlot = (slot == E_EquipmentSlot.Accessory2) ? E_EquipmentSlot.Accessory1 : slot;

        var names = GetSlotNames(slot);
        var data = new EquipData(
            System.Guid.NewGuid().ToString(),
            storeSlot,
            affixes,
            RandomWeakness(),
            chaosLevel,
            0,
            names[Random.Range(0, names.Length)]
        );
        data.price = CalculatePrice(data);
        return data;
    }

    /// <summary>生成1件随机装备</summary>
    public static EquipData Generate(int chaosLevel)
    {
        var slot = allSlots[Random.Range(0, allSlots.Length)];
        var pool = GetAffixPool(slot);

        // 从词条池中随机无重复抽取3个词条
        var pickedIndices = RandomUtility.GetUniqueRandomList(3, 0, pool.Length - 1);
        var affixes = new EquipAffix[3];
        for (int i = 0; i < 3; i++)
        {
            var affixType = pool[pickedIndices[i]];
            var (min, max) = GetAffixRange(affixType, chaosLevel);
            affixes[i] = new EquipAffix
            {
                type = affixType,
                value = Random.Range(min, max + 1)
            };
        }

        // 随机弱点类型(排除"无"和"通解")
        var weakness = RandomWeakness();

        var equipId = System.Guid.NewGuid().ToString();

        var data = new EquipData(equipId, slot, affixes, weakness, chaosLevel, 0);
        data.price = CalculatePrice(data);
        return data;
    }

    /// <summary>批量生成装备</summary>
    public static List<EquipData> GenerateBatch(int count, int chaosLevel)
    {
        var list = new List<EquipData>(count);
        for (int i = 0; i < count; i++)
            list.Add(Generate(chaosLevel));
        return list;
    }

    /// <summary>计算装备价格(整百)</summary>
    public static int CalculatePrice(EquipData data)
    {
        float statScore = 0f;
        if (data.affixes != null)
        {
            foreach (var affix in data.affixes)
            {
                float weight = affix.type switch
                {
                    E_EquipAffixType.HP => 1f,
                    E_EquipAffixType.Mana => 1f,
                    E_EquipAffixType.PhysATK => 2f,
                    E_EquipAffixType.MagATK => 2f,
                    E_EquipAffixType.PhysDEF => 1.5f,
                    E_EquipAffixType.MagDEF => 1.5f,
                    E_EquipAffixType.ShieldPoints => 3f,
                    _ => 1f,
                };
                statScore += affix.value * weight;
            }
        }
        int rawPrice = Mathf.RoundToInt(statScore * 10f / 100f) * 100;
        return Mathf.Max(100, rawPrice);
    }

    static E_WeaknessType RandomWeakness()
    {
        var weakPool = new E_WeaknessType[]
        {
            E_WeaknessType.剑, E_WeaknessType.刀, E_WeaknessType.斧,
            E_WeaknessType.杖, E_WeaknessType.弓, E_WeaknessType.枪,
            E_WeaknessType.风, E_WeaknessType.雷, E_WeaknessType.冰,
            E_WeaknessType.火, E_WeaknessType.光, E_WeaknessType.暗,
            E_WeaknessType.究极,
        };
        return weakPool[Random.Range(0, weakPool.Length)];
    }
}
