using UnityEngine;

[RequireComponent(typeof(HexRoomStyleHandler),
                  typeof(HexTerrainStyleHandler),
                  typeof(HexJumpAnimHandler))]
public class HexRoomHandler : MonoBehaviour
{
    HexRoomStyleHandler roomStyleHandler;
    HexTerrainStyleHandler terrainStyleHandler;
    HexJumpAnimHandler jumpAnimHandler;

    public void InitHexRoomHandler(HexRoomTag roomTag, E_HexTerrainType _hexTerrainType)
    {
        //房间类型初始化
        roomStyleHandler = GetComponentInChildren<HexRoomStyleHandler>();
        roomStyleHandler.InitRoomStyle(roomTag);
        //地块类型初始化
        terrainStyleHandler = GetComponent<HexTerrainStyleHandler>();
        terrainStyleHandler.InitTerrainStyle(_hexTerrainType);
        //动画组件初始化
        jumpAnimHandler = GetComponent<HexJumpAnimHandler>();
        jumpAnimHandler.TriggerJump(0.4f);
    }
}
