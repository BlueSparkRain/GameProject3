using Core;
using UnityEngine;

/// <summary>
/// 调试快捷键面板 — 挂到 MapScene 任意 GameObject 上即可
/// </summary>
public class MapSceneDebugTest : MonoBehaviour
{
    [Multiline]
    public string DebugText;

    void Update()
    {
        //if (!Input.GetKey(KeyCode.LeftControl)) return;

        // ========== 金币 ==========
        if (Input.GetKeyDown(KeyCode.G))
            GameRoot.GetManager<GoldManager>()?.AddGold(500);

        // ========== 活力 ==========
        if (Input.GetKeyDown(KeyCode.V))
            GameRoot.GetManager<VitalityPointsManager>()?.AdjustVolityPoints(-1);

        if (Input.GetKeyDown(KeyCode.B))
            GameRoot.GetManager<VitalityPointsManager>()?.AdjustVolityPoints(+3);

        // ========== 混沌等级 ==========
        if (Input.GetKeyDown(KeyCode.C))
        {
            var chaos = GameRoot.GetManager<ChaosLevelManager>();
            if (chaos != null)
            {
                chaos.AdjustChaosLevelByRound(chaos.currentLevel * 10 + 1);
                DebugManager.Log(EDebugCategory.General, $"[Debug] 混沌等级提升至 {chaos.currentLevel}，敌人倍率 x{chaos.EnemyStrengthMultiplier:F2}");
            }
        }

        // ========== 行动点 ==========
        if (Input.GetKeyDown(KeyCode.A))
        {
            var ap = GameRoot.GetManager<ActionPointsManager>();
            if (ap != null)
                DebugManager.Log(EDebugCategory.General, $"[Debug] 当前行动点: {ap.RemainActionPoints}/{ap.MaxActionPoints}");
        }

        // ========== 切换商店(打开/关闭) ==========
        if (Input.GetKeyDown(KeyCode.P))
        {
            var uiMgr = GameRoot.GetManager<UIManager>();
            if (uiMgr == null) return;
            var panel = uiMgr.GetPanel<ShopPanel>(E_UIPanelType.ShopPanel);
            if (panel != null && panel.IsAnimating) return;
            if (panel != null && panel.gameObject.activeSelf)
                panel.Hide();
            else
                uiMgr.OpenPanel<ShopPanel>(E_UIPanelType.ShopPanel);
        }
        if (Input.GetKeyDown(KeyCode.R)){
            GameRoot.GetManager<UIManager>().OpenPanel<RewardPanel>(E_UIPanelType.RewardPanel,null);
        }
    }
}
