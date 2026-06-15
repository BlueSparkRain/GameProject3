using Core;
using System.Collections;
using UnityEngine;

/// <summary>
/// 释放多次攻击的技能装饰器
/// </summary>
public class MultiTime_SkillDecorator : SkillDecorator
{
    /// <summary>
    /// 额外效果的释放次数
    /// </summary>
    int excuteTime = 1;

    /// <summary>
    /// 每次效果的间隔
    /// </summary>
    float interval = 0;

    public MultiTime_SkillDecorator(ISkill skill, int excuteTime, float interval) : base(skill)
    {
        this.excuteTime = excuteTime;
        this.interval = interval;
    }
    IEnumerator DoMultiEffect(IBattlable self, IBattlable target)
    {
        WaitForSeconds delay = new WaitForSeconds(interval);
        for (int i = 0; i < excuteTime; i++)
        {
            // 每次执行前验活：底层Unity对象可能已在场景卸载时被销毁
            if ((self as Object) == null || (target as Object) == null)
            {
                DebugManager.LogWarning(EDebugCategory.SkillExecution,"[MultiTime] 目标已销毁，中断多段效果");
                yield break;
            }
            base.Excute(self, target);
            yield return delay;
        }
    }
    public override void Excute(IBattlable self, IBattlable target)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"[MultiTime_SkillDecorator]>>多次技能效果");
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DoMultiEffect(self, target), self as UnityEngine.Object);
    }
}
