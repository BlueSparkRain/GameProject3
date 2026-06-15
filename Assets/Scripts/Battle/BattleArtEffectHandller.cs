using UnityEngine;
using DG.Tweening;
public class BattleArtEffectHandller : MonoBehaviour{
    [Header("受击红色面板")]
    public CanvasGroup flashRedBoard;
    [Header("动画曲线")]
    public AnimationCurve animCurve;

    [Header("受击震屏Trans")]
    public Transform shakeTran;

    float maxShakeVal=2.0f;
    float minShakeVal=0.5f;

    float minDamage = 0.0f;
    float maxDamage = 300.0f;

    BattleDamageHandler damageHandler;
    public void InitArtEffectHandler(BattleDamageHandler damageHandler){
        EventCenter.AddEventListener<BattleDamageHandler,float>(E_EventType.Get_Damage, HandleSelfDamageArtEffect);
        this.damageHandler = damageHandler;
    }
    private void OnDestroy(){
        EventCenter.RemoveEventListener<BattleDamageHandler,float>(E_EventType.Get_Damage, HandleSelfDamageArtEffect);
    }

    void HandleSelfDamageArtEffect(BattleDamageHandler damageHandler,float getDamage) {

        if (damageHandler == this.damageHandler) {
            float colorRate = (getDamage - minDamage) / (maxDamage - minDamage);
            float shakeRate = (getDamage - minDamage) / (maxDamage - minDamage);
            DebugManager.Log(EDebugCategory.BattleDamage,$"[BattleArtEffectHandller]-{this.damageHandler.name},动画强度：{colorRate}");
            DamageFlash(getDamage,colorRate);
            DamageShake(getDamage,shakeRate);
        }
    }
    void DamageFlash(float getDamage, float intensity)
    {
        flashRedBoard.DOKill(); // 打断之前的动画，防止叠加变鬼畜

        // 重点：闪烁必须【极快】才有打击感！时间不能长
        float fadeIn = Mathf.Clamp(intensity * 0.1f, 0.05f, 0.15f);  // 瞬间变红
        float fadeOut = Mathf.Clamp(intensity * 0.2f, 0.1f, 0.3f);  // 快速淡出
        float alpha = Mathf.Clamp(intensity * 0.9f, 0.5f, 1f);      // 红屏透明度

        // 瞬间闪红 → 快速消失 → 打击感爆炸
        flashRedBoard.DOFade(alpha, fadeIn)
            .SetEase(Ease.OutExpo)  // 瞬间顶满，最爽
            .OnComplete(() =>
                flashRedBoard.DOFade(0, fadeOut)
                    .SetEase(Ease.InExpo)
            );
    }
    void DamageShake(float getDamage, float intensity)
    {
        float duration = Mathf.Clamp(intensity * 0.25f, 0.2f, 0.6f);

        // 兼容所有DOTween版本 · 无报错 · 打击感拉满
        shakeTran.DOShakeRotation(
            duration,                    // 时长
            90 * Mathf.Max(intensity, 0.6f), // 震动强度（角度）
            20                               // 震动次数（拉满）
        )
        .SetEase(Ease.OutExpo)  // 重击感核心曲线
        .SetUpdate(true);       // 不受游戏暂停影响
    }
}
