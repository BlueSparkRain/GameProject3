using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗中玩家信息面板 — 管理自动化/主动技能图标生成，显示HP/SP条
/// 当场景中放置了 PlayerBattleBoard 时，玩家不再从 CharacterBattleArea 预制件生成，
/// 而是由 GameBattleManager 直接传递 CharacterData 到此面板完成初始化。
/// </summary>
public class PlayerBattleBoard : MonoBehaviour
{
    [Header("技能生成器")]
    public SkillIconSpawner autoSkillSpawner;
    public SkillIconSpawner activeSkillSpawner;

    [Header("技能面板")]
    public GameObject AutoBoard;
    public GameObject ATBBoard;
    [Header("技能面板切换Button")]
    public Button SwitchSkillBoard;

    [Header("BUFF 面板")]
    public Button buffToggleButton;
    public GameObject buffPanel;
    public Text buffInfoText;

    bool useAutoBoarding = true;
    bool _buffPanelVisible;
    /// <summary>HP/SP 条更新已由 Battle_Viewer 统一负责，此处不再重复</summary>

    bool _initialized;

    void Start()
    {
        if (!_initialized)
            StartCoroutine(FallbackPolling());
        if (SwitchSkillBoard)
            SwitchSkillBoard.onClick.AddListener(SwitchBoard);
        if (buffToggleButton)
            buffToggleButton.onClick.AddListener(ToggleBuffPanel);
        if (buffPanel)
            buffPanel.SetActive(false);
    }

    void Update()
    {
        if (_buffPanelVisible && buffPanel != null && buffPanel.activeSelf)
            RefreshBuffDisplay();
    }

    void ToggleBuffPanel()
    {
        if (buffPanel == null) return;
        _buffPanelVisible = !_buffPanelVisible;
        buffPanel.SetActive(_buffPanelVisible);
        if (_buffPanelVisible)
            RefreshBuffDisplay();
    }

    void RefreshBuffDisplay()
    {
        if (buffInfoText == null) return;

        // BattleBuffHandler 可能在 PlayerBattleBoard 自身、子物体、或通过 BattleHandler 引用
        var bh = GetComponentInChildren<BattleBuffHandler>();
        if (bh == null)
        {
            var handler = GetComponentInChildren<BattleHandler>();
            bh = handler?.buffHandler;
        }
        if (bh == null)
        {
            // 回退：场景中全局查找玩家 BuffHandler
            foreach (var h in FindObjectsOfType<BattleHandler>())
            {
                if (h.MVCHandler?.BattleController?.CharacterData?.characterType == E_CharacterType.P_海螺骑士)
                {
                    bh = h.buffHandler;
                    break;
                }
            }
        }

        if (bh == null)
        {
            buffInfoText.text = "未找到 BUFF 组件";
            return;
        }

        var buffs = bh.GetAllBuffInfo();
        if (buffs.Count == 0)
        {
            buffInfoText.text = "当前无 BUFF";
            return;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var (name, remaining, total) in buffs)
        {
            sb.AppendLine($"{name}  {remaining:F0}/{total:F0}s");
        }
        buffInfoText.text = sb.ToString();
    }

    /// <summary>
    /// 交换 AutoBoard / ATBBoard 的 sibling index，不 SetActive(false)，
    /// 避免自动化技能的协程被中断。
    /// </summary>
    void SwitchBoard()
    {
        if (AutoBoard == null || ATBBoard == null) return;
        useAutoBoarding = !useAutoBoarding;

        int autoIdx = AutoBoard.transform.GetSiblingIndex();
        int atbIdx = ATBBoard.transform.GetSiblingIndex();
        AutoBoard.transform.SetSiblingIndex(atbIdx);
        ATBBoard.transform.SetSiblingIndex(autoIdx);
    }


    /// <summary>
    /// 由 GameBattleManager 直接调用（推荐路径），传入玩家 CharacterData 完成初始化。
    /// </summary>
    public void InitPlayerBoard(CharacterData data)
    {
        _initialized = true;
        StopAllCoroutines();

        var handler = GetComponent<BattleHandler>();
        if (handler == null)
            handler = gameObject.AddComponent<BattleHandler>();

        var skillHandler = GetComponentInChildren<BattleSkillHandler>();
        if (skillHandler != null)
        {
            if (skillHandler.normalSkillIconSpawner == null)
                skillHandler.normalSkillIconSpawner = autoSkillSpawner;
            if (skillHandler.atbSkillIconSpawner == null)
                skillHandler.atbSkillIconSpawner = activeSkillSpawner;
        }

        handler.InitBattler(data);

        DebugManager.Log(EDebugCategory.PlayerBattleBoard,
            $"[PlayerBattleBoard] 直接初始化 — {handler.MVCHandler.BattleController.CharacterData.Character_Name}, " +
            $"mapNormalIDs={data.mapNormalSkillIDs?.Count ?? 0}, " +
            $"mapATBIDs={data.mapATBSkillIDs?.Count ?? 0} " +
            $"(技能由InitMonsterAI→BattleSkiller加载，Spawner已注入)");
    }

    IEnumerator FallbackPolling()
    {
        DebugManager.Log(EDebugCategory.PlayerBattleBoard, "[PlayerBattleBoard] 回退至轮询模式…");
        BattleHandler handler = null;
        int findAttempts = 0;
        while (handler == null)
        {
            handler = FindPlayerBattleHandler();
            findAttempts++;
            if (findAttempts % 60 == 1)
                DebugManager.Log(EDebugCategory.PlayerBattleBoard, $"[PlayerBattleBoard] 等待BattleHandler... 尝试{findAttempts}次, handler={handler != null}");
            if (findAttempts > 600)
            {
                Debug.LogError("[PlayerBattleBoard] 超时！10秒内未找到玩家BattleHandler，中止");
                yield break;
            }
            yield return null;
        }

        DebugManager.Log(EDebugCategory.PlayerBattleBoard, $"[PlayerBattleBoard] 找到玩家BattleHandler! 尝试{findAttempts}次");
        var data = handler.MVCHandler.BattleController.CharacterData;
        DebugManager.Log(EDebugCategory.PlayerBattleBoard, $"[PlayerBattleBoard] CharacterData: {data.Character_Name}, " +
                  $"mapNormalIDs={data.mapNormalSkillIDs?.Count ?? 0}, " +
                  $"mapATBIDs={data.mapATBSkillIDs?.Count ?? 0}");
        LoadSkills(data);
    }

    BattleHandler FindPlayerBattleHandler()
    {
        foreach (var h in FindObjectsOfType<BattleHandler>())
        {
            if (h.MVCHandler != null
                && h.MVCHandler.BattleController != null
                && h.MVCHandler.BattleController.CharacterData != null
                && h.MVCHandler.BattleController.CharacterData.characterType == E_CharacterType.P_海螺骑士)
                return h;
        }
        return null;
    }

    void LoadSkills(CharacterData data)
    {
        if (autoSkillSpawner == null)
        {
            DebugManager.LogWarning(EDebugCategory.PlayerBattleBoard, "[PlayerBattleBoard] autoSkillSpawner 未赋值！");
        }
        else
        {
            var autoDatas = BuildSkillDataList(data.mapNormalSkillIDs);
            DebugManager.Log(EDebugCategory.PlayerBattleBoard, $"[PlayerBattleBoard] 自动化技能: {autoDatas.Count}个 — {string.Join(", ", autoDatas.ConvertAll(d => d.skill_Name))}");
            autoSkillSpawner.LoadSlotsAndSkills(
                Mathf.Min(data.AutoSkillSlotCount, autoDatas.Count), autoDatas, false);
        }

        if (activeSkillSpawner == null)
        {
            DebugManager.LogWarning(EDebugCategory.PlayerBattleBoard, "[PlayerBattleBoard] activeSkillSpawner 未赋值！");
        }
        else
        {
            var activeDatas = BuildSkillDataList(data.mapATBSkillIDs);
            DebugManager.Log(EDebugCategory.PlayerBattleBoard, $"[PlayerBattleBoard] 主动技能: {activeDatas.Count}个 — {string.Join(", ", activeDatas.ConvertAll(d => d.skill_Name))}");
            activeSkillSpawner.LoadSlotsAndSkills(
                Mathf.Min(data.AtbSkillSlotCount, activeDatas.Count), activeDatas, false);
        }
    }

    List<SkillData> BuildSkillDataList(List<int> ids)
    {
        var list = new List<SkillData>();
        if (ids == null) return list;
        foreach (int id in ids)
        {
            var so = ResourcesLoader.FindSkillSOByID(id);
            if (so != null)
                list.Add(new SkillData(so));
        }
        return list;
    }
}
