using UnityEngine;

/// <summary>
/// 弱点击中处理——读取对应角色弱点配置、维护弱点列表、处理弱点命中时的护盾扣除。
/// 重构原因：原本散落在 Attack_Skill 中的"弱点判定→伤害x2+破盾"逻辑。
/// </summary>
public class BattleWeaknessHandler : MonoBehaviour{
    IBattlable self;
    /// <summary>
    /// 加载弱点配置并初始化到战斗单位上。
    /// </summary>
    public void InitWeaknessHandle(IBattlable _self, CharacterWeaknessConfigSO config){
        self = _self;

        if (config != null){
            foreach (var w in config.weaknesses)
                self.AddWeakness(w);
            Debug.Log($"[BattleWeaknessHandler] {config.characterType} 初始化弱点:{self.weaknesses.Count}个");
        }
    }

    /// <summary>
    /// 检查当前战斗单位是否具有指定的弱点类型。
    /// </summary>
    public bool HasWeakness(E_WeaknessType type){
        return self.GetWeakAttack(type);
    }

    /// <summary>
    /// 为当前战斗单位添加一个弱点（战斗中动态增删）。
    /// </summary>
    public void AddWeakness(E_WeaknessType w){
        self.AddWeakness(w);
    }

    /// <summary>
    /// 从当前战斗单位移除一个弱点。
    /// </summary>
    public void RemoveWeakness(E_WeaknessType w){
        self.RemoveWeakness(w);
    }

    /// <summary>
    /// 处理一次弱点类型攻击命中的倍率逻辑：
    /// 命中弱点时，扣除1点护盾，返回2倍伤害倍率；
    /// 未命中弱点时，返回1倍。
    /// </summary>
    /// <param name="attackType">攻击的弱点类型</param>
    /// <returns>伤害倍率，弱点=2，普通=1</returns>
    public float ProcessWeaknessHit(E_WeaknessType attackType){
        if (!self.GetWeakAttack(attackType))
            return 1f;

        self.battleDamageHandler.DoModelValue(E_BattleModelType.ShieldPoints, -1);
        return 2f;
    }
}
