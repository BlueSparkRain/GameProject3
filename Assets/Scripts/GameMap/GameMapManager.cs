using Core;
using Core.Interfaces;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameMapManager : IGlobalManager
{
    float x_Offset;//每行内的偏移
    float y_Offset;//相邻行的偏移
    GameObject roomPrefab;

    int MapRow = 30;
    int MapCol = 30;

    /// <summary>
    /// 相邻行生成房间间隔
    /// </summary>
    float rowBatchInterval =0.1f;
    /// <summary>
    /// 相邻行生成房间间隔
    /// </summary>
    float bornRoomInterval =0.05f;

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

    Transform hexRoomsParent;
    HexGridInteractManager hexGridClickManager;
    CoroutineManager coroutineManager;
    public void CreateMap()
    {
        coroutineManager=GameRoot.GetManager<CoroutineManager>();
        hexGridClickManager = GameRoot.GetManager<HexGridInteractManager>();
        hexRoomsParent = new GameObject("hexRoomsParent").transform;
        coroutineManager.StartCoroutine(MapCreate());
    }

    IEnumerator MapCreate(){
        WaitForSeconds rowBatchDealy= new WaitForSeconds(rowBatchInterval);
        bool fromleft = true;
        for (int i = 0; i < MapRow; i++){
            coroutineManager.StartCoroutine(CreatRowRooms(i,fromleft));
            fromleft = !fromleft;
            yield return  rowBatchDealy;
        }      
   }

    //创建一整行
    IEnumerator CreatRowRooms(int _row,bool fromleft) {
        WaitForSeconds roomDealy = new WaitForSeconds(bornRoomInterval);
        if (fromleft){
            for (int _col = 0; _col < MapCol; _col++){
                CreateOneRoom(_row, _col);
                yield return roomDealy;
            }
        }
        else {
            for (int _col = MapCol-1; _col >=0; _col--){
                CreateOneRoom(_row, _col);
                yield return roomDealy;
            }
        }
    }

    void CreateOneRoom(int _row,int _col) {
        var newHexRoom = CreateHexRoom(_row, _col);
        newHexRoom.GetComponent<HexJumpAnimation>().TriggerJump(0.4f);
        //根据一张映射Int数组来决定房间类型
        SettleRoomStyle(newHexRoom, E_HexRoomType.Battle_战斗);
    }

    /// <summary>
    /// 创建并注册一个新的HexRoom
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    HexRoom CreateHexRoom(int row, int col)
    {
        HexRoom newHexRoom = null;
        if (row % 2 == 0)
            newHexRoom = GameObject.Instantiate(roomPrefab, MapPivotPos + new Vector3(y_Offset * col, 0, x_Offset * row),
                Quaternion.Euler(-90, 0, 0), hexRoomsParent).GetComponent<HexRoom>();
        else
            newHexRoom = GameObject.Instantiate(roomPrefab, MapPivotPos + new Vector3(y_Offset * (col + 0.5f), 0, x_Offset * row),
                Quaternion.Euler(-90, 0, 0), hexRoomsParent).GetComponent<HexRoom>();
        if (newHexRoom)
        {
            newHexRoom.InitRoomID(row, col);
            hexGridClickManager.RegisterHexRoom(newHexRoom);
        }
        return newHexRoom;
    }
    void SettleRoomStyle(HexRoom newHexRoom, E_HexRoomType roomType)
    {
        if (newHexRoom)
        {
            newHexRoom.InitRoomStyle(roomType);
        }

    }





    public void MgrDispose() { }

    public void MgrInit(GameRoot gameRoot) { }

    public void MgrUpdate(float deltatime) { }
}
