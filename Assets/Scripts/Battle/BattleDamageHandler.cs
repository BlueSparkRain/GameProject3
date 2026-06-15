using UnityEngine;

/// <summary>
/// 战斗角色伤害处理器 —— 负责【技能】和【Buff/Dot】等伤害/模型的修改
/// </summary>
public class BattleDamageHandler : MonoBehaviour{
    BattleDamager_Magic magic_damageChecker;
    BattleDamager_Physic physic_damageChecker;
    Battle_Controller battleController;
    BattleBuffHandler buffHandler;
    public BattleDotHandler DotHandler => dotHandler;
    BattleDotHandler dotHandler;
    BattleWeaknessHandler weaknessHandler;
    public BattleBuffHandler BuffHandler => buffHandler;
    public BattleWeaknessHandler WeaknessHandler => weaknessHandler;
    public Battle_Controller BattleController=>battleController;
    public void InitDataHandler(BattleMVCHandler mvcHandler, BattleBuffHandler buffHandler,BattleDotHandler dotHandler, BattleWeaknessHandler weaknessHandler)
    {
        battleController = mvcHandler.BattleController;
        this.buffHandler = buffHandler;
        this.dotHandler = dotHandler;
        this.weaknessHandler = weaknessHandler;
        magic_damageChecker = new BattleDamager_Magic(battleController);
        physic_damageChecker = new BattleDamager_Physic(battleController);
    }

    /// <summary>
    /// 攻击：对角色造成一次税前伤害
    /// </summary>
    /// <param name="damageType"></param>
    /// <param name="skillBaseDamage">技能的基础伤害</param>
    public float DoDamage(E_Skill_DamageType damageType, float skillBaseDamage){
        float damageBoomerRate =1.0f+buffHandler.GetDamageRate();
        switch (damageType){
            case E_Skill_DamageType.物理:
                return physic_damageChecker.DoDamage(skillBaseDamage* damageBoomerRate);
            case E_Skill_DamageType.魔法:
                return magic_damageChecker.DoDamage(skillBaseDamage*damageBoomerRate);
        }
        return 0;
    }
    /// <summary>
    /// 检查本次攻击是否命中目标弱点→返回弱点倍率(1.0或2.0)
    /// </summary>
    public float CheckWeakness(E_WeaknessType weaknessType) {
        if (weaknessHandler == null)
            return 1f;
        return weaknessHandler.ProcessWeaknessHit(weaknessType);
    }
    /// <summary>
    /// 外部接口：手动对battleController的属性进行修改
    /// </summary>
    public void DoPropertyValue(E_CharacterPropertyType propertyType, float value)
    {
        magic_damageChecker.DoPropertyValue(propertyType, value);
    }
    /// <summary>
    /// 外部接口：手动对battleController的模型进行修改
    /// </summary>
    public void DoModelValue(E_BattleModelType modelType, float value)
    {
        magic_damageChecker.DoModelValue(modelType, value);
    }

    /// <summary>
    /// 结算：外部税前伤害 经 实际减免计算 后 修改角色模型
    /// </summary>
    /// <param name="damageType"></param>
    /// <param name="damageValue"></param>
    public void GetDamage(E_Skill_DamageType damageType, float damageValue)
    {
        switch (damageType)
        {
            case E_Skill_DamageType.物理:
                float da = physic_damageChecker.GetDamage(damageValue);

                if (battleController.IsBreak)
                    da *= 2f;

                da = Mathf.Min(da, battleController.IsBreak ? 9999f : 999f);

                DebugManager.Log(EDebugCategory.BattleDamage,name + "Get-----税后伤害:" + da);
                BattleDebugManager.LogFormat("{0} 受到 {1:0.#} 点物理伤害{2}",
                    battleController.CharacterData.Character_Name, da,
                    battleController.IsBreak ? " (力竭加成)" : "");

                battleController.AdjustCharacterModelValue(E_BattleModelType.HP, da);
                break;
            case E_Skill_DamageType.魔法:
                float db = magic_damageChecker.GetDamage(damageValue);

                if (battleController.IsBreak)
                    db *= 2f;

                db = Mathf.Min(db, battleController.IsBreak ? 9999f : 999f);

                DebugManager.Log(EDebugCategory.BattleDamage,name + "Get-----税后伤害:" + db);
                BattleDebugManager.LogFormat("{0} 受到 {1:0.#} 点魔法伤害{2}",
                    battleController.CharacterData.Character_Name, db,
                    battleController.IsBreak ? " (力竭加成)" : "");
                battleController.AdjustCharacterModelValue(E_BattleModelType.HP, db);
                break;
            default:
                break;
        }

    }


    /// <summary>
    /// 获取已损失生命值
    /// </summary>
    public int GetLostHealth() {
        return (int)(battleController.GetCharacterModelValue(E_BattleModelType.MAX_HP)-battleController.GetCharacterModelValue(E_BattleModelType.HP));
    }

    /// <summary>
    /// 获取最大生命值
    /// </summary>
    public int GetMaxHealth() {
        return (int)battleController.GetCharacterModelValue(E_BattleModelType.MAX_HP);
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    public int GetCurrentHealth()
    {
        return (int)battleController.GetCharacterModelValue(E_BattleModelType.HP);
    }
}
