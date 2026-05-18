using System.Diagnostics;

public class LevelUpGradeMode : IUpGradable
{
    //private int currentLevel;
    //private int baseExpGoal;
    //private int baseExpGrowth;

    public CharcterPropertyGrowthSO growthData { get; set; }
    public CharacterData characterData { get; set; }

    public LevelUpGradeMode(E_CharacterType characterType, CharacterData data)
    {
        UnityEngine.Debug.Log("我是每级升级型角色");
        growthData = ResourcesLoader.FindCharaterGrowthSO(characterType);
        characterData = data;
    }
    
}
