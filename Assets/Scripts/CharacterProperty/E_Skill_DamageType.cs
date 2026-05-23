public enum E_Skill_DamageType{
    物理,
    魔法
}
public enum E_WeaknessType{
    剑, 刀, 斧, 杖, 弓, 枪, 通解,
    风, 雷, 冰, 火, 光, 暗, 究极,
    无,//单纯的伤害，不削盾
}

/// <summary>
/// 用于查询弱点 对应的伤害类型
/// </summary>
public static class DamageTypeChecker {
    public static E_Skill_DamageType GetDamageType(E_WeaknessType Weakness)
    {
        E_Skill_DamageType type = E_Skill_DamageType.魔法;
        if (
            Weakness == E_WeaknessType.剑 ||
            Weakness == E_WeaknessType.刀 ||
            Weakness == E_WeaknessType.斧 ||
            Weakness == E_WeaknessType.杖 ||
            Weakness == E_WeaknessType.弓 ||
            Weakness == E_WeaknessType.枪 ||
            Weakness == E_WeaknessType.通解)
            type = E_Skill_DamageType.物理;

        return type;
}

}
