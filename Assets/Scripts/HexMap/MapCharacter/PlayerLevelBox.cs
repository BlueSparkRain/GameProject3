using Core;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelBox : MonoBehaviour
{

    [Header("经验Circle条")]
    public Image expCircleFillbar;
    [Header("经验Circle条_Fast0.2")]
    public Image expCircleFillbar_Fast;

    [Header("等级文本")]
    public TMP_Text levelText;

    [Header("不使用经验Box//下方字段都需使用")]
    public bool UseLevelBox = true;
    //每次获取经验都会更新UI
    [Header("经验条")]
    public Image expFillbar;
    public Image expFillbarWhite;

    float transSpeed=2;


    [Header("当前经验值文本")]
    public TMP_Text currentExpText;

    [Header("下次升级所需经验值文本")]
    public TMP_Text nextExpGoalText;

    void Start(){
        if (UseLevelBox)
        {
            expFillbar.fillAmount = 0;
            expFillbarWhite.fillAmount = 0;
        }
        expCircleFillbar.fillAmount = 0;

        if (expCircleFillbar_Fast != null)
            expCircleFillbar_Fast.fillAmount = 0;

        EventCenter.AddEventListener<EXPUpdateInfo>(E_EventType.AdjustEXP, OnEXPAdjusted);
    }

    void OnDestroy()
    {
        EventCenter.RemoveEventListener<EXPUpdateInfo>(E_EventType.AdjustEXP, OnEXPAdjusted);
    }

    void OnEXPAdjusted(EXPUpdateInfo info)
    {
        UpdateMapPlayerIconUI(info.currentLevel, info.currentEXP, info.levelGoalEXP, info.skip);
    }

    public void UpdateMapPlayerIconUI(int currenLevel, float currentEXP, float levelGoal, bool skip = false)
    {
        if (UseLevelBox)
        {
            currentExpText.text = currentEXP.ToString();
            nextExpGoalText.text = $"/{levelGoal}";
        }
        levelText.text = currenLevel.ToString();
        float targetFillAmount = currentEXP / levelGoal;
        StartCoroutine(UpdateAnim(targetFillAmount, skip));
    }

    IEnumerator UpdateAnim(float targetFillAmount, bool skip)
    {
        KillCircleTweens();

        if (skip)
        {
            // Phase 1: 填满 → 升级动画
            if (UseLevelBox)
            {
                expFillbarWhite.DOFillAmount(1f, 0.08f * 1.0f/transSpeed).SetEase(Ease.InOutCubic);
                expFillbar.DOFillAmount(1f, 0.15f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
            }
            if (expCircleFillbar_Fast != null)
                expCircleFillbar_Fast.DOFillAmount(1.02f, 0.12f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
            expCircleFillbar.DOFillAmount(1f, 0.15f * 1.0f / transSpeed).SetEase(Ease.InOutCubic).SetDelay(0.05f);

            yield return new WaitForSeconds(0.2f);

            // 重置
            KillCircleTweens();
            if (UseLevelBox)
            {
                expFillbarWhite.fillAmount = 0;
                expFillbar.fillAmount = 0;
            }
            expCircleFillbar.fillAmount = 0;
            if (expCircleFillbar_Fast != null)
                expCircleFillbar_Fast.fillAmount = 0;

            // Phase 2: 从0填充到当前经验占比
            if (UseLevelBox)
            {
                expFillbarWhite.DOFillAmount(targetFillAmount, 0.08f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
                expFillbar.DOFillAmount(targetFillAmount, 0.15f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
            }
            if (expCircleFillbar_Fast != null)
                expCircleFillbar_Fast.DOFillAmount(targetFillAmount + 0.02f, 0.12f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
            expCircleFillbar.DOFillAmount(targetFillAmount, 0.15f * 1.0f / transSpeed).SetEase(Ease.InOutCubic).SetDelay(0.05f);
        }
        else
        {
            if (UseLevelBox)
            {
                expFillbarWhite.DOFillAmount(targetFillAmount, 0.15f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
                expFillbar.DOFillAmount(targetFillAmount, 0.3f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
            }
            if (expCircleFillbar_Fast != null)
                expCircleFillbar_Fast.DOFillAmount(targetFillAmount + 0.02f, 0.25f * 1.0f / transSpeed).SetEase(Ease.InOutCubic);
            expCircleFillbar.DOFillAmount(targetFillAmount, 0.3f * 1.0f / transSpeed).SetEase(Ease.InOutCubic).SetDelay(0.05f);
        }
    }

    void KillCircleTweens()
    {
        expCircleFillbar?.DOKill();
        if (expCircleFillbar_Fast != null)
            expCircleFillbar_Fast.DOKill();
        if (UseLevelBox)
        {
            expFillbar?.DOKill();
            expFillbarWhite?.DOKill();
        }
    }
}
