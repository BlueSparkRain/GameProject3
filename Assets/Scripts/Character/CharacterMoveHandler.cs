using UnityEngine;

public class CharacterMapMoveHandle : MonoBehaviour
{
    public IMapMoveable iMapMover;
    /// <summary>
    /// 已知是高级角色（可以寻路）
    /// </summary>
    /// <param name="isPlayer"></param>
    public void InitMover(bool isPlayer, E_CharacterType characterType) {

        characterType = GetComponent<CharacterData>().characterType;
        iMapMover = isPlayer? 
            new Player_CharacterMapMover(characterType, transform):
            new Robot_CharacterMapMover();
    }

    void UpdateCurrentRoom(HexRoomData hexRoomData)
    {
        iMapMover.currentRoom = hexRoomData;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 4);
    }


}
