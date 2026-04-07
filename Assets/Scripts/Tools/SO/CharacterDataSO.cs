using UnityEngine;

public enum E_CharacterType{
    P_1,
    P_2,
    P_3,
    P_4,
    LE_1,
    LE_2,
    Boss_1,
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

}
