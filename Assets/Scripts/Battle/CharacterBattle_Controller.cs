using Core;
using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class CharacterBattle_Controller : MonoBehaviour
{
    public CharacterBattle_Viewer viewer;
    CharacterBattle_Model  model;
    Dictionary<E_BattleModelType, Action<float>> modelDic=new Dictionary<E_BattleModelType, Action<float>>();
    //上个场景中传递而来的对局角色属性数据
    CharacterData CharacterData;

    [Header("测试用，将来通过上个场景传输而来的CharacterData")]
    public E_CharacterType characterType;

    private CharacterDataSO sodata;

    private void Start()
    {
        viewer = GetComponent<CharacterBattle_Viewer>();
        //model=new CharacterBattle_Model(CharacterData.Maximum_Health, CharacterData.Maximum_Mana,(int)CharacterData.Maximum_ATB);
        sodata = ResourcesLoader.FindCharaterSO(characterType);
        model = new CharacterBattle_Model(sodata.Maximum_Health, sodata.Maximum_Mana,(int)sodata.Maximum_ATB);
        
        
        viewer.UpdataUI(model);
        modelDic.Add(E_BattleModelType.HP,val=> model.HP+=val);
        modelDic.Add(E_BattleModelType.MAX_HP,val=>model.MaxHP+=val);
        modelDic.Add(E_BattleModelType.SP, val => model.SP += val);
        modelDic.Add(E_BattleModelType.MAX_SP, val => model.MaxSP += val);
        modelDic.Add(E_BattleModelType.AG, val => model.AG += val);
        modelDic.Add(E_BattleModelType.MAX_AG, val => model.MaxAG += val);
        modelDic.Add(E_BattleModelType.ATBPoints,val=> model.ATBPoints+=(int)val);
        modelDic.Add(E_BattleModelType.MAX_ATBPoints,val=> model.MaxATBPoints+=(int)val);

        model.OnDataChanged +=()=> viewer.UpdataUI(model);
        //model.OnHPZero += CharacterDead;
    }

    //角色死亡
    public bool charcaterDead;
    void CharacterDead() {
        Debug.Log(gameObject.name+"角色已死亡");
        charcaterDead = true;
        EventCenter.EventTrigger<CharacterBattle_Controller>
            (E_EventType.CharacterDead, this);
    }

    public float GetCharacterData(E_CharacterPropertyType propertyType)
    {
        //return CharacterData.GetProperty(propertyType);
        return sodata.GetProperty(propertyType);
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
    public void AdjustCharacterModelValue(E_BattleModelType modelType, float targetValue) {
        modelDic[modelType].Invoke(targetValue);
        //Debug.Log(name +"——"+ modelType+"调整:" +targetValue+" 改变后的值：" +GetCharacterModelValue(modelType));
    }

    public float GetCharacterModelValue(E_BattleModelType modelType) {
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
    HP,
    MAX_HP,
    SP,
    MAX_SP,
    AG,
    MAX_AG,
    ATBPoints,
    MAX_ATBPoints
}
