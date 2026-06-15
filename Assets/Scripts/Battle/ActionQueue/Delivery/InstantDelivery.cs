using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 立即结算：极短停顿（0.12s），在目标位置播放冲击缩放 + 渐隐。
/// </summary>
public class InstantDelivery : ISkillDeliveryExecutor
{
    readonly SkillVfxDirectorManager _mgr;

    public InstantDelivery(SkillVfxDirectorManager mgr) => _mgr = mgr;

    public IEnumerator Deliver(BattleAction action)
    {
        yield return new WaitForSeconds(0.12f);

        var target = action.Target;
        if (target == null) yield break;

        var pos = _mgr.GetBattleUIPosition(target);
        var vfx = _mgr.SpawnVfx(
            _mgr.GetVfxPrefab(SkillDeliveryType.Instant),
            pos, Quaternion.identity);
        if (vfx == null) yield break;

        _mgr.TweenPunchScale(vfx, Vector3.one * 0.5f, 0.2f, 1, 0.5f);
        _mgr.TweenFadeOut(vfx, 0.3f);
    }
}
