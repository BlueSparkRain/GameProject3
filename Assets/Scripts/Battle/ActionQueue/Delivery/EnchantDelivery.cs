using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 附魔驻留：施法者位置生成持续特效 → 脉冲/旋转 → 长时间后自动销毁。
/// 不阻塞队列（立即返回），适用于武器附魔、护盾加固等 BUFF 类技能。
/// </summary>
public class EnchantDelivery : ISkillDeliveryExecutor
{

    readonly SkillVfxDirectorManager _mgr;

    public EnchantDelivery(SkillVfxDirectorManager mgr) => _mgr = mgr;

    public IEnumerator Deliver(BattleAction action)
    {
        var pos = _mgr.GetBattleUIPosition(action.Caster);

        var vfx = _mgr.SpawnVfx(
            _mgr.GetVfxPrefab(SkillDeliveryType.Enchant),
            pos, Quaternion.identity);
        if (vfx == null) yield break;

        _mgr.TweenScale(vfx, Vector3.one * 0.2f, 0f);

        // 入场动画：放大 + 渐显
        float appearTime = 0.3f;
        _mgr.TweenScale(vfx, Vector3.one, appearTime).SetEase(Ease.OutBack);

        var graphic = vfx.GetComponent<Graphic>();
        if (graphic != null)
        {
            var c = graphic.color;
            graphic.color = new Color(c.r, c.g, c.b, 0);
            graphic.DOColor(c, appearTime);
        }

        // 驻留脉冲
        vfx.transform.DOScale(1.1f, 0.6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(appearTime);

        // 长时间后自动销毁（8 秒）
        DOVirtual.DelayedCall(8f, () =>
        {
            _mgr.TweenFadeOut(vfx, 0.4f);
        });

        // 不阻塞队列
        yield return new WaitForSeconds(0.15f);
    }
}
