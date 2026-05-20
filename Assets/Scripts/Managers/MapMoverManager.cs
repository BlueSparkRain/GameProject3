using Core;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MapMoverManager : MonoGlobalManager
{
    public Transform mapIconParent;
    public IMapMoveable currentIMovable;
    string mapIconPrefabPath = "Prefab/MapUI/PlayerMapIcon";
    HexPathFindingManager hexPathFindingManager;
    public override void MgrUpdate(float deltaTime) { }
    //玩家角色的映射
    private Dictionary<PlayerMapIcon, IMapMoveable> playerIconMoverDic = new Dictionary<PlayerMapIcon, IMapMoveable>();
    private Dictionary<IMapMoveable, MapMoverPosition> posDic = new Dictionary<IMapMoveable, MapMoverPosition>();
    MapMoverPosition posSaveData;
    int iconNum = 0;

    //所有的移动对象每回合的行动情况
    //只有所有都是ready，才能进入下个回合
    public Dictionary<IMapMoveable, bool> roundMoveDic = new Dictionary<IMapMoveable, bool>();

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener<IMapMoveable, Vector3>(E_EventType.Mover_CheckCurrrentRoom, CheckCurrentRoom);
        hexPathFindingManager = GameRoot.GetManager<HexPathFindingManager>();
        EventCenter.AddEventListener<IMapMoveable>(E_EventType.Character_Mover_Regist, RegisterBornMover);
        EventCenter.AddEventListener<IMapMoveable>(E_EventType.OneMoverEndRound, OneMoverEndRound);
        //需要存储玩家对应Mover
    }

    //每回合，所有可以移动的角色会依次行动（先按照固定顺序）
    //玩家回合，无限/有限时间，可以根据玩家鼠标来寻路
    //敌人回合，时间，根据策略来自动调用寻路。

    #region 玩家Mover相关逻辑：（1）MapIcon创建注册（2）获取MapIcon关联Mover

    /// <summary>
    /// 创建一个玩家操作角色的UIIcon
    /// </summary>
    /// <param name="characterRoomMover"></param>
    /// <param name="charcaterTrans"></param>
    /// <returns></returns>
    public PlayerMapIcon CreateNewMapIcon(Player_CharacterMapMover characterRoomMover, Transform charcaterTrans)
    {
        if (mapIconParent == null){
            iconNum = 0;
            mapIconParent = GameObject.FindWithTag("MapIconContent").transform;
        }
        var newIcon = GameObject.Instantiate(Resources.Load<GameObject>(mapIconPrefabPath), mapIconParent).GetComponent<PlayerMapIcon>();
        newIcon.transform.localScale = Vector3.zero;
        newIcon.GetComponent<RectTransform>().localPosition += new Vector3(200, 0, 0) * iconNum;
        newIcon.transform.DOLocalMoveY(120, 0.3f).From(-400);
        newIcon.transform.DOScale(new Vector3(1, 1, 0), 0.5f).SetEase(Ease.OutQuad).From(new Vector3(1, 0, 0));
        playerIconMoverDic.Add(newIcon, characterRoomMover);
        newIcon.InitIcon(characterRoomMover.CharacterType, charcaterTrans);
        iconNum++;
        return newIcon;
    }


    /// <summary>
    /// 注册移动角色的位置数据，便于更新
    /// </summary>
    /// <param name="moveHandle"></param>
    public void RegisterMoverPostion(IMapMoveable iMapMover, MapMoverPosition moverPosData)
    {
        posDic.Add(iMapMover, moverPosData);
        Debug.Log("可移动MOver-位置数据保存-注册:" + posDic.Count);
    }

    /// <summary>
    /// 只有玩家自身是通过MapIcon来交互移动的
    /// </summary>
    /// <param name="mapIcon"></param>
    /// <returns></returns>
    public IMapMoveable GetTargetPlayerMover(PlayerMapIcon mapIcon)
    {
        if (playerIconMoverDic.ContainsKey(mapIcon))
        {
            currentIMovable = playerIconMoverDic[mapIcon];
            if ((currentIMovable as Player_CharacterMapMover).IsMoving)
            {
                Debug.Log("[MapMoverManager]---请求失败！目标玩家Mover正在移动中");
                mapIcon.FlashWarnning();
                return null;
            }
            else
                return currentIMovable;
        }
        else
            return null;
    }

    #endregion

    public void SetCurrentMover(IMapMoveable iMover, Vector3 rayStartPos)
    {
        currentIMovable = iMover;
        CheckCurrentRoom(currentIMovable, rayStartPos);
    }

    public void MoverGo(List<HexRoomTag> path)
    {
        currentIMovable.DoMoveFunc(path);
    }

    ///// <summary>
    ///// 所有Mover在每次移动后都会更新当前所处的Room
    ///// </summary>
    void CheckCurrentRoom(IMapMoveable imover, Vector3 rayStart)
    {
        Ray ray = new Ray(rayStart, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5, LayerMask.GetMask("HexRoom")))
        {
            HexRoomTag downRoom = hit.collider.GetComponent<HexRoomTag>();

            if (downRoom != imover.currentRoom)
            {
                hexPathFindingManager.SetPlayerStartRoom(downRoom);
                //Debug.Log($"玩家位置更新 row:{downRoom.row},col:{downRoom.col}");
                posDic[imover].SetPos(downRoom.row, downRoom.col);
            }
            if (downRoom != null)
            {
                imover.currentRoom = downRoom;
                //触发对应的房间逻辑
               
                imover.currentRoom.IHexRoom.DoHexRoomLogic();
            }
        }
    }

    //加载数据对应
    public override void MgrDispose()
    {
        //卸载场景前保存
        base.MgrDispose();
    }

    //关闭程序前保存
    private void OnApplicationQuit()
    {
        foreach (var item in posDic)
        {

            JsonSaver.Save(item.Value, item.Value.uniqueId);
        }
    }

    #region 回合检测
    /// <summary>
    /// 一个Mover宣布结束自身回合
    /// </summary>
    /// <param name="mover"></param>
    void OneMoverEndRound(IMapMoveable mover)
    {
        if (roundMoveDic.ContainsKey(mover))
        {
            roundMoveDic[mover] = true;
        }
        CheckNewRound();
    }

    /// <summary>
    /// 登记场上新的Mover
    /// </summary>
    /// <param name="mover"></param>
    void RegisterBornMover(IMapMoveable mover)
    {
        if (!roundMoveDic.ContainsKey(mover))
        {
            roundMoveDic.Add(mover, false);
            Debug.Log("新添加一个Mover");
        }
    }

    /// <summary>
    /// 每当一个Mover宣布结束回合，就检测一下
    /// </summary>
    void CheckNewRound()
    {
        foreach (var item in roundMoveDic)
        {
            if (!item.Value)
            {
                Debug.Log("[MapMoverManager]---存在Mover未行动，本回合尚未结束");
                return;
            }
        }
        Debug.Log("[MapMoverManager]---本回合结束!");
        EventCenter.EventTrigger(E_EventType.NewRound);
    }
    #endregion
}

