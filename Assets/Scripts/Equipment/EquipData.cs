using System;

/// <summary>
/// 装备数据类——可序列化，用于JSON存档和运行时传递
/// </summary>
[Serializable]
public class EquipData : IValidatable{
    /// <summary>装备名称</summary>
    public string equipName;
    /// <summary>装备唯一ID</summary>
    public string equipId;
    /// <summary>装备部位</summary>
    public E_EquipmentSlot slot;
    /// <summary>随机词条(1~3条, 50%/30%/20%)</summary>
    public EquipAffix[] affixes;
    /// <summary>1个随机弱点类型</summary>
    public E_WeaknessType weakness;
    /// <summary>生成时的混沌等级</summary>
    public int chaosLevel;
    /// <summary>售价(金币)</summary>
    public int price;
    /// <summary>图标路径(Resources相对路径, 如 Sprite/EquipmentIcons/Icon_Sword)</summary>
    public string iconResourcePath;

    public EquipData() { }

    public EquipData(string equipId, E_EquipmentSlot slot, EquipAffix[] affixes, E_WeaknessType weakness, int chaosLevel, int price, string equipName = "", string iconResourcePath = "")
    {
        this.equipId = equipId;
        this.slot = slot;
        this.affixes = affixes;
        this.weakness = weakness;
        this.chaosLevel = chaosLevel;
        this.price = price;
        this.equipName = equipName;
        this.iconResourcePath = iconResourcePath;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(equipId)
            && affixes != null
            && affixes.Length >= 1
            && affixes.Length <= 3;
    }

    /// <summary>获取装备部位的中文名</summary>
    public string GetSlotName()
    {
        switch (slot)
        {
            case E_EquipmentSlot.Sword: return "剑";
            case E_EquipmentSlot.Spear: return "枪";
            case E_EquipmentSlot.Bow: return "弓";
            case E_EquipmentSlot.Shield: return "盾";
            case E_EquipmentSlot.Head: return "头部防具";
            case E_EquipmentSlot.Body: return "身体防具";
            case E_EquipmentSlot.Accessory1: return "饰品";
            case E_EquipmentSlot.Accessory2: return "饰品";
            default: return "未知";
        }
    }

    /// <summary>获取词条类型的中文名</summary>
    public static string GetAffixTypeName(E_EquipAffixType type){
        switch (type){
            case E_EquipAffixType.HP: return "生命值";
            case E_EquipAffixType.Mana: return "法力值";
            case E_EquipAffixType.PhysATK: return "物攻";
            case E_EquipAffixType.MagATK: return "魔攻";
            case E_EquipAffixType.PhysDEF: return "物抗";
            case E_EquipAffixType.MagDEF: return "魔抗";
            case E_EquipAffixType.ShieldPoints: return "护盾点数";
            default: return "未知";
        }
    }

    /// <summary>获取装备名称(优先使用自定义名称，否则部位+随机后缀)</summary>
    public string GetEquipName()
    {
        if (!string.IsNullOrEmpty(equipName)) return equipName;
        string slotName = GetSlotName();
        string suffix = equipId.Length >= 4 ? equipId.Substring(equipId.Length - 4) : equipId;
        return $"{slotName}[{suffix}]";
    }

    /// <summary>获取词条描述文本</summary>
    public string GetAffixDescription()
    {
        if (affixes == null || affixes.Length == 0) return "无词条";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < affixes.Length; i++)
        {
            if (i > 0) sb.Append("\n");
            sb.Append($"+{affixes[i].value} {GetAffixTypeName(affixes[i].type)}");
        }
        sb.Append($"\n弱点: {weakness}");
        return sb.ToString();
    }
}
