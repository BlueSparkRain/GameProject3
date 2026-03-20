using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
public class MapSceneSetUp : MonoBehaviour
{
   GameRoot gameRoot;

    public float x_Offset=1;//每行内的偏移
    public float y_Offset=0.7f;//相邻行的偏移
    public GameObject roomPrefab;
    public int MapRow = 50;
    public int MapCol = 50;
    //地图的左下角
    public Transform MapPivot;

    GameMapManager gameMapManager;
    private void Start()
    {
        gameRoot = GameRoot.Instance;
        gameRoot.RegisterGlobal_CSManager(new GameMapManager(x_Offset,y_Offset,roomPrefab,MapRow,MapCol,MapPivot.position));
        gameRoot.RegisterGlobal_MonoManager<HexGridClickManager>();
        gameRoot.RegisterGlobal_MonoManager<HexRoomObjectPool>();
        gameMapManager = GameRoot.GetManager<GameMapManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {
            Debug.Log("创建期");
            gameMapManager.CreateMap();
        }
    }

}
