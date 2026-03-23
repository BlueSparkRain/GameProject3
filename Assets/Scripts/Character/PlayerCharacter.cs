using Core;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    /// <summary>
    /// 当前所在的格子
    /// </summary>
    HexRoom currentRoom;
    public HexRoom CurrentRooom =>currentRoom;


    private void Start()
    {
    }
    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 5);
    }
    void Update(){
        CheckCurrentRoom();
    }

    /// <summary>
    /// 每次移动后都会更新当前所处的Room
    /// </summary>
    void CheckCurrentRoom() {
        Ray ray = new Ray(transform.position,Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit)){
            HexRoom downRoom = hit.collider.GetComponent<HexRoom>();
            Debug.Log(downRoom);
            if (downRoom != currentRoom){
                GameRoot.GetManager<HexPathFindingManager>().SetPlayerStartRoom(downRoom);
                Debug.Log($"玩家位置更新 row:{downRoom.row},col:{downRoom.col}");
            }
                
            if (downRoom != null)
            currentRoom = downRoom;
        }

    }
}
