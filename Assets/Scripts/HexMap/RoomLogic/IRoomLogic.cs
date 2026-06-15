using UnityEngine.Events;

/// <summary>
/// 房间触发逻辑接口——从HexRoom解耦，独立于地块数据结构
/// 每个特殊房间类型的逻辑组件实现此接口
/// </summary>
public interface IRoomLogic
{
    /// <summary>当前房间是否可触发(未消耗/已重生)</summary>
    bool CanTrigger { get; }

    /// <summary>玩家到达本房间时调用</summary>
    void OnPlayerEnter(HexRoomTag roomTag);

    /// <summary>初始化逻辑组件(替代原IHexRoom.DoHexRoomInit)</summary>
    void InitLogic(HexRoomTag roomTag);

    /// <summary>生成视觉模型(替代原IHexRoom.DoHexRoomModel)</summary>
    void SpawnModel(UnityEngine.Vector3 modelPos);

    /// <summary>消耗本房间逻辑——触发后变为普通地块</summary>
    void Consume();
}
