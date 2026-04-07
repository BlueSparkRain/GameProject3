using Core;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

/// <summary>
/// 战斗技能管理器，专门管理局内的技能循环
/// 进入战场前，上个场景中的CharacterSkiller会将目前玩家的技能数据传递过来（*-*）(或一个管理器)，
/// 生成对应的技能图标，并注册到skiller中
/// </summary>
public class CharacterBattleSkiller : MonoBehaviour
{
    private List<SkillIcon> normalSkillIcons = new List<SkillIcon>();
    private List<SkillIcon> atbSkillIcons = new List<SkillIcon>();

    private List<SkillData> skillDatas = new List<SkillData>();
    private List<int> skillIDs = new List<int>() { 0, 0, };

    SkillIconSpawner normalSkillIconSpawner;
    SkillIconSpawner atbSkillIconSpawner;


    CharacterBattle_Controller battleController;
    List<ISkill> skills=new List<ISkill>();
    //Dictionary<SkillIcon, ISkill> skillIconDic = new Dictionary<SkillIcon, ISkill>();

    private void Start()
    {
        normalSkillIconSpawner = GetComponentInChildren<SkillIconSpawner>();
        battleController = GetComponent<CharacterBattle_Controller>();
        EventCenter.AddEventListener<int>(E_EventType.Battle_LoadASkill,RegisterSkill);
        BattleSkillFactory.CreateBatch(skillIDs);
        normalSkillIcons = normalSkillIconSpawner.LoadSlotsAndSkills(5,skillDatas,false);
       
        //battleTargetsManager = GameRoot.GetManager<BattleTargetsSelectManager>();
        //GameRoot.GetManager<BattleTargetsSelectManager>().RegisterSkiller(isPlayer, GetComponent<CharacterBattle_Controller>());
        //StartCoroutine(LoadSkills());
        //GameRoot.GetManager<EventCenterManager>().AddEventListener(E_EventType.BattleEnd, BattleEnd);
    }

   

    public void RegisterSkill(int skillID)
    {
        skills.Add(BattleSkillFactory.Create(skillID)); 
    }
    //IEnumerator LoadSkills()
    //{
    //    yield return new WaitForSeconds(1);
    //    skillIcons = skillIconSpawner.LoadSkillIcons(isPlayer, skillIDs, GetComponent<CharacterBattle_Controller>());
    //}

    void BattleEnd()
    {
        battleEnd = true;
    }
    bool battleEnd;
    private void Update()
    {
        if (battleEnd)
            return;
        if (!battleController.charcaterDead)
        {
            foreach (var skill in normalSkillIcons)
            {
                skill.IconCycleUpdate();
                skill.CheckSkillCanExcute(battleController.GetCharacterModelValue(EModelType.SP));
            }
        }
    }


    void UpdateBattleModel()
    {


    }

    /// <summary>
    /// 根据请求的技能ID来产生对应的skillIcon
    /// </summary>
    /// <param name="skillID"></param>
    void LoadASkill(int skillID)
    {

        var newSkillIcon = GameRoot.GetManager<ObjectPoolManager>().
        GetInstance(EPoolType.SkillIcon_技能图标).GetComponent<SkillIcon>();
    }
    void FreezeASkill()
    {

    }
    void DelASkill()
    {

    }
}
