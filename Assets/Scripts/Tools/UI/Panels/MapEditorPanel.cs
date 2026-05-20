using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapTerrainEditorPanel : UIPanelBase
{
    [Header("当前房间-行.列")]
    public TMP_Text currentRoomPosText;
    [Header("当前Tag是否被操作过")]
    public TMP_Text currentRoomHasEditedText;

    [Header("海洋标记按钮")]
    public Button OceanButton;
    [Header("陆地标记按钮")]
    public Button LandButton;
    [Header("树木标记按钮")]
    public Button TreeButton;
    [Header("石头标记按钮")]
    public Button StoneButton;
    [Header("山脉标记按钮")]
    public Button MountainButton;

    [Header("小怪战斗标记按钮")]
    public Button LowBattleButton;
    [Header("精英战斗标记按钮")]
    public Button MidBattleButton;
    [Header("Boss战斗标记按钮")]
    public Button HighBattleButton;
    [Header("事件标记按钮")]
    public Button EventButton;
    [Header("奖励标记按钮")]
    public Button RewardButton;
    [Header("城镇标记按钮")]
    public Button CityButton;

    private HexTerrainStyleHandler currentTag;
    private HexRoomTag roomData;
    public GameObject ArrowPrefab;
    GameObject Arrow;
    public Button hideButton;

    void ExitEdit()
    {
        if (Arrow != null)
            Arrow.transform.position = Vector3.zero;
    }

    public void GetHexTag(HexRoomTag roomData)
    {
        this.roomData = roomData;
        currentRoomPosText.text = $"({roomData.row},{roomData.col})";
        Arrow.transform.position = roomData.transform.position + Vector3.up * 2;
        currentTag = roomData.GetComponent<HexTerrainStyleHandler>();
        currentRoomHasEditedText.text = currentTag.isEdited ? "本次编辑已配置:" + currentTag.hexTerrainType : "本次编辑尚未配置(默认Ocean):";
    }


    void SetButtonTag(E_HexTerrainType hexTerrainType)
    {
        EventCenter.EventTrigger(E_EventType.Editor_Terrain, new Vector2Int(roomData.row, roomData.col), hexTerrainType);
        currentTag.InitTerrainStyle(hexTerrainType);
        EventCenter.EventTrigger(E_EventType.Editor_Terrain_OneTime);
        ExitEdit();
    }

    protected override void OnInit()
    {
        base.OnInit();
        Arrow = GameObject.Instantiate(ArrowPrefab);
        OceanButton.onClick.AddListener(() =>
            SetButtonTag(E_HexTerrainType.Obstacle_Ocean));
        LandButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Walkable_EmptyLand));
        TreeButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Obstacle_Tree));
        StoneButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Obstacle_Stone));
        MountainButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Obstacle_Mountain));

        LowBattleButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Walkable_LowLevel_BattleRoom));
        MidBattleButton.onClick.AddListener(() =>
           SetButtonTag(E_HexTerrainType.Walkable_MidLevel_BattleRoom)); 
        HighBattleButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Walkable_HighLevel_BattleRoom));


        EventButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Walkable_UnknownEventRoom));
        RewardButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Walkable_RewardRoom));
        CityButton.onClick.AddListener(() =>
                   SetButtonTag(E_HexTerrainType.Walkable_CityShopRoom));

        hideButton.onClick.AddListener(() => Hide());
        EventCenter.AddEventListener(E_EventType.Editor_Terrain_ExitEdit, ExitEdit);
    }

    protected override void BeforeFadeInAnimCallBack()
    {
        base.BeforeFadeInAnimCallBack();
        canOpen = !canOpen;
    }

    protected override void EnterAnimCallBack()
    {
        base.EnterAnimCallBack();
        //canOpen = !canOpen;
    }

    protected override void BeforeFadeOutAnimCallBack()
    {
        //canOpen = !canOpen;
        base.BeforeFadeOutAnimCallBack();
    }
    protected override void ExitAnimCallBack()
    {
        base.ExitAnimCallBack();
        canOpen = !canOpen;

    }
}
