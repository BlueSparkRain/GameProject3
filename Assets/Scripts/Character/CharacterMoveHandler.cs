using Core;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;


public class CharacterMapMoveHandle : MonoBehaviour, ICanSave_And_Load
{
    [Header("唯一ID（自动生成，请勿手动修改）")]
    string uniqueId = "player";

    [Header("当前坐标")]
    public int currentRow;
    public int currentCol;

    MapMoverPosition moverPosData;
    bool _isPlayer;

    public IMapMoveable iMapMover;

    public void SetUniqueId(string id) { uniqueId = id; }

    /// <summary>
    /// 已知是高级角色（可以寻路）
    /// </summary>
    /// <param name="isPlayer"></param>
    public void InitMover(bool isPlayer, E_CharacterType characterType)
    {
        _isPlayer = isPlayer;
        //characterType = GetComponent<CharacterData>().characterType;
        iMapMover = isPlayer ?
            new Player_CharacterMapMover(characterType, transform) :
            new Robot_CharacterMapMover(transform, GetComponent<CharacterHandler>(),
                GetComponent<CharacterLevelUpHandler>(), GetComponent<CharacterMapSkiller>());
        JsonSaver.InitData<MapMoverPosition>(this, uniqueId);
        //StartCoroutine(WaitMapLoad());
        //注册Mover
        if (moverPosData == null)
            moverPosData = new MapMoverPosition(uniqueId);
        EventCenter.EventTrigger(E_EventType.Character_Mover_Regist, iMapMover);
        GameRoot.GetManager<MapMoverManager>().RegisterMoverPostion(this.iMapMover, moverPosData);

    }


    IEnumerator WaitMapLoad()
    {
        yield return new WaitForSeconds(1);
        JsonSaver.InitData<MapMoverPosition>(this, uniqueId);
    }

    void UpdateCurrentRoom(HexRoomTag hexRoomData){
        iMapMover.currentRoom = hexRoomData;
    }
    /// <summary>由 CheckCurrentRoom 直接调用，不依赖事件链</summary>
    public void UpdatePosition(int row, int col)
    {
        currentRow = row;
        currentCol = col;
        if (moverPosData != null)
            moverPosData.SetPos(row, col);
        // 立即持久化到 JSON
        JsonSaver.Save(moverPosData, moverPosData.uniqueId);
    }
    void OnDrawGizmosSelected(){
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 4);
    }
    void OnDestroy(){
        JsonSaver.Save(moverPosData, moverPosData.uniqueId);
    }
    IEnumerator SetCharacterPos(Vector3 pos){
        transform.position = pos + Vector3.up * GameRoot.GetManager<GameMapManager>().characterYOffset;
        transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(1.2f);
        transform.DOScale(1.5f, 0.3f).SetEase(Ease.InQuart).From(0);
        yield return new WaitForSeconds(0.3f);
        transform.DOScale(1, 0.2f).SetEase(Ease.OutQuart);
    }
    /// <summary>
    /// 从【自己的存档】加载数据
    /// </summary>
    public void InitBySaveData()
    {
        MapMoverPosition data = JsonSaver.Load<MapMoverPosition>(uniqueId);
        //Debug.Log($"读取到位置记录：{data.pos.x},{data.pos.y}");
        // 把存档数据赋值给角色实例
        currentRow = data.pos.x;
        currentCol = data.pos.y;
        if (moverPosData == null)
            moverPosData = data;
        else
            moverPosData.SetPos(data.pos.x, data.pos.y);

        StartCoroutine(SetCharacterPos(GameRoot.GetManager<GameMapManager>().GetTargetRoom(data.pos).transform.position ));
    }

    /// <summary>
    /// 无存档时，初始化默认数据
    /// </summary>
    public void InitBySelf(){
        if (!JsonSaver.HasValidData<MapMoverPosition>(uniqueId)){
            var mapMgr = GameRoot.GetManager<GameMapManager>();
            HexRoomTag room = _isPlayer
                ? mapMgr.GetTargetRoom(new Vector2Int(10, 43))
                : mapMgr.GetRnadomRoom();
            if (room == null){
                Debug.LogError("无法获取随机房间，地图可能未初始化");
                return;
            }
            StartCoroutine(SetCharacterPos(room.transform.position ));
            currentRow = room.row;
            currentCol = room.col;

            if (moverPosData == null)
                moverPosData = new MapMoverPosition(uniqueId);
            moverPosData.SetPos(currentRow, currentCol);
            JsonSaver.Save(moverPosData, uniqueId);
            //Debug.Log("ID首次生成：" + uniqueId);
        }
    }
}


[Serializable]
public class MapMoverPosition : IValidatable
{
    // ✅ 核心：唯一ID（绑定角色实例的关键）
    public string uniqueId = "";
    public Vector2Int pos;
    public void SetPos(int row, int col)
    {
        //Debug.Log($"{uniqueId}+号Mover已更新位置:row:{row},col:{col}");
        pos.Set(row, col);
    }
    public MapMoverPosition() { }
    public MapMoverPosition(string _unique)
    {
        uniqueId = _unique;
    }
    public bool IsValid()
    {
        return uniqueId != "";
    }
}