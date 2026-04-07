using Core;
using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class MapSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;

    public float x_Offset = 0.9f;//每行内的偏移
    public float y_Offset = 0.8f;//相邻行的偏移
    public GameObject roomPrefab;
    //public int MapRow = 20;
    //public int MapCol = 20;

    public int MapRadius=20;

    //地图的左下角
    public Transform MapPivot;

    GameMapManager gameMapManager;

    public CharacterMapMover PlayerCharacter;

    public Button EndRoundButton;

    private void Awake()
    {
        ObjectPoolManager obj = GameRoot.GetManager<ObjectPoolManager>();
        gameRoot = GameRoot.Instance;
        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();
        gameRoot.RegisterScene_MonoManager<MapMoverChecker>();
        gameRoot.RegisterScene_MonoManager<MapSkillerCheker>();
        gameMapManager = GameRoot.GetManager<GameMapManager>();
        //角色射线检测管理器
        gameRoot.RegisterScene_MonoManager<CharacterRayCaster>();
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, MapRadius, MapPivot.position);
        if(EndRoundButton)
        EndRoundButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.Player_RoundEnd));
    }
    private void Start()
    {
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.MapRoom_地图房间);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.RoomCloude_房间遮云);
        StartCoroutine(CreateMap());
    }
    IEnumerator CreateMap()
    { 
        yield return new WaitForSeconds(1);
        gameMapManager.CreateWholeMap();
        yield return new WaitForSeconds(3);
        //制造玩家
        HexRoomData randonoom = GameRoot.GetManager<HexMapInteractManager>().GetRnadomRoom();

        if (PlayerCharacter != null)
        {
            PlayerCharacter.transform.position = randonoom.transform.position + Vector3.up * 0.55f;
            //把玩家放到一个特殊的位置,然后原地走一格
            GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(PlayerCharacter.gameObject);
            PlayerCharacter.ZeroMove();
            yield return new WaitForSeconds(1.2f);
            PlayerCharacter.transform.DOScale(1.5f, 0.3f).SetEase(Ease.InQuart).From(0);
            yield return new WaitForSeconds(0.3f);
            PlayerCharacter.transform.DOScale(1, 0.2f).SetEase(Ease.OutQuart);
        }
    }

}
