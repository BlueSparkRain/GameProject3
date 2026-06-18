using Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 获取到战斗中的所有单位，在任意技能初始化时分配施法目标
/// </summary>
public class BattleStateManager : MonoSceneManager
{
    private List<BattlerStateTag> playerControllers = new List<BattlerStateTag>();
    private List<BattlerStateTag> enemyControllers = new List<BattlerStateTag>();
    private Dictionary<BattlerStateTag, Transform> enemyTransforms = new Dictionary<BattlerStateTag, Transform>();
    public IReadOnlyList<BattlerStateTag> EnemyControllers => enemyControllers;
    private List<CharacterLevelUpHandler> playerLevelHandlers = new List<CharacterLevelUpHandler>();

    // 记录本局战斗中所有的技能释放者，方便技能初始化时分配目标
    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener<BattlerStateTag>(E_EventType.Battle_CharacterDead, CheckBattleEnd);
    }

    public void RegisterSkiller(bool isPlayer, BattlerStateTag battleController, CharacterLevelUpHandler levelUpHandler = null)
    {
        (isPlayer ? playerControllers : enemyControllers).Add(battleController);
        if (isPlayer && levelUpHandler != null)
            playerLevelHandlers.Add(levelUpHandler);
    }

    /// <summary>注册敌人 Transform（由 BattleHandler 调用）</summary>
    public void RegisterEnemyTransform(BattlerStateTag tag, Transform t)
    {
        enemyTransforms[tag] = t;
    }

    /// <summary>通过 BattlerStateTag 获取敌人 Transform</summary>
    public Transform GetEnemyTransform(BattlerStateTag tag)
    {
        enemyTransforms.TryGetValue(tag, out var t);
        return t;
    }

    bool gameEnd = false;

    void CheckBattleEnd(BattlerStateTag characterBattle_Controller){
        if (!gameEnd && PlayerWinBattle()){
            GameEnd(true);
        }
        if (!gameEnd && EnemyWinBattle()){
            GameEnd(false);
        }
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.Space)){
            DebugManager.Log(EDebugCategory.General, "直接获胜");
            GameEnd(true);
        }
    }

    void GameEnd(bool playWin) {
        gameEnd = true;
        GameRoot.GetManager<BattlePhaseManager>()?.TriggerBattleEnd(playWin);
        EventCenter.EventTrigger(E_EventType.BattleEnd);
        int vitalityAdjust = CalcVitalityLoss();
        if (playWin) {
            int goldReward = CalcGoldReward();
            GameRoot.GetManager<GoldManager>()?.AddGold(goldReward);
            float expReward = CalcEXPReward();
            AwardEXPToPlayer(expReward);
            vitalityAdjust = 1;
        }

        GameRoot.GetManager<GameBattleManager>()?.OnBattleResult(playWin);

        GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(E_UIPanelType.MessagePanel,
            p => p.SetMessage("战斗"+ (playWin?"胜利":"失败"!), () =>{
                GameRoot.GetManager<ObjectPoolManager>()?.ReclaimAll(E_PoolType.FloatingText_跳字);
                GameRoot.GetManager<VitalityPointsManager>().AdjustVolityPoints(-vitalityAdjust);
                GameRoot.GetManager<SceneSwitchManager>().SwitchSceneAsync("MapScene", SceneSwitchManager.LoadMode.Single);
            }));
        return;

    }

    int CalcVitalityLoss()
    {
        int loss = 0;
        foreach (var enemy in enemyControllers)
        {
            string t = enemy.CharacterType.ToString();
            if (t.StartsWith("ME_")) loss += 1;
            else if (t.StartsWith("BOSS_")) loss += 2;
        }
        int chaos = GameRoot.GetManager<ChaosLevelManager>()?.currentLevel ?? 1;
        loss += chaos;
        return loss;
    }

    int CalcGoldReward()
    {
        int baseGold = 0;
        foreach (var enemy in enemyControllers)
        {
            string t = enemy.CharacterType.ToString();
            if (t.StartsWith("LE_")) baseGold += 150;
            else if (t.StartsWith("ME_")) baseGold += 1000;
            else if (t.StartsWith("BOSS_")) baseGold += 5000;
        }
        var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
        float rewardMulti = chaosMgr != null ? chaosMgr.RewardMultiplier : 1f;
        return Mathf.RoundToInt(baseGold * rewardMulti);
    }

    float CalcEXPReward()
    {
        float baseEXP = 0;
        foreach (var enemy in enemyControllers)
        {
            string t = enemy.CharacterType.ToString();
            if (t.StartsWith("LE_")) baseEXP += 1000;
            else if (t.StartsWith("ME_")) baseEXP += 5000;
            else if (t.StartsWith("BOSS_")) baseEXP += 20000;
        }
        var chaosMgr = GameRoot.GetManager<ChaosLevelManager>();
        float rewardMulti = chaosMgr != null ? chaosMgr.RewardMultiplier : 1f;
        return baseEXP * rewardMulti;
    }

    void AwardEXPToPlayer(float amount)
    {
        foreach (var handler in playerLevelHandlers)
            handler.AdjustEXP(amount);
    }

    bool EnemyWinBattle()
    {
        foreach (var battler in playerControllers)
        {
            if (!battler.State_Dead)
                return false;
        }
        return true;
    }

    bool PlayerWinBattle()
    {
        foreach (var battler in enemyControllers)
        {
            if (!battler.State_Dead)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 每当战斗中的角色出现变动，会更新目标
    /// </summary>
    public List<BattlerStateTag> GetSkillTarget(E_SkillTargetType_Auto skillTargetType)
    {
        List<BattlerStateTag> targets = new List<BattlerStateTag>();
        switch (skillTargetType)
        {
            case E_SkillTargetType_Auto.对单体:
                int randomIndex1 = UnityEngine.Random.Range(0, playerControllers.Count);
                targets.Add(playerControllers[0]);
                break;
            case E_SkillTargetType_Auto.对全体:
                for (int i = 0; i < playerControllers.Count; i++)
                    targets.Add(playerControllers[i]);
                break;
            case E_SkillTargetType_Auto.对N目标:
                break;
            default:
                break;
        }
        return targets;
    }

    public override void MgrUpdate(float deltaTime)
    {
    }
}
