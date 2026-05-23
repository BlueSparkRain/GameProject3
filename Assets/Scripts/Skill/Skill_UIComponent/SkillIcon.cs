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
    public SkillData SkillData => skillData;

    //管理技能释放后的模型的变化，如果没蓝图标变蓝，无法释放技能

    /// <summary>
    /// 按钮&技能被冻结！
    /// </summary>
    bool isFreezzing;

    /// <summary>
    /// 关联的BattleSkill
    /// </summary>
    SkillBase currentSkill;

    /// <summary>
    /// 战斗中无法拖拽，但是可以在地图界面中拖拽换槽位
    /// </summary>
    bool canDrag;

    /// <summary>
    /// 现在根据SkillData（可能是保存的数据）中来读取并加载一个Icon
    /// </summary>
    /// <param name="_skilldata"></param>
    public void InitSkillIcon(SkillData _skilldata,SkillSlot slot, bool _canDrag)
    {
        skillData = _skilldata;
        canDrag = _canDrag;
        //EventCenter.EventTrigger(E_EventType.Battle_LoadASkill,skillData.skill_ID);
        //替换图标
        skillImage.sprite = SkillData.skill_Sprite;
        skillTimer = skillData.skill_CoolDown;
        GetComponent<SlotSwaperHandler>().InitSlot(slot);
    }

    /// <summary>
    /// 将一个具体的BattleSkill关联给此Icon
    /// </summary>
    /// <param name="skill"></param>
    public void InitBattleSkill(SkillBase skill){
        currentSkill = skill;
    }

    /// <summary>
    /// 技能被禁用
    /// </summary>
    public void FreezeIcon(bool freeze){
        isFreezzing = freeze;
        //可能出现一个锁链的UI_Image
    }

    void CheckSkillCanExcute(float currentSP)
    {
        hasNoSP = (skillData.skill_sp_cost < currentSP ? hasNoSP = false : hasNoSP = true);
        if (hasNoSP)
            skillImage.color = Color.blue;
        else
            skillImage.color = Color.white;
    }

    /// <summary>
    /// (只适用与背包技能)根据技能数据来更新图标，并检测触发对应的技能
    /// </summary>
    public void IconCycleUpdate(float currentSP)
    {
        if (isFreezzing || currentSkill == null){
            Debug.Log("本技能暂时冻结中");
            return;
        }

        if (skillTimer > -0.01){
            skillCoolDownImage.fillAmount = skillTimer / skillData.skill_CoolDown;
            skillTimer -= Time.deltaTime;
        }
        else{
            //根据是否有蓝来通知Skiller释放对应的技能
            CheckSkillCanExcute(currentSP);
            if (hasNoSP)
                return;
            
            //冷却好且具备蓝量
            //如果使用加强版技能
            currentSkill.SkillExcute(E_SkillLevel.基础版本);
            skillTimer = skillData.skill_CoolDown;
            //技能释放者消耗蓝量
            EventCenter.EventTrigger(E_EventType.SkillExcute,skillData.skill_sp_cost);
        }
    }
    /// <summary>
    /// 技能被打断：计时器归零并冻结，当角色力竭状态恢复后再重新计时
    /// </summary>
    public void SkillBreak() {
        isFreezzing = true;
        skillTimer = skillData.skill_CoolDown;
    }

}

