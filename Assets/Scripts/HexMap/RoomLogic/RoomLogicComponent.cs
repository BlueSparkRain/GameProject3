using UnityEngine;

/// <summary>
/// 房间逻辑组件抽象基类——挂在HexRoom子物体上，通过IRoomLogic接口解耦
/// 子类重写各方法实现具体房间行为
/// </summary>
public abstract class RoomLogicComponent : MonoBehaviour, IRoomLogic
{
    [Header("逻辑状态")]
    [SerializeField] protected bool _canTrigger = true;
    [SerializeField] protected E_HexRoomType _roomType;

    public bool CanTrigger => _canTrigger;
    public E_HexRoomType RoomType => _roomType;

    protected HexRoomTag _roomTag;
    protected HexTerrainStyleHandler _terrainStyle;

    public virtual void InitLogic(HexRoomTag roomTag)
    {
        _roomTag = roomTag;
        _terrainStyle = roomTag.GetComponent<HexTerrainStyleHandler>();
    }

    public abstract void OnPlayerEnter(HexRoomTag roomTag);

    public virtual void SpawnModel(Vector3 modelPos) { }

    /// <summary>
    /// 消耗房间——将地块类型重置为空白可走，移除特殊逻辑
    /// </summary>
    public virtual void Consume()
    {
        _canTrigger = false;

        if (_terrainStyle != null)
        {
            _terrainStyle.InitTerrainStyle(E_HexTerrainType.Walkable_EmptyLand, _roomTag);
        }
    }
}
