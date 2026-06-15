/// <summary>
/// 装备效果上下文——OnEquip 时传入，提供所属角色的关键引用
/// </summary>
public class EquipmentEffectContext{
    /// <summary>所属角色数据（可读写属性）</summary>
    public CharacterData characterData;
    /// <summary>所有者标识（用于 EventCenter 事件过滤，区分多角色）</summary>
    public object ownerId;
    /// <summary>所属装备控制器(EquipHandler引用)</summary>
    public object controller;
    public EquipmentEffectContext(CharacterData cd, object owner, object ctrl)
    {
        characterData = cd;
        ownerId = owner;
        controller = ctrl;
    }
}
