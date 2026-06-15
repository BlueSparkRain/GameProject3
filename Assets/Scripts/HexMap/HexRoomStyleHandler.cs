using Core;
using UnityEngine;

/// <summary>
/// 负责房间类型分配、房间逻辑组件的创建与模型生成
/// </summary>
public class HexRoomStyleHandler : MonoBehaviour{
    [Header("房间类型")]
    E_HexRoomType roomType = E_HexRoomType.None;

    HexJumpAnimHandler hexJumpAnimation;
    HexRoomTag roomTag;
    public E_HexRoomType RoomType => roomType;
    public void InitRoomStyle(HexRoomTag _roomTag){
        hexJumpAnimation = GetComponent<HexJumpAnimHandler>();
        roomTag = _roomTag;

        return;
        if (GetComponent<HexTerrainStyleHandler>().HexTerrainType != E_HexTerrainType.Obstacle_Ocean)
        {
            LoadRoomCloude();
        }
    }

    void LoadRoomCloude(){
        var cloude = GameRoot.GetManager<ObjectPoolManager>().GetInstance(E_PoolType.RoomCloude_房间遮云);
        cloude.transform.position = transform.position + Vector3.up * 23f;
        hexJumpAnimation.CloudeAppear(cloude.transform);
    }

    public void SetRoomType(E_HexRoomType _roomType,HexRoomTag _roomTag){
        roomType = _roomType;
        roomTag = _roomTag;

        // 非 None 类型生成房间图标
        if (_roomType != E_HexRoomType.None)
            HexRoomIcon.CreateForRoom(transform, _roomType);

        // 1. 清理旧逻辑组件
        RemoveOldLogicComponents();

        // 2. 创建新逻辑组件(GameObject子物体 + RoomLogicComponent)
        RoomLogicComponent logic = CreateLogicComponent(_roomType);
        if (logic != null)
        {
            roomTag.SetRoomLogic(logic);
            logic.SpawnModel(transform.position + Vector3.up);
        }

        // 3. 旧IHexRoom兼容(后续逐步移除)
        // 清理旧 IHexRoom 模型再替换
        roomTag.IHexRoom?.DestroyModel();
        IHexRoom iHexRoom = null;
        switch (roomType){
            case E_HexRoomType.None: iHexRoom = new NoneHexRoom(); break;
            case E_HexRoomType.Battle_LowLevel:
            case E_HexRoomType.Battle_MidLevel:
            case E_HexRoomType.Battle_HighLevel:
                iHexRoom = new BattleHexRoom(_roomTag, roomType.ToBattleType()); break;
            case E_HexRoomType.NPC: iHexRoom = new NPCHexRoom(); break;
            case E_HexRoomType.UnknownEvent: iHexRoom = new UnknownEventHexRoom(); break;
            case E_HexRoomType.Reward: iHexRoom = new RewardHexRoom(); break;
            case E_HexRoomType.CityShop: iHexRoom = new CityShopHexRoom(); break;
            default: break;
        }
        roomTag.GetIHexRoom(iHexRoom);
        iHexRoom.DoHexRoomModel(transform.position + Vector3.up);
    }

    RoomLogicComponent CreateLogicComponent(E_HexRoomType rType)
    {
        GameObject child = new GameObject("[Logic] " + rType);
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;

        switch (rType)
        {
            case E_HexRoomType.None: return child.AddComponent<NoneRoomLogic>();
            case E_HexRoomType.Battle_LowLevel:
            case E_HexRoomType.Battle_MidLevel:
            case E_HexRoomType.Battle_HighLevel:
                var battle = child.AddComponent<BattleRoomLogic>();
                battle.SetBattleType(rType.ToBattleType());
                return battle;
            case E_HexRoomType.NPC: return child.AddComponent<NPCRoomLogic>();
            case E_HexRoomType.UnknownEvent: return child.AddComponent<UnknownEventRoomLogic>();
            case E_HexRoomType.Reward: return child.AddComponent<RewardRoomLogic>();
            case E_HexRoomType.CityShop: return child.AddComponent<CityShopRoomLogic>();
            default: return null;
        }
    }

    void RemoveOldLogicComponents()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("[Logic]"))
                Destroy(child.gameObject);
        }
    }
}
