using Core;
using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
public class MapSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;

    public float x_Offset = 0.88f;//每行内的偏移
    public float y_Offset = 0.75f;//相邻行的偏移

    public int MapRadius=20;
    public Transform MapPivot;
    GameMapManager gameMapManager;
    MapCharacterCallerManager  characterCallerManager;
    public Button EndRoundButton;

    public bool needDelay = false;
    private void Awake()
    {
        ObjectPoolManager obj = GameRoot.GetManager<ObjectPoolManager>();
        gameRoot = GameRoot.Instance;
        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();
        //移动管理器
        gameRoot.RegisterScene_MonoManager<MapMoverChecker>();
        //技能管理器
        gameRoot.RegisterScene_MonoManager<MapSkillerCheker>();
        //角色生成管理器
        gameRoot.RegisterScene_MonoManager<MapCharacterCallerManager>();
        //角色射线检测管理器
        gameRoot.RegisterScene_MonoManager<CharacterRayCaster>();

        gameMapManager = GameRoot.GetManager<GameMapManager>();
        characterCallerManager = GameRoot.GetManager<MapCharacterCallerManager>();
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, MapRadius, MapPivot.position);
        if(EndRoundButton)
        EndRoundButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.Player_RoundEnd));

        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.MapRoom_地图房间);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.RoomCloude_房间遮云);
    }
    private void Start()
    {
        if (needDelay)
            StartCoroutine(WaitMapCreate());
        else
            gameMapManager.CreateWholeMap();
    }
    IEnumerator WaitMapCreate()
    {
        yield return new WaitForSeconds(2);
        gameMapManager.CreateWholeMap();

        //产生角色
       var player1= characterCallerManager.CallNewCharacter("Moveable");
        //支持外部角色调整
       player1.InitCharacter(E_CharacterType.P_1, true,true);

        yield return new WaitForSeconds(2);
        //地图还没有加载，还没来得及注册
        HexRoomData randonoom = gameMapManager.GetRnadomRoom();

        player1.transform.position = randonoom.transform.position + Vector3.up * 0.6f;
        player1.transform.localScale = Vector3.zero;
        //把玩家放到一个特殊的位置,然后原地走一格
        GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(player1.gameObject);

        (player1.GetComponent<CharacterMapMoveHandle>().iMapMover as Player_CharacterMapMover).CharacterZeroMove();
        yield return new WaitForSeconds(1.2f);
        player1.transform.DOScale(1.5f, 0.3f).SetEase(Ease.InQuart).From(0);
        yield return new WaitForSeconds(0.3f);
        player1.transform.DOScale(1, 0.2f).SetEase(Ease.OutQuart);
    }
}
