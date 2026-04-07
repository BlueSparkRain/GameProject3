using Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattle_Controller : MonoBehaviour
{
    public CharacterBattle_Viewer viewer;
    CharacterBattle_Model  model;
    Dictionary<EModelType, Action<float>> modelDic=new Dictionary<EModelType, Action<float>>();
    //对局中的角色属性数据
    //public CharacterData characterData;
    public CharacterDataSO CharacterDataSO;

    private void Start()
    {
        viewer = GetComponent<CharacterBattle_Viewer>();
        //model = new CharacterBattle_Model(characterData.GetProperty(E_CharacterPropertyType.Maximum_Health),
        //   characterData.GetProperty(E_CharacterPropertyType.Maximum_Mana),
        //   (int)characterData.GetProperty(E_CharacterPropertyType.Maximum_ATB));
        model=new CharacterBattle_Model(CharacterDataSO.Maximum_Health, CharacterDataSO.Maximum_Mana,(int)CharacterDataSO.Maximum_ATB);
        viewer.UpdataUI(model);
        modelDic.Add(EModelType.HP,val=> model.HP+=val);
        modelDic.Add(EModelType.MAX_HP,val=>model.MaxHP+=val);
        modelDic.Add(EModelType.SP, val => model.SP += val);
        modelDic.Add(EModelType.MAX_SP, val => model.MaxSP += val);
        modelDic.Add(EModelType.AG, val => model.AG += val);
        modelDic.Add(EModelType.MAX_AG, val => model.MaxAG += val);
        modelDic.Add(EModelType.ATBPoints,val=> model.ATBPoints+=(int)val);
        modelDic.Add(EModelType.MAX_ATBPoints,val=> model.MaxATBPoints+=(int)val);

        model.OnDataChanged +=()=> viewer.UpdataUI(model);
        model.OnHPZero += CharacterDead;
    }

    //角色死亡
    public bool charcaterDead;
    void CharacterDead() {
        Debug.Log(gameObject.name+"角色已死亡");
        charcaterDead = true;
        EventCenter.EventTrigger<CharacterBattle_Controller>
            (E_EventType.CharacterDead, this);


    }
    ///// <summary>
    ///// 修改角色的属性
    ///// </summary>
    //public void AdjustCharacterData(E_CharacterPropertyType propertyType,float targetValue) { 
    //    characterData.AdjustProperty(propertyType, targetValue);
    //}

    /// <summary>
    /// 修改角色模型
    /// </summary>
    public void AdjustCharacterModel(EModelType modelType, float targetValue) {
        modelDic[modelType].Invoke(targetValue);
        //Debug.Log(name +"——"+ modelType+"调整:" +targetValue+" 改变后的值：" +GetCharacterModelValue(modelType));
    }

    public float GetCharacterModelValue(EModelType modelType) {
        return modelType switch
        {
            EModelType.HP => model.HP,
            EModelType.MAX_HP => model.MaxHP,
            EModelType.SP => model.SP,
            EModelType.MAX_SP => model.MaxSP,
            EModelType.AG => model.AG,
            EModelType.MAX_AG => model.MaxAG,
            EModelType.ATBPoints => model.ATBPoints,
            EModelType.MAX_ATBPoints => model.MaxATBPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(modelType), modelType, null)
        };
    }

}

public enum EModelType
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
