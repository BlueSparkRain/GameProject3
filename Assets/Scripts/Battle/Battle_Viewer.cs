using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗过程中的人物卡牌
/// </summary>
public class Battle_Viewer : MonoBehaviour
{
    [Header("生命条")]
    public Image HP_fillImage;
    [Header("法力条")]
    public Image SP_fillImage;
    [Header("怒气条")]
    public Image AG_fillImage;

    [Header("生命值文本")]
    public TMP_Text HP_text;
    [Header("法力值文本")]
    public TMP_Text SP_text;
    [Header("怒气值文本")]
    public TMP_Text AG_text;
    [Header("ATB点数文本")]
    public TMP_Text ATB_point_Text;

    public void UpdataUI(Battle_Model model)
    {
        HP_fillImage.fillAmount = model.HP / model.MaxHP;
        SP_fillImage.fillAmount = model.SP / model.MaxSP;
        AG_fillImage.fillAmount = model.AG / model.MaxAG;
        SP_text.text = $"{Mathf.FloorToInt(model.SP)}/{Mathf.FloorToInt(model.MaxSP)}";
        HP_text.text = $"{Mathf.FloorToInt(model.HP)}/{Mathf.FloorToInt(model.MaxHP)}";
        AG_text.text = $"{Mathf.FloorToInt(model.AG)}/{Mathf.FloorToInt(model.MaxAG)}";
        ATB_point_Text.text = $"{model.ATBPoints}/{model.MaxATBPoints}";
    }
}
