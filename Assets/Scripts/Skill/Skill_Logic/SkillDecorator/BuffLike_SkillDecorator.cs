using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BuffLike_SkillDecorator : SkillDecorator
{
    public BuffLike_SkillDecorator(ISkill skill,float _duration ) : base(skill){
      
    }
    public override void Excute(IBattlable self, IBattlable target)
    {
   
    }
 
}
