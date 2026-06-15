using Core;
using System.Collections;
using UnityEngine;

public class DelayTrigger_SkillDecorator : SkillDecorator
{
    public float duration;
    WaitForSeconds delay;
    public DelayTrigger_SkillDecorator(ISkill skill, float _duration) : base(skill)
    {
        duration = _duration;
        delay = new WaitForSeconds(duration);
    }
    IEnumerator DelayTrigger(IBattlable self, IBattlable target)
    {
        yield return delay;
        // 延迟结束后验活：底层Unity对象可能已在场景卸载时被销毁
        if ((self as Object) == null || (target as Object) == null)
        {
            DebugManager.LogWarning(EDebugCategory.SkillExecution,"[DelayTrigger] 目标已销毁，跳过延迟效果执行");
            yield break;
        }
        base.Excute(self, target);
    }

    public override void Excute(IBattlable self, IBattlable target)
    {
        DebugManager.Log(EDebugCategory.SkillExecution,"[DelayTrigger_SkillDecorator]>>延迟触发技能效果");
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DelayTrigger(self, target), self as UnityEngine.Object);
    }
}
