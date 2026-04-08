using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//技能类内不应出现关于释放判断和释放目标的选择逻辑
//假设：通过记录来决定此技能的释放目标


public class SkillBucket { }


/// <summary>
/// 对一名敌人角色造成伤害的技能
/// </summary>
public class Skill_BaseAttack : ISkill
{
    public Skill_BaseAttack(E_SkillTargetType skillTargetType) { 
    this.skillTargetType= skillTargetType;
    }

    //决定伤害类型

    float baseAttackValue = -5;
    float baseAttackRate;

    public E_SkillTargetType skillTargetType { get; set; }

    public void SkillEnhance(IBattlable caster)
    {

    }

    public void SkillExcute(IBattlable caster)
    {

    }


    //void UpdateTargets(CharacterBattle_Controller battler)
    //{
    //    Debug.Log(battler.gameObject.name + "已死亡!!");
    //    if (targets.Contains(battler)){
    //        targets.Remove(battler);
    //        Debug.Log("从目标中删除" + battler + targets.Count);}
    //    else
    //        Debug.Log("当前目标中无死亡者，雨我无瓜");
    //    if (targets.Count <= 0)
    //        GetSkillTraget();
    //}




    //public override void SkillExcute(){
    //    //if (targets.Count <= 0)
    //    //    return;
    //    //foreach (var target in targets){
    //    //    Debug.Log("我是" + self_controller.gameObject.name + ",我的攻击对象" + target + "  " + targets.Count);
    //    //    //Debug.Log(target.name + "收到伤害" + baseAttackValue * baseAttackRate);
    //    //    target.AdjustCharacterModel(EModelType.HP, baseAttackValue * baseAttackRate);
    //    //}

    //    //施法耗蓝 //增加怒气
    //    base.SkillExcute();
    //}


}

///// <summary>
///// 对玩家角色造成多段治疗的技能
///// </summary>
//public class Skill_2 : ISkill
//{
//    int healTime = 5;
//    float baseHealValue = 5;
//    float healInterval = 1;

//    //public void SkillExcute(){
//    //    foreach (CharacterBattle_Controller target in targets){
//    //        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DoHeal(target));
//    //    }
//    //}

//    public void SkillEnhance(List<CharacterBattle_Controller> battlers)
//    {

//    }

//    public void SkillExcute(List<CharacterBattle_Controller> battlers)
//    {

//    }

//    //IEnumerator DoHeal(CharacterBattle_Controller target){
//    //    for (int i = 0; i < healTime; i++){
//    //        //target.AdjustHP(baseHealValue);
//    //        yield return new WaitForSeconds(healInterval);
//    //    }
//    //}
//}

///// <summary>
///// 对多名敌人角色造成伤害的群体技能
///// </summary>
//public class Skill_3 : ISkill
//{
//    int targetNum;
//    float baseAttackValue = -8;
//    public List<CharacterBattle_Controller> targets { get; set; }
//    public void SkillEnhance()
//    {

//    }

//    public void SkillEnhance(List<CharacterBattle_Controller> battlers)
//    {
//        throw new System.NotImplementedException();
//    }

//    public void SkillExcute()
//    {
//        foreach (var target in targets)
//        {
//        }
//    }

//    public void SkillExcute(List<CharacterBattle_Controller> battlers)
//    {
//        throw new System.NotImplementedException();
//    }
//}

