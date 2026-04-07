using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责应对“游戏内所有需要加载技能图标的地方”的需求
/// </summary>
public class SkillIconLoadManager : MonoGlobalManager
{
    public override void MgrUpdate(float deltaTime)
    {

    }

        ObjectPoolManager objectPoolManager;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        objectPoolManager=GameRoot.GetManager<ObjectPoolManager>();
        //首先加载对象池中的SkillIcon
    }

    /// <summary>
    /// 将一个技能图标加载到一个技能槽位上
    /// </summary>
    /// <param name="skillSlot"></param>
    public void LoadNewSkillIcon(Transform skillSlot) {

        //产生对应的skillIcon
        var newSkillIcon = objectPoolManager.
        GetInstance(EPoolType.SkillIcon_技能图标).GetComponent<SkillIcon>();

        newSkillIcon.transform.SetParent(skillSlot);
        //变换重制
        newSkillIcon.transform.localScale = Vector3.one;
        newSkillIcon.transform.localRotation= Quaternion.identity;
        newSkillIcon.transform.localPosition = Vector3.zero;
    }


  
}
