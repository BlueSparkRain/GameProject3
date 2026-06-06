using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂在场景内金币 Text 上，自动监听金币变化并更新显示
/// </summary>
public class GoldTextUpdater : MonoBehaviour{
    public TMP_Text goldText;

    void Start(){
        EventCenter.AddEventListener(E_EventType.UpdateUIGold, Refresh);
        Refresh();
    }
    void OnDestroy(){
        EventCenter.RemoveEventListener(E_EventType.UpdateUIGold, Refresh);
    }

    void Refresh(){
        if (goldText != null){
            var gm = GameRoot.GetManager<GoldManager>();
            goldText.text = gm != null ? gm.Gold.ToString() : "0";
        }
    }
}
