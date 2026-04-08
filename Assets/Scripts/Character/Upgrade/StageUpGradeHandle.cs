using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageUpGradeHandle : IUpGradable
{
    public StageUpGradeHandle() {
        UnityEngine.Debug.Log("我是阶段升级型角色");
    }
    public void UpGrade(CharacterData characterData)
    {

    }
}
