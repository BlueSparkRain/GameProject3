using Core;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapSceneSetUp : MonoBehaviour, ICanSave_And_Load{
    GameRoot gameRoot;

    float x_Offset = 0.88f;//每行内的偏移
    float y_Offset = 0.8f;//相邻行的偏移

    int mapWidth = 55;
    int mapHeight = 35;
    public Transform MapPivot;
    GameMapManager gameMapManager;
    public Button EndRoundButton;

    [Header("Test-游戏回合文本")]
    public TMP_Text roundUIText;
    [Header("Test-活力点数文本")]
    public TMP_Text vitalityPointsUIText;
    [Header("混沌等级文本")]
    public TMP_Text chaosLevelUIText;
    [Header("玩家等级文本")]
    public TMP_Text playerLevelText;

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
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, mapWidth, mapHeight, MapPivot.position);

        BindUI();
        HexRoomStyleHandler.DeferIconCreation = true;
        StartCoroutine(LoadSequence(0f));
    }

    private void OnApplicationQuit(){
        JsonSaver.Save(new FirstLoadMap(false));

        var cam = Camera.main;
        if (cam != null)
            JsonSaver.Save(new CameraSaveData(cam.transform.position, cam.orthographicSize));
    }

    public void InitBySelf(){
        gameRoot = GameRoot.Instance;

        gameMapManager = GameRoot.GetManager<GameMapManager>();
        gameMapManager.GameMapManagerInit(y_Offset, x_Offset, mapWidth, mapHeight, MapPivot.position);
        BindUI();
        HexRoomStyleHandler.DeferIconCreation = true;
        StartCoroutine(LoadSequence(0f));
        JsonSaver.Save(new FirstLoadMap(true));
    }

    /// <summary>顺序加载：地图格子动画 → RegionTextureMapper → 图标 + 玩家</summary>
    IEnumerator LoadSequence(float characterDelay)
    {
        // 加载期间禁用空格寻路
        MapMoverManager.PlayerReadyForPathfinding = false;

        // 第一步：加载对象池 + 地图（房间逐个出现动画）
        yield return StartCoroutine(LoadAllPool());
        yield return StartCoroutine(LoadMap());

        // 第二步：等待 RegionTextureMapper 完成面片映射+渐变
        bool mappingDone = false;
        UnityEngine.Events.UnityAction onMappingDone = () => mappingDone = true;
        EventCenter.AddEventListener(E_EventType.RegionMappingDone, onMappingDone);
        yield return new WaitUntil(() => mappingDone);
        EventCenter.RemoveEventListener(E_EventType.RegionMappingDone, onMappingDone);

        // 第三步：在地图完全就绪后，统一创建 HexRoomIcon
        HexRoomStyleHandler.DeferIconCreation = false;
        HexRoomStyleHandler.CreateAllDeferredIcons();

        // 清理上轮残留的回合追踪（跨场景持久化导致旧 Mover 引用未释放）
        GameRoot.GetManager<MapMoverManager>()?.ClearRoundTracking();

        // 第四步：加载玩家角色（延迟控制出场节奏）
        yield return new WaitForSeconds(characterDelay);
        yield return StartCoroutine(LoadCharacter(0));

        // 玩家就绪，允许空格寻路
        MapMoverManager.PlayerReadyForPathfinding = true;
    }

    void BindUI(){
        if (EndRoundButton){
            EndRoundButton.onClick.RemoveAllListeners();
            EndRoundButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.Player_RoundEnd));
        }

        PlayerSkillButton.onClick.RemoveAllListeners();
        PlayerSkillButton.onClick.AddListener(() => EventCenter.EventTrigger(E_EventType.CallSkillPanel));

        if (PlayerEquipmentButton){
            PlayerEquipmentButton.onClick.RemoveAllListeners();
            PlayerEquipmentButton.onClick.AddListener(() =>{
                var uiMgr = GameRoot.GetManager<UIManager>();
                if (uiMgr == null) return;
                var panel = uiMgr.GetPanel<EquipmentPanel>(E_UIPanelType.EquipmentPanel);
                if (panel != null && panel.gameObject.activeSelf)
                    panel.Hide();
                else
                    uiMgr.OpenPanel<EquipmentPanel>(E_UIPanelType.EquipmentPanel);
            });
        }
    }

    IEnumerator LoadAllPool(){
        WaitForSeconds delay = new WaitForSeconds(0.4f);
        yield return delay;
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillSlot_技能槽位);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.SkillIcon_技能图标);
        EventCenter.EventTrigger(E_EventType.LoadObjPool, E_PoolType.FloatingText_跳字);

    }
    void Awake(){
        EventCenter.ClearAllEvents();
        //读取是否加载过地图,如果加载过，忽略
        gameRoot = GameRoot.Instance;
        BattleSkillFactory.RegisterAllSkills();

        // 在注册OrthoCameraNavigator之前恢复相机存档，确保其Awake读到正确位置
        var csd = JsonSaver.Load<CameraSaveData>();
        if (csd.IsValid()){
            var cam = Camera.main;
            if (cam != null){
                cam.transform.position = new Vector3(csd.cPosX, csd.cPosY, csd.cPosZ);
                cam.orthographicSize = csd.cSize;
            }
        }

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

        //正交相机漫游管理器（此时Camera.main已在上面恢复好位置）
        gameRoot.RegisterScene_MonoManager<OrthoCameraNavigator>();
        //技能管理器
        gameRoot.RegisterScene_MonoManager<MapSkillerCheker>();
        //角色射线检测管理器
        gameRoot.RegisterScene_MonoManager<CharacterRayCasterManager>();
        //混沌等级管理器
        gameRoot.RegisterScene_MonoManager<ChaosLevelManager>();
        //等级奖励管理器
        gameRoot.RegisterScene_MonoManager<LevelRewardManager>();

        JsonSaver.InitData<FirstLoadMap>(this, JsonSaver.Load<FirstLoadMap>().GetState);

        EventCenter.AddEventListener(E_EventType.UpdateRoundState, UpdateRoundText);
        EventCenter.AddEventListener(E_EventType.UpdateUIVitalityPoints, UpdateValityText);
        EventCenter.AddEventListener<int>(E_EventType.ChaosLevelUP, UpdateChaosLevelUI);

        // ClearAllEvents 会清掉全局管理器的监听，重新绑定房间重生管理器的事件
        GameRoot.GetManager<RoomRespawnManager>()?.RebindEvents();
        // 重新绑定 HexMapInteractManager 的事件（悬浮云朵检测、编辑器地形切换）
        GameRoot.GetManager<HexMapInteractManager>()?.RebindEvents();
        // 重新绑定 MapMoverManager 的事件（玩家位置更新、回合检测、机器人队列）
        GameRoot.GetManager<MapMoverManager>()?.RebindEvents();
        // 每次进入 MapScene 刷新寻路管理器的依赖引用
        GameRoot.GetManager<HexPathFindingManager>()?.ReInitForScene();
    }
    private void Start()
    {
        EventCenter.EventTrigger(E_EventType.UpdateRoundState);
        EventCenter.EventTrigger(E_EventType.UpdateUIVitalityPoints);
        // 初始化混沌等级 UI
        int chaosLevel = GameRoot.GetManager<ChaosLevelManager>()?.currentLevel ?? 1;
        UpdateChaosLevelUI(chaosLevel);
        // 初始化玩家等级 UI
        UpdatePlayerLevelUI();
        StartCoroutine(WaitBGM());
    }

    void OnEnable()
    {
        EventCenter.AddEventListener<int, int>(E_EventType.CharacterLevelUp, OnPlayerLevelUp);
        EventCenter.AddEventListener(E_EventType.Mover_PlayerStartMove, OnPlayerStartMove);
        EventCenter.AddEventListener(E_EventType.Mover_MoveStop, OnPlayerMoveStop);
    }

    void OnDisable()
    {
        EventCenter.RemoveEventListener<int, int>(E_EventType.CharacterLevelUp, OnPlayerLevelUp);
        EventCenter.RemoveEventListener(E_EventType.Mover_PlayerStartMove, OnPlayerStartMove);
        EventCenter.RemoveEventListener(E_EventType.Mover_MoveStop, OnPlayerMoveStop);
    }

    void OnPlayerStartMove()
    {
        if (EndRoundButton != null) EndRoundButton.interactable = false;
    }

    void OnPlayerMoveStop()
    {
        if (EndRoundButton != null) EndRoundButton.interactable = true;
    }

    void OnPlayerLevelUp(int oldLevel, int newLevel)
    {
        UpdatePlayerLevelUI();
    }

    void UpdatePlayerLevelUI()
    {
        if (playerLevelText == null) return;
        int lv = CharacterHandler.PlayerInstance?.CharacterData?.CurrentLevel ?? 1;
        playerLevelText.text = $"Lv.{lv}";
    }
    IEnumerator WaitBGM()
    {
        yield return new WaitForSeconds(2);
        GameRoot.GetManager<AudioManager>().PlayBGM("Music/BGM/地图BGM");
    }

    void UpdateChaosLevelUI(int level)
    {
        if (chaosLevelUIText != null)
            chaosLevelUIText.text = level.ToString();
    }

    IEnumerator LoadCharacter(float delay)
    {
        yield return new WaitForSeconds(delay);
        //产生角色
        var player1 = MapCharacterCaller.CallNewCharacter("Moveable");
        //支持外部角色调整
        player1.InitCharacterDataTag(E_CharacterType.P_海螺骑士, true, true);

        var mover = player1.GetComponent<CharacterMapMoveHandle>().iMapMover as Player_CharacterMapMover;
        var battleMgr = GameRoot.GetManager<GameBattleManager>();

        // 战败踢出：先站在战败房间上2秒，再用DoKickMove强制移动1格（不检查行动点）
        if (battleMgr != null && battleMgr.TryGetKickTarget(player1.transform.position, out HexRoomTag kickTarget))
        {
            battleMgr.SuppressBattleTrigger = true;
            mover.CharacterZeroMove();
            battleMgr.SuppressBattleTrigger = false;

            yield return new WaitForSeconds(2f);

            // 关闭寻路状态（防止2秒等待期间玩家按Space开启的寻路残留在地块上）
            GameRoot.GetManager<HexPathFindingManager>()?.SetPathFindState(false);

            mover.DoKickMove(new System.Collections.Generic.List<HexRoomTag> { kickTarget });

            battleMgr.ClearPendingKick();
        }
        else
        {
            mover.CharacterZeroMove();
        }

        // 首次存档授予14个初始技能（Skill0~Skill13，每个存档只一次）
        yield return new WaitForSeconds(2f);
        yield return null; // 等一帧确保 CharacterMapSkiller.Start() 已执行
        var flag = JsonSaver.Load<InitialSkillsGranted>();
        if (!flag.granted)
        {
            var skiller = player1.GetComponent<CharacterMapSkiller>();
            if (skiller != null)
            {
                for (int i = 0; i <= 13; i++)
                    skiller.GetNewSkill(i);
                skiller.UpdateActableDataList(skiller.RestWholeSkillDatas, skiller.NormalSkillDatas, skiller.ATBSkillDatas);
                JsonSaver.Save(new InitialSkillsGranted(true));

                GameRoot.GetManager<UIManager>().OpenPanel<MessagePanel>(
                    E_UIPanelType.MessagePanel,
                    p => p.SetMessage("获得14个初始技能")
                    //p => p.SetMessage("获得59个全部技能")
                );
            }
        }
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

[Serializable]
public class CameraSaveData : IValidatable
{
    public float cPosX, cPosY, cPosZ;
    public float cSize;
    public bool hasData;

    public CameraSaveData() { hasData = false; }
    public CameraSaveData(Vector3 pos, float size)
    {
        cPosX = pos.x; cPosY = pos.y; cPosZ = pos.z;
        cSize = size;
        hasData = true;
    }

    public bool IsValid() => hasData;
}

[Serializable]
public class InitialSkillsGranted : IValidatable
{
    public bool granted;
    public bool IsValid() => true;
    public InitialSkillsGranted() { }
    public InitialSkillsGranted(bool g) { granted = g; }
}