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

    public void InitMVCHandler(bool isplayer,CharacterData characterData, BattlerStateTag _battlerStateTag, int initialShieldPoints = 5)
    {
        battlerStateTag = _battlerStateTag;
        viewer = GetComponent<Battle_Viewer>();
        battleController = new Battle_Controller(characterData, viewer, battlerStateTag, initialShieldPoints);

        var battlemanager = GameRoot.GetManager<BattleTargetsSelectManager>();
        Debug.Log(battlemanager + "///");
        var levelUpHandler = GetComponent<CharacterLevelUpHandler>();
        battlemanager.RegisterSkiller(isplayer, battlerStateTag, levelUpHandler);
    }

    public void OnMVCHandlerUpdate() {
        battleController.OnBattleControlUpdate();
    }
}

