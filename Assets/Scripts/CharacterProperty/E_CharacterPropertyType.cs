public enum E_CharacterPropertyType{
    /// <summary>
    /// 物理固穿：固定减少对方物抗
    /// </summary>
    Phy_Flat_Penetration,

    /// <summary>
    /// 法术固穿：固定减少对方魔抗
    /// </summary>
    Mag_Flat_Penetration,

    /// <summary>
    /// 物抗：减少受到的物理伤害
    /// </summary>
    Phy_Resistance,

    /// <summary>
    /// 魔抗：减少受到的魔法伤害
    /// </summary>
    Mag_Resistance,

    /// <summary>
    /// 物攻：影响物理伤害
    /// </summary>
    Phy_Attack,

    /// <summary>
    /// 法强：影响魔法伤害
    /// </summary>
    Magic_Attack,

    /// <summary>
    /// 最大法力值：决定法力值的上限
    /// </summary>
    Maximum_Mana,

    /// <summary>
    /// 法力值回复：自动回复法力值的速度
    /// </summary>
    Mana_Regeneration,

    /// <summary>
    /// 最大生命值：决定生命值的上限
    /// </summary>
    Maximum_Health,

    /// <summary>
    /// 生命值回复：自动回复生命值的速度
    /// </summary>
    Health_Regeneration,

    /// <summary>
    /// 生命偷取：攻击伤害转为治疗的百分比
    /// </summary>
    Life_Steal,

    /// <summary>
    /// 韧性：减免受到的负面效果的时长
    /// </summary>
    Tenacity,

    /// <summary>
    /// 耐力：被击破状态恢复的速度
    /// </summary>
    Endurance,

    /// <summary>
    /// 闪避率：闪避攻击伤害的概率
    /// </summary>
    Dodge_Rate,

    /// <summary>
    /// 治疗强化：获得治疗值的强化百分比
    /// </summary>
    Heal_Amplification,

    /// <summary>
    /// 护盾强化：获得护盾值的强化百分比
    /// </summary>
    Shield_Amplification,
}
