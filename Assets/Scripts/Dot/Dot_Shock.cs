public class Dot_Shock : DotBase{
    E_WeaknessType weaknessType = E_WeaknessType.雷;
    public Dot_Shock(E_Dot _dotType, IBattlable _self, int _dotCount) : base(_dotType, _self, _dotCount){

    }
    /// <summary>
    /// 总伤害机制,感电伤害(=层数*目标已损失生命值*3%)
    /// </summary>
    public override void OnDotTrigger(){
        base.OnDotTrigger();
        DotShock();
    }
    public override void OnDotUpdate(){
        base.OnDotUpdate();
    }
    /// <summary>
    /// 造成感电伤害(=层数*目标已损失生命值*3%)
    /// </summary>
    void DotShock(){
        DebugManager.Log(EDebugCategory.BattleDOT, $"{self.Camp}触发{weaknessType},当前层数:{dot_count}");
        float lostHp = self.battleDamageHandler.GetLostHealth();
        float baseDamage = lostHp * dot_count * 0.03f;
        //检查攻击对象的状态（弱点->造成伤害x2 + 削减1点）
        if (self.GetWeakAttack(weaknessType)){
            baseDamage *= weakMulti;
            DebugManager.Log(EDebugCategory.BattleDOT, $"{self.Camp}受{dot_type} Dot伤害,造成一次[(弱点)]伤害:[已损失生命值]{lostHp}*[Dot层数]{dot_count}*[弱点倍率]{weakMulti}={baseDamage}");
            self.battleDamageHandler.DoModelValue(E_BattleModelType.ShieldPoints, -1);
        }
        else{
            DebugManager.Log(EDebugCategory.BattleDOT, $"{self.Camp}受{dot_type} Dot伤害,造成一次伤害:[已损失生命值]{lostHp}*[Dot层数]{dot_count}={baseDamage}");
        }
        self.battleDamageHandler.GetDamage(E_Skill_DamageType.魔法, baseDamage);
    }
}
