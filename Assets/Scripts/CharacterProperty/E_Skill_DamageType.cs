public enum E_Skill_DamageType{
    物理,
    魔法
}
public enum E_WeaknessType{
    剑, 刀_, 斧_, 杖_, 弓, 枪, 通解_,
    风_, 雷, 冰, 火, 光_, 暗_, 究极_,
    无_,//单纯的伤害，不削盾
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
            Weakness == E_WeaknessType.刀_ ||
            Weakness == E_WeaknessType.斧_ ||
            Weakness == E_WeaknessType.杖_ ||
            Weakness == E_WeaknessType.弓 ||
            Weakness == E_WeaknessType.枪 ||
            Weakness == E_WeaknessType.通解_)
            type = E_Skill_DamageType.物理;

        return type;
}

}
