using System;
using UnityEngine;


/// <summary>
/// 记录一名角色当前的属性数据（战斗中读取的是当前的属性数据（而非SOData））
/// </summary>
public class CharacterData : ISaveable
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

    #region 角色基础属性声明——(游戏初始化读取SO数据/Jason文件中读取数据)
    /// <summary>
    /// 1.物理固穿：固定减少对方物抗
    /// </summary>
    private float phy_Flat_Penetration;
    /// <summary>
    /// 2.法术固穿：固定减少对方魔抗
    /// </summary>
    private float mag_Flat_Penetration;
    /// <summary>
    /// 3.物抗：减少受到的物理伤害
    /// </summary>
    private float phy_Resistance;
    /// <summary>
    /// 4.魔抗：减少受到的魔法伤害
    /// </summary>
    private float mag_Resistance;
    /// <summary>
    /// 5.物攻：影响物理伤害
    /// </summary>
    private float phy_Attack;
    /// <summary>
    /// 6.法强：影响魔法伤害
    /// </summary>
    private float magic_Attack;
    /// <summary>
    /// 7.最大法力值：决定法力值的上限
    /// </summary>
    private float maximum_Mana;
    /// <summary>
    /// 8.法力值回复：自动回复法力值的速度
    /// </summary>
    private float mana_Regeneration;
    /// <summary>
    /// 9.最大生命值：决定生命值的上限
    /// </summary>
    private float maximum_Health;
    /// <summary>
    /// 10.生命值回复：自动回复生命值的速度
    /// </summary>
    private float health_Regeneration;
    /// <summary>
    /// 11.生命偷取：攻击伤害转为治疗的百分比
    /// </summary>
    private float life_Steal;
    /// <summary>
    /// 12.韧性：减免受到的负面效果的时长
    /// </summary>
    private float tenacity;
    /// <summary>
    /// 13.耐力：被击破状态恢复的速度
    /// </summary>
    private float endurance;
    /// <summary>
    /// 14.闪避率：闪避攻击伤害的概率
    /// </summary>
    private float dodge_Rate;
    /// <summary>
    /// 15.治疗强化：获得治疗值的强化百分比
    /// </summary>
    private float heal_Amplification;
    /// <summary>
    /// 16.护盾强化：获得护盾值的强化百分比
    /// </summary>
    private float shield_Amplification;
    /// <summary>
    /// 17.最大ATB点数
    /// </summary>
    private int maximum_ATB;


    public float Phy_Flat_Penetration => phy_Flat_Penetration;
    public float Mag_Flat_Penetration => mag_Flat_Penetration;
    public float Phy_Resistance => phy_Resistance;
    public float Mag_Resistance => mag_Resistance;
    public float Phy_Attack => phy_Attack;
    public float Magic_Attack => magic_Attack;
    public float Maximum_Mana => maximum_Mana;
    public float Mana_Regeneration => mana_Regeneration;
    public float Maximum_Health => maximum_Health;
    public float Health_Regeneration => health_Regeneration;
    public float Life_Steal => life_Steal;
    public float Tenacity => tenacity;
    public float Endurance => endurance;
    public float Dodge_Rate => dodge_Rate;
    public float Heal_Amplification => heal_Amplification;
    public float Shield_Amplification => shield_Amplification;
    public int Maximum_ATB => maximum_ATB;
    #endregion

    public int CurrentLevel => currentLevel;

    /// <summary>
    /// 18.角色当前等级
    /// </summary>
    private int currentLevel;

    public CharacterData(E_CharacterType _characterType) {
        characterType = _characterType;
        currentLevel = 1;
        JsonSaver.InitData<Save_CharacterData>(this);
    }

    public void InitBySaveData()
    {
        var characterSaveData = JsonSaver.Load<Save_CharacterData>();
        phy_Flat_Penetration = characterSaveData.Phy_Flat_Penetration;
        mag_Flat_Penetration = characterSaveData.Mag_Flat_Penetration;
        phy_Resistance = characterSaveData.Phy_Resistance;
        mag_Resistance = characterSaveData.Mag_Resistance;
        phy_Attack = characterSaveData.Phy_Attack;
        magic_Attack = characterSaveData.Magic_Attack;
        maximum_Mana = characterSaveData.Maximum_Mana;
        mana_Regeneration = characterSaveData.Mana_Regeneration;
        maximum_Health = characterSaveData.Maximum_Health;
        health_Regeneration = characterSaveData.Health_Regeneration;
        life_Steal = characterSaveData.Life_Steal;
        tenacity = characterSaveData.Tenacity;
        endurance = characterSaveData.Endurance;
        dodge_Rate = characterSaveData.Dodge_Rate;
        heal_Amplification = characterSaveData.Heal_Amplification;
        shield_Amplification = characterSaveData.Shield_Amplification;
        maximum_ATB = characterSaveData.Maximum_ATB;
        currentLevel = characterSaveData.CurrentLevel;
    }

    public void InitBySelf()
    {
        characterData = Resources.Load<CharacterDataSO>(characterSO_ParentPath + characterType);
        phy_Flat_Penetration = characterData.Phy_Flat_Penetration;
        mag_Flat_Penetration = characterData.Mag_Flat_Penetration;
        phy_Resistance = characterData.Phy_Resistance;
        mag_Resistance = characterData.Mag_Resistance;
        phy_Attack = characterData.Phy_Attack;
        magic_Attack = characterData.Magic_Attack;
        maximum_Mana = characterData.Maximum_Mana;
        mana_Regeneration = characterData.Mana_Regeneration;
        maximum_Health = characterData.Maximum_Health;
        health_Regeneration = characterData.Health_Regeneration;
        life_Steal = characterData.Life_Steal;
        tenacity = characterData.Tenacity;
        endurance = characterData.Endurance;
        dodge_Rate = characterData.Dodge_Rate;
        heal_Amplification = characterData.Heal_Amplification;
        shield_Amplification = characterData.Shield_Amplification;
        maximum_ATB = characterData.Maximum_ATB;
    }
  
    public float GetProperty(E_CharacterPropertyType type)
    {
        switch (type)
        {
            case E_CharacterPropertyType.Phy_Flat_Penetration: return phy_Flat_Penetration;
            case E_CharacterPropertyType.Mag_Flat_Penetration: return mag_Flat_Penetration;
            case E_CharacterPropertyType.Phy_Resistance: return phy_Resistance;
            case E_CharacterPropertyType.Mag_Resistance: return mag_Resistance;
            case E_CharacterPropertyType.Phy_Attack: return phy_Attack;
            case E_CharacterPropertyType.Magic_Attack: return magic_Attack;
            case E_CharacterPropertyType.Maximum_Mana: return maximum_Mana;
            case E_CharacterPropertyType.Mana_Regeneration: return mana_Regeneration;
            case E_CharacterPropertyType.Maximum_Health: return maximum_Health;
            case E_CharacterPropertyType.Health_Regeneration: return health_Regeneration;
            case E_CharacterPropertyType.Life_Steal: return life_Steal;
            case E_CharacterPropertyType.Tenacity: return tenacity;
            case E_CharacterPropertyType.Endurance: return endurance;
            case E_CharacterPropertyType.Dodge_Rate: return dodge_Rate;
            case E_CharacterPropertyType.Heal_Amplification: return heal_Amplification;
            case E_CharacterPropertyType.Shield_Amplification: return shield_Amplification;
            case E_CharacterPropertyType.Maximum_ATB: return maximum_ATB;
            default: Debug.LogError("属性不存在"); return 0;
        }
    }

    // 修改属性（直接改私有字段，改完直接存档，100%同步）
    public void AdjustProperty(E_CharacterPropertyType type, float value)
    {
        switch (type)
        {
            case E_CharacterPropertyType.Phy_Flat_Penetration: phy_Flat_Penetration += value; break;
            case E_CharacterPropertyType.Mag_Flat_Penetration: mag_Flat_Penetration += value; break;
            case E_CharacterPropertyType.Phy_Resistance: phy_Resistance += value; break;
            case E_CharacterPropertyType.Mag_Resistance: mag_Resistance += value; break;
            case E_CharacterPropertyType.Phy_Attack: phy_Attack += value; break;
            case E_CharacterPropertyType.Magic_Attack: magic_Attack += value; break;
            case E_CharacterPropertyType.Maximum_Mana: maximum_Mana += value; break;
            case E_CharacterPropertyType.Mana_Regeneration: mana_Regeneration += value; break;
            case E_CharacterPropertyType.Maximum_Health: maximum_Health += value; break;
            case E_CharacterPropertyType.Health_Regeneration: health_Regeneration += value; break;
            case E_CharacterPropertyType.Life_Steal: life_Steal += value; break;
            case E_CharacterPropertyType.Tenacity: tenacity += value; break;
            case E_CharacterPropertyType.Endurance: endurance += value; break;
            case E_CharacterPropertyType.Dodge_Rate: dodge_Rate += value; break;
            case E_CharacterPropertyType.Heal_Amplification: heal_Amplification += value; break;
            case E_CharacterPropertyType.Shield_Amplification: shield_Amplification += value; break;
            case E_CharacterPropertyType.Maximum_ATB: maximum_ATB += (int)value; break;
            default: Debug.LogError("属性不存在"); return;
        }
        Debug.Log("保存Save_CharacterData文件:" + JsonSaver.GetSaveFilePath<Save_CharacterData>());
        // 修改完直接保存，永久生效
        JsonSaver.Save(new Save_CharacterData(this));
        Debug.Log($"属性修改成功: {type} = {GetProperty(type)}");
    }
}


[Serializable]
public class Save_CharacterData : IValidatable
{
    public Save_CharacterData() { }
    public Save_CharacterData(CharacterData characterSaveData)
    {
        Phy_Flat_Penetration = characterSaveData.Phy_Flat_Penetration;
        Mag_Flat_Penetration = characterSaveData.Mag_Flat_Penetration;
        Phy_Resistance = characterSaveData.Phy_Resistance;
        Mag_Resistance = characterSaveData.Mag_Resistance;
        Phy_Attack = characterSaveData.Phy_Attack;
        Magic_Attack = characterSaveData.Magic_Attack;
        Maximum_Mana = characterSaveData.Maximum_Mana;
        Mana_Regeneration = characterSaveData.Mana_Regeneration;
        Maximum_Health = characterSaveData.Maximum_Health;
        Health_Regeneration = characterSaveData.Health_Regeneration;
        Life_Steal = characterSaveData.Life_Steal;
        Tenacity = characterSaveData.Tenacity;
        Endurance = characterSaveData.Endurance;
        Dodge_Rate = characterSaveData.Dodge_Rate;
        Heal_Amplification = characterSaveData.Heal_Amplification;
        Shield_Amplification = characterSaveData.Shield_Amplification;
        Maximum_ATB = characterSaveData.Maximum_ATB;
        CurrentLevel = characterSaveData.CurrentLevel;
    }
    /// <summary>
    /// [Save]物理固穿
    /// </summary>
    public float Phy_Flat_Penetration;

    /// <summary>
    /// [Save]法术固穿
    /// </summary>
    public float Mag_Flat_Penetration;

    /// <summary>
    /// [Save]物抗
    /// </summary>
    public float Phy_Resistance;

    /// <summary>
    /// [Save]魔抗
    /// </summary>
    public float Mag_Resistance;

    /// <summary>
    /// [Save]物攻
    /// </summary>
    public float Phy_Attack;

    /// <summary>
    /// [Save]法强
    /// </summary>
    public float Magic_Attack;

    /// <summary>
    /// [Save]最大法力值
    /// </summary>
    public float Maximum_Mana;

    /// <summary>
    /// [Save]法力值回复
    /// </summary>
    public float Mana_Regeneration;

    /// <summary>
    /// [Save]最大生命值
    /// </summary>
    public float Maximum_Health;

    /// <summary>
    /// [Save]生命值回复
    /// </summary>
    public float Health_Regeneration;

    /// <summary>
    /// [Save]生命偷取
    /// </summary>
    public float Life_Steal;

    /// <summary>
    /// [Save]韧性
    /// </summary>
    public float Tenacity;

    /// <summary>
    /// [Save]耐力
    /// </summary>
    public float Endurance;

    /// <summary>
    /// [Save]闪避率
    /// </summary>
    public float Dodge_Rate;

    /// <summary>
    /// [Save]治疗强化
    /// </summary>
    public float Heal_Amplification;

    /// <summary>
    /// [Save]护盾强化
    /// </summary>
    public float Shield_Amplification;

    /// <summary>
    /// 当前最大ATB值
    /// </summary>
    public int Maximum_ATB;

    /// <summary>
    /// [Save]角色当前等级
    /// </summary>
    public int CurrentLevel;

    public bool IsValid()
    {
        return CurrentLevel > 0;
    }
}