using Core;
using System.Collections;
using UnityEngine;


public class SkillBucke_0_to_19 { }

#region 技能描述
/***
 (0)   造成剑 / 枪 / 弓弱点的较低伤害（初始技能）（方向键切换弱点类型）
（1）   回复{200*（等级//10+1）}点法力值（初始技能）
（2）   赋予自身一个随机属性上升类型的强化效果，持续20S（初始技能）
（3）   无视弱点，削减对方1点护盾点数，并造成中量物理伤害（中低耗）（初始技能）
（4）   获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
（5）   获得【雷电风暴】状态（每10S对1名随机敌人造成0.4倍率雷弱点伤害），持续30S（初始技能）
（6）   获得【冰雪场地】状态（每4秒使敌方全体获得1层冻结），持续40S（中耗）（初始技能）
（7）   对随机敌人造成枪弱点伤害，重放（3~5）次（区间内随机）（初始技能）
（8）   获得【物攻+1】（物攻+20%），持续20S（初始技能）
（9）   回复 30% 最大生命值，每次击破后 ATB-2/充能时间减少40S，释放后重置（初始技能）
（10）  获得【战意】状态（造成伤害增加30%）持续20S，减少20%当前生命值（初始技能）
（11）  获得【大魔法化】状态（魔法类型攻击技能将会额外释放1次）持续20S（初始技能）
（12）  对全体敌人造成冰/雷/火弱点的伤害（中耗）（方向键切换）（初始技能）
（13）  获得【魔攻+1】（魔攻+20%），持续20S（初始技能）
（14）  获得【物防+1】（物防+20%），持续20S
（15）  获得【魔防+1】（魔防+20%），持续20S
（16）  对1名敌人施加【物攻-1】（物攻-15%），持续20S
（17）  对1名敌人施加【魔攻-1】（魔攻-15%），持续20S
（18）  对1名敌人施加【物防-1】（物防-15%），持续20S
（19）  对1名敌人施加【魔防-1】（魔防-15%），持续20S
***/
#endregion

/// <summary>
///（0）造成剑/枪/弓弱点的较低伤害（初始技能）（方向键切换弱点类型）
/// </summary>
[SkillID(0)]
public class Skill_BaseAttack : SkillBase{

    float baseAttackValue = -2;
    float baseAttackRate = 0.4f;

    //决定伤害类型
    E_WeaknessType weakness = E_WeaknessType.剑;

    public Skill_BaseAttack(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能0--$$$$$");
    }

    public void SwitchWeakness(E_WeaknessType type){
        weakness = type;
    }
    public override void SkillExcuteSingle(IBattlable target) {
        E_Skill_DamageType damageType = DamageTypeChecker.GetDamageType(weakness);
        float value = self.battlerDataHandler.DoDamage(damageType, baseAttackRate * baseAttackValue);
        Debug.Log($"[Skill 0]{self.Camp}对{target.battlerDataHandler.name}发动技能0-----[税前伤害：{value}]");
        target.battlerDataHandler.GetDamage(damageType, value);
    }
    public override void SkillEnhanceSingle(IBattlable target){
    }
}

/// <summary>
/// （1）回复{200*（等级//10+1）}点法力值（初始技能）
/// </summary>
[SkillID(1)]
public class Skill_1 : SkillBase{
    float baseHealValue = 200;
    float divRate = 10;

    public Skill_1(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能1--$$$$$");
    }
    public override void SkillExcuteSingle(IBattlable target){
        //character还没有导入
        //float value = baseHealValue * ((self.battlerDataHandler.CharacterData.CurrentLevel / divRate) + 1);
        float value = baseHealValue * ((5 / divRate) + 1);
        self.battlerDataHandler.DoModelValue(E_BattleModelType.SP,value);
        Debug.Log($"[Skill 1]{self.Camp}发动技能1-----[回复法力值{value}]");
    }
    public override void SkillEnhanceSingle(IBattlable target){
    }
}


/// <summary> 
///（2）赋予自身一个随机属性上升类型的强化效果，持续20S（初始技能）
/// </summary>
[SkillID(2)]
public class Skill_2 : SkillBase{
    WaitForSeconds Buff_delay;
    //20sBUFF持续时间
    float Buff_duration = 20;
    CoroutineManager corManager;
    public Skill_2(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能2--$$$$$");
        Buff_delay = new WaitForSeconds(Buff_duration);
        corManager = GameRoot.GetManager<CoroutineManager>();
    }
    E_CharacterPropertyType RandomProperty(){
        int index = Random.Range(0, 4);

        E_CharacterPropertyType propertyType = E_CharacterPropertyType.Phy_Attack;
        switch (index){
            case 0:
                Debug.Log("[Skill2]-----0");
                propertyType = E_CharacterPropertyType.Phy_Attack; break;
            case 1:
                Debug.Log("[Skill2]-----1");
                propertyType = E_CharacterPropertyType.Phy_Resistance; break;
            case 2:
                Debug.Log("[Skill2]-----2");
                propertyType = E_CharacterPropertyType.Mag_Attack; break;
            case 3:
                Debug.Log("[Skill2]-----3");
                propertyType = E_CharacterPropertyType.Mag_Resistance; break;
        }
        return propertyType;
    }
    IEnumerator BuffStart(E_CharacterPropertyType propertyType){
        //CharacterData data = self.battlerDataHandler.CharacterData;
        //CharacterData尚未导入
        //data.AdjustProperty(propertyType, baseRiseRate, true);
        Debug.Log($"[Skill 2]{self.Camp}发动技能2-----[{propertyType}属性加强1.5倍]");
        yield return Buff_delay;
        //CharacterData尚未导入
        //data.AdjustProperty(propertyType, 1 / baseRiseRate, true);
        Debug.Log($"[Skill 2]{self.Camp}技能2-----Buff失效");
    }

    public override void SkillExcuteSingle(IBattlable target){
        corManager.StartCoroutine(BuffStart(RandomProperty()));

    }
    public override void SkillEnhanceSingle(IBattlable target){
    }
}


/// <summary>
/// （3）无视弱点，削减对方1点护盾点数，并造成中量物理伤害（中低耗）（初始技能）
/// </summary>
[SkillID(3)]
public class Skill_3 : SkillBase
{
    public Skill_3(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能3--$$$$$");
    }
    float baseDamageValue=100; 
    public override void SkillEnhanceSingle(IBattlable target){
        //无视弱点进行削韧
        target.battlerDataHandler.DoModelValue(E_BattleModelType.ShieldPoints,-1);
        float value = self.battlerDataHandler.DoDamage(E_Skill_DamageType.物理,baseDamageValue);
        Debug.Log($"[Skill 3] {self.Camp} 对 {target.battlerDataHandler.name} 发动技能3-----[盾点-1 + 税前伤害{value}]");
        target.battlerDataHandler.GetDamage(E_Skill_DamageType.物理, value);

    }
    public override void SkillExcuteSingle(IBattlable target){
    }
}


/// <summary>
/// （4) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(4)]
public class Skill_4 : SkillBase
{
    public Skill_4(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        Debug.Log("$$$$$--技能4--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target){
     
    }

    public override void SkillExcuteSingle(IBattlable target){

    }
}

//（5）   获得【雷电风暴】状态（每10S对1名随机敌人造成0.4倍率雷弱点伤害），持续30S（初始技能）


    /// <summary>
    /// （5) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
    /// </summary>
[SkillID(5)]
public class Skill_5 : SkillBase
{
    public Skill_5(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能5--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}


/// <summary>
/// （6) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(6)]
public class Skill_6 : SkillBase
{
    public Skill_6(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能6--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}

/// <summary>
/// （7) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(7)]
public class Skill_7 : SkillBase
{
    public Skill_7(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能7--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}


/// <summary>
/// （8) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(8)]
public class Skill_8 : SkillBase
{
    public Skill_8(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能8--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}


/// <summary>
/// （9) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(9)]
public class Skill_9 : SkillBase
{
    public Skill_9(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能9--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}


/// <summary>
/// （10) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(10)]
public class Skill_10 : SkillBase
{
    public Skill_10(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能10--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}

/// <summary>
/// （11) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(11)]
public class Skill_11 : SkillBase
{
    public Skill_11(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能11--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （12) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(12)]
public class Skill_12 : SkillBase
{
    public Skill_12(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能12--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （13) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(13)]
public class Skill_13 : SkillBase
{
    public Skill_13(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能13--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （14) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(14)]
public class Skill_14 : SkillBase
{
    public Skill_14(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能14--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （15) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(15)]
public class Skill_15 : SkillBase
{
    public Skill_15(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能15--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （16) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(16)]
public class Skill_16 : SkillBase
{
    public Skill_16(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能16--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （17) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(17)]
public class Skill_17 : SkillBase
{
    public Skill_17(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能17--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （18) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(18)]
public class Skill_18 : SkillBase
{
    public Skill_18(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能18--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
/// <summary>
/// （15) 获得【炽焰连锁】状态（物理攻击技能附带火弱点攻击伤害）持续20S（初始技能）
/// </summary>
[SkillID(19)]
public class Skill_19 : SkillBase
{
    public Skill_19(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        Debug.Log("$$$$$--技能19--$$$$$");
    }

    public override void SkillEnhanceSingle(IBattlable target)
    {

    }

    public override void SkillExcuteSingle(IBattlable target)
    {

    }
}
