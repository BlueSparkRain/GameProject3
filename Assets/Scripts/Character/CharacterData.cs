using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 记录一名角色当前的属性数据（战斗中读取的是当前的属性数据（而非SOData））
/// </summary>
public class CharacterData : MonoBehaviour
{
    [Header("角色")]
    public E_CharacterType characterType;

    string characterSO_ParentPath = "SOData/CharacterSOData/";
    /// <summary>
    /// 角色初始数据
    /// </summary>
    CharacterDataSO characterData;

    /// <summary>
    /// 角色属性成长数据
    /// </summary>
    CharcterPropertyGrowthSO CharcterPropertyGrowthSO;


    #region 角色基础属性声明——(游戏初始化读取SOData中的数据)
    /// <summary>
    /// 物理固穿：固定减少对方物抗
    /// </summary>
    private float Phy_Flat_Penetration;

    /// <summary>
    /// 法术固穿：固定减少对方魔抗
    /// </summary>
    private float Mag_Flat_Penetration;

    /// <summary>
    /// 物抗：减少受到的物理伤害
    /// </summary>
    private float Phy_Resistance;

    /// <summary>
    /// 魔抗：减少受到的魔法伤害
    /// </summary>
    private float Mag_Resistance;

    /// <summary>
    /// 物攻：影响物理伤害
    /// </summary>
    private float Phy_Attack;

    /// <summary>
    /// 法强：影响魔法伤害
    /// </summary>
    private float Magic_Attack;

    /// <summary>
    /// 最大法力值：决定法力值的上限
    /// </summary>
    private float Maximum_Mana;

    /// <summary>
    /// 法力值回复：自动回复法力值的速度
    /// </summary>
    private float Mana_Regeneration;

    /// <summary>
    /// 最大生命值：决定生命值的上限
    /// </summary>
    private float Maximum_Health;

    /// <summary>
    /// 生命值回复：自动回复生命值的速度
    /// </summary>
    private float Health_Regeneration;

    /// <summary>
    /// 生命偷取：攻击伤害转为治疗的百分比
    /// </summary>
    private float Life_Steal;

    /// <summary>
    /// 韧性：减免受到的负面效果的时长
    /// </summary>
    private float Tenacity;

    /// <summary>
    /// 耐力：被击破状态恢复的速度
    /// </summary>
    private float Endurance;

    /// <summary>
    /// 闪避率：闪避攻击伤害的概率
    /// </summary>
    private float Dodge_Rate;

    /// <summary>
    /// 治疗强化：获得治疗值的强化百分比
    /// </summary>
    private float Heal_Amplification;

    /// <summary>
    /// 护盾强化：获得护盾值的强化百分比
    /// </summary>
    private float Shield_Amplification;

    private int Maximum_ATB;

    #endregion
    Dictionary<E_CharacterPropertyType, float> propertyDic = new Dictionary<E_CharacterPropertyType, float>();

    IBattleUnit battleUnit;
    public void InitCharacter() {
        battleUnit = new Player(); 
    
    }
    CharacterDataSO LoadCharacterSOData()
    {
        return Resources.Load<CharacterDataSO>(characterSO_ParentPath + characterType);
    }

    void Start()
    {
        InitCharacterData();
        InitSkillDic();
    }
    /// <summary>
    /// 读取角色初始数据
    /// </summary>
    void InitCharacterData()
    {
        characterData = LoadCharacterSOData();
        Phy_Flat_Penetration = characterData.Phy_Flat_Penetration;
        Mag_Flat_Penetration = characterData.Mag_Flat_Penetration;
        Phy_Resistance = characterData.Phy_Resistance;
        Mag_Resistance = characterData.Mag_Resistance;
        Phy_Attack = characterData.Phy_Attack;
        Magic_Attack = characterData.Magic_Attack;
        Maximum_Mana = characterData.Maximum_Mana;
        Mana_Regeneration = characterData.Mana_Regeneration;
        Maximum_Health = characterData.Maximum_Health;
        Health_Regeneration = characterData.Health_Regeneration;
        Life_Steal = characterData.Life_Steal;
        Tenacity = characterData.Tenacity;
        Endurance = characterData.Endurance;
        Dodge_Rate = characterData.Dodge_Rate;
        Heal_Amplification = characterData.Heal_Amplification;
        Shield_Amplification = characterData.Shield_Amplification;
        Maximum_ATB= characterData.Maximum_ATB;
    }

    /// <summary>
    /// 注册属性字典
    /// </summary>
    void InitSkillDic()
    {
        propertyDic.Add(E_CharacterPropertyType.Phy_Flat_Penetration, Phy_Flat_Penetration);
        propertyDic.Add(E_CharacterPropertyType.Mag_Flat_Penetration, Mag_Flat_Penetration);
        propertyDic.Add(E_CharacterPropertyType.Phy_Resistance, Phy_Resistance);
        propertyDic.Add(E_CharacterPropertyType.Mag_Resistance, Mag_Resistance);
        propertyDic.Add(E_CharacterPropertyType.Phy_Attack, Phy_Attack);
        propertyDic.Add(E_CharacterPropertyType.Magic_Attack, Magic_Attack);
        propertyDic.Add(E_CharacterPropertyType.Maximum_Mana, Maximum_Mana);
        propertyDic.Add(E_CharacterPropertyType.Mana_Regeneration, Mana_Regeneration);
        propertyDic.Add(E_CharacterPropertyType.Maximum_Health, Maximum_Health);
        propertyDic.Add(E_CharacterPropertyType.Health_Regeneration, Health_Regeneration);
        propertyDic.Add(E_CharacterPropertyType.Life_Steal, Life_Steal);
        propertyDic.Add(E_CharacterPropertyType.Tenacity, Tenacity);
        propertyDic.Add(E_CharacterPropertyType.Endurance, Endurance);
        propertyDic.Add(E_CharacterPropertyType.Dodge_Rate, Dodge_Rate);
        propertyDic.Add(E_CharacterPropertyType.Heal_Amplification, Heal_Amplification);
        propertyDic.Add(E_CharacterPropertyType.Shield_Amplification, Shield_Amplification);
        propertyDic.Add(E_CharacterPropertyType.Maximum_ATB, Maximum_ATB);
    }

    public float GetProperty(E_CharacterPropertyType propertyType)
    {
        if (propertyDic.ContainsKey(propertyType))
            return propertyDic[propertyType];
        else
        {
            Debug.Log("未查找到目标属性");
            return 0;
        }
    }

    /// <summary>
    /// 对目标属性应用一次变化
    /// </summary>
    /// <param name="targetPropertyType"></param>
    /// <param name="expression"></param>
    public void AdjustProperty(E_CharacterPropertyType targetPropertyType, float expression)
    {
        if (propertyDic.ContainsKey(targetPropertyType))
            propertyDic[targetPropertyType] += expression;
        else
        {
            Debug.Log("未查找到目标属性");
            return;
        }
    }
}
