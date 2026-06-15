using UnityEngine;

/// <summary>
/// 攻击回复效果——拥有者每次物理攻击后回复一定生命值，内置冷却
/// 事件型：通过 EventCenter 订阅 Do_PhyAttack，自维护计时器
/// 完全自包含，零外部修改
/// </summary>
public class EquipmentEffect_FirstStrikeBoost : IEquipmentEffect{
    public float healAmount;
    public float cooldownSeconds;
    object _ownerId;
    float _lastTriggerTime = float.MinValue;
    CharacterData _characterData;
    public EquipmentEffect_FirstStrikeBoost(float healAmount, float cooldownSeconds = 5f){
        this.healAmount = healAmount;
        this.cooldownSeconds = cooldownSeconds;
    }
    public void OnEquip(EquipmentEffectContext ctx){
        _ownerId = ctx.ownerId;
        _characterData = ctx.characterData;
        _lastTriggerTime = float.MinValue;
        EventCenter.AddEventListener<BattleBuffHandler>(E_EventType.Do_PhyAttack, OnOwnerPhyAttack);
    }
    public void OnUnequip()
    {
        EventCenter.RemoveEventListener<BattleBuffHandler>(E_EventType.Do_PhyAttack, OnOwnerPhyAttack);
        _ownerId = null;
    }
    void OnOwnerPhyAttack(BattleBuffHandler buffHandler){
        // 过滤：只响应本角色的攻击
        if (!IsOwner(buffHandler)) return;

        // CD 检查
        if (Time.time - _lastTriggerTime < cooldownSeconds) return;
        _lastTriggerTime = Time.time;

        // 回复生命
        var controller = buffHandler.GetComponent<BattleHandler>().MVCHandler.BattleController;
        controller.AdjustCharacterModelValue(E_BattleModelType.HP, healAmount);
        DebugManager.Log(EDebugCategory.Equipment,$"[攻击回复] {_characterData.Character_Name} 触发攻击回复 +{healAmount} HP (CD={cooldownSeconds}s)");
    }

    bool IsOwner(BattleBuffHandler buffHandler)
    {
        if (_ownerId == null || buffHandler == null) return false;
        var battler = _ownerId as IBattlable;
        if (battler == null) return false;
        return battler.battleDamageHandler != null
            && battler.battleDamageHandler.BuffHandler == buffHandler;
    }
}
