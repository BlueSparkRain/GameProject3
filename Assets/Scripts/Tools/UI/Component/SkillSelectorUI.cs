using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectorUI : MonoBehaviour
{
    [Header("技能Image")]
    public Image skillImage;

    [Header("技能名称")]
    public TMP_Text skillNameText;

    [Header("刷新按钮")]
    public Button refreshButton;


    /// <summary>
    /// 当前持有的技能数据
    /// </summary>
    SkillPropertySO skillData;
    //随机一种技能数据,由Panel传入分配
    
   

    /// <summary>
    ///根据传入的数据来初始化选择器
    /// </summary>
    public void InitSelf(SkillPropertySO skillData) {

        this.skillData = skillData;
        skillImage.sprite=skillData.skill_Sprite;
        skillNameText.text=skillData.skill_Name;
    }

    void OnClickRefreshButton()
    {


    }



    // Start is called before the first frame update
    void Start()
    {
        refreshButton.onClick.AddListener(OnClickRefreshButton);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
