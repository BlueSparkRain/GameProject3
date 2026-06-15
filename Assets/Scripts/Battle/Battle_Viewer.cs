using System.Collections.Generic;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗过程中的人物卡牌
/// </summary>
public class Battle_Viewer : MonoBehaviour{
    /// <summary>
    /// 正常条过渡时间
    /// </summary>
    float normalFollowDuration = 0.05f;
    /// <summary>
    /// 慢速条过渡时间
    /// </summary>
    float slowFollowDuration = 0.1f;

    [Header("生命条")]
    public Image HP_fillImage;
    [Header("生命条-慢速过渡")]
    public Image HP_fillImage_Slow;
    [Header("法力条")]
    public Image SP_fillImage;
    [Header("法力条-慢速过渡")]
    public Image SP_fillImage_Slow;
    [Header("怒气条")]
    public Image AG_fillImage;
    [Header("怒气条-慢速过渡")]
    public Image AG_fillImage_Slow;

    [Header("生命值文本")]
    public TMP_Text HP_text;
    [Header("法力值文本")]
    public TMP_Text SP_text;
    [Header("怒气值文本")]
    public TMP_Text AG_text;
    [Header("ATB点数文本")]
    public TMP_Text ATB_point_Text;
    [Header("护盾值文本")]
    public TMP_Text Shield_text;

    [Header("力竭状态UI")]
    public CanvasGroup breakCanvasGroup;
    public Image breakProgressImage;

    [Header("弱点图标")]
    public Transform weaknessIconContent;

    // ATB 点数显示（从对象池取放）
    Transform _atbDotSpawnRoot;
    Vector2 _atbDotOffset;
    float _atbDotScale = 1f;
    List<GameObject> _atbDots = new List<GameObject>();
    int _lastATBPoints = -1;

    public void SetupATBDots(Transform spawnRoot, Vector2 offset, float dotScale = 1f)
    {
        _atbDotSpawnRoot = spawnRoot;
        _atbDotOffset = offset;
        _atbDotScale = dotScale;
    }

    public void UpdataUI(Battle_Model model)
    {
        NormalFollow(HP_fillImage, model.HP / model.MaxHP);
        NormalFollow(SP_fillImage, model.SP / model.MaxSP);
        NormalFollow(AG_fillImage, model.AG / model.MaxAG);
        SP_text.text = $"{Mathf.FloorToInt(model.SP)}/{Mathf.FloorToInt(model.MaxSP)}";
        HP_text.text = $"{Mathf.FloorToInt(model.HP)}/{Mathf.FloorToInt(model.MaxHP)}";
        AG_text.text = $"{Mathf.FloorToInt(model.AG)}/{Mathf.FloorToInt(model.MaxAG)}";
        if (Shield_text != null)
            Shield_text.text = $"{model.ShieldPoints}/{model.MaxShieldPoints}";

        SlowFollow(HP_fillImage_Slow, model.HP / model.MaxHP);
        SlowFollow(SP_fillImage_Slow, model.SP / model.MaxSP);
        SlowFollow(AG_fillImage_Slow, model.AG / model.MaxAG);

        if (_atbDotSpawnRoot != null && model.ATBPoints != _lastATBPoints)
            SyncATBDots(model.ATBPoints);
    }

    void NormalFollow(Image img, float target)
    {
        if (img == null) return;
        img.DOKill();
        img.DOFillAmount(target, normalFollowDuration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    void SlowFollow(Image slow, float target)
    {
        if (slow == null) return;
        slow.DOKill();
        slow.DOFillAmount(target, slowFollowDuration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    void SyncATBDots(int targetCount)
    {
        var pool = GameRoot.GetManager<ObjectPoolManager>();
        float s = _atbDotScale;

        while (_atbDots.Count < targetCount)
        {
            var dot = pool.GetInstance(E_PoolType.ATBDot_ATB点数);
            if (dot == null) break;

            dot.transform.SetParent(_atbDotSpawnRoot);
            dot.transform.localPosition = (_atbDots.Count + 1) * _atbDotOffset;
            dot.transform.localScale = Vector3.zero;

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(dot.transform.DOScale(s * 1.35f, 0.18f).SetEase(Ease.OutQuad));
            seq.Append(dot.transform.DOScale(s, 0.12f).SetEase(Ease.InQuad));
            _atbDots.Add(dot);
        }

        while (_atbDots.Count > targetCount && _atbDots.Count > 0)
        {
            var last = _atbDots[_atbDots.Count - 1];
            _atbDots.RemoveAt(_atbDots.Count - 1);

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(last.transform.DOScale(s * 1.2f, 0.06f).SetEase(Ease.OutQuad));
            seq.Append(last.transform.DOScale(0f, 0.18f).SetEase(Ease.InQuad));
            seq.OnComplete(() => pool.ReturnPool(E_PoolType.ATBDot_ATB点数, last));
        }

        _lastATBPoints = targetCount;
    }

    /// <summary>
    /// 同步弱点图标 —— 根据当前弱点列表重建 weaknessIconContent 下的所有图标
    /// </summary>
    public void SyncWeaknessIcons(List<E_WeaknessType> weaknesses, WeaknessIconConfigSO iconConfig)
    {
        if (weaknessIconContent == null) return;

        // 清除现有图标
        for (int i = weaknessIconContent.childCount - 1; i >= 0; i--)
        {
            var child = weaknessIconContent.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        if (weaknesses == null || iconConfig == null) return;

        var prefab = ResourcesLoader.FindWeaknessIconObj();
        if (prefab == null)
        {
            Debug.LogWarning("[Battle_Viewer] 未找到 WeaknessIcon 预制件，路径: Prefab/BattleArea/CharacterBattle/WeaknessIcon");
            return;
        }

        foreach (var w in weaknesses)
        {
            var iconGo = Instantiate(prefab, weaknessIconContent);
            var iconUI = iconGo.GetComponent<IconUI>();
            var sprite = iconConfig.GetSprite(w);
            if (iconUI != null && sprite != null)
                iconUI.SetIcon(sprite);
        }
    }

    // ── 力竭显示 ──
    System.Collections.IEnumerator _breakProgressRoutine;

    /// <summary>力竭开始：淡入 + 启动进度条倒计时</summary>
    public void OnBreakStarted(float duration)
    {
        if (breakCanvasGroup != null)
        {
            breakCanvasGroup.DOKill();
            breakCanvasGroup.alpha = 0f;
            breakCanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
        }
        if (breakProgressImage != null)
            breakProgressImage.fillAmount = 1f;

        if (_breakProgressRoutine != null) StopCoroutine(_breakProgressRoutine);
        _breakProgressRoutine = BreakProgressRoutine(duration);
        StartCoroutine(_breakProgressRoutine);
    }

    /// <summary>力竭结束：进度归零 + 淡出</summary>
    public void OnBreakEnded()
    {
        if (_breakProgressRoutine != null)
        {
            StopCoroutine(_breakProgressRoutine);
            _breakProgressRoutine = null;
        }
        if (breakProgressImage != null)
            breakProgressImage.fillAmount = 0f;
        if (breakCanvasGroup != null)
        {
            breakCanvasGroup.DOKill();
            breakCanvasGroup.DOFade(0f, 0.3f).SetUpdate(true);
        }
    }

    System.Collections.IEnumerator BreakProgressRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (breakProgressImage != null)
                breakProgressImage.fillAmount = 1f - elapsed / duration;
            yield return null;
        }
        if (breakProgressImage != null)
            breakProgressImage.fillAmount = 0f;
    }
}
