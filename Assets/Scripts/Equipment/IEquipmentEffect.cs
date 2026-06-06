/// <summary>
/// 装备效果接口——所有装备效果（被动/事件型）必须实现
/// OnEquip:   装备时调用，在此订阅事件、注册被动值等
/// OnUnequip: 卸载时调用，在此取消订阅、清理状态
/// </summary>
public interface IEquipmentEffect
{
    void OnEquip(EquipmentEffectContext context);
    void OnUnequip();
}
