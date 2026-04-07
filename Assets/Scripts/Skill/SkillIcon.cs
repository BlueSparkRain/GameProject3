using AmplifyShaderEditor;
using Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 每一枚技能图标(管理一个技能的冷却等基础数据成长或读取)
/// </summary>
public class SkillIcon : MonoBehaviour
{
    #region UI组件引用
    [Header("技能图标Image")]
    public Image skillImage;
    [Header("技能冷却Image")]
    public Image skillCoolDownImage;
    #endregion

    #region 技能状态字段
    [Header("技能计时器")]
    public float skillTimer = 0;
    public bool hasNoSP;
    #endregion

    //当前的技能数据
    private SkillData skillData;
    public SkillData SkillData=>skillData;

    //从运行时SkillData中获取的当前技能基础数据
    #region 技能基础属性
    //[Header("技能ID")]
    //public int skill_ID;
    //[Header("[当前]技能图标")]
    //public Sprite skill_Sprite;
    //[Header("技能名称")]
    //public string skill_Name;
    //[Header("技能描述")]
    //[Multiline]
    //public string skill_Description;
    //[Header("[当前]技能冷却")]
    //public float skill_CoolDown;
    //[Header("[当前]技能法力消耗")]
    //public float skill_sp_cost;
    //[Header("[当前]技能怒气增长")]
    //public float skill_ang_grow;
    //[Header("[当前]技能的目标类型")]
    //public E_SkillTargetType skill_targetType;
    #endregion

    //管理技能释放后的模型的变化，如果没蓝图标变蓝，无法释放技能


    ISkill currentSkill;

 
    bool canDrag;//战斗中无法拖拽，但是可以在地图界面中拖拽换槽位
    /// <summary>
    /// 现在根据SkillData（可能是保存的数据）中来读取并加载一个Icon
    /// </summary>
    /// <param name="_skilldata"></param>
    public void InitSkillIcon(SkillData _skilldata,bool _canDrag)
    {
        skillData = _skilldata;
        canDrag = _canDrag;
        EventCenter.EventTrigger(E_EventType.Battle_LoadASkill,skillData.skill_ID);
        
        //在战斗过程中，技能数据可能会有成长或修改，应该再次保存
        //skill_ID = _skilldata.skill_ID;
        //skill_Sprite = _skilldata.skill_Sprite;
        //skill_Name= _skilldata.skill_Name;
        //skill_Description= _skilldata.skill_Description;
        //skill_sp_cost= _skilldata.skill_sp_cost;
        //skill_ang_grow= _skilldata.skill_ang_grow;
        //skill_targetType = _skilldata.skill_targetType;
        //skill_CoolDown = _skilldata.skill_CoolDown;
        //skillImage.sprite = _skilldata.skill_Sprite;
        //skillTimer = skill_CoolDown;
        //currentSkill=BattleSkillFactory.Create(skillData.skill_ID);
    }
 
    public void CheckSkillCanExcute(float currentSP)
    {
        hasNoSP = (skillData.skill_sp_cost < currentSP ? hasNoSP = false : hasNoSP = true);
        if (hasNoSP)
            skillImage.color = Color.blue;
        else
            skillImage.color = Color.white;
    }

    /// <summary>
    /// 根据技能数据来更新图标，并检测触发对应的技能
    /// </summary>
    public void IconCycleUpdate()
    {
        if (skillTimer > -0.01)
        {
            skillCoolDownImage.fillAmount = skillTimer/skillData.skill_CoolDown;
            skillTimer -= Time.deltaTime;
        }
        else
        {
            //根据是否有蓝来通知Skiller释放对应的技能
            if (hasNoSP)
            {
                return;
            }
            skillTimer =skillData.skill_CoolDown;
        }
    }
}

