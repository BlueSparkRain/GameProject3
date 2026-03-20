using Core;
using Core.Interfaces;
using UnityEngine;

public class GameMapManager : IGlobalManager
{
    float x_Offset;//每行内的偏移
    float y_Offset;//相邻行的偏移
    GameObject roomPrefab;

    int MapRow = 50;
    int MapCol = 50;
    //地图的左下角
    Vector3 MapPivotPos;


    public GameMapManager(float _x_offset, float _y_offset, GameObject _roomPrefab, int _MapRow, int _MapCol, Vector3 _MapPivotPos)
    {
        x_Offset = _x_offset;
        y_Offset = _y_offset;
        roomPrefab = _roomPrefab;
        MapCol = _MapCol;
        MapRow = _MapRow;
        MapPivotPos = _MapPivotPos;
    }

    public void CreateMap()
    {
        HexGridClickManager hexGridClickManager = GameRoot.GetManager<HexGridClickManager>();
        HexRoom newRoom = null;
        for (int i = 0; i < MapRow; i++)
        {
            for (int j = 0; j < MapCol; j++)
            {
                if (i % 2 == 0)
                    newRoom = GameObject.Instantiate(roomPrefab, MapPivotPos + new Vector3(y_Offset * j, 0, x_Offset * i), Quaternion.Euler(-90, 0, 0)).GetComponent<HexRoom>();
                else
                    newRoom = GameObject.Instantiate(roomPrefab, MapPivotPos + new Vector3(y_Offset * (j + 0.5f), 0, x_Offset * i), Quaternion.Euler(-90, 0, 0)).GetComponent<HexRoom>();

                if (newRoom)
                {
                    newRoom.InitRoomID(i, j);
                    hexGridClickManager.RegisterHexRoom(newRoom);
                }
            }
        }
    }


    public void MgrDispose() { }

    public void MgrInit(GameRoot gameRoot) { }

    public void MgrUpdate(float deltatime) { }
}
