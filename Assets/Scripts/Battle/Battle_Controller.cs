using System;
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
    public Battle_Controller(CharacterData _charData, Battle_Viewer _viewer)
    {
        characterData = _charData;
        viewer = _viewer;
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

        model.OnDataChanged += () => viewer.UpdataUI(model);
        model.OnHPZero += CharacterDead;
    }
    //角色死亡
    public bool charcaterDead;
    void CharacterDead()
    {
        Debug.Log(characterData.Character_Name + "角色已死亡");
        if (!charcaterDead)
        {
            charcaterDead = true;
            //禁用本角色的技能更新+检测游戏结束
            EventCenter.EventTrigger<Battle_Controller>
                (E_EventType.CharacterDead, this);
        }
    }

    public float GetCharacterData(E_CharacterPropertyType propertyType)
    {
        return characterData.GetProperty(propertyType);
    }

    /// <summary>
    /// 修改角色的属性
    /// </summary>
    public void AdjustCharacterData(E_CharacterPropertyType propertyType, float targetValue)
    {
        CharacterData.AdjustProperty(propertyType, targetValue);
    }

    /// <summary>
    /// 修改角色模型
    /// </summary>
    public void AdjustCharacterModelValue(E_BattleModelType modelType, float targetValue)
    {
        modelDic[modelType].Invoke(targetValue);
        //Debug.Log(name +"——"+ modelType+"调整:" +targetValue+" 改变后的值：" +GetCharacterModelValue(modelType));
    }

    public float GetCharacterModelValue(E_BattleModelType modelType)
    {
        return modelType switch
        {
            E_BattleModelType.HP => model.HP,
            E_BattleModelType.MAX_HP => model.MaxHP,
            E_BattleModelType.SP => model.SP,
            E_BattleModelType.MAX_SP => model.MaxSP,
            E_BattleModelType.AG => model.AG,
            E_BattleModelType.MAX_AG => model.MaxAG,
            E_BattleModelType.ATBPoints => model.ATBPoints,
            E_BattleModelType.MAX_ATBPoints => model.MaxATBPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(modelType), modelType, null)
        };
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
