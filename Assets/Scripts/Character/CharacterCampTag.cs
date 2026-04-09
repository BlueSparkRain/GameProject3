using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCampTag : MonoBehaviour
{
    public bool isPlayer {  get; private set; }
    IUpGradable upgradeHandle;
    IBattlable ibattle;
   
    public void InitCharacterTag(E_CharacterType characterType,bool isPlayer, bool canLevelUP)
    {
        this.isPlayer = isPlayer;
        if (isPlayer) ibattle = new Player();
        else ibattle = new Enemy();

        if (canLevelUP)
        {
            upgradeHandle = new LevelUpGradeHandle();
            GetComponent<CharacterMapMoveHandle>().InitMover(isPlayer, characterType);
        }
        else upgradeHandle = new StageUpGradeHandle();

    }
}
