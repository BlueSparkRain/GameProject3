using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelBox : MonoBehaviour
{
    //每次获取经验都会更新UI
    [Header("经验条")]
    public Image expFillbar;

    [Header("经验Circle条")]
    public Image expCircleFillbar;

    [Header("经验值文本")]
    public TMP_Text expText;

    [Header("等级文本")]
    public TMP_Text levelText;

    /// <summary>
    /// 基础经验 + (当前等级 - 1) × 每级增量（基础：200；增量：100）
    /// </summary>

    void Start()
    {
        expFillbar.fillAmount = 0;
    }

    void Update()
    {

    }
}
