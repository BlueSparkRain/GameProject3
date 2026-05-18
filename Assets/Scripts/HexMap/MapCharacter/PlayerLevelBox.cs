using Core;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelBox : MonoBehaviour
{
    //每次获取经验都会更新UI
    [Header("经验条")]
    public Image expFillbar;
    public Image expFillbarWhite;

    [Header("经验Circle条")]
    public Image expCircleFillbar;

    [Header("当前经验值文本")]
    public TMP_Text currentExpText;

    [Header("据升级所需总经验值文本")]
    public TMP_Text nextExpGoalText;

    [Header("等级文本")]
    public TMP_Text levelText;

    void Start()
    {
        expFillbar.fillAmount = 0;
        expFillbarWhite.fillAmount = 0;
        expCircleFillbar.fillAmount = 0;
    }

    public void UpdateMapPlayerIconUI(int currenLevel,float currentEXP,float levelGoal,bool skip=false) {
        levelText.text=currenLevel.ToString();
        currentExpText.text=currentEXP.ToString(); 
        nextExpGoalText.text=$"/{levelGoal}";

        float  targetfillamount= currentEXP/levelGoal;
        StartCoroutine(UpdateAnim(targetfillamount,skip));
    }

    IEnumerator UpdateAnim(float targetfillamount , bool skip) { 
        if (skip)
        {
            //跳级需要增加动画

            StartCoroutine(TweenHelper.MakeLerp(expFillbarWhite.fillAmount, 1, 0.05f, val => expFillbarWhite.fillAmount = val));
            StartCoroutine(TweenHelper.MakeLerp(expFillbar.fillAmount, 1, 0.15f, val => expFillbar.fillAmount = val));
            StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar.fillAmount, 1, 0.15f, val => expCircleFillbar.fillAmount = val));
            yield return new WaitForSeconds(0.2f);
            expFillbarWhite.fillAmount = 0;
            expFillbar.fillAmount = 0;
            expCircleFillbar.fillAmount = 0;
            StartCoroutine(TweenHelper.MakeLerp(expFillbarWhite.fillAmount, targetfillamount, 0.05f, val => expFillbarWhite.fillAmount = val));
            StartCoroutine(TweenHelper.MakeLerp(expFillbar.fillAmount, targetfillamount, 0.15f, val => expFillbar.fillAmount = val));
            StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar.fillAmount, targetfillamount, 0.15f, val => expCircleFillbar.fillAmount = val));

        }
        else
        {

            StartCoroutine(TweenHelper.MakeLerp(expFillbarWhite.fillAmount, targetfillamount, 0.1f, val => expFillbarWhite.fillAmount = val));
            StartCoroutine(TweenHelper.MakeLerp(expFillbar.fillAmount, targetfillamount, 0.3f, val => expFillbar.fillAmount = val));
            StartCoroutine(TweenHelper.MakeLerp(expCircleFillbar.fillAmount, targetfillamount, 0.3f, val => expCircleFillbar.fillAmount = val));
        }
    }
}
