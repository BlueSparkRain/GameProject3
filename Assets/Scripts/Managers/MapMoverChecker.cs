using Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMoverChecker : MonoSceneManager
{
    string mapIconPrefabPath = "Prefab/MapUI/CharacterMapIcon";

    public Transform mapIconParent;

    public IMapMoveable currentIMovable;
    //public PlayerCharacterMapMover currentMover;

    HexPathFindingManager hexPathFindingManager;

    public override void MgrUpdate(float deltaTime) { }

    private Dictionary<CharacterMapIcon, IMapMoveable> imapMovableDic = new Dictionary<CharacterMapIcon, IMapMoveable>();

    int iconNUm = 0;

    protected override void MgrOnInit()
    {
        base.MgrOnInit();
        EventCenter.AddEventListener<IMapMoveable,Vector3>(E_EventType.Mover_CheckCurrrentRoom, CheckCurrentRoom);
        hexPathFindingManager =GameRoot.GetManager<HexPathFindingManager>();
    }

    //每回合，所有可以移动的角色会依次行动（先按照固定顺序）
    //玩家回合，无限/有限时间，可以根据玩家鼠标来寻路
    //敌人回合，时间，根据策略来自动调用寻路。

    public CharacterMapIcon CreateNewMapIcon(Player_CharacterMapMover characterRoomMover, Transform charcaterTrans)
    {
        var newIcon = GameObject.Instantiate(Resources.Load<GameObject>(mapIconPrefabPath), mapIconParent).GetComponent<CharacterMapIcon>();
        newIcon.transform.localScale = Vector3.zero;
        newIcon.transform.DOScale(1, 0.4f).SetEase(Ease.OutQuad).From(0);
        newIcon.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
        imapMovableDic.Add(newIcon, characterRoomMover);
        newIcon.InitIcon(characterRoomMover.CharacterType, charcaterTrans);
        newIcon.GetComponent<RectTransform>().localPosition += new Vector3(200, 0, 0) * iconNUm;
        iconNUm++;
        return newIcon;
    }

    /// <summary>
    /// 只有玩家自身是通过MapIcon来交互移动的
    /// </summary>
    /// <param name="mapIcon"></param>
    /// <returns></returns>
    public IMapMoveable GetTargetPlayerMover(CharacterMapIcon mapIcon)
    {
        if (imapMovableDic.ContainsKey(mapIcon))
        {
            currentIMovable = imapMovableDic[mapIcon];

            if ((currentIMovable as Player_CharacterMapMover).IsMoving)
            {
                Debug.Log("[MapMoverChecker]---请求失败！目标玩家Mover正在移动中");
                mapIcon.FlashWarnning();
                return null;
            }
            else
                return currentIMovable;
        }
        else
        {
            Debug.Log("[MapMoverChecker]---请求失败！目标Mover未注册");
            return null;
        }
    }
    //public void SetCurrentMover(PlayerCharacterMapMover characterRoomMover)
    //{
    //    currentMover = characterRoomMover;

    //}
        
    public void SetCurrentMover(IMapMoveable iMover)
    {
        currentIMovable = iMover;
    }

    public void MoverGo(List<HexRoomData> path)
    {
        currentIMovable.DoMoveFunc(path);
        //currentMover.MoveByPath(path);
    }

    ///// <summary>
    ///// 所有Mover在每次移动后都会更新当前所处的Room
    ///// </summary>
    void CheckCurrentRoom(IMapMoveable imover,Vector3 rayStart)
    {
        Ray ray = new Ray(rayStart, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5, LayerMask.GetMask("HexRoom")))
        {
            HexRoomData downRoom = hit.collider.GetComponent<HexRoomData>();

            if (downRoom != imover.currentRoom)
            {
                hexPathFindingManager.SetPlayerStartRoom(downRoom);
                Debug.Log($"玩家位置更新 row:{downRoom.row},col:{downRoom.col}");
            }
            if (downRoom != null)
            {
                imover.currentRoom = downRoom;
            }
        }
    }
}
