using Core;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapSceneSetUp : MonoBehaviour
{
    GameRoot gameRoot;

    float x_Offset = 0.88f;//每行内的偏移
    float y_Offset = 0.75f;//相邻行的偏移

    int MapRadius = 30;
    public Transform MapPivot;
    GameMapManager gameMapManager;
    public Button EndRoundButton;

    [Header("Test-游戏回合文本")]
    public TMP_Text roundUIText;
    [Header("Test-活力点数文本")]
    public TMP_Text vitalityPointsUIText;

    public bool needDelay = false;

    public float characterHeight = 1;


    private void UpdateRoundText()
    {
       roundUIText.text=GameRoot.GetManager<GameRoundManager>().RoundNum.ToString();
    }
    private void UpdateValityText()
    {
        Debug.Log("更新！！！！！！！！！");
        vitalityPointsUIText.text = GameRoot.GetManager<VitalityPointsManager>().valityPoint.ToString();
    }


    private void Awake()
    {
        gameRoot = GameRoot.Instance;
        BattleSkillFactory.RegisterAllSkills();



        //地图生成管理器
        gameRoot.RegisterScene_MonoManager<GameMapManager>();
        //地图寻路管理器
        gameRoot.RegisterGlobal_MonoManager<HexPathFindingManager>();
        //地图房间交互管理器
        gameRoot.RegisterGlobal_MonoManager<HexMapInteractManager>();
        //正交相机漫游管理器
        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();
        //移动管理器
        gameRoot.RegisterScene_MonoManager<MapMoverChecker>();
        //技能管理器
        gameRoot.RegisterScene_MonoManager<MapSkillerCheker>();
        //角色射线检测管理器
        gameRoot.RegisterScene_MonoManager<CharacterRayCasterManager>();

        //回合记录管理器
        gameRoot.RegisterScene_MonoManager<GameRoundManager>();
        //活力点数管理器
        gameRoot.RegisterScene_MonoManager<VitalityPointsManager>();

        gameMapManager = GameRoot.GetManager<GameMapManager>();
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, MapRadius, MapPivot.position);

        //测试代码
        EventCenter.AddEventListener(E_EventType.NewRound, UpdateRoundText);
        EventCenter.AddEventListener(E_EventType.AdjustVitalityPoints, UpdateValityText);

        EventCenter.EventTrigger(E_EventType.NewRound);
        EventCenter.EventTrigger(E_EventType.AdjustVitalityPoints);

        if (EndRoundButton)
            EndRoundButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.Player_RoundEnd));
        //EndRoundButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.OneMoverEndRound));


        //EventCenter.EventTrigger(E_EventType.AdjustVitalityPoints);
    }

    IEnumerator LoadAllPool()
    {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.SkillSlot_技能槽位);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, EPoolType.SkillIcon_技能图标);
    }
    private void Start()
    {
        StartCoroutine(LoadAllPool());
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
        var player1 = MapCharacterCaller.CallNewCharacter("Moveable");

          //支持外部角色调整
            player1.InitCharacterDataTag(E_CharacterType.P_1, true, true);

        yield return new WaitForSeconds(4f);
        //地图还没有加载，还没来得及注册
        HexRoomData randonoom = gameMapManager.GetRnadomRoom();

        player1.transform.position = randonoom.transform.position + Vector3.up * characterHeight;
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
