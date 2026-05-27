using Core;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Battle_Viewer))]
public class BattleMVCHandler : MonoBehaviour
{
    //IBattlable self;
    Battle_Controller battleController;
    public Battle_Viewer viewer;
    public Battle_Controller BattleController=>battleController;

    BattlerStateTag battlerStateTag;

    public void InitMVCHandler(CharacterData characterData, BattlerStateTag _battlerStateTag)
    {
        bool isplayer = (characterData.characterType == E_CharacterType.P_1 ||
                        characterData.characterType == E_CharacterType.P_2 ||
                        characterData.characterType == E_CharacterType.P_3);

        battlerStateTag = _battlerStateTag;
        viewer = GetComponent<Battle_Viewer>();
        battleController = new Battle_Controller(characterData, viewer,battlerStateTag);

        var battlemanager = GameRoot.GetManager<BattleTargetsSelectManager>();
        Debug.Log(battlemanager + "///");
        battlemanager.RegisterSkiller(isplayer, battlerStateTag);
    }

    public void OnMVCHandlerUpdate() {
        battleController.OnBattleControlUpdate();
    }
}

