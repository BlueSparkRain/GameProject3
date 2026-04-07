using UnityEngine;
[CreateAssetMenu(menuName = "SOData/CharcterPropertyGrowthSO", fileName = "CharcterPropertyGrowthSO")]
public class CharcterPropertyGrowthSO : ScriptableObject
{
    [Header("物理固穿")]
    public float Phy_Flat_Penetration_grow = 0.02f;

    [Header("法术固穿")]
    public float Mag_Flat_Penetration_grow = 0.02f;

    [Header("物抗")]
    public float Phy_Resistance_grow = 0.02f;

    [Header("魔抗")]
    public float Mag_Resistance_grow = 0.02f;

    [Header("物攻")]
    public float Phy_Attack_grow = 0.02f;

    [Header("法强")]
    public float Magic_Attack_grow = 0.02f;

    [Header("最大法力值")]
    public float Maximum_Mana_grow = 0.02f;

    [Header("法力值回复")]
    public float Mana_Regeneration_grow = 0.02f;

    [Header("最大生命值")]
    public float Maximum_Health_grow = 0.02f;

    [Header("生命值回复")]
    public float Health_Regeneration_grow = 0.02f;

    [Header("生命偷取")]
    public float Life_Steal_grow = 0.02f;

    [Header("韧性")]
    public float Tenacity_grow = 0.02f;

    [Header("耐力")]
    public float Endurance_grow = 0.02f;

    [Header("闪避率")]
    public float Dodge_Rate_grow = 0.02f;

    [Header("治疗强化")]

    public float Heal_Amplification_grow = 0.02f;

    [Header("护盾强化")]
    public float Shield_Amplification_grow = 0.02f;


    [Header("初始ATB点数")]
    public int Maximum_ATB_grow = 1;

    [Header("成长间隔（默认每级成长）")]
    public int growthInterval = 1;
}
