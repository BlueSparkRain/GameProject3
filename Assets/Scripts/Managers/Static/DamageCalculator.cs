using System;
using UnityEngine;
public static class DamageCalculator
{
    /// <summary>
    /// 攻击方造成的初始伤害
    /// 公式:(基础攻击力*技能倍率)*其他修正
    /// </summary>
    /// <param name="_baseAttack">基础攻击力（物理/法术）</param>
    /// <param name="_skillMultiRate">技能基础倍率</param>
    /// <param name="_otherMulitiRate">其他修正</param>
    /// <returns></returns>
    public static float DoBaseDamage(float _baseAttack, float _skillMultiRate)//, Func<float, float> _otherMulitiRate)
    {
        //攻击方造成一定数值的伤害
        return _baseAttack * _skillMultiRate;
    }

    /// <summary>
    /// 受击方减免后的受到的实际伤害
    /// 公式：(原始伤害*（1-伤害减免))*其他修正
    /// </summary>
    /// <param name="_damageValue"></param>
    /// <param name="_resistanceRate"></param>
    /// <param name="_otherMulitiRate"></param>
    /// <returns></returns>
    public static float GetFinalDamage(float _damageValue, float _resistanceRate)
    {
        float value = _damageValue *  _resistanceRate;
        Debug.Log($"收到实际伤害计算： {_damageValue}*{_resistanceRate}={value}");
        return _damageValue * _resistanceRate;
    }
}
public abstract class DamageChecker {
    protected Battle_Controller self;
    public DamageChecker(Battle_Controller _self) {
        self = _self;}
    public abstract float DoDamage(float skill_attack_rate);
    public abstract float GetDamage(float damage_value); 
    public void DoPropertyValue(E_CharacterPropertyType propertyType,float value){ 
        self.AdjustCharacterData(propertyType,value);
    }
    public void DoModelValue(E_BattleModelType  modelType, float value){
        self.AdjustCharacterModelValue(modelType,value);
    }
}
public class DamageChecker_Physic : DamageChecker{
    public DamageChecker_Physic(Battle_Controller _self) : base(_self){}

    public override float DoDamage(float skill_attack_baseDamage)
    {
       return  DamageCalculator.DoBaseDamage(self.GetCharacterData(E_CharacterPropertyType.Phy_Attack),skill_attack_baseDamage);
    }

    //物理伤害减免=实际物抗/（100+实际物抗）
    public override float GetDamage(float damage_value)
    {
        float resistance = self.GetCharacterData(E_CharacterPropertyType.Phy_Resistance);
        float resistanceRate = resistance / (100 + resistance);
        return DamageCalculator.GetFinalDamage(damage_value, resistanceRate);
    }
}
public class DamageChecker_Magic : DamageChecker {
    public DamageChecker_Magic(Battle_Controller _self) : base(_self){}

    public override float DoDamage(float skill_attack_rate)
    {
        return DamageCalculator.DoBaseDamage(self.GetCharacterData(E_CharacterPropertyType.Mag_Attack), skill_attack_rate);
    }

    public override float GetDamage(float damage_value)
    {
        float resistance = self.GetCharacterData(E_CharacterPropertyType.Mag_Resistance);
        float resistanceRate = resistance / (100 + resistance);
        return DamageCalculator.GetFinalDamage(damage_value, resistanceRate);
    }

}
