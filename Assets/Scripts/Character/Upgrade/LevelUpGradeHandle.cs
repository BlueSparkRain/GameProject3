using System.Diagnostics;

public class LevelUpGradeHandle : IUpGradable
{
    private int currentLevel;
    private int baseExpGoal;
    private int baseExpGrowth;
    public LevelUpGradeHandle() {
        UnityEngine.Debug.Log("我是每级升级型角色");
        
    }

    public void UpGrade(CharacterData characterData)
    {
        //提升属性值

        //characterData.

    }
}
