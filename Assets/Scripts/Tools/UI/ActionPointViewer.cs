using Core;
using TMPro;
using UnityEngine;

/// <summary>
/// 挂在场景内 Text 上，监听 ActionPointsManager 自动更新行动点显示
/// </summary>
public class ActionPointViewer : MonoBehaviour{
    public TMP_Text actionPointText;
    void Start(){
        EventCenter.AddEventListener(E_EventType.UpdateUIActionPoints, Refresh);
        Refresh();
    }
    void OnDestroy(){
        EventCenter.RemoveEventListener(E_EventType.UpdateUIActionPoints, Refresh);
    }
    void Refresh(){
        if (actionPointText == null) return;
        var ap = GameRoot.GetManager<ActionPointsManager>();
        if (ap != null)
            //actionPointText.text = $"{ap.RemainActionPoints}/{ap.MaxActionPoints}";
            actionPointText.text = $"{ap.RemainActionPoints}";
        else
            actionPointText.text = "0";
    }
}
