using Core;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Battle_Viewer))]
public class BattleMVCHandler : MonoBehaviour
{
    Battle_Controller battleController;
    public Battle_Viewer viewer;
    public Battle_Controller BattleController => battleController;

    BattlerStateTag battlerStateTag;

    [Header("ATB点数显示")]
    [SerializeField] Transform _atbDotSpawnRoot;
    [SerializeField] Vector2 _atbDotOffset = new Vector2(30f, 0f);
    [SerializeField] [Range(0.5f, 3f)] float _atbDotScale = 1f;

    public void InitMVCHandler(bool isplayer, CharacterData characterData, BattlerStateTag _battlerStateTag, IBattlable battler, int initialShieldPoints = 5)
    {
        battlerStateTag = _battlerStateTag;
        viewer = GetComponent<Battle_Viewer>();
        battleController = new Battle_Controller(characterData, viewer, battlerStateTag, battler, initialShieldPoints);

        if (_atbDotSpawnRoot != null)
            viewer.SetupATBDots(_atbDotSpawnRoot, _atbDotOffset, _atbDotScale);

        var battlemanager = GameRoot.GetManager<BattleStateManager>();
        DebugManager.Log(EDebugCategory.BattleState, battlemanager + "///");
        var levelUpHandler = GetComponent<CharacterLevelUpHandler>();
        battlemanager.RegisterSkiller(isplayer, battlerStateTag, levelUpHandler);
    }

    public void OnMVCHandlerUpdate()
    {
        battleController.OnBattleControlUpdate();
    }
}

