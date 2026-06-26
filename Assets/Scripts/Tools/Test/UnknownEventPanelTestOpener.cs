using Core;
using UnityEngine;

/// <summary>
/// 测试脚本——按U切换随机事件面板(打开/关闭)，每次打开随机选取一个事件
/// </summary>
public class UnknownEventPanelTestOpener : MonoBehaviour
{
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    var uiMgr = GameRoot.GetManager<UIManager>();
        //    if (uiMgr == null) return;
        //    var panel = uiMgr.GetPanel<UnknownEventPanel>(E_UIPanelType.UnknownEventPanel);
        //    if (panel != null && panel.IsAnimating) return;
        //    if (panel != null && panel.gameObject.activeSelf)
        //        panel.Hide();
        //    else
        //        uiMgr.OpenPanel<UnknownEventPanel>(E_UIPanelType.UnknownEventPanel);
        //}
    }
}
