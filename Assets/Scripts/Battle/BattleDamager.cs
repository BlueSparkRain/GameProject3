public abstract class BattleDamager
{
    protected Battle_Controller self;
    public BattleDamager(Battle_Controller _self)
    {
        self = _self;
    }
    public abstract float DoDamage(float skill_attack_rate);
    public abstract float GetDamage(float damage_value);
    public void DoPropertyValue(E_CharacterPropertyType propertyType, float value)
    {
        self.AdjustCharacterPropertyValue(propertyType, value);
    }
    public void DoModelValue(E_BattleModelType modelType, float value)
    {
        self.AdjustCharacterModelValue(modelType, value);
    }
}
public class BattleDamager_Physic : BattleDamager
{
    public BattleDamager_Physic(Battle_Controller _self) : base(_self) { }

    public override float DoDamage(float skill_attack_baseDamage)
    {
        return DamageCalculator.DoBaseDamage(self.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack), skill_attack_baseDamage);
    }

    //物理伤害减免=实际物抗/（100+实际物抗）
    public override float GetDamage(float damage_value)
    {
        float resistance = self.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Resistance);
        float resistanceRate = resistance / (100 + resistance);
        return DamageCalculator.GetFinalDamage(damage_value, resistanceRate);
    }
}
public class BattleDamager_Magic : BattleDamager
{
    public BattleDamager_Magic(Battle_Controller _self) : base(_self) { }

    public override float DoDamage(float skill_attack_rate)
    {
        return DamageCalculator.DoBaseDamage(self.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack), skill_attack_rate);
    }

    public override float GetDamage(float damage_value)
    {
        float resistance = self.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Resistance);
        float resistanceRate = resistance / (100 + resistance);
        return DamageCalculator.GetFinalDamage(damage_value, resistanceRate);
    }

}
