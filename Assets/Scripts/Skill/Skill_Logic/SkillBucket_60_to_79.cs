using UnityEngine;

public class SkillBucket_60_to_79 : MonoBehaviour { }

#region 剑系物理攻击 (60-64)

/// <summary>
/// 60) 斩——对单体造成剑弱点伤害（基础版）
/// </summary>
[SkillID(60)]
public class Skill_60 : SkillBase{
    float baseAttackValue = -1;
    float baseAttackRate = 0.3f;
    Attack_Skill atk;
    public Skill_60(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 60]{self.Camp}发动[斩]-剑弱点伤害");
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.2f * henceTime);
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 61) 三连斩——对单体造成剑弱点伤害*3
/// </summary>
[SkillID(61)]
public class Skill_61 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.9f;
    Attack_Skill atk;

    public Skill_61(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 61]{self.Camp}发动[重斩]-剑弱点重击");
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 62) 五连斩——对单体造成剑弱点伤害*5
/// </summary>
[SkillID(62)]
public class Skill_62 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 1.5f;
    Attack_Skill atk;

    public Skill_62(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 62]{self.Camp}发动[超重斩]-剑弱点超重击");
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.3f * henceTime);
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 63) 巨斩——对全体造成剑弱点伤害
/// </summary>
[SkillID(63)]
public class Skill_63 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.4f;
    Attack_Skill atk;

    public Skill_63(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 63]{self.Camp}发动[大斩]-全体剑弱点伤害");
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.2f * henceTime);
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 64) 超巨斩——对全体造成大量剑弱点伤害
/// </summary>
[SkillID(64)]
public class Skill_64 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.8f;
    Attack_Skill atk;

    public Skill_64(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 64]{self.Camp}发动[大重斩]-全体剑弱点重击");
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.剑, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

#endregion

#region 枪系物理攻击 (65-69)

/// <summary>
/// 65) 刺——对单体造成枪弱点伤害（基础版）
/// </summary>
[SkillID(65)]
public class Skill_65 : SkillBase{
    float baseAttackValue = -1;
    float baseAttackRate = 0.3f;
    Attack_Skill atk;
    public Skill_65(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        atk = new Attack_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 65]{self.Camp}发动[刺]-枪弱点伤害");
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        float enRate = baseAttackRate * (1 + 0.2f * henceTime);
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}
/// <summary>
/// 66) 三连刺——对单体造成枪弱点伤害*3
/// </summary>
[SkillID(66)]
public class Skill_66 : SkillBase{
    float baseAttackValue = -1;
    float baseAttackRate = 0.9f;
    Attack_Skill atk;
    public Skill_66(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        atk = new Attack_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 66]{self.Camp}发动[重刺]-枪弱点重击");
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}
/// <summary>
/// 67) 五连刺——对单体造成枪弱点伤害*5
/// </summary>
[SkillID(67)]
public class Skill_67 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 1.5f;
    Attack_Skill atk;

    public Skill_67(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 67]{self.Camp}发动[超重刺]-枪弱点超重击");
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.3f * henceTime);
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 68) 强刺——对全体造成枪弱点伤害
/// </summary>
[SkillID(68)]
public class Skill_68 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.4f;
    Attack_Skill atk;

    public Skill_68(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 68]{self.Camp}发动[强刺]-全体枪弱点伤害");
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.2f * henceTime);
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 69) 超强刺——对全体造成大量枪弱点伤害
/// </summary>
[SkillID(69)]
public class Skill_69 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.8f;
    Attack_Skill atk;

    public Skill_69(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 69]{self.Camp}发动[大强刺]-全体枪弱点重击");
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.枪, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

#endregion

#region 弓系物理攻击 (70-74)

/// <summary>
/// 70) 射——对单体造成弓弱点伤害（基础版）
/// </summary>
[SkillID(70)]
public class Skill_70 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.3f;
    Attack_Skill atk;

    public Skill_70(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 70]{self.Camp}发动[射]-弓弱点伤害");
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.2f * henceTime);
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 71) 三连射——对单体造成弓弱点伤害*3
/// </summary>
[SkillID(71)]
public class Skill_71 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.9f;
    Attack_Skill atk;

    public Skill_71(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 71]{self.Camp}发动[重射]-弓弱点重击");
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 72) 五连射——对单体造成弓弱点伤害*5
/// </summary>
[SkillID(72)]
public class Skill_72 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 1.5f;
    Attack_Skill atk;

    public Skill_72(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 72]{self.Camp}发动[超重射]-弓弱点超重击");
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.3f * henceTime);
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 73) 猛射——对目标造成弓弱点伤害
/// </summary>
[SkillID(73)]
public class Skill_73 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.4f;
    Attack_Skill atk;

    public Skill_73(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 73]{self.Camp}发动[强射]-弓弱点伤害");
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.2f * henceTime);
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 74) 超猛射——对目标造成大量弓弱点伤害
/// </summary>
[SkillID(74)]
public class Skill_74 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.8f;
    Attack_Skill atk;

    public Skill_74(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 74]{self.Camp}发动[大强射]-弓弱点重击");
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.弓, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

#endregion

#region 元素魔法攻击 (75-78)

/// <summary>
/// 75) 雪球——对单体造成冰弱点伤害
/// </summary>
[SkillID(75)]
public class Skill_75 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_75(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 75]{self.Camp}发动[雪球]-冰弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 76) 雷球——对单体造成雷弱点伤害
/// </summary>
[SkillID(76)]
public class Skill_76 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_76(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 76]{self.Camp}发动[雷击]-雷弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 77) 火球——对单体造成火弱点伤害
/// </summary>
[SkillID(77)]
public class Skill_77 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_77(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 77]{self.Camp}发动[火球]-火弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}


/// <summary>
/// 78) 大雪球——对单体造成中量冰弱点伤害
/// </summary>
[SkillID(78)]
public class Skill_78 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_78(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 75]{self.Camp}发动[雪球]-冰弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 76) 大雷球——对单体造成中量雷弱点伤害
/// </summary>
[SkillID(79)]
public class Skill_79 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_79(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 76]{self.Camp}发动[雷击]-雷弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 77) 大火球——对单体造成中量火弱点伤害
/// </summary>
[SkillID(80)]
public class Skill_80 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_80(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 77]{self.Camp}发动[火球]-火弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 81) 超大雪球——对单体造成大量冰弱点伤害
/// </summary>
[SkillID(81)]
public class Skill_81 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_81(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 75]{self.Camp}发动[雪球]-冰弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 82) 超大雷球——对单体造成大量雷弱点伤害
/// </summary>
[SkillID(82)]
public class Skill_82 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_82(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 76]{self.Camp}发动[雷击]-雷弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}

/// <summary>
/// 83) 超大火球——对单体造成大量火弱点伤害
/// </summary>
[SkillID(83)]
public class Skill_83 : SkillBase{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;

    public Skill_83(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 77]{self.Camp}发动[火球]-火弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, baseAttackRate);
        atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, enRate);
        atk.Excute(self, target);
    }
}




/// <summary>
/// 84) 大雪球连射——对单体造成中量冰弱点伤害*2
/// </summary>
[SkillID(84)]
public class Skill_84 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;
    int attackTime = 2;
    public Skill_84(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 75]{self.Camp}发动[雪球]-冰弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, baseAttackRate);
        
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, enRate);
        
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }
}

/// <summary>
/// 76) 大雷球连射——对单体造成中量雷弱点伤害*2
/// </summary>
[SkillID(85)]
public class Skill_85 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;
    int attackTime = 2;
    public Skill_85(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 76]{self.Camp}发动[雷击]-雷弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, baseAttackRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, enRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }
}

/// <summary>
/// 77) 大火球连射——对单体造成中量火弱点伤害
/// </summary>
[SkillID(86)]
public class Skill_86 : SkillBase{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;
    int attackTime = 2;
    public Skill_86(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 77]{self.Camp}发动[火球]-火弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, baseAttackRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, enRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }
}

/// <summary>
/// 87) 超大雪球连射——对单体造成大量冰弱点伤害*3
/// </summary>
[SkillID(87)]
public class Skill_87 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;
    int attackTime = 3;
    public Skill_87(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 75]{self.Camp}发动[雪球]-冰弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, baseAttackRate);

        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.冰, baseAttackValue, enRate);

        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }
}

/// <summary>
/// 88) 超大雷球连射——对单体造成大量雷弱点伤害*3
/// </summary>
[SkillID(88)]
public class Skill_88 : SkillBase{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;
    int attackTime = 3;
    public Skill_88(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 76]{self.Camp}发动[雷击]-雷弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, baseAttackRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.雷, baseAttackValue, enRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }
}

/// <summary>
/// 89) 超大火球连射——对单体造成大量火弱点伤害*3
/// </summary>
[SkillID(89)]
public class Skill_89 : SkillBase
{
    float baseAttackValue = -1;
    float baseAttackRate = 0.35f;
    Attack_Skill atk;
    int attackTime = 3;
    public Skill_89(E_SkillTargetType _skillTargetType) : base(_skillTargetType)
    {
        atk = new Attack_Skill();
    }
    public override bool IsMagicType => true;

    public override void SkillEffect_Base(IBattlable target)
    {
        Debug.Log($"[Skill 77]{self.Camp}发动[火球]-火弱点魔法伤害");
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, baseAttackRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }

    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        float enRate = baseAttackRate * (1 + 0.25f * henceTime);
        atk.SetAttackState(E_WeaknessType.火, baseAttackValue, enRate);
        for (int i = 0; i < attackTime; i++)
            atk.Excute(self, target);
    }
}
#endregion
#region 护盾恢复 (79)

/// <summary>
/// 90) 加固——恢复自身3点护盾点数
/// </summary>
[SkillID(90)]
public class Skill_90 : SkillBase
{
    int shieldRecover = 3;
    ModelAdjust_Skill modelAdj;
    public Skill_90(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        modelAdj = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 79]{self.Camp}发动[加固]-恢复{shieldRecover}点护盾");
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, shieldRecover, 1f);
        modelAdj.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        int enShield = shieldRecover + henceTime;
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, enShield, 1f);
        modelAdj.Excute(self, target);
    }
}

/// <summary>
/// 91) 继续加固——恢复自身8点护盾点数
/// </summary>
[SkillID(91)]
public class Skill_91 : SkillBase{
    int shieldRecover = 3;
    ModelAdjust_Skill modelAdj;
    public Skill_91(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        modelAdj = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 79]{self.Camp}发动[加固]-恢复{shieldRecover}点护盾");
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, shieldRecover, 1f);
        modelAdj.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        int enShield = shieldRecover + henceTime;
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, enShield, 1f);
        modelAdj.Excute(self, target);
    }
}

/// <summary>
/// 92) 继续继续加固——恢复自身17点护盾点数
/// </summary>
[SkillID(92)]
public class Skill_92 : SkillBase{
    int shieldRecover = 3;
    ModelAdjust_Skill modelAdj;
    public Skill_92(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        modelAdj = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 79]{self.Camp}发动[加固]-恢复{shieldRecover}点护盾");
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, shieldRecover, 1f);
        modelAdj.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime){
        int enShield = shieldRecover + henceTime;
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, enShield, 1f);
        modelAdj.Excute(self, target);
    }
}
/// <summary>
/// 93) 继续继续继续加固——恢复自身28点护盾点数
/// </summary>
[SkillID(93)]
public class Skill_93 : SkillBase{
    int shieldRecover = 3;
    ModelAdjust_Skill modelAdj;
    public Skill_93(E_SkillTargetType _skillTargetType) : base(_skillTargetType){
        modelAdj = new ModelAdjust_Skill();
    }
    public override void SkillEffect_Base(IBattlable target){
        Debug.Log($"[Skill 79]{self.Camp}发动[加固]-恢复{shieldRecover}点护盾");
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, shieldRecover, 1f);
        modelAdj.Excute(self, target);
    }
    public override void SkillEffect_Enhence(IBattlable target, int henceTime)
    {
        int enShield = shieldRecover + henceTime;
        modelAdj.SetModelState(E_BattleModelType.ShieldPoints, enShield, 1f);
        modelAdj.Excute(self, target);
    }
}

#endregion