using Core;
using UnityEngine;

/// <summary>
/// 六边形房间基础类（仅保存坐标信息，无业务逻辑）
/// </summary>
public class HexRoom : MonoBehaviour
{
    [Header("六边形轴向坐标")]
    public int row; // 轴向坐标q
    public int col; // 轴向坐标r
    public void InitRoomID(int _row,int _col) { 
        row= _row; col = _col;
    }
    public void ResetSelf(){

    }

    public void CallBattle() {
        Debug.Log("Go");
        GameRoot.GetManager<UIManager>().OpenPanel<BattlePanel>(UIPanelType.BattlePanel);
        //panel => panel.SetTragetPos(0.5f* Camera.main.WorldToScreenPoint(transform.position)));
        //panel => panel.SetTragetPos(new Vector3(-1000,500),new Vector3(500,-500)));
    }
}