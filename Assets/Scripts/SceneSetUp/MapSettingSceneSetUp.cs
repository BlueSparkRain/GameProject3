using Core;
using System.Collections;
using UnityEngine;

public class MapSettingSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;
    public float x_Offset = 0.88f;//每行内的偏移
    public float y_Offset = 0.75f;//相邻行的偏移
    public GameObject roomPrefab;

    public int mapWidth = 40;
    public int mapHeight = 30;
    //地图的左下角
    [Header("地图左下角锚点")]
    public Transform MapPivot;
    GameMapManager gameMapManager;

    [Header("地图地形数据")]
    public MapSaveSOData MapSOData;

    public bool needDelay = false;

    private void Awake(){
        ObjectPoolManager obj = GameRoot.GetManager<ObjectPoolManager>();
        gameRoot = GameRoot.Instance;
        BattleSkillFactory.RegisterAllSkills();
        gameRoot.RegisterGlobal_MonoManager<GameMapManager>();
        gameRoot.RegisterGlobal_MonoManager<HexPathFindingManager>();
        gameRoot.RegisterGlobal_MonoManager<HexMapInteractManager>();
        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();
        //角色射线检测管理器
        gameRoot.RegisterScene_MonoManager<CharacterRayCasterManager>();
        gameMapManager = GameRoot.GetManager<GameMapManager>();
        MapSOData.mapCols = mapWidth;
        MapSOData.mapRows = mapHeight;
        MapSOData.mapShape = E_MapShape.Rectangle;
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, mapWidth, mapHeight, MapPivot.position);
        EventCenter.AddEventListener<Vector2Int, E_HexTerrainType>(E_EventType.Editor_Terrain, EditorOneRoomTexrrainTag);
        GameRoot.GetManager<HexMapInteractManager>().USEEditMode();

    }
    void EditorOneRoomTexrrainTag(Vector2Int pos,E_HexTerrainType terrainType) {
        MapSOData.cellData[pos.x, pos.y] = terrainType;
        MapSOData.SaveData();
        Debug.Log("HexMapSO已更新：" + pos + " → " + MapSOData.cellData[pos.x, pos.y]);
    }

    private void Start()
    {
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.MapRoom_地图房间);

        if (needDelay) 
            StartCoroutine(WaitMapCreate());
        else
            gameMapManager.CreateWholeMap();
    }
    IEnumerator WaitMapCreate() {
        yield return new WaitForSeconds(1);
        gameMapManager.CreateWholeMap();
        
    }
   
}
