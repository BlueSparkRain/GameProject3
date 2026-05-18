using UnityEngine;

/// <summary>
/// 管理角色数据的读取和更新(升级)和阵营
/// 管理角色升级时的属性数值调整
/// </summary>
public class CharacterDataTag : MonoBehaviour
{
    public bool isPlayer { get; private set; }
    IUpGradable upgradeHandle;
    IBattlable ibattle;
    CharacterData characterData;

    //读取存档数据
    void TryGetSaveData() { 
    
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="characterType"></param>
    /// <param name="isPlayer"></param>
    /// <param name="canLevelUP"></param>
    public void InitCharacterDataTag(E_CharacterType characterType, bool isPlayer, bool canLevelUP)
    {
        characterData = new CharacterData(characterType);

        this.isPlayer = isPlayer;
        if (isPlayer) ibattle = new Player();
        else ibattle = new Enemy();

        if (canLevelUP)
        {
            upgradeHandle = new LevelUpGradeMode(characterType, characterData);
            GetComponent<CharacterMapMoveHandle>().InitMover(isPlayer, characterType);
        }
        else upgradeHandle = new StageUpGradeMode(characterType, characterData);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            //AdjustProperty(E_CharacterPropertyType.Phy_Attack, 5);
            upgradeHandle.UpGrade();
        }
    }

}
