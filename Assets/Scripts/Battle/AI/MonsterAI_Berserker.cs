using UnityEngine;

/// <summary>
/// 狂战士AI：当HP首次低于40%时，释放一次指定技能并获得属性增益。
/// 作为IMonsterAIComponent的示例实现，展示条件触发→技能释放→属性调整的完整链路。
/// </summary>
[MonsterAIFor(E_CharacterType.LE_狂战兵)]
public class MonsterAI_Berserker : IMonsterAIComponent
{
    [Header("触发阈值(生命值百分比)")]
    public float hpThreshold = 0.4f;

    [Header("狂暴时释放的技能")]
    public E_SkillName rageSkill = E_SkillName.雷电风暴;

    [Header("物理攻击加成百分比(0.2 = +20%)")]
    public float attackBoostRate = 0.2f;

    bool hasTriggered;
    Battle_Controller controller;
    BattleSkiller skiller;

    public void OnBattleStart(Battle_Controller controller, BattleSkiller skiller)
    {
        this.controller = controller;
        this.skiller = skiller;
        hasTriggered = false;
        Debug.Log($"[BerserkerAI] {controller.CharacterData.Character_Name} 狂战士AI就绪，阈值:{hpThreshold * 100}%");
    }

    public void OnBattleUpdate(Battle_Controller controller, BattleSkiller skiller)
    {
        if (hasTriggered) return;

        float hpPercent = controller.GetHPPercentage();
        if (hpPercent > hpThreshold) return;

        TriggerRage();
    }

    public void OnHPChanged(float currentHP, float maxHP, Battle_Controller controller, BattleSkiller skiller)
    {
        if (hasTriggered) return;

        float hpPercent = currentHP / maxHP;
        if (hpPercent <= hpThreshold)
            TriggerRage();
    }

    void TriggerRage()
    {
        hasTriggered = true;
        Debug.Log($"[BerserkerAI] {controller.CharacterData.Character_Name} 血量过低，进入狂暴状态!");

        // 释放狂暴技能 —— 通过SO体系解析枚举→技能ID→技能实例
        var rageSO = ResourcesLoader.FindSkillSOBySkillName(rageSkill);
        if (rageSO != null)
        {
            var skill = BattleSkillFactory.CreateBattleSkill(rageSO.skill_ID, skiller.Self);
            skill?.SkillExcute(E_SkillLevel.基础版本);
        }

        // 属性增幅
        controller.AdjustCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack,
            controller.GetCharacterPropertyValue(E_CharacterPropertyType.Phy_Attack) * attackBoostRate);
        controller.AdjustCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack,
            controller.GetCharacterPropertyValue(E_CharacterPropertyType.Mag_Attack) * attackBoostRate);

        Debug.Log($"[BerserkerAI] {controller.CharacterData.Character_Name} 攻击力提升{attackBoostRate * 100}%");
    }
}
