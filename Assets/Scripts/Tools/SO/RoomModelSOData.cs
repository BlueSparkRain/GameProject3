using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "RoomModelSOData", menuName = "SOData/RoomModelData")]
public class RoomModelSOData : ScriptableObject
{
    [Header("房间对应精灵")]
    public List<Sprite> roomSprites;
    
}
