using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 投射物：从施法者 BattleUI 位置飞行到目标 BattleUI 位置 → 命中冲击 → 等待结算。
/// </summary>
public class ProjectileDelivery : ISkillDeliveryExecutor
{
    readonly SkillVfxDirectorManager _mgr;

    public ProjectileDelivery(SkillVfxDirectorManager mgr) => _mgr = mgr;

    public IEnumerator Deliver(BattleAction action)
    {
        var casterPos = _mgr.GetBattleUIPosition(action.Caster);
        var targetPos = _mgr.GetBattleUIPosition(action.Target);

        if (action.Caster == null || action.Target == null)
        {
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        var vfx = _mgr.SpawnVfx(
            _mgr.GetVfxPrefab(SkillDeliveryType.Projectile),
            casterPos, Quaternion.identity);
        if (vfx == null)
        {
            yield return new WaitForSeconds(0.3f);
            yield break;
        }

        _mgr.TweenScale(vfx, Vector3.one * 0.3f, 0f);

        float flyTime = 0.45f;
        var seq = DOTween.Sequence();
        seq.Join(_mgr.TweenMove(vfx, targetPos, flyTime).SetEase(Ease.InQuad));
        seq.Join(_mgr.TweenScale(vfx, Vector3.one * 0.1f, flyTime * 0.5f).SetEase(Ease.OutSine));
        yield return seq.WaitForCompletion();

        // 命中冲击
        var impact = _mgr.SpawnVfx(null, targetPos, Quaternion.identity);
        if (impact != null)
        {
            _mgr.TweenPunchScale(impact, Vector3.one * 0.6f, 0.25f, 1, 0.5f);
            _mgr.TweenFadeOut(impact, 0.25f);
        }

        Object.Destroy(vfx);
        yield return new WaitForSeconds(0.05f);
    }
}
