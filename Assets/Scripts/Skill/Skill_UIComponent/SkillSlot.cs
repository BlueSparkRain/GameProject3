using UnityEngine;
using DG.Tweening;
//一个技能槽位，技能图标会被放在这个槽位里
/// <summary>
/// 负责管理本槽内的技能图标/交换逻辑
/// </summary>
public class SkillSlot : MonoBehaviour{
    [Header("当前Icon")]
    public SkillIcon icon;
    public void SetIcon(SkillIcon icon) {
        this.icon = icon;
    }

    /// <summary>
    ///检测到鼠标拖拽着SKillIcon靠近自身，产生图标交换行为
    ///将当前的icon 
    /// </summary>
    /// <param name="targetSlot"></param>
    public void SwapIcon(SkillSlot targetSlot)
    {
        if (!GetComponentInChildren<SkillIcon>()) icon = null;
        else icon=transform.GetComponentInChildren<SkillIcon>();
        if (icon == null)return;
        icon.GetComponent<SlotSwaperHandler>().MoveToTargetSlot(targetSlot);
        DebugManager.Log(EDebugCategory.SkillUI, "交换成功");
    }
}
