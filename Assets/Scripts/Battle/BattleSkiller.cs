using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗技能管理器，专门管理局内的技能Icon的技能循环
/// 进入战场前，上个场景中的CharacterSkiller会将目前玩家的技能数据传递过来（*-*）(或一个管理器)，
/// 生成对应的技能图标，并注册到skiller中
/// </summary>
public class BattleSkiller 
{
    List<SkillIcon> normalSkillIcons = new List<SkillIcon>();
    List<SkillIcon> atbSkillIcons = new List<SkillIcon>();

    List<int> normalSkillIDs = new List<int>();
    List<int> atbSkillIDs = new List<int>();

    SkillIconSpawner normalSkillIconSpawner;
    SkillIconSpawner atbSkillIconSpawner;
    Battle_Controller battleController;

    List<SkillBase> normalSkills = new List<SkillBase>();
    List<SkillBase> atbSkills = new List<SkillBase>();
    Dictionary<SkillIcon, SkillBase> skillIconDic = new Dictionary<SkillIcon, SkillBase>();

    public BattleSkiller(SkillIconSpawner _normalSkillIconSpawner,SkillIconSpawner _atbSkillIconSpawner,IBattlable self){
        normalSkillIconSpawner=_normalSkillIconSpawner;
        atbSkillIconSpawner=_atbSkillIconSpawner;
        InitSkiller(self);
    }


    IBattlable self;//由上个场景中的战斗双方角色传输
    public bool IsPlayer;

    bool DoCycle;
    //bool battleEnd;

    /// <summary>
    /// 注册每个实际技能效果逻辑
    /// </summary>
    /// <param name="skillIDList"></param>
    void InitSkillsBatch(List<int> skillIDList)
    {
        var _normalSkills = BattleSkillFactory.CreateBattleSkillsBatch(skillIDList, self);
        foreach (var skill in _normalSkills)
        {
            normalSkills.Add(skill);
        }
    }
    void InitSkiller(IBattlable self)
    {
        this.self = self;
      
        List<SkillData> normalSkillDatas = new List<SkillData>();
        //List<SkillData> atbSkillDatas = new List<SkillData>();

        //测试！随机添加 5种技能
        for (int i = 0; i < 5; i++)
            //normalSkillIDs.Add(Random.Range(0, 5));
            normalSkillIDs.Add(Random.Range(0, 3));

        for (int i = 0; i < normalSkillIDs.Count; i++)
        {
            var newSkillData = new SkillData(ResourcesLoader.FindSkillSOByID(normalSkillIDs[i]));
            normalSkillDatas.Add(newSkillData);
        }

        #region 创建SkillIcon

        //5个背包槽
        normalSkillIcons = normalSkillIconSpawner.LoadSlotsAndSkills(5, normalSkillDatas, false, true);

        //5个ATB槽
        //atbSkillIcons=atbSkillIconSpawner.LoadSlotsAndSkills(5,)
        #endregion


        #region 创建BattleSkill
        InitSkillsBatch(normalSkillIDs);
        #endregion

        //注册按钮-Skill字典 + 关联Icon&Skill
        for (int i = 0; i < normalSkillIcons.Count; i++)
        {
            Debug.Log(i + "()Icon:" + normalSkillIcons[i] + " Skill:" + normalSkills[i]);
            normalSkillIcons[i].InitBattleSkill(normalSkills[i]);
            skillIconDic.Add(normalSkillIcons[i], normalSkills[i]);
        }
        //for (int i = 0; i < atbSkillIcons.Count; i++){
        //    atbSkillIcons[i].InitBattleSkill(atbSkills[i]);
        //    skillIconDic.Add(atbSkillIcons[i], atbSkills[i]);
        //}
        DoCycle = true;
    }

    ///// <summary>
    ///// 本角色死亡，技能停止循环
    ///// </summary>
    ///// <param name="battler"></param>
    //void StopCylcle(Battle_Controller battler) {
    //    if (battler != battleController) return;
    //    SelfEnd();
    //}


    public void OnSkillUpdate(float currentSP)
    {
        if (DoCycle)
            DoSkillsUpdate(currentSP);
    }    
    void FreezeSkill(int ID, bool freeze)
    {
        foreach (var icon in normalSkillIcons)
            if (icon.SkillData.skill_ID == ID)
                icon.FreezeIcon(freeze);

        foreach (var icon in atbSkillIcons)
            if (icon.SkillData.skill_ID == ID)
                icon.FreezeIcon(freeze);
    }

    /// <summary>
    /// 追加次级技能效果
    /// </summary>
    /// <param name="skillID"></param>
    void AppendBattleSkillEffect(int skillID)
    {
        normalSkills.Add(BattleSkillFactory.CreateBattleSkill(skillID, self));
    }

    //void SelfEnd()=> battleEnd = true;
  


    void DoSkillsUpdate(float currentSP)
    {
        //if (battleEnd)
        //    return;

        //只有背包技能才会自动循环释放
        //if (!battleController.charcaterDead)
        //{
            foreach (var SkillIcon in normalSkillIcons)
            {
                SkillIcon.IconCycleUpdate(currentSP);
                //SkillIcon.CheckSkillCanExcute(battleController.GetCharacterModelValue(E_BattleModelType.SP));
            }
        //}
    }


    //void SkillCost(Battle_Controller battleController, float sp_cost)
    //{
    //    battleController.AdjustCharacterModelValue(E_BattleModelType.SP, sp_cost);
    //}
}
