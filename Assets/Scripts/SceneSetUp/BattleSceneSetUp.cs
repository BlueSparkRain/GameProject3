using UnityEngine;
using Core;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class BattleSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;
    [Header("当释放主动技能过程中会进入专注模式")]
    public CanvasGroup FreezeBlack;
    [Header("专注模式过渡时间")]
    public float freezeFadeDuration = 0.15f;

    private void Awake(){
        gameRoot=GameRoot.Instance;
        gameRoot.RegisterScene_MonoManager<BattleStateManager>();
        gameRoot.RegisterScene_MonoManager<BattleLoadManager>();
        gameRoot.RegisterScene_MonoManager<BattlePhaseManager>();
        gameRoot.RegisterScene_MonoManager<BattleDebugManager>();
        gameRoot.RegisterScene_MonoManager<BattleActionQueue>();
        gameRoot.RegisterScene_MonoManager<SkillVfxDirectorManager>();


        EventCenter.AddEventListener<bool>(E_EventType.PrepareATBSkillExcute, OnPrepareATB);
    }

    void OnPrepareATB(bool entering)
    {
        if (FreezeBlack == null) return;
        FreezeBlack.DOKill();
        FreezeBlack.DOFade(entering ? 1f : 0f, freezeFadeDuration).SetUpdate(true);
    }

    private void OnDestroy()
    {
        EventCenter.RemoveEventListener<bool>(E_EventType.PrepareATBSkillExcute, OnPrepareATB);
    }

    private void Start(){
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame() {
        yield return  new WaitForSeconds(2);
        GameRoot.GetManager<GameBattleManager>().SpawnBattleCharacter();
    }
}
