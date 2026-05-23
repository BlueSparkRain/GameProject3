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
