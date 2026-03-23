using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
public class MapSceneSetUp : MonoBehaviour
{
   GameRoot gameRoot;

    public float x_Offset=0.5f;//每行内的偏移
    public float y_Offset=0.35f;//相邻行的偏移
    public GameObject roomPrefab;
    public int MapRow = 50;
    public int MapCol = 50;
    //地图的左下角
    public Transform MapPivot;

    GameMapManager gameMapManager;

    HexPathFindingManager hexPathFindingManager;


    public PlayerCharacter PlayerCharacter;
    private void Start()
    {
        gameRoot = GameRoot.Instance;
        gameRoot.RegisterGlobal_CSManager(new GameMapManager(x_Offset,y_Offset,roomPrefab,MapRow,MapCol,MapPivot.position));

        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();

        gameMapManager = GameRoot.GetManager<GameMapManager>();
        hexPathFindingManager = GameRoot.GetManager<HexPathFindingManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) {
            gameMapManager.CreateMap();
        }
    }

}
