using Core;
using System.Collections;
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
    public bool UseLevelBox=true;
    //每次获取经验都会更新UI
    [Header("经验条")]
    public Image expFillbar;
    public Image expFillbarWhite;


    [Header("当前经验值文本")]
    public TMP_Text currentExpText;

    [Header("下次升级所需经验值文本")]
    public TMP_Text nextExpGoalText;

    void Start(){
        if (UseLevelBox){
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

    public void UpdateMapPlayerIconUI(int currenLevel,float currentEXP,float levelGoal,bool skip=false) {
        if (UseLevelBox){
            currentExpText.text = currentEXP.ToString();
            nextExpGoalText.text = $"/{levelGoal}";
        }
        levelText.text = currenLevel.ToString();
        float  targetfillamount= currentEXP/levelGoal;
        StartCoroutine(UpdateAnim(targetfillamount,skip));
    }

    IEnumerator UpdateAnim(float targetfillamount , bool skip) {
        if (skip){
            //升级需要播放动画
            if (UseLevelBox){
                StartCoroutine(TweenHelper.MakeLerp(expFillbarWhite.fillAmount, 1, 0.05f, val => expFillbarWhite.fillAmount = val));
                StartCoroutine(TweenHelper.MakeLerp(expFillbar.fillAmount, 1, 0.1f, val => expFillbar.fillAmount = val));
            }
            StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar.fillAmount, 1, 0.1f, val => expCircleFillbar.fillAmount = val));
            if (expCircleFillbar_Fast != null){
                StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar_Fast.fillAmount, 1, 0.1f, val => expCircleFillbar_Fast.fillAmount = val+0.02f));
            }
            yield return new WaitForSeconds(0.2f);

            if (UseLevelBox){
                expFillbarWhite.fillAmount = 0;
                expFillbar.fillAmount = 0;
                StartCoroutine(TweenHelper.MakeLerp(expFillbarWhite.fillAmount, targetfillamount, 0.05f, val => expFillbarWhite.fillAmount = val));
                StartCoroutine(TweenHelper.MakeLerp(expFillbar.fillAmount, targetfillamount, 0.1f, val => expFillbar.fillAmount = val));
            }
            expCircleFillbar.fillAmount = 0;
            StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar.fillAmount, targetfillamount, 0.1f, val => expCircleFillbar.fillAmount = val));
            if (expCircleFillbar_Fast != null){
                expCircleFillbar_Fast.fillAmount = 0;
                StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar_Fast.fillAmount, targetfillamount, 0.1f, val => expCircleFillbar_Fast.fillAmount = val+0.02f));
            }
        }
        else{
            if (UseLevelBox){
                StartCoroutine(TweenHelper.MakeLerp(expFillbarWhite.fillAmount, targetfillamount, 0.1f, val => expFillbarWhite.fillAmount = val));
                StartCoroutine(TweenHelper.MakeLerp(expFillbar.fillAmount, targetfillamount, 0.2f, val => expFillbar.fillAmount = val));
            }
            StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar.fillAmount, targetfillamount, 0.2f, val => expCircleFillbar.fillAmount = val));
            if (expCircleFillbar_Fast != null){
                StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar_Fast.fillAmount, targetfillamount, 0.2f, val => expCircleFillbar_Fast.fillAmount = val+0.02f));
            }
        }
    }
}
