using Core;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 六边形房间基础类（仅保存坐标信息，无业务逻辑）
/// </summary>
public class HexRoomData : MonoBehaviour
{
    [Header("六边形轴向坐标")]
    public int row; // 轴向坐标q
    public int col; // 轴向坐标r

    [Header("是否可行走")]
    public bool walkable;

    [Header("房间类型")]
    public E_HexRoomType roomType = E_HexRoomType.None_无;

    HexJumpAnimation hexJumpAnimation;
    CoroutineManager coroutineManager;

    string roomModelPath = "Prefab/Model/";

    GameObject roomModel;
    public IHexRoom IHexRoom=>iHexRoom;
    IHexRoom iHexRoom;
    public void InitRoomID(int _row, int _col, E_HexRoomType _roomType)
    {
        roomType = _roomType;
       
        InitRoomStyle();

        row = _row; col = _col;
        hexJumpAnimation = GetComponent<HexJumpAnimation>();
        coroutineManager = GameRoot.GetManager<CoroutineManager>();
        hexJumpAnimation.TriggerJump(0.4f);
        //LoadRoomModel();

    }

    void LoadRoomModel()
    {
        if (roomType != E_HexRoomType.None_无)
            roomModel = Resources.Load<GameObject>(roomModelPath + roomType);
        if (roomModel)
            Instantiate(roomModel, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform);
        
        LoadRoomCloude();
    }

    void LoadRoomCloude() {
        var cloude = GameRoot.GetManager<ObjectPoolManager>().GetInstance(EPoolType.RoomCloude_房间遮云);
        cloude.transform.position = transform.position + Vector3.up * 20f;
        //var cloude= Instantiate(roomCloude, transform.position + Vector3.up * 20f, Quaternion.Euler(-90, 0, 0));
        hexJumpAnimation.CloudeAppear(cloude.transform);
    }

    

    void InitRoomStyle()
    {
        switch (roomType)
        {
            case E_HexRoomType.None_无:
                iHexRoom = new NoneHexRoom();
                break;
            case E_HexRoomType.Battle_战斗:
                iHexRoom = new BattleHexRoom(E_CharacterType.LE_1);
                break;
            case E_HexRoomType.NPC_特定交互:
                iHexRoom = new NPCHexRoom();
                break;
            case E_HexRoomType.Unknown_随机事件:
                iHexRoom = new UnknownHexRoom();
                break;
            default:
                break;
        }
    }
    public virtual void ResetSelf()
    {

    }

    public void SetCellState(bool _walkable)
    {
        walkable = _walkable;
        if (walkable)
            coroutineManager.StartDelayedCoroutine(0.4f, () => hexJumpAnimation.WalkableUpAnim());
    }


    public void CallBattle()
    {
        Debug.Log("Go");
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(E_UIPanelType.BattlePanel);
    }
}

public interface IHexRoom
{
    public void DoRoomLogic(UnityAction roomJob=null);
}

/// <summary>
/// 各种模型的类型
/// </summary>

public enum E_ModelType
{
    None,
    NPC1, NPC2,
    Enemy_1, Enemy_2, Enemy_3,
    Unknown,
    NewArea,
    Rewards,
}
