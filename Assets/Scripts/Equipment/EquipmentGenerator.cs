using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装备图标路径统一维护
/// </summary>
public static class EquipIconPath{
    static string iconDir = "Sprite/EquipmentIcons";
    static string GetSpriteName(E_EquipmentSlot slot) => slot switch{
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

    /// <summary>加载部位槽位图标Sprite(通用槽位图标, 非具体装备图标)</summary>
    public static Sprite LoadSlotIcon(E_EquipmentSlot slot)
        => Resources.Load<Sprite>($"{iconDir}/{GetSpriteName(slot)}");

    /// <summary>根据EquipData中的iconResourcePath加载装备图标</summary>
    public static Sprite LoadEquipIcon(string resourcePath){
        if (string.IsNullOrEmpty(resourcePath)) return null;
        return Resources.Load<Sprite>(resourcePath);
    }
}

/// <summary>
/// 装备生成器——根据混沌等级随机生成装备
/// 装备名称/基础售价/图标路径从EquipmentSO读取，词条按规则随机生成
/// </summary>
public static class EquipmentGenerator{
    const string soPath = "SOData/EquipmentSOData";
    static readonly E_EquipmentSlot[] allSlots ={
        E_EquipmentSlot.Sword, E_EquipmentSlot.Spear, E_EquipmentSlot.Bow,
        E_EquipmentSlot.Shield,
        E_EquipmentSlot.Head, E_EquipmentSlot.Body,
        E_EquipmentSlot.Accessory1, E_EquipmentSlot.Accessory2,
    };

    static readonly E_EquipAffixType[] weaponAffixPool =  { E_EquipAffixType.PhysATK, E_EquipAffixType.HP, E_EquipAffixType.Mana };
    static readonly E_EquipAffixType[] shieldAffixPool =  { E_EquipAffixType.ShieldPoints, E_EquipAffixType.HP, E_EquipAffixType.Mana, E_EquipAffixType.PhysDEF, E_EquipAffixType.MagDEF };
    static readonly E_EquipAffixType[] armorAffixPool =   { E_EquipAffixType.MagATK, E_EquipAffixType.PhysDEF, E_EquipAffixType.MagDEF, E_EquipAffixType.HP, E_EquipAffixType.Mana };

    static Dictionary<E_EquipmentSlot, List<EquipmentSO>> soCache;
    static bool soCacheLoaded;

    /// <summary>加载并缓存所有EquipmentSO</summary>
    static void EnsureSOCache(){
        if (soCacheLoaded) return;
        soCache = new Dictionary<E_EquipmentSlot, List<EquipmentSO>>();
        var allSO = Resources.LoadAll<EquipmentSO>(soPath);
        foreach (var so in allSO)
        {
            if (!soCache.ContainsKey(so.slot))
                soCache[so.slot] = new List<EquipmentSO>();
            soCache[so.slot].Add(so);
        }
        soCacheLoaded = true;
        DebugManager.Log(EDebugCategory.Equipment,$"[EquipmentGenerator] 从 Resources/{soPath} 加载了{allSO.Length}个EquipmentSO");
    }
    /// <summary>清除SO缓存(编辑器下热重载时调用)</summary>
    public static void ClearSOCache(){
        soCacheLoaded = false;
        soCache = null;
    }
    /// <summary>获取指定部位的随机SO(无SO时返回null)</summary>
    public static EquipmentSO GetRandomSO(E_EquipmentSlot slot){
        EnsureSOCache();
        if (soCache.TryGetValue(slot, out var list) && list.Count > 0)
            return list[Random.Range(0, list.Count)];
        // 饰品兼容: Accessory1和Accessory2共享
        if (slot == E_EquipmentSlot.Accessory2 && soCache.TryGetValue(E_EquipmentSlot.Accessory1, out var accList) && accList.Count > 0)
            return accList[Random.Range(0, accList.Count)];
        if (slot == E_EquipmentSlot.Accessory1 && soCache.TryGetValue(E_EquipmentSlot.Accessory2, out var acc2List) && acc2List.Count > 0)
            return acc2List[Random.Range(0, acc2List.Count)];
        return null;
    }

    /// <summary>获取指定部位可用的词条池</summary>
    public static E_EquipAffixType[] GetAffixPool(E_EquipmentSlot slot){
        switch (slot){
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
    public static (int min, int max) GetAffixRange(E_EquipAffixType type, int chaosLevel){
        int x = Mathf.Max(1, chaosLevel);
        switch (type){
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
    /// <summary>生成指定部位的装备(名称/价格/图标来自SO，词条随机)</summary>
    public static EquipData GenerateForSlot(E_EquipmentSlot slot, int chaosLevel){
        var affixes = GenerateAffixes(slot, chaosLevel);
        var so = GetRandomSO(slot);
        int affixBonus = affixes.Length * Random.Range(500, 801);
        // 饰品: Accessory2统一存为Accessory1
        E_EquipmentSlot storeSlot = (slot == E_EquipmentSlot.Accessory2) ? E_EquipmentSlot.Accessory1 : slot;
        var data = new EquipData(
            System.Guid.NewGuid().ToString(),
            storeSlot,
            affixes,
            RandomWeakness(),
            chaosLevel,
            (so != null ? so.basePrice : CalculatePriceByAffix(affixes)) + affixBonus,
            so != null ? so.equipName : $"未知{slot}装备",
            so != null ? so.iconResourcePath : ""
        );
        if (so == null)
            DebugManager.LogWarning(EDebugCategory.Equipment,$"[EquipmentGenerator] 未找到{slot}部位的EquipmentSO，使用默认名称/价格");
        return data;
    }
    /// <summary>生成1件随机装备</summary>
    public static EquipData Generate(int chaosLevel){
        var slot = allSlots[Random.Range(0, allSlots.Length)];
        return GenerateForSlot(slot, chaosLevel);
    }
    /// <summary>批量生成装备</summary>
    public static List<EquipData> GenerateBatch(int count, int chaosLevel){
        var list = new List<EquipData>(count);
        for (int i = 0; i < count; i++)
            list.Add(Generate(chaosLevel));
        return list;
    }

    static EquipAffix[] GenerateAffixes(E_EquipmentSlot slot, int chaosLevel){
        var pool = GetAffixPool(slot);
        // 词条数概率: 50%出1条, 30%出2条, 20%出3条
        int affixCount = 1;
        int roll = Random.Range(0, 100);
        if (roll < 50) affixCount = 1;
        else if (roll < 80) affixCount = 2;
        else affixCount = 3;
        var pickedIndices = RandomUtility.GetUniqueRandomList(affixCount, 0, pool.Length - 1);
        var affixes = new EquipAffix[affixCount];
        for (int i = 0; i < affixCount; i++){
            var affixType = pool[pickedIndices[i]];
            var (min, max) = GetAffixRange(affixType, chaosLevel);
            affixes[i] = new EquipAffix { type = affixType, value = Random.Range(min, max + 1) };
        }
        return affixes;
    }
    /// <summary>仅根据词条计算价格(无SO时的fallback)</summary>
    static int CalculatePriceByAffix(EquipAffix[] affixes){
        float statScore = 0f;
        if (affixes != null){
            foreach (var affix in affixes){
                float weight = affix.type switch{
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

    static E_WeaknessType RandomWeakness(){
        var weakPool = new E_WeaknessType[]{
            E_WeaknessType.剑, E_WeaknessType.刀_, E_WeaknessType.斧_,
            E_WeaknessType.杖_, E_WeaknessType.弓, E_WeaknessType.枪,
            E_WeaknessType.风_, E_WeaknessType.雷, E_WeaknessType.冰,
            E_WeaknessType.火, E_WeaknessType.光_, E_WeaknessType.暗_,
            E_WeaknessType.究极_,
        };
        return weakPool[Random.Range(0, weakPool.Length)];
    }
}
