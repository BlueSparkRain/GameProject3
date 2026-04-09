using UnityEngine;

public enum E_CharacterType{
    P_1,
    P_2,
    P_3,
    P_4,

    LE_1,
    LE_2,
    LE_3,

    ME_1,
    ME_2,
    ME_3,

    Boss_1,
    Boss_2,
    Boss_3,
}



[CreateAssetMenu(menuName = "SOData/CharacterData", fileName = "CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    [Header("角色")]
    public E_CharacterType characterType;

    [Header("角色图标")]
    public Sprite characterSprite;

    [Header("角色昵称")]
    public string  characterName;

    #region 人物基础属性声明
    [Header("物理固穿")]
    [Tooltip("固定减少对方物抗")]
    public float Phy_Flat_Penetration;

    [Header("法术固穿")]
    [Tooltip("固定减少对方魔抗")]
    public float Mag_Flat_Penetration;

    [Header("物抗")]
    [Tooltip("减少受到的物理伤害")]
    public float Phy_Resistance;

    [Header("魔抗")]
    [Tooltip("减少受到的魔法伤害")]
    public float Mag_Resistance;

    [Header("物攻")]
    [Tooltip("影响物理伤害")]
    public float Phy_Attack;

    [Header("法强")]
    [Tooltip("影响魔法伤害")]
    public float Magic_Attack;

    [Header("最大法力值")]
    [Tooltip("决定法力值的上限")]
    public float Maximum_Mana;

    [Header("法力值回复")]
    [Tooltip("自动回复法力值的速度")]
    public float Mana_Regeneration;

    [Header("最大生命值")]
    [Tooltip("决定生命值的上限")]
    public float Maximum_Health;

    [Header("生命值回复")]
    [Tooltip("自动回复生命值的速度")]
    public float Health_Regeneration;

    [Header("生命偷取")]
    [Tooltip("攻击伤害转为治疗的百分比")]
    public float Life_Steal;

    [Header("韧性")]
    [Tooltip("减免受到的负面效果的时长")]
    public float Tenacity;

    [Header("耐力")]
    [Tooltip("被击破状态恢复的速度")]
    public float Endurance;

    [Header("闪避率")]
    [Tooltip("闪避攻击伤害的概率")]
    public float Dodge_Rate;

    [Header("治疗强化")]
    [Tooltip("获得治疗值的强化百分比")]
    public float Heal_Amplification;

    [Header("护盾强化")]
    [Tooltip("获得护盾值的强化百分比")]
    public float Shield_Amplification;


    [Tooltip("初始ATB点数上限")]
    public int Maximum_ATB;
    #endregion

    public float GetProperty(E_CharacterPropertyType type)
    {
        switch (type)
        {
            case E_CharacterPropertyType.Phy_Flat_Penetration: return Phy_Flat_Penetration;
            case E_CharacterPropertyType.Mag_Flat_Penetration: return Mag_Flat_Penetration;
            case E_CharacterPropertyType.Phy_Resistance: return Phy_Resistance;
            case E_CharacterPropertyType.Mag_Resistance: return Mag_Resistance;
            case E_CharacterPropertyType.Phy_Attack: return Phy_Attack;
            case E_CharacterPropertyType.Magic_Attack: return Magic_Attack;
            case E_CharacterPropertyType.Maximum_Mana: return Maximum_Mana;
            case E_CharacterPropertyType.Mana_Regeneration: return Mana_Regeneration;
            case E_CharacterPropertyType.Maximum_Health: return Maximum_Health;
            case E_CharacterPropertyType.Health_Regeneration: return Health_Regeneration;
            case E_CharacterPropertyType.Life_Steal: return Life_Steal;
            case E_CharacterPropertyType.Tenacity: return Tenacity;
            case E_CharacterPropertyType.Endurance: return Endurance;
            case E_CharacterPropertyType.Dodge_Rate: return Dodge_Rate;
            case E_CharacterPropertyType.Heal_Amplification: return Heal_Amplification;
            case E_CharacterPropertyType.Shield_Amplification: return Shield_Amplification;
            case E_CharacterPropertyType.Maximum_ATB: return Maximum_ATB;
            default: Debug.LogError("属性不存在"); return 0;
        }
    }
}
