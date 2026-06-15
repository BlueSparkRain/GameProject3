using Core;
using System.Collections.Generic;
using UnityEngine;

public class MapSkillerCheker : MonoSceneManager
{
    public Dictionary<CharacterMapSkiller, E_HexRoomType> skillersRoomDic = new Dictionary<CharacterMapSkiller, E_HexRoomType>();

    CharacterMapSkiller currentSkiller;
    UIManager uiManager;
    CharacterMapSkiller playerSkiller;
    public CharacterMapSkiller PlayerSkiller => playerSkiller;
    public void UpdateSkillSettle(List<SkillData> restWholeDatas,
                                     List<SkillData> normalDatas,
                                     List<SkillData> atbDatas)
    {
        currentSkiller.UpdateSkilerSettle(restWholeDatas, normalDatas, atbDatas);
        DebugManager.Log(EDebugCategory.MapRoom, currentSkiller.gameObject.name + "已更新技能配置");
    }

    void SelectCharacter_UpdateCurrentSkiller(CharacterMapSkiller skiller)
    {
        currentSkiller = skiller;
        DebugManager.Log(EDebugCategory.MapRoom, "当前选中skiller:" + skiller.gameObject.name);
    }

    // 角色进入特殊房间获得技能奖励
    void DoSkillReward(CharacterMapSkiller skiller, E_HexRoomType roomType)
    {
        if (skillersRoomDic.ContainsKey(skiller))
        {
            skillersRoomDic[skiller] = roomType;
            switch (roomType)
            {
                case E_HexRoomType.Battle_LowLevel:
                case E_HexRoomType.NPC:
                    skiller.GetNewSkill(0);
                    break;
                default:
                    break;
            }
        }
    }
    public void RegisterSkiller(CharacterMapSkiller mapSkiller, bool isPlayer = false)
    {
        if (!skillersRoomDic.ContainsKey(mapSkiller))
            skillersRoomDic.Add(mapSkiller, E_HexRoomType.None);
        if (isPlayer)
            playerSkiller = mapSkiller;
    }
    protected override void MgrOnInit(){
        base.MgrOnInit();
        EventCenter.AddEventListener<CharacterMapSkiller>(E_EventType.Select_Characer, SelectCharacter_UpdateCurrentSkiller);
        EventCenter.AddEventListener<CharacterMapSkiller, bool>(E_EventType.Character_Skiller_Regist, RegisterSkiller);
        EventCenter.AddEventListener<CharacterMapSkiller, E_HexRoomType>(E_EventType.Mover_IntoSpecialRoom, DoSkillReward);
        EventCenter.AddEventListener(E_EventType.CallSkillPanel, CallSkillPanel);
        uiManager = GameRoot.GetManager<UIManager>();
    }
    public override void MgrUpdate(float deltaTime){
        //打开技能配置面板
        if (Input.GetKeyDown(KeyCode.U)){
            if (currentSkiller == null)
                currentSkiller = playerSkiller;
            CallSkillPanel();
        }
    }
    void CallSkillPanel(){
        if (currentSkiller == null)
            currentSkiller = playerSkiller;
        var panel = uiManager.GetPanel<SkillAssignPanel>(E_UIPanelType.SkillAssignPanel);
        if (panel != null && panel.gameObject.activeSelf){
            if (panel.canOpen)
                panel.Hide();
            return;
        }

        if (panel != null)
            panel.canOpen = true;
        uiManager.OpenPanel<SkillAssignPanel>(E_UIPanelType.SkillAssignPanel,
            p => p.LoadSkillIconBySettle(
                currentSkiller.canActSettle,
                currentSkiller.restSkillSlotNum,
                currentSkiller.RestWholeSkillDatas,
                currentSkiller.normalSkillSlotNum,
                currentSkiller.NormalSkillDatas,
                currentSkiller.atbSkillSlotNum,
                currentSkiller.ATBSkillDatas
            ));
    }

    public void GetSkill(CharacterMapSkiller skiller, int skillID)
    {
        skiller.GetNewSkill(skillID);
    }
}
