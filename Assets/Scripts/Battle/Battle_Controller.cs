using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle_Controller
{
    public Battle_Viewer viewer;
    Battle_Model model;
    Dictionary<E_BattleModelType, Action<float>> modelDic = new Dictionary<E_BattleModelType, Action<float>>();

    //上个场景中传递而来的对局角色属性数据,后期继续将角色属性数据解耦
    CharacterData characterData;
    public CharacterData CharacterData => characterData;

    BattlerStateTag battlerStateTag;


    WaitForSeconds breakRefreshDelay;
    /// <summary>
    /// 力竭恢复时间
    /// </summary>
    float breakRefreshDuration = 5;

    float modelUpdateTimer;
    float modelUpdateInterval = 1;
    public Battle_Controller(CharacterData _charData, Battle_Viewer _viewer, BattlerStateTag _battlerStateTag){
        characterData = _charData;
        viewer = _viewer;
        battlerStateTag= _battlerStateTag;
        model = new Battle_Model(characterData.Maximum_Health, characterData.Maximum_Mana, (int)characterData.Maximum_ATB);
        viewer.UpdataUI(model);
        modelDic.Add(E_BattleModelType.HP, val => model.HP += val);
        modelDic.Add(E_BattleModelType.MAX_HP, val => model.MaxHP += val);
        modelDic.Add(E_BattleModelType.SP, val => model.SP += val);
        modelDic.Add(E_BattleModelType.MAX_SP, val => model.MaxSP += val);
        modelDic.Add(E_BattleModelType.AG, val => model.AG += val);
        modelDic.Add(E_BattleModelType.MAX_AG, val => model.MaxAG += val);
        modelDic.Add(E_BattleModelType.ATBPoints, val => model.ATBPoints += (int)val);
        modelDic.Add(E_BattleModelType.MAX_ATBPoints, val => model.MaxATBPoints += (int)val);
        modelDic.Add(E_BattleModelType.ShieldPoints, val => model.ShieldPoints += (int)val);
        modelDic.Add(E_BattleModelType.Max_ShieldPoints, val => model.MaxShieldPoints += (int)val);

        model.OnDataChanged += () => viewer.UpdataUI(model);
        model.OnHPZero += CharacterDead;
        model.OnShieldBreak += CharacterBreak;

        breakRefreshDelay=new WaitForSeconds(breakRefreshDuration);
        modelUpdateTimer = modelUpdateInterval;
    }

    /// <summary>
    /// 角色死亡-Model私有委托
    /// </summary>
    void CharacterDead(){
        Debug.Log(characterData.Character_Name + "角色已死亡");
        if (!battlerStateTag.State_Dead){
            battlerStateTag.SetDeadState(true);
            //禁用本角色的技能更新+检测游戏结束
            EventCenter.EventTrigger(E_EventType.Battle_CharacterDead, battlerStateTag);
        }
    }

    /// <summary>
    /// 角色力竭-Model私有委托
    /// </summary>
    void CharacterBreak() {
        Debug.Log(characterData.Character_Name + "角色已力竭");
        if (!battlerStateTag.State_Break) {
            battlerStateTag.SetBreakState(true);
            //打断本角色的技能更新
            EventCenter.EventTrigger(E_EventType.Battle_CharacterBreak,battlerStateTag);
            GameRoot.GetManager<CoroutineManager>().StartCoroutine(BreakRefresh(), viewer);
        }
    }

    IEnumerator BreakRefresh() {
        Debug.Log("角色力竭中");
        yield return breakRefreshDelay;
        battlerStateTag.SetBreakState(false);
        EventCenter.EventTrigger(E_EventType.Battle_CharacterBreakRefresh);
        Debug.Log("角色力竭结束");
    }

    public float GetCharacterPropertyValue(E_CharacterPropertyType propertyType)=>characterData.GetProperty(propertyType);
 
    /// <summary>
    /// 修改角色的属性
    /// </summary>
    public void AdjustCharacterPropertyValue(E_CharacterPropertyType propertyType, float targetValue,bool useMulti=false){
        CharacterData.AdjustProperty(propertyType, targetValue,useMulti);
    }

    /// <summary>
    /// 修改角色模型
    /// </summary>
    public void AdjustCharacterModelValue(E_BattleModelType modelType, float targetValue){
        modelDic[modelType].Invoke(targetValue);
    }

    public float GetCharacterModelValue(E_BattleModelType modelType){
        return modelType switch{
            E_BattleModelType.HP => model.HP,
            E_BattleModelType.MAX_HP => model.MaxHP,
            E_BattleModelType.SP => model.SP,
            E_BattleModelType.MAX_SP => model.MaxSP,
            E_BattleModelType.AG => model.AG,
            E_BattleModelType.MAX_AG => model.MaxAG,
            E_BattleModelType.ATBPoints => model.ATBPoints,
            E_BattleModelType.MAX_ATBPoints => model.MaxATBPoints,
            E_BattleModelType.ShieldPoints => model.ShieldPoints,
            E_BattleModelType.Max_ShieldPoints => model.MaxShieldPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(modelType), modelType, null)
        };
    }

    /// <summary>
    /// 每隔1s回复生命值和法力值
    /// </summary>
    public void OnBattleControlUpdate(){
        if (modelUpdateTimer >= 0)
            modelUpdateTimer -= Time.deltaTime;
        else{
            modelUpdateTimer = modelUpdateInterval;
            AdjustCharacterModelValue(E_BattleModelType.SP,20);
            AdjustCharacterModelValue(E_BattleModelType.AG,10);
        }
    }
}

public enum E_BattleModelType
{
    //血量值
    HP,
    MAX_HP,
    //蓝量值
    SP,
    MAX_SP,
    //怒气值
    AG,
    MAX_AG,
    //ATB点数
    ATBPoints,
    MAX_ATBPoints,
    //盾点值
    ShieldPoints,
    Max_ShieldPoints,
}
