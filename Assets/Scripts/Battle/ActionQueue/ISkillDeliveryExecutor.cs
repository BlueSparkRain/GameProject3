using System.Collections;

/// <summary>
/// 技能投递执行器接口 — 每种 SkillDeliveryType 对应一个实现。
/// 返回 IEnumerator 控制演出时序，协程结束时即表示结算时机已到。
/// </summary>
public interface ISkillDeliveryExecutor
{
    /// <summary>播放投递演出。协程结束 = 该行动可以结算。</summary>
    IEnumerator Deliver(BattleAction action);
}
