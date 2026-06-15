using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 范围爆发：目标位置直接爆发 → 扩大 + 渐隐 → 结算。
/// UI 友好，无预警圈。适用于领域/风暴/大魔法等范围技。
/// </summary>
public class AOEBurstDelivery : ISkillDeliveryExecutor
{
    readonly SkillVfxDirectorManager _mgr;

    public AOEBurstDelivery(SkillVfxDirectorManager mgr) => _mgr = mgr;

    public IEnumerator Deliver(BattleAction action)
    {
        if (action.Target == null)
        {
            yield return new WaitForSeconds(0.3f);
            yield break;
        }

        var pos = _mgr.GetBattleUIPosition(action.Target);

        var burst = _mgr.SpawnVfx(
            _mgr.GetVfxPrefab(SkillDeliveryType.AOE_Burst),
            pos, Quaternion.identity);
        if (burst == null) yield break;

        _mgr.TweenScale(burst, Vector3.one * 0.3f, 0f);
        _mgr.TweenScale(burst, Vector3.one * 2f, 0.35f).SetEase(Ease.OutQuad);

        _mgr.TweenFadeOut(burst, 0.35f);

        yield return new WaitForSeconds(0.15f);
    }
}
