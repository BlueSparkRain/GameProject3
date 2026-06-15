using UnityEngine;

public class Dot_Burn : DotBase
{
    E_WeaknessType weaknessType = E_WeaknessType.火;

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
    /// 造成燃烧的魔法伤害:层数*目标当前生命值*1%
    /// </summary>
    void DotBurn(){
        DebugManager.Log(EDebugCategory.BattleDOT, $"{self.Camp}结算{dot_type},当前层数:{dot_count}");

        float curHp = self.battleDamageHandler.GetCurrentHealth();
        float baseDamage = curHp * dot_count * 0.01f;
        //检查攻击弱点状态（如是->结算伤害x2 + 削盾1点）
        if (self.GetWeakAttack(weaknessType)){
            baseDamage *= weakMulti;
            DebugManager.Log(EDebugCategory.BattleDOT, $"{self.Camp}的{dot_type} Dot触发,结算了一次[(弱点)]伤害:[当前生命值]{curHp}*[Dot层数]{dot_count}*[弱点倍率]{weakMulti}={baseDamage}");
            self.battleDamageHandler.DoModelValue(E_BattleModelType.ShieldPoints, -1);
        }
        else{
            DebugManager.Log(EDebugCategory.BattleDOT, $"{self.Camp}的{dot_type} Dot触发,结算了一次伤害:[当前生命值]{curHp}*[Dot层数]{dot_count}={baseDamage}");
        }
        self.battleDamageHandler.GetDamage(E_Skill_DamageType.魔法, baseDamage);
    }

    public override void OnDotUpdate(){
        base.OnDotUpdate();
        if (timer >= 0) {
            timer-=Time.deltaTime;
        }
        else{
            timer = intreval;
            DebugManager.Log(EDebugCategory.BattleDOT, "5s到了，触发一次"+dot_type);
            OnDotTrigger();
        }
    }

}
