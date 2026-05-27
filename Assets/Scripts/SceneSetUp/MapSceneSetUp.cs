using Core;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapSceneSetUp : MonoBehaviour, ICanSave_And_Load
{
    GameRoot gameRoot;

    float x_Offset = 0.88f;//每行内的偏移
    float y_Offset = 0.8f;//相邻行的偏移

    int MapRadius = 25;
    public Transform MapPivot;
    GameMapManager gameMapManager;
    public Button EndRoundButton;

    [Header("Test-游戏回合文本")]
    public TMP_Text roundUIText;
    [Header("Test-活力点数文本")]
    public TMP_Text vitalityPointsUIText;

    [Header("玩家技能按钮")]
    public Button PlayerSkillButton;
    [Header("玩家装备按钮")]
    public Button PlayerEquipmentButton;

    public bool needDelay = false;
    public float characterHeight = 1;
    private void UpdateRoundText()
    {
        roundUIText.text = GameRoot.GetManager<GameRoundManager>().RoundNum.ToString();
    }
    private void UpdateValityText(){
        //存档里更新，每次加载场景后出现前再更新
        vitalityPointsUIText.text = GameRoot.GetManager<VitalityPointsManager>().valityPoint.ToString();
    }

    public void InitBySaveData(){
        gameMapManager = GameRoot.GetManager<GameMapManager>();
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, MapRadius, MapPivot.position);

        StartCoroutine(LoadCharacter(1));
    }

    private void OnApplicationQuit(){
        JsonSaver.Save(new FirstLoadMap(false));
    }

    public void InitBySelf(){
        //读取是否加载过地图,如果加载过，忽略
        gameRoot = GameRoot.Instance;
        //地图生成管理器
        gameRoot.RegisterGlobal_MonoManager<GameMapManager>();
        gameRoot.RegisterGlobal_CSManager(new GameBattleManager());

        //地图寻路管理器
        gameRoot.RegisterGlobal_MonoManager<HexPathFindingManager>();
        //地图房间交互管理器
        gameRoot.RegisterGlobal_MonoManager<HexMapInteractManager>();
        //移动管理器
        gameRoot.RegisterGlobal_MonoManager<MapMoverManager>();
        //活力点数管理器
        gameRoot.RegisterGlobal_MonoManager<VitalityPointsManager>();

        //回合记录管理器
        gameRoot.RegisterGlobal_MonoManager<GameRoundManager>();

        gameMapManager = GameRoot.GetManager<GameMapManager>();
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, MapRadius, MapPivot.position);

        if (EndRoundButton)
        {
            EndRoundButton.onClick.RemoveAllListeners();
            EndRoundButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.Player_RoundEnd));
        }

        PlayerSkillButton.onClick.RemoveAllListeners();
        PlayerSkillButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.CallSkillPanel));
        
        //PlayerSkillButton.onClick.AddListener(() => GameRoot.GetManager<UIManager>().OpenPanel<SkillPanel>(E_UIPanelType.SkillPanel, (panel) => Debug.Log("打开技能面板")));

        ////测试代码
        //EventCenter.AddEventListener(E_EventType.NewRound, UpdateRoundText);
        //EventCenter.AddEventListener(E_EventType.UpdateUIVitalityPoints, UpdateValityText);

        //地图加载
        StartCoroutine(LoadSkillInfoPool());
        StartCoroutine(LoadMap());
        StartCoroutine(LoadCharacter(2));
        JsonSaver.Save(new FirstLoadMap(true));
    }
    void Awake()
    {
        EventCenter.ClearAllEvents();
        //读取是否加载过地图,如果加载过，忽略
        gameRoot = GameRoot.Instance;
        BattleSkillFactory.RegisterAllSkills();

        //正交相机漫游管理器
        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();
        //技能管理器
        gameRoot.RegisterScene_MonoManager<MapSkillerCheker>();
        //角色射线检测管理器
        gameRoot.RegisterScene_MonoManager<CharacterRayCasterManager>();
        //混沌等级管理器
        gameRoot.RegisterScene_MonoManager<ChaosLevelManager>();


        JsonSaver.InitData<FirstLoadMap>(this, JsonSaver.Load<FirstLoadMap>().GetState);

        //测试代码
        //EventCenter.AddEventListener(E_EventType.NewRound, UpdateRoundText);
        EventCenter.AddEventListener(E_EventType.UpdateRoundState, UpdateRoundText);
        EventCenter.AddEventListener(E_EventType.UpdateUIVitalityPoints, UpdateValityText);
    }
    private void Start()
    {
        EventCenter.EventTrigger(E_EventType.UpdateRoundState);
        EventCenter.EventTrigger(E_EventType.UpdateUIVitalityPoints);
    }



    IEnumerator LoadSkillInfoPool()
    {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillSlot_技能槽位);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillIcon_技能图标);
    }

    IEnumerator LoadCharacter(float delay)
    {
        yield return new WaitForSeconds(delay);
        //产生角色
        var player1 = MapCharacterCaller.CallNewCharacter("Moveable");
        //支持外部角色调整
        player1.InitCharacterDataTag(E_CharacterType.P_1, true, true);

        //GameRoot.GetManager<OrthoCameraNavigator>().FocusOnTarget(player1.gameObject);
        (player1.GetComponent<CharacterMapMoveHandle>().iMapMover as Player_CharacterMapMover).CharacterZeroMove();
        //yield return new WaitForSeconds(delay/2);
        
        ////地图还没有加载，还没来得及注册
        //HexRoomTag randonoom = gameMapManager.GetRnadomRoom();

        //player1.transform.position = randonoom.transform.position + Vector3.up * characterHeight;
        //player1.transform.localScale = Vector3.zero;
        ////把玩家放到一个特殊的位置,然后原地走一格

        //yield return new WaitForSeconds(1.2f);
        //player1.transform.DOScale(1.5f, 0.3f).SetEase(Ease.InQuart).From(0);
        //yield return new WaitForSeconds(0.3f);
        //player1.transform.DOScale(1, 0.2f).SetEase(Ease.OutQuart);
    }

    IEnumerator LoadMap(){
        yield return new WaitForSeconds(0.5f);
        gameMapManager.CreateWholeMap();
    }
}

[Serializable]
public class FirstLoadMap : IValidatable
{
    public bool hasLoadMap = false;

    public bool GetState()
    {
        return hasLoadMap;
    }
    public FirstLoadMap() { }
    public FirstLoadMap(bool _load = true)
    {
        hasLoadMap = _load;

    }
    public bool IsValid()
    {
        return true;
    }
}