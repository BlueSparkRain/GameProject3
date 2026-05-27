using UnityEngine;

public class Dot_Burn : DotBase
{
    E_WeaknessType weaknessType=E_WeaknessType.火;

    float timer;
    /// <summary>
    /// 每5s结算一次
    /// </summary>
    float intreval = 5;
    public Dot_Burn(E_Dot _dotType, IBattlable _self, int _dotCount) : base(_dotType, _self, _dotCount){
        timer = intreval; 
    }

    public override void OnDotTrigger(){
        base.OnDotTrigger();
        DotBurn();
    }

    /// <summary>
    /// 造成伤害:层数*目标生命值1%
    /// </summary>
    void DotBurn(){
        Debug.Log($"{self.Camp}结算{dot_type},当前层数:{dot_count}");
        E_Skill_DamageType damageType = DamageTypeChecker.GetDamageType(weaknessType);

        if (damageType == E_Skill_DamageType.物理)
            EventCenter.EventTrigger(E_EventType.Do_PhyAttack, self.battleDamageHandler.BuffHandler);

        float lostHp = self.battleDamageHandler.GetLostHealth();
        float baseDamage = lostHp * dot_count;
        //检查攻击弱点状态（如是->结算伤害x2 + 削盾1点）
        if (self.GetWeakAttack(weaknessType))
        {
            baseDamage *= weakMulti;
            UnityEngine.Debug.Log($"{self.Camp}的{dot_type} Dot触发,结算了一次[(弱点)]伤害:[已损生命值]{lostHp}*[Dot层数]{dot_count}*[弱点倍率]{weakMulti}={baseDamage}");
            self.battleDamageHandler.DoModelValue(E_BattleModelType.ShieldPoints, -1);
        }
        else
        {
            UnityEngine.Debug.Log($"{self.Camp}的{dot_type} Dot触发,结算了一次伤害:[已损生命值]{lostHp}*[Dot层数]{dot_count}={baseDamage}");
        }
        self.battleDamageHandler.GetDamage(damageType, baseDamage);
    }

    public override void OnDotUpdate(){
        base.OnDotUpdate();
        if (timer >= 0) { 
            timer-=Time.deltaTime;
        }
        else{
            timer = intreval;
            Debug.Log("5s到了，触发一次"+dot_type);
            OnDotTrigger();
        }
    }

}
