using System;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 每一枚技能图标。战斗中的技能模式（Auto/ATB）由 ISkillMode 接管，解耦充能与输入逻辑。
/// 悬停展示技能描述由纯C#工具类 SkillTooltipHover 驱动。
/// </summary>
public class SkillIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    #region UI组件引用
    [Header("技能图标Image")]
    public Image skillImage;
    [Header("技能冷却Image")]
    public Image skillCoolDownImage;
    [Header("技能名称Text")]
    public Text skillNameText;

    [Header("ATB主动模式UI(可选)")]
    [SerializeField] Button _atbSelectButton;
    [SerializeField] Image _atbEnhanceImage;

    [Header("增幅等级颜色 [0]白 [1]蓝 [2]黄 [3]红")]
    [SerializeField] Color[] _enhanceColors = new Color[]
    {
        Color.white,
        Color.blue,
        Color.yellow,
        Color.red,
    };
    #endregion
    #region 技能状态字段
    [Header("技能计时器")]
    public float skillTimer = 0;
    public bool hasNoSP;
    #endregion

    private SkillData skillData;
    public SkillData SkillData => skillData;
    /// <summary>战斗中的技能模式（AutoMode / ATBMode）</summary>
    public ISkillMode SkillMode { get; private set; }
    /// <summary>由 SkillIconSpawner 设置，InitBattleSkill 未传参时默认使用</summary>
    public E_SkillMode PendingSkillMode { get; set; } = E_SkillMode.Auto;
    /// <summary>向后兼容：Auto模式返回内部Charger，ATB模式返回null</summary>
    public SkillCharger Charger => (SkillMode as AutoMode)?.Charger;
    bool canDrag;

    [Header("悬停提示")]
    [SerializeField] float _hoverDelay = 0.3f;
    [SerializeField] Vector2 _tooltipOffset = new Vector2(0, 80f);
    SkillTooltipHover _tooltip;

    public void InitSkillIcon(SkillData _skilldata, SkillSlot slot, bool _canDrag)
    {
        skillData = _skilldata;
        canDrag = _canDrag;
        skillImage.sprite = SkillData.skill_Sprite;
        skillNameText.text = SkillData.skill_Name;
        skillTimer = skillData.skill_CoolDown;
        GetComponent<SlotSwaperHandler>().InitSlot(slot);

        _tooltip?.Dispose();
        _tooltip = new SkillTooltipHover(this, transform, skillData.skill_Description, _hoverDelay, _tooltipOffset);
    }

    /// <summary>
    /// 关联 BattleSkill 并根据模式创建 ISkillMode 接管战斗逻辑。
    /// </summary>
    public void InitBattleSkill(SkillBase skill)
    {
        // 清理旧模式
        SkillMode?.Dispose();
        SkillMode = null;

        switch (PendingSkillMode)
        {
            case E_SkillMode.Auto:
                var auto = new AutoMode();
                auto.OnCooldownChanged += f => { if (skillCoolDownImage) skillCoolDownImage.fillAmount = f; };
                auto.OnSPStatusChanged += noSP => { if (skillImage) skillImage.color = noSP ? Color.blue : Color.white; };
                auto.Init(skillData, skill);
                SkillMode = auto;
                break;

            case E_SkillMode.ATB:
                var atb = new ATBMode();
                atb.OnSPStatusChanged += noSP => { if (skillImage) skillImage.color = noSP ? Color.blue : Color.white; };
                atb.OnSelectionChanged += OnATBSelectionChanged;
                atb.OnEnhanceLevelChanged += OnATBEnhanceChanged;
                atb.Init(skillData, skill);
                SkillMode = atb;
                SetupATBUI();
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => _tooltip?.Enter();
    public void OnPointerExit(PointerEventData eventData) => _tooltip?.Exit();

    void OnDisable() => _tooltip?.Exit();

    void OnDestroy()
    {
        _tooltip?.Dispose();
        _tooltip = null;
        SkillMode?.Dispose();
        SkillMode = null;
    }

    public void FreezeIcon(bool freeze)
    {
        SkillMode?.Freeze(freeze);
    }

    public void IconCycleUpdate(float currentSP)
    {
        if (SkillMode != null)
        {
            SkillMode.Update(currentSP, Time.deltaTime);
            return;
        }

        // 非战斗回退逻辑
        if (skillTimer > -0.01f)
        {
            skillCoolDownImage.fillAmount = skillTimer / skillData.skill_CoolDown;
            skillTimer -= Time.deltaTime;
        }
    }

    public void SkillBreak()
    {
        SkillMode?.SkillBreak();
    }

    #region ATB UI
    void SetupATBUI()
    {
        if (_atbSelectButton == null)
        {
            var btnGo = new GameObject("ATBButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(transform, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = btnRt.anchorMax = new Vector2(1f, 0.5f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.sizeDelta = new Vector2(28f, 28f);
            btnRt.anchoredPosition = new Vector2(18f, 0f);
            btnGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.6f);
            _atbSelectButton = btnGo.GetComponent<Button>();
        }

        _atbSelectButton.onClick.RemoveAllListeners();
        _atbSelectButton.onClick.AddListener(() =>
        {
            var atbMode = SkillMode as ATBMode;
            if (atbMode != null)
            {
                EventCenter.EventTrigger(E_EventType.SkillIconATBSelected, (RectTransform)transform);
                atbMode.ToggleSelection();
            }
        });

        if (_atbEnhanceImage == null)
        {
            var imgGo = new GameObject("ATB_Enhance", typeof(RectTransform), typeof(Image));
            imgGo.transform.SetParent(transform, false);
            var imgRt = imgGo.GetComponent<RectTransform>();
            imgRt.anchorMin = imgRt.anchorMax = new Vector2(1f, 0.5f);
            imgRt.pivot = new Vector2(0.5f, 0.5f);
            imgRt.sizeDelta = new Vector2(14f, 14f);
            imgRt.anchoredPosition = new Vector2(38f, 0f);
            _atbEnhanceImage = imgGo.GetComponent<Image>();
        }

        _atbEnhanceImage.color = _enhanceColors[0];
    }

    void OnATBSelectionChanged(bool selected)
    {
        if (_atbSelectButton != null)
            _atbSelectButton.GetComponent<Image>().color = selected
                ? new Color(1f, 0.85f, 0.3f, 1f)
                : new Color(1f, 1f, 1f, 0.6f);
    }

    void OnATBEnhanceChanged(int level)
    {
        if (_atbEnhanceImage != null && level >= 0 && level < _enhanceColors.Length)
            _atbEnhanceImage.color = _enhanceColors[level];
    }
    #endregion
}

