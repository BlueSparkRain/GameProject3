public interface IUpGradable
{
    CharcterPropertyGrowthSO growthData { get; set; }
    CharacterData characterData { get; set; }
    public void UpGrade()
    {
        UnityEngine.Debug.Log("升级了！属性提升并保存");
        characterData.AdjustProperty(E_CharacterPropertyType.Phy_Flat_Penetration, growthData.Phy_Flat_Penetration_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Mag_Flat_Penetration, growthData.Mag_Flat_Penetration_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Phy_Resistance, growthData.Phy_Resistance_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Mag_Resistance, growthData.Mag_Resistance_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Phy_Attack, growthData.Phy_Attack_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Mag_Attack, growthData.Magic_Attack_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Maximum_Mana, growthData.Maximum_Mana_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Mana_Regeneration, growthData.Mana_Regeneration_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Maximum_Health, growthData.Maximum_Health_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Health_Regeneration, growthData.Health_Regeneration_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Life_Steal, growthData.Life_Steal_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Tenacity, growthData.Tenacity_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Endurance, growthData.Endurance_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Dodge_Rate, growthData.Dodge_Rate_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Heal_Amplification, growthData.Heal_Amplification_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Shield_Amplification, growthData.Shield_Amplification_grow);
        characterData.AdjustProperty(E_CharacterPropertyType.Maximum_ATB, growthData.Maximum_ATB_grow);

        UnityEngine.Debug.Log("保存Save_CharacterData文件:" + JsonSaver.GetSavePath<Save_CharacterData>(characterData.characterType.ToString()));
        // 修改完直接保存，永久生效
        JsonSaver.Save(new Save_CharacterData(characterData), characterData.characterType.ToString());
    }
}


