using Core;
using System.Collections;
using UnityEngine;

/// <summary>
/// �ͷŶ�ι����ļ���װ����
/// </summary>
public class MultiTime_SkillDecorator : SkillDecorator
{
    /// <summary>
    /// ����������ͷŶ���
    /// </summary>
    int excuteTime = 1;

    /// <summary>
    /// ÿ����Ч�ļ��
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
                Debug.LogWarning("[MultiTime] 目标已销毁，中断多段效果");
                yield break;
            }
            base.Excute(self, target);
            yield return delay;
        }
    }
    public override void Excute(IBattlable self, IBattlable target)
    {
        UnityEngine.Debug.Log("[MultiTime_SkillDecorator]>>��μ���Ч��");
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(DoMultiEffect(self, target), self as UnityEngine.Object);
    }
}
