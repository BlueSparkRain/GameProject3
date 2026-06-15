using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 自身增益：施法者处浮现光环 → 上升 + 放大 + 渐隐 → 等待结算。
/// </summary>
public class SelfBuffDelivery : ISkillDeliveryExecutor
{
    readonly SkillVfxDirectorManager _mgr;

    public SelfBuffDelivery(SkillVfxDirectorManager mgr) => _mgr = mgr;

    public IEnumerator Deliver(BattleAction action)
    {
        var pos = _mgr.GetBattleUIPosition(action.Caster);

        var vfx = _mgr.SpawnVfx(
            _mgr.GetVfxPrefab(SkillDeliveryType.SelfBuff),
            pos, Quaternion.identity);
        if (vfx == null)
        {
            yield return new WaitForSeconds(0.25f);
            yield break;
        }

        _mgr.TweenScale(vfx, Vector3.one * 0.2f, 0f);

        // 上升偏移：UI 用 _mgr.UiMoveOffsetY 像素，3D 用世界单位
        float riseOffset = vfx.GetComponent<RectTransform>() != null
            ? _mgr.UiMoveOffsetY
            : 2f;
        var targetPos = pos + new Vector3(0, riseOffset, 0);

        float duration = 0.4f;
        var seq = DOTween.Sequence();
        seq.Join(_mgr.TweenMove(vfx, targetPos, duration).SetEase(Ease.OutCubic));
        seq.Join(_mgr.TweenScale(vfx, Vector3.one, duration).SetEase(Ease.OutBack));
        yield return seq.WaitForCompletion();

        _mgr.TweenFadeOut(vfx, duration * 0.7f);
        yield return new WaitForSeconds(0.05f);
    }
}
