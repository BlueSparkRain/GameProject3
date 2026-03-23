using Core;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 六边形房间基础类（仅保存坐标信息，无业务逻辑）
/// </summary>
public class HexRoom : MonoBehaviour,IHexRoom
{
    [Header("六边形轴向坐标")]
    public int row; // 轴向坐标q
    public int col; // 轴向坐标r

    [Header("房间类型")]
    public E_HexRoomType roomType=E_HexRoomType.None_无;

    public void InitRoomID(int _row,int _col) {
        row= _row; col = _col;
    }

    public void InitRoomStyle(E_HexRoomType _roomType) {
        roomType=_roomType;
    }
    public virtual void ResetSelf(){

    }

    public void CallBattle() {
        Debug.Log("Go");
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(UIPanelType.BattlePanel);
        //panel => panel.SetTragetPos(0.5f* Camera.main.WorldToScreenPoint(transform.position)));
        //panel => panel.SetTragetPos(new Vector3(-1000,500),new Vector3(500,-500)));
    }

    public virtual void DoRoomJob(UnityAction roomJob){

    }
}

public interface IHexRoom {
    public void DoRoomJob(UnityAction roomJob);
}

