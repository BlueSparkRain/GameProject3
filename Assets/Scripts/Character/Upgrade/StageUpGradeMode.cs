using System.Diagnostics;

public class StageUpGradeMode : IUpGradable
{

    public CharcterPropertyGrowthSO growthData { get; set; }
    public CharacterData characterData { get; set; }

    public StageUpGradeMode(E_CharacterType characterType, CharacterData data)
    {
        UnityEngine.Debug.Log("我是阶段升级型角色");
        growthData = ResourcesLoader.FindCharaterGrowthSO(characterType);
        characterData = data;
    }
}
