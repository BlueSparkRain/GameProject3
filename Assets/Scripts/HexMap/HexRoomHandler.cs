using Core;
using UnityEngine;

[RequireComponent(typeof(HexRoomStyleHandler),
                  typeof(HexTerrainStyleHandler),
                  typeof(HexJumpAnimHandler))]
public class HexRoomHandler : MonoBehaviour
{
    HexRoomStyleHandler roomStyleHandler;
    HexTerrainStyleHandler terrainStyleHandler;
    HexJumpAnimHandler jumpAnimHandler;

    public void InitHexRoomHandler(HexRoomTag roomTag, E_HexTerrainType _hexTerrainType, bool playAnim = true)
    {
        terrainStyleHandler = GetComponent<HexTerrainStyleHandler>();
        terrainStyleHandler.InitTerrainStyle(_hexTerrainType, roomTag);
        roomStyleHandler = GetComponentInChildren<HexRoomStyleHandler>();
        roomStyleHandler.InitRoomStyle(roomTag);
        jumpAnimHandler = GetComponent<HexJumpAnimHandler>();
        if (_hexTerrainType != E_HexTerrainType.Obstacle_Ocean)
            GameRoot.GetManager<CoroutineManager>().StartDelayedCoroutine(0.4f, () => jumpAnimHandler.WalkableUpAnim());

        if (playAnim)
            jumpAnimHandler.TriggerJump(0.4f);
    }

    public void PlayAppearAnimation()
    {
        if (jumpAnimHandler == null)
            jumpAnimHandler = GetComponent<HexJumpAnimHandler>();
        jumpAnimHandler.TriggerJump(0.4f);
    }
}
