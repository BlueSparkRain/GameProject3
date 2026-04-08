using Core;
using System.Collections.Generic;
using UnityEngine;
public class MapSkillerCheker : MonoSceneManager
{
    public Dictionary<CharacterMapSkiller, E_HexRoomType> skillersRoomDic = new Dictionary<CharacterMapSkiller, E_HexRoomType>();
    //List<CharacterMapSkiller> characterMapSkillers = new List<CharacterMapSkiller>();

    //当前选中的角色skiller
    CharacterMapSkiller currentSkiller;
    UIManager uiManager;

    CharacterMapSkiller playerSkiller;

    public CharacterMapSkiller PlayerSkiller=>playerSkiller;

    /// <summary>
    /// 更新当前操作的角色对象
    /// </summary>
    void SelectCharacter_UpdateCurrentSkiller(CharacterMapSkiller skiller)
    {
        currentSkiller = skiller;
        Debug.Log("当前选中skiller:" + skiller.gameObject.name);
    }
    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        //在Raycaster中进行操作对象的选择，如果为空，默认打开玩家自身的
        EventCenter.AddEventListener<CharacterMapSkiller>(E_EventType.Select_Characer, SelectCharacter_UpdateCurrentSkiller);

        EventCenter.AddEventListener<CharacterMapSkiller,bool>(E_EventType.Character_Born, RegisterSkiller);
        EventCenter.AddEventListener<CharacterMapSkiller, E_HexRoomType>(E_EventType.Mover_IntoSpecialRoom, DoSkillReward);

        uiManager = GameRoot.GetManager<UIManager>();
    }
    //角色（移动中断）进入特殊房间后获得的技能奖励
    void DoSkillReward(CharacterMapSkiller skiller, E_HexRoomType roomType)
    {

        if (skillersRoomDic.ContainsKey(skiller))
        {
            skillersRoomDic[skiller] = roomType;

            switch (roomType)
            {
                case E_HexRoomType.None_无:

                    break;
                case E_HexRoomType.Battle_LowLevel_战斗_杂鱼:
                    //随机的技能或道具奖励+经验奖励
                    skiller.GetNewSkill(0);
                    break;
                case E_HexRoomType.NPC_特定交互:
                    //随机的技能或道具奖励+经验奖励
                    skiller.GetNewSkill(0);
                    break;
                case E_HexRoomType.UnknownEvent_随机事件:

                    break;
                default:
                    break;
            }
        }
    }
    //每次移动结束后，如果对应的房间是特殊房间，立刻进行属性或技能的奖励

    public override void MgrUpdate(float deltaTime)
    {
        //打开技能面板
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (currentSkiller != null && currentSkiller!=playerSkiller)
                CallSkillPanel();
            else{
                currentSkiller = playerSkiller;
                CallSkillPanel();
                Debug.Log("[MapSkillerCheker]---当前可操作对象为null,使用玩家");
            }
        }
    }

    void CallSkillPanel() {
        Debug.Log("Mann");
        uiManager.OpenPanel<SkillPanel>(E_UIPanelType.SkillPanel,
                     (p) => p.LoadSkillIconBySettle(
                         currentSkiller.canActSettle,
                         currentSkiller.restSkillSlotNum,
                         currentSkiller.RestWholeSkillDatas,
                         currentSkiller.normalSkillSlotNum,
                         currentSkiller.NormalSkillDatas,
                         currentSkiller.atbSkillSlotNum,
                         currentSkiller.ATBSkillDatas
                     ));
    }
    public void RegisterSkiller(CharacterMapSkiller mapSkiller,bool isPlayer=false)
    {
        if (!skillersRoomDic.ContainsKey(mapSkiller)) 
            skillersRoomDic.Add(mapSkiller, E_HexRoomType.None_无);
        if (isPlayer)
        {
            Debug.Log("玩家！");
            playerSkiller = mapSkiller;
        }
    }

    public void Update()
    {

    }


    public void GetSkill(CharacterMapSkiller skiller, int skillID)
    {
        skiller.GetNewSkill(skillID);
    }

}
