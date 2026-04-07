using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterData))]
/// <summary>
/// 更新角色当前坐标
/// </summary>
public class CharacterMapMover : MonoBehaviour
{
    public E_CharacterType CharacterType => characterType;
    E_CharacterType characterType;
    /// <summary>
    /// 当前所在的格子
    /// </summary>
    HexRoomData currentRoom;
    public HexRoomData CurrentRooom => currentRoom;

    private IHexRoom currentIHexRoom;

    bool isMoving = false;
    public bool IsMoving { get { return isMoving; } }


    public int max_Actionpoints=7;
    public int remain_Acionpoints;

    HexPathFindingManager  pathFindingManager;

    CharacterMapIcon mapIcon;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 5);
    }

    void PlayerEndRound() {
        Debug.Log("玩家主动结束回合，回复行动点");
        remain_Acionpoints = max_Actionpoints;
        moveStop = remain_Acionpoints <= 0;
        mapIcon.SetMoveDot(remain_Acionpoints);
    }
    private void Start()
    {
        characterType = GetComponent<CharacterData>().characterType;
        pathFindingManager = GameRoot.GetManager<HexPathFindingManager>();
        transform.localScale= Vector3.zero;
        StartCoroutine(WaitIcon());
    }
    
    IEnumerator WaitIcon() {
        yield return new WaitForSeconds(1.5f);
        mapIcon= GameRoot.GetManager<MapMoverChecker>().CreateNewMapIcon(this);
        mapIcon.SetMoveDot(remain_Acionpoints);
        PlayerEndRound();
    }
    void OnEnable()
    {
        EventCenter.AddEventListener(E_EventType.LoadMapStart, CheckCurrentRoom);
        EventCenter.AddEventListener(E_EventType.Mover_OneTimeMove, OneTimeMove);
        EventCenter.AddEventListener(E_EventType.Mover_MoveStop, MoveStop);
        EventCenter.AddEventListener(E_EventType.Player_RoundEnd, PlayerEndRound);
    }
    void OneTimeMove() {
        remain_Acionpoints--;
    }

    void MoveStop() { 
        moveStop=true;
        //将剩余行动点纯递给寻路管理器
        mapIcon.SetMoveDot(remain_Acionpoints);
    }

    /// <summary>
    /// 每次移动后都会更新当前所处的Room
    /// </summary>
    void CheckCurrentRoom()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit,5,LayerMask.GetMask("HexRoom")))
        {
            HexRoomData downRoom = hit.collider.GetComponent<HexRoomData>();
            if (downRoom == null) {
                Debug.Log("dad大会的胃口很好");
            }

            if (downRoom != currentRoom){
                GameRoot.GetManager<HexPathFindingManager>().SetPlayerStartRoom(downRoom);
                Debug.Log($"玩家位置更新 row:{downRoom.row},col:{downRoom.col}");
            }
            if (downRoom != null){
                currentRoom = downRoom;
                currentIHexRoom = currentRoom.IHexRoom;
            }
        }
    }
    public void ZeroMove()
    {
        CheckCurrentRoom();
        GameRoot.GetManager<MapMoverChecker>().SetCurrentMover(this);
        EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);
    }
    public void MoveByPath(List<HexRoomData> roomPath)
    {
        EventCenter.EventTrigger(E_EventType.Mover_StartMove);
        GameRoot.GetManager<CoroutineManager>().StartCoroutine(MoveAnim(roomPath));
    }

    public bool moveStop;
    
    IEnumerator MoveAnim(List<HexRoomData> roomPath)
    {
        moveStop = remain_Acionpoints <= 0;
        Debug.Log("Mover开始移动：路径长度" + roomPath.Count);
        isMoving = true;

        for (int i = 0; i < roomPath.Count; i++){
            if (moveStop) {
                //只有敌人AI才能直接应用房间的结果，玩家需经理房间后才能获得奖励
                EventCenter.EventTrigger(E_EventType.Mover_IntoSpecialRoom, GetComponent<CharacterMapSkiller>(), currentRoom.roomType);
                Debug.Log("Mover被打断，剩余移动点"+remain_Acionpoints);
                break;
            }
            transform.position = roomPath[i].transform.position + Vector3.up * 2;
            CheckCurrentRoom();
            EventCenter.EventTrigger(E_EventType.Mover_OneTimeMove);
            yield return new WaitForSeconds(0.2f);
            currentIHexRoom?.DoRoomLogic();


        }
        EventCenter.EventTrigger(E_EventType.Mover_MoveStop);
        isMoving = false;
    }
}
